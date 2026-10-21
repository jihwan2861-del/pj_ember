using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EmberPrototype
{
    /// <summary>Draws a continuous illustrated skin over occupied cells without changing collision.</summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("Ember/Doodle Terrain Skin")]
    public sealed class DoodleTerrainSkin : MonoBehaviour
    {
        [SerializeField] private Tilemap source;
        [SerializeField] private Material fillMaterial;
        [SerializeField] private Material outlineMaterial;
        [SerializeField] private Sprite grassSprite;
        [SerializeField, Min(0.01f)] private float textureWorldSize = 2f;
        [SerializeField, Range(0f, 0.15f)] private float cornerRadius = 0.06f;
        [SerializeField, Range(0.005f, 0.12f)] private float outlineWidth = 0.045f;
        [SerializeField, Min(0.1f)] private float grassPatchWidth = 1f;
        [SerializeField, Range(0.05f, 2f)] private float grassHeight = 0.28f;
        [Header("Dark Infill")]
        [Tooltip("Ignore bottom edges at or below this height when terrain continues below the visible room.")]
        [SerializeField] private bool openBottomInfill;
        [SerializeField] private float openBottomY;

        private const string GeneratedName = "Doodle Terrain Skin (Generated)";
        private const int ArcSteps = 4;
        private readonly HashSet<Vector3Int> cells = new HashSet<Vector3Int>();
        private readonly List<Mesh> generatedMeshes = new List<Mesh>();
        private GameObject generated;
        private TilemapRenderer hiddenSourceRenderer;
        private bool previousForceRenderingOff;
        private bool needsRebuild = true;
        private bool rebuilding;
        private Matrix4x4 previousSourceMatrix;
        private Matrix4x4 previousOwnerMatrix;
        private int previousSortingLayer, previousSortingOrder;
        private readonly List<Vector2> boundaryStarts = new List<Vector2>();
        private readonly List<Vector2> boundaryEnds = new List<Vector2>();
        private readonly Dictionary<Vector3, float> boundaryDistanceCache = new Dictionary<Vector3, float>();
        private Vector3 previousCellSize, previousCellGap;
        private GridLayout.CellLayout previousCellLayout;
        private GridLayout.CellSwizzle previousCellSwizzle;
#if UNITY_EDITOR
        private double lastEditorRebuild;
#endif

        private struct Edge
        {
            internal Vector3Int from, to;
            internal Edge(Vector3Int from, Vector3Int to) { this.from = from; this.to = to; }
        }

        public void Configure(Tilemap terrain, Material fill, Material outline, Sprite grass)
        {
            source = terrain;
            fillMaterial = fill;
            outlineMaterial = outline;
            grassSprite = grass;
            needsRebuild = true;
        }

        private void OnEnable()
        {
            Tilemap.tilemapTileChanged += OnTilesChanged;
            needsRebuild = true;
#if UNITY_EDITOR
            EditorApplication.update += EditTick;
            Undo.undoRedoPerformed += OnEditorUndoRedo;
#endif
        }

        // Validation may run while Unity is loading or importing. It must not create objects.
        private void OnValidate() => needsRebuild = true;

        private void OnDisable()
        {
            Tilemap.tilemapTileChanged -= OnTilesChanged;
#if UNITY_EDITOR
            EditorApplication.update -= EditTick;
            Undo.undoRedoPerformed -= OnEditorUndoRedo;
#endif
            ClearGenerated();
        }

        private void OnTilesChanged(Tilemap changed, Tilemap.SyncTile[] changes)
        {
            if (changed == source) needsRebuild = true;
        }

        private void DetectSourceChanges()
        {
            if (source != null && (source.transform.localToWorldMatrix != previousSourceMatrix
                || transform.localToWorldMatrix != previousOwnerMatrix)) needsRebuild = true;
            if (source != null && source.TryGetComponent(out TilemapRenderer sourceRenderer)
                && (sourceRenderer.sortingLayerID != previousSortingLayer
                    || sourceRenderer.sortingOrder != previousSortingOrder)) needsRebuild = true;
            if (source != null && source.layoutGrid != null)
            {
                Grid grid = source.layoutGrid;
                if (grid.cellSize != previousCellSize || grid.cellGap != previousCellGap
                    || grid.cellLayout != previousCellLayout || grid.cellSwizzle != previousCellSwizzle)
                    needsRebuild = true;
            }
        }

        private void LateUpdate()
        {
            DetectSourceChanges();
            if (!needsRebuild || rebuilding) return;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // EditorApplication.update also runs when the Inspector is not repainting.
                return;
            }
#endif
            Rebuild();
        }

#if UNITY_EDITOR
        private void OnEditorUndoRedo() => needsRebuild = true;

        private void EditTick()
        {
            if (this == null || !isActiveAndEnabled || EditorApplication.isPlayingOrWillChangePlaymode
                || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            DetectSourceChanges();
            if (!needsRebuild || rebuilding || EditorApplication.timeSinceStartup - lastEditorRebuild < 0.05) return;
            lastEditorRebuild = EditorApplication.timeSinceStartup;
            Rebuild();
            EditorApplication.QueuePlayerLoopUpdate();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
#endif

        public void Rebuild()
        {
            if (rebuilding || !isActiveAndEnabled) return;
            rebuilding = true;
            try
            {
                needsRebuild = false;
                ClearGenerated();
                cells.Clear();
                if (source == null) return;
                previousSourceMatrix = source.transform.localToWorldMatrix;
                previousOwnerMatrix = transform.localToWorldMatrix;
                if (source.layoutGrid != null)
                {
                    Grid grid = source.layoutGrid;
                    previousCellSize = grid.cellSize;
                    previousCellGap = grid.cellGap;
                    previousCellLayout = grid.cellLayout;
                    previousCellSwizzle = grid.cellSwizzle;
                }
                if (fillMaterial == null) return;
                foreach (Vector3Int cell in source.cellBounds.allPositionsWithin)
                    if (source.HasTile(cell)) cells.Add(cell);
                if (cells.Count == 0) return;

                generated = NewChild(GeneratedName, transform);
                TilemapRenderer sourceRenderer = source.GetComponent<TilemapRenderer>();
                int layer = sourceRenderer != null ? sourceRenderer.sortingLayerID : 0;
                int order = sourceRenderer != null ? sourceRenderer.sortingOrder : 0;
                previousSortingLayer = layer;
                previousSortingOrder = order;
                BuildFill(layer, order + 1);
                if (outlineMaterial != null) BuildContours(layer, order + 2);
                if (grassSprite != null) BuildGrass(layer, order + 3);
                if (sourceRenderer != null)
                {
                    hiddenSourceRenderer = sourceRenderer;
                    previousForceRenderingOff = sourceRenderer.forceRenderingOff;
                    sourceRenderer.forceRenderingOff = true;
                }
            }
            finally { rebuilding = false; }
        }

        private GameObject NewChild(string objectName, Transform parent)
        {
            var child = new GameObject(objectName) { hideFlags = HideFlags.HideAndDontSave };
            child.transform.SetParent(parent, false);
            child.layer = gameObject.layer;
            return child;
        }

        private Vector3 World(Vector3Int corner) => source.CellToWorld(corner);

        private void BuildFill(int layer, int order)
        {
            var vertices = new List<Vector3>(cells.Count * 12);
            var uv = new List<Vector2>(cells.Count * 12);
            var distances = new List<Vector2>(cells.Count * 12);
            var triangles = new List<int>(cells.Count * 30);
            var polygon = new List<Vector3>(20);
            float uvSize = Mathf.Max(0.01f, textureWorldSize);
            BuildDistanceBoundaries();
            foreach (Vector3Int cell in cells)
            {
                Vector3 origin = World(cell);
                Vector3 axisX = World(cell + Vector3Int.right) - origin;
                Vector3 axisY = World(cell + Vector3Int.up) - origin;
                float radius = Mathf.Min(cornerRadius, axisX.magnitude * 0.2f, axisY.magnitude * 0.2f);
                float rx = radius / Mathf.Max(0.00001f, axisX.magnitude);
                float ry = radius / Mathf.Max(0.00001f, axisY.magnitude);
                bool left = cells.Contains(cell + Vector3Int.left);
                bool right = cells.Contains(cell + Vector3Int.right);
                bool down = cells.Contains(cell + Vector3Int.down);
                bool up = cells.Contains(cell + Vector3Int.up);
                polygon.Clear();
                AddCorner(polygon, origin, axisX, axisY, 0f, 0f, rx, ry, 180f, !left && !down);
                AddCorner(polygon, origin, axisX, axisY, 1f, 0f, rx, ry, 270f, !right && !down);
                AddCorner(polygon, origin, axisX, axisY, 1f, 1f, rx, ry, 0f, !right && !up);
                AddCorner(polygon, origin, axisX, axisY, 0f, 1f, rx, ry, 90f, !left && !up);
                int center = vertices.Count;
                AddVertex(origin + (axisX + axisY) * 0.5f, vertices, uv, distances, uvSize);
                foreach (Vector3 point in polygon) AddVertex(point, vertices, uv, distances, uvSize);
                for (int i = 0; i < polygon.Count; i++)
                {
                    triangles.Add(center);
                    triangles.Add(center + 1 + (i + 1) % polygon.Count);
                    triangles.Add(center + 1 + i);
                }
            }
            var mesh = new Mesh { name = "Doodle Terrain Fill", hideFlags = HideFlags.HideAndDontSave };
            if (vertices.Count > 65535) mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uv);
            mesh.SetUVs(1, distances);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            generatedMeshes.Add(mesh);
            generated.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = generated.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = fillMaterial;
            renderer.sortingLayerID = layer;
            renderer.sortingOrder = order;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private void AddVertex(Vector3 world, List<Vector3> vertices, List<Vector2> uv,
            List<Vector2> distances, float uvSize)
        {
            vertices.Add(transform.InverseTransformPoint(world));
            uv.Add(new Vector2(world.x / uvSize, world.y / uvSize));
            distances.Add(new Vector2(DistanceToBoundary(world), 0f));
        }

        private void BuildDistanceBoundaries()
        {
            boundaryStarts.Clear();
            boundaryEnds.Clear();
            boundaryDistanceCache.Clear();
            foreach (Vector3Int cell in cells)
            {
                Vector3Int r = cell + Vector3Int.right, t = cell + Vector3Int.up;
                Vector3Int tr = r + Vector3Int.up;
                if (!cells.Contains(cell + Vector3Int.down)
                    && (!openBottomInfill || Mathf.Max(World(cell).y, World(r).y) > openBottomY + 0.0001f))
                    AddDistanceBoundary(cell, r);
                if (!cells.Contains(r)) AddDistanceBoundary(r, tr);
                if (!cells.Contains(t)) AddDistanceBoundary(tr, t);
                if (!cells.Contains(cell + Vector3Int.left)) AddDistanceBoundary(t, cell);
            }
        }

        private void AddDistanceBoundary(Vector3Int a, Vector3Int b)
        {
            boundaryStarts.Add(World(a));
            boundaryEnds.Add(World(b));
        }

        private float DistanceToBoundary(Vector3 world)
        {
            if (boundaryDistanceCache.TryGetValue(world, out float cached)) return cached;
            Vector2 point = world;
            float nearestSquared = float.PositiveInfinity;
            for (int i = 0; i < boundaryStarts.Count; i++)
            {
                Vector2 a = boundaryStarts[i], delta = boundaryEnds[i] - a;
                float parameter = Mathf.Clamp01(Vector2.Dot(point - a, delta) / Mathf.Max(delta.sqrMagnitude, 0.000001f));
                nearestSquared = Mathf.Min(nearestSquared, (point - a - delta * parameter).sqrMagnitude);
            }
            float distance = boundaryStarts.Count > 0 ? Mathf.Sqrt(nearestSquared) : 0f;
            boundaryDistanceCache[world] = distance;
            return distance;
        }

        private static void AddCorner(List<Vector3> polygon, Vector3 origin, Vector3 axisX,
            Vector3 axisY, float x, float y, float rx, float ry, float angle, bool round)
        {
            if (!round || rx <= 0f || ry <= 0f)
            {
                polygon.Add(origin + axisX * x + axisY * y);
                return;
            }
            float cx = x == 0f ? rx : 1f - rx;
            float cy = y == 0f ? ry : 1f - ry;
            for (int step = 0; step <= ArcSteps; step++)
            {
                float radians = (angle + step * 90f / ArcSteps) * Mathf.Deg2Rad;
                polygon.Add(origin + axisX * (cx + Mathf.Cos(radians) * rx)
                    + axisY * (cy + Mathf.Sin(radians) * ry));
            }
        }

        private void BuildContours(int layer, int order)
        {
            var edges = new List<Edge>();
            var starts = new Dictionary<Vector3Int, List<int>>();
            foreach (Vector3Int cell in cells)
            {
                Vector3Int right = cell + Vector3Int.right;
                Vector3Int top = cell + Vector3Int.up;
                Vector3Int topRight = right + Vector3Int.up;
                if (!cells.Contains(cell + Vector3Int.down)) AddEdge(edges, starts, cell, right);
                if (!cells.Contains(right)) AddEdge(edges, starts, right, topRight);
                if (!cells.Contains(top)) AddEdge(edges, starts, topRight, top);
                if (!cells.Contains(cell + Vector3Int.left)) AddEdge(edges, starts, top, cell);
            }
            var used = new bool[edges.Count];
            var corners = new List<Vector3Int>();
            var points = new List<Vector3>();
            for (int start = 0; start < edges.Count; start++)
            {
                if (used[start]) continue;
                corners.Clear();
                int current = start;
                bool closed = false;
                for (int guard = 0; guard <= edges.Count; guard++)
                {
                    if (current < 0 || used[current]) break;
                    Edge edge = edges[current];
                    used[current] = true;
                    corners.Add(edge.from);
                    if (edge.to == edges[start].from) { closed = true; break; }
                    current = FindNextEdge(edge, edges, starts, used);
                }
                if (!closed || corners.Count < 3) continue;
                points.Clear();
                for (int i = 0; i < corners.Count; i++)
                {
                    Vector3Int before = corners[(i + corners.Count - 1) % corners.Count];
                    Vector3Int at = corners[i];
                    Vector3Int after = corners[(i + 1) % corners.Count];
                    Vector3Int incoming = at - before, outgoing = after - at;
                    Vector3 world = World(at);
                    if (incoming.x * outgoing.y - incoming.y * outgoing.x <= 0 || cornerRadius <= 0f)
                    {
                        points.Add(world);
                        continue;
                    }
                    Vector3 inWorld = world - World(before), outWorld = World(after) - world;
                    float radius = Mathf.Min(cornerRadius, inWorld.magnitude * 0.2f, outWorld.magnitude * 0.2f);
                    Vector3 entry = world - inWorld.normalized * radius;
                    Vector3 exit = world + outWorld.normalized * radius;
                    Vector3 center = entry + outWorld.normalized * radius;
                    for (int step = 0; step <= ArcSteps; step++)
                        points.Add(center + Vector3.Slerp(entry - center, exit - center, step / (float)ArcSteps));
                }
                var outline = NewChild("Outline", generated.transform).AddComponent<LineRenderer>();
                outline.useWorldSpace = true;
                outline.loop = true;
                outline.positionCount = points.Count;
                outline.SetPositions(points.ToArray());
                outline.widthMultiplier = outlineWidth;
                outline.numCornerVertices = 2;
                outline.numCapVertices = 2;
                outline.alignment = LineAlignment.View;
                outline.sharedMaterial = outlineMaterial;
                outline.startColor = outline.endColor = Color.black;
                outline.sortingLayerID = layer;
                outline.sortingOrder = order;
                outline.shadowCastingMode = ShadowCastingMode.Off;
                outline.receiveShadows = false;
            }
        }

        private static void AddEdge(List<Edge> edges, Dictionary<Vector3Int, List<int>> starts,
            Vector3Int from, Vector3Int to)
        {
            if (!starts.TryGetValue(from, out List<int> list))
            {
                list = new List<int>(2);
                starts.Add(from, list);
            }
            list.Add(edges.Count);
            edges.Add(new Edge(from, to));
        }

        private static int FindNextEdge(Edge edge, List<Edge> edges,
            Dictionary<Vector3Int, List<int>> starts, bool[] used)
        {
            if (!starts.TryGetValue(edge.to, out List<int> candidates)) return -1;
            Vector3Int incoming = edge.to - edge.from;
            int result = -1;
            float bestTurn = float.NegativeInfinity;
            foreach (int candidate in candidates)
            {
                if (used[candidate]) continue;
                Vector3Int outgoing = edges[candidate].to - edges[candidate].from;
                float turn = Mathf.Atan2(incoming.x * outgoing.y - incoming.y * outgoing.x,
                    incoming.x * outgoing.x + incoming.y * outgoing.y);
                // Prefer the left turn where two islands touch only at one vertex.
                if (turn > bestTurn) { bestTurn = turn; result = candidate; }
            }
            return result;
        }

        private void BuildGrass(int layer, int order)
        {
            var visited = new HashSet<Vector3Int>();
            foreach (Vector3Int cell in cells)
            {
                if (visited.Contains(cell) || cells.Contains(cell + Vector3Int.up)) continue;
                Vector3Int left = cell;
                while (cells.Contains(left + Vector3Int.left) && !cells.Contains(left + Vector3Int.left + Vector3Int.up))
                    left += Vector3Int.left;
                Vector3Int right = left;
                do { visited.Add(right); right += Vector3Int.right; }
                while (cells.Contains(right) && !cells.Contains(right + Vector3Int.up));
                Vector3 a = World(left + Vector3Int.up), b = World(right + Vector3Int.up);
                float width = Vector3.Distance(a, b);
                int count = Mathf.Max(1, Mathf.CeilToInt(width / Mathf.Max(0.1f, grassPatchWidth)));
                float patchWidth = width / count;
                Vector3 up = (World(left + Vector3Int.up) - World(left)).normalized;
                Vector3 along = (b - a).normalized;
                float baseScaleX = patchWidth / Mathf.Max(0.0001f, grassSprite.bounds.size.x);
                float baseScaleY = grassHeight / Mathf.Max(0.0001f, grassSprite.bounds.size.y);
                for (int i = 0; i < count; i++)
                {
                    var patch = NewChild("Top Grass", generated.transform);
                    float variation = ((left.x * 17 + left.y * 31 + i * 13) & 7) / 7f;
                    float scaleY = baseScaleY * Mathf.Lerp(0.85f, 1.1f, variation);
                    Vector3 bottomCenter = a + along * (patchWidth * (i + 0.5f));
                    // Grass sprites use an authored ground-line pivot. Transparent padding
                    // below that pivot must remain below the landing line.
                    patch.transform.position = bottomCenter;
                    patch.transform.rotation = Quaternion.LookRotation(Vector3.forward, up);
                    Vector3 parentScale = patch.transform.parent.lossyScale;
                    patch.transform.localScale = new Vector3(baseScaleX / Mathf.Max(0.0001f, Mathf.Abs(parentScale.x)),
                        scaleY / Mathf.Max(0.0001f, Mathf.Abs(parentScale.y)), 1f);
                    var renderer = patch.AddComponent<SpriteRenderer>();
                    renderer.sprite = grassSprite;
                    renderer.sortingLayerID = layer;
                    renderer.sortingOrder = order;
                    renderer.flipX = (i & 1) != 0;
                }
            }
        }

        private void ClearGenerated()
        {
            if (hiddenSourceRenderer != null)
                hiddenSourceRenderer.forceRenderingOff = previousForceRenderingOff;
            hiddenSourceRenderer = null;
            var meshesToDestroy = new HashSet<Mesh>(generatedMeshes);
            GameObject previousGenerated = generated;
            if (previousGenerated != null)
            {
                CollectOwnedMeshes(previousGenerated, meshesToDestroy);
                previousGenerated.SetActive(false);
                DestroyOwned(previousGenerated);
            }
            generated = null;
            // A domain reload can lose the field while an unsaved preview child remains.
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child.gameObject != previousGenerated && child.name == GeneratedName
                    && (child.gameObject.hideFlags & HideFlags.DontSaveInEditor) != 0)
                {
                    CollectOwnedMeshes(child.gameObject, meshesToDestroy);
                    DestroyOwned(child.gameObject);
                }
            }
            foreach (Mesh mesh in meshesToDestroy) if (mesh != null) DestroyOwned(mesh);
            generatedMeshes.Clear();
        }

        private static void CollectOwnedMeshes(GameObject owner, HashSet<Mesh> meshes)
        {
            foreach (MeshFilter filter in owner.GetComponentsInChildren<MeshFilter>(true))
                if (filter.sharedMesh != null
                    && (filter.sharedMesh.hideFlags & HideFlags.DontSaveInEditor) != 0)
                    meshes.Add(filter.sharedMesh);
        }

        private static void DestroyOwned(Object value)
        {
            if (Application.isPlaying) Destroy(value);
            else DestroyImmediate(value);
        }
    }
}
