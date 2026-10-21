using UnityEngine;
using UnityEngine.Tilemaps;

namespace EmberPrototype
{
    /// <summary>Copies solid cells into a dedicated visual Tilemap without changing terrain or collision.</summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("Ember/Tilemap Visual Skin")]
    public sealed class TilemapVisualSkin : MonoBehaviour
    {
        [SerializeField] private Tilemap terrain;
        [SerializeField] private Tilemap visual;
        [SerializeField] private TileBase fillTile;
        [Tooltip("선택: 큰 그림을 나눈 타일을 왼쪽 아래부터 행 순서로 연결합니다.")]
        [SerializeField] private TileBase[] patternTiles = new TileBase[0];
        [SerializeField] private Vector2Int patternSize = new Vector2Int(4, 4);
        private bool needsRebuild;

        private void OnEnable()
        {
            Tilemap.tilemapTileChanged += OnTilesChanged;
            needsRebuild = true;
        }

        private void OnDisable() => Tilemap.tilemapTileChanged -= OnTilesChanged;
        private void OnValidate() => needsRebuild = true;

        private void OnTilesChanged(Tilemap changed, Tilemap.SyncTile[] changes)
        {
            if (changed == terrain) needsRebuild = true;
        }

        private void LateUpdate()
        {
            if (!needsRebuild) return;
            needsRebuild = false;
            RebuildSkin();
        }

        /// <summary>The destination is owned by this component; place hand-painted decoration on another Tilemap.</summary>
        public void RebuildSkin()
        {
            if (terrain == null || visual == null || terrain == visual || fillTile == null) return;
            // Cell coordinates can only be copied when both maps share exactly the same grid transform.
            if (terrain.layoutGrid != visual.layoutGrid
                || terrain.transform.localToWorldMatrix != visual.transform.localToWorldMatrix)
            {
                Debug.LogWarning("Terrain과 Visual Tilemap의 Grid와 Transform을 맞춰주세요.", this);
                return;
            }
            visual.ClearAllTiles();
            BoundsInt bounds = terrain.cellBounds;
            bool usePattern = patternSize.x > 0 && patternSize.y > 0 && patternTiles != null
                && patternTiles.Length == patternSize.x * patternSize.y;
            foreach (Vector3Int cell in bounds.allPositionsWithin)
                if (terrain.HasTile(cell))
                {
                    TileBase tile = fillTile;
                    if (usePattern)
                    {
                        int x = ((cell.x % patternSize.x) + patternSize.x) % patternSize.x;
                        int y = ((cell.y % patternSize.y) + patternSize.y) % patternSize.y;
                        tile = patternTiles[y * patternSize.x + x] ?? fillTile;
                    }
                    visual.SetTile(cell, tile);
                }
            visual.CompressBounds();
        }
    }
}
