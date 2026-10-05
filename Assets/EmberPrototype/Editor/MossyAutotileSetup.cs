using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace EmberPrototype.Editor
{
    /// <summary>One-time setup for the Mossy atlas; existing generated assets are left editable.</summary>
    public static class MossyAutotileSetup
    {
        private const string SourcePath = "Assets/tile/Mossy Tileset/Mossy - TileSet.png";
        private const string OutputFolder = "Assets/tile/Mossy Tileset/Autotile";
        private const string ManualFolder = OutputFolder + "/Manual Tiles";
        private const string RulePath = OutputFolder + "/Mossy Terrain RuleTile.asset";
        private const string PaletteName = "Mossy Terrain Palette";
        private const string PalettePath = OutputFolder + "/" + PaletteName + ".prefab";
        private const int TilePixels = 512;
        private const int Side = 7;

        // Rows run top to bottom in the source PNG. Bits: N E S W NE SE SW NW.
        // 255 at R2C2 is the solid interior; R7C1/C2 remain manual-only variants.
        private static readonly int[,] Masks =
        {
            { 38, 110, 76, 4, 223, 191, 111 },
            { 55, 255, 205, 5, 239, 127, 63 },
            { 19, 155, 137, 1, 79, 31, 207 },
            { 2, 10, 8, 0, 143, 47, 159 },
            { 23, 141, 78, 46, 6, 14, 12 },
            { 39, 77, 139, 27, 7, 15, 13 },
            { 255, 255, 175, 95, 3, 11, 9 }
        };

        private static readonly Vector3Int[] NeighborPositions =
        {
            Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left,
            new Vector3Int(1, 1, 0), new Vector3Int(1, -1, 0),
            new Vector3Int(-1, -1, 0), new Vector3Int(-1, 1, 0)
        };

        [InitializeOnLoadMethod]
        private static void QueueInitialSetup()
        {
            EditorApplication.delayCall += TryInitialSetup;
        }

        private static void TryInitialSetup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.update -= TryInitialSetup;
                EditorApplication.update += TryInitialSetup;
                return;
            }

            EditorApplication.update -= TryInitialSetup;
            if (AssetDatabase.LoadAssetAtPath<RuleTile>(RulePath) != null ||
                AssetImporter.GetAtPath(SourcePath) == null)
                return;

            try { SetupMissingAssets(); }
            catch (Exception exception) { Debug.LogException(exception); }
        }

        [MenuItem("Tools/Ember/Tiles/Setup Mossy Autotile")]
        public static void SetupMissingAssets()
        {
            if (!CanSetup())
                return;

            EnsureFolder(OutputFolder);
            EnsureFolder(ManualFolder);
            SliceAtlasIfNeeded();

            Dictionary<string, Sprite> sprites = AssetDatabase.LoadAllAssetsAtPath(SourcePath)
                .OfType<Sprite>().ToDictionary(sprite => sprite.name);
            if (sprites.Count != Side * Side)
                throw new InvalidOperationException("Mossy atlas must contain 49 grid sprites.");

            RuleTile ruleTile = AssetDatabase.LoadAssetAtPath<RuleTile>(RulePath);
            if (ruleTile == null)
            {
                ruleTile = ScriptableObject.CreateInstance<RuleTile>();
                ruleTile.name = "Mossy Terrain RuleTile";
                ruleTile.m_DefaultSprite = sprites[SpriteName(1, 1)];
                ruleTile.m_DefaultColliderType = Tile.ColliderType.Grid;
                var addedMasks = new HashSet<int>();
                for (int row = 0; row < Side; row++)
                {
                    for (int column = 0; column < Side; column++)
                    {
                        int mask = Masks[row, column];
                        if (!addedMasks.Add(mask))
                            continue;
                        ruleTile.m_TilingRules.Add(CreateRule(mask, sprites[SpriteName(row, column)]));
                    }
                }

                ruleTile.UpdateNeighborPositions();
                AssetDatabase.CreateAsset(ruleTile, RulePath);
            }

            var manualTiles = new Tile[Side, Side];
            for (int row = 0; row < Side; row++)
            {
                for (int column = 0; column < Side; column++)
                {
                    string name = SpriteName(row, column);
                    string path = ManualFolder + "/" + name + ".asset";
                    Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
                    if (tile == null)
                    {
                        tile = ScriptableObject.CreateInstance<Tile>();
                        tile.name = name;
                        tile.sprite = sprites[name];
                        tile.colliderType = Tile.ColliderType.Grid;
                        AssetDatabase.CreateAsset(tile, path);
                    }
                    manualTiles[row, column] = tile;
                }
            }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(PalettePath) == null)
                CreatePalette(ruleTile, manualTiles);

            AssetDatabase.SaveAssets();
            ValidateGeneratedAssets();
            Debug.Log("Mossy autotile ready: 49 sprites, 47 neighbor rules, " +
                      "49 manual tiles, and Mossy Terrain Palette. Existing scenes were not changed.");
        }

        [MenuItem("Tools/Ember/Tiles/Setup Mossy Autotile", true)]
        private static bool CanSetup()
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode &&
                   !EditorApplication.isCompiling && !EditorApplication.isUpdating &&
                   AssetImporter.GetAtPath(SourcePath) is TextureImporter;
        }

        private static void SliceAtlasIfNeeded()
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(SourcePath);
            importer.GetSourceTextureWidthAndHeight(out int width, out int height);
            if (width != Side * TilePixels || height != Side * TilePixels)
                throw new InvalidOperationException("Expected a 3584 x 3584 Mossy atlas.");

            var factories = new SpriteDataProviderFactories();
            factories.Init();
            ISpriteEditorDataProvider provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            Dictionary<string, SpriteRect> existing = provider.GetSpriteRects()
                .ToDictionary(rect => rect.name);
            var rects = new List<SpriteRect>();
            for (int row = 0; row < Side; row++)
            {
                for (int column = 0; column < Side; column++)
                {
                    string name = SpriteName(row, column);
                    existing.TryGetValue(name, out SpriteRect previous);
                    rects.Add(new SpriteRect
                    {
                        name = name,
                        rect = new Rect(column * TilePixels, (Side - 1 - row) * TilePixels,
                                        TilePixels, TilePixels),
                        pivot = new Vector2(0.5f, 0.5f),
                        alignment = SpriteAlignment.Center,
                        spriteID = previous != null ? previous.spriteID : GUID.Generate()
                    });
                }
            }

            if (existing.Count == rects.Count && rects.All(rect =>
                existing.TryGetValue(rect.name, out SpriteRect old) && old.rect == rect.rect))
                return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = TilePixels;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.maxTextureSize = 4096;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);

            provider.SetSpriteRects(rects.ToArray());
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>()
                .SetNameFileIdPairs(rects.Select(rect => new SpriteNameFileIdPair(rect.name, rect.spriteID)));
            provider.Apply();
            importer.SaveAndReimport();
        }

        private static RuleTile.TilingRule CreateRule(int mask, Sprite sprite)
        {
            var rule = new RuleTile.TilingRule
            {
                m_Id = mask,
                m_Sprites = new[] { sprite },
                m_Output = RuleTile.TilingRuleOutput.OutputSprite.Single,
                m_ColliderType = Tile.ColliderType.Grid,
                m_RuleTransform = RuleTile.TilingRuleOutput.Transform.Fixed,
                m_NeighborPositions = new List<Vector3Int>(NeighborPositions),
                m_Neighbors = new List<int>()
            };

            for (int index = 0; index < NeighborPositions.Length; index++)
            {
                // A diagonal matters only when both adjoining cardinal cells are filled.
                int adjacent = index < 4 ? 0 : new[] { 3, 6, 12, 9 }[index - 4];
                bool ignored = index >= 4 && (mask & adjacent) != adjacent;
                rule.m_Neighbors.Add(ignored ? 0 : ((mask & (1 << index)) != 0
                    ? RuleTile.TilingRuleOutput.Neighbor.This
                    : RuleTile.TilingRuleOutput.Neighbor.NotThis));
            }
            return rule;
        }

        private static void CreatePalette(RuleTile ruleTile, Tile[,] manualTiles)
        {
            GridPaletteUtility.CreateNewPalette(OutputFolder, PaletteName,
                GridLayout.CellLayout.Rectangle, GridPalette.CellSizing.Manual,
                new Vector3(1f, 1f, 0f), GridLayout.CellSwizzle.XYZ);
            GameObject root = PrefabUtility.LoadPrefabContents(PalettePath);
            try
            {
                Tilemap tilemap = root.GetComponentInChildren<Tilemap>();
                tilemap.gameObject.name = "Top = Auto | Bottom = Manual";
                // The connected sample at the top is the brush for normal terrain painting.
                for (int y = 10; y < 13; y++)
                    for (int x = 0; x < 3; x++)
                        tilemap.SetTile(new Vector3Int(x, y, 0), ruleTile);
                tilemap.SetTile(new Vector3Int(5, 11, 0), ruleTile);
                for (int row = 0; row < Side; row++)
                    for (int column = 0; column < Side; column++)
                        tilemap.SetTile(new Vector3Int(column, Side - 1 - row, 0), manualTiles[row, column]);
                tilemap.CompressBounds();
                PrefabUtility.SaveAsPrefabAsset(root, PalettePath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        [MenuItem("Tools/Ember/Tiles/Validate Mossy Autotile")]
        public static void ValidateGeneratedAssets()
        {
            RuleTile tile = AssetDatabase.LoadAssetAtPath<RuleTile>(RulePath);
            if (tile == null || tile.m_TilingRules.Count != 47 ||
                tile.m_TilingRules.Any(rule => rule.m_Sprites.Length != 1 || rule.m_Sprites[0] == null))
                throw new InvalidOperationException("Mossy RuleTile has missing sprites or rules.");

            // Exhaust all 256 surrounding-cell patterns, including ignored diagonals.
            for (int mask = 0; mask < 256; mask++)
            {
                int matches = tile.m_TilingRules.Count(rule => rule.m_Neighbors
                    .Select((neighbor, index) => neighbor == 0 ||
                        neighbor == (((mask & (1 << index)) != 0) ? 1 : 2)).All(match => match));
                if (matches != 1)
                    throw new InvalidOperationException("Mossy neighbor mask " + mask +
                                                        " matched " + matches + " rules, expected one.");
            }
            Debug.Log("Mossy autotile validation passed: all 256 neighbor patterns have exactly one rule.");
        }

        private static string SpriteName(int row, int column)
        {
            return "Mossy_Terrain_R" + (row + 1) + "_C" + (column + 1);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            int separator = path.LastIndexOf('/');
            string parent = path.Substring(0, separator);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(separator + 1));
        }
    }
}
