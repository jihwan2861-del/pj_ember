using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class GroundTileSetup
{
    const string Source = "Assets/tile/GroundTile.png";
    const string Folder = "Assets/tile/Ground Autotile";
    public static string Analyze()
    {
        var image = new Texture2D(2, 2);
        image.LoadImage(System.IO.File.ReadAllBytes(Source));
        var report = new StringBuilder();
        foreach (var sprite in AssetDatabase.LoadAllAssetsAtPath(Source).OfType<Sprite>().OrderBy(s => -s.rect.y).ThenBy(s => s.rect.x))
        {
            int x = (int)sprite.rect.x, y = (int)sprite.rect.y;
            var counts = new Dictionary<Color32, int>();
            for (int cy = 5; cy < 10; cy++) for (int cx = 5; cx < 10; cx++)
            { Color32 c = image.GetPixel(x + cx, y + cy); counts[c] = counts.ContainsKey(c) ? counts[c] + 1 : 1; }
            var center = counts.OrderByDescending(p => p.Value).First().Key;
            var symbols = new List<string>();
            for (int row = 0; row < 3; row++) for (int col = 0; col < 3; col++)
            {
                if (row == 1 && col == 1) { symbols.Add("*"); continue; }
                int matches = 0;
                for (int cy = 0; cy < 5; cy++) for (int cx = 0; cx < 5; cx++)
                { Color32 c = image.GetPixel(x + col * 5 + cx, y + (2-row)*5 + cy); if (Math.Abs(c.r-center.r)<=10 && Math.Abs(c.g-center.g)<=10 && Math.Abs(c.b-center.b)<=10 && Math.Abs(c.a-center.a)<=10) matches++; }
                symbols.Add(matches >= 19 ? "." : "X");
            }
            report.AppendLine(sprite.name + " R" + (12-y/16) + "C" + x/16 + ": " + string.Join(" / ", Enumerable.Range(0,3).Select(r => string.Join(" ", symbols.Skip(r*3).Take(3)))));
        }
        UnityEngine.Object.DestroyImmediate(image);
        return report.ToString();
    }
    public static string Main()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new Exception("Exit Play Mode first.");
        var importer = (TextureImporter)AssetImporter.GetAtPath(Source);
        importer.spritePixelsToUnits = 16;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/tile", "Ground Autotile");
        if (!AssetDatabase.IsValidFolder(Folder + "/Manual")) AssetDatabase.CreateFolder(Folder, "Manual");
        var sprites = AssetDatabase.LoadAllAssetsAtPath(Source).OfType<Sprite>().ToArray();
        Func<int,int,Sprite> at = (row,col) => sprites.First(s => (int)s.rect.x == col*16 && (int)s.rect.y == (12-row)*16);
        var soil = Make("Ground Terrain RuleTile", at, new int[,] {{4,0,4,1,4,5},{5,0,7,5,5,5},{6,0,7,2,6,5}});
        var corners = new [] {new Vector3Int(1,1,0),new Vector3Int(-1,1,0),new Vector3Int(1,-1,0),new Vector3Int(-1,-1,0)};
        var cornerRows = new [] {5,5,6,6}; var cornerCols = new [] {1,4,1,4};
        for(int i=3;i>=0;i--)
        {
            var inner = new RuleTile.TilingRule {m_Sprites=new [] {at(cornerRows[i],cornerCols[i])},m_ColliderType=Tile.ColliderType.Grid,m_Output=RuleTile.TilingRuleOutput.OutputSprite.Single,m_RuleTransform=RuleTile.TilingRuleOutput.Transform.Fixed,
                m_NeighborPositions=new List<Vector3Int>{Vector3Int.up,Vector3Int.right,Vector3Int.down,Vector3Int.left,corners[i]},m_Neighbors=new List<int>{1,1,1,1,2}};
            soil.m_TilingRules.Insert(0,inner);
        }
        soil.UpdateNeighborPositions(); EditorUtility.SetDirty(soil);
        var rock = Make("Ground Rock RuleTile", at, new int[,] {{9,0,9,1,9,2},{10,0,10,1,10,2},{11,0,11,1,11,2}});
        string palettePath = Folder + "/Ground Terrain Palette.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(palettePath) == null)
            GridPaletteUtility.CreateNewPalette(Folder, "Ground Terrain Palette", GridLayout.CellLayout.Rectangle, GridPalette.CellSizing.Manual, new Vector3(1,1,0), GridLayout.CellSwizzle.XYZ);
        var root = PrefabUtility.LoadPrefabContents(palettePath);
        try
        {
            var map = root.GetComponentInChildren<Tilemap>();
            map.ClearAllTiles();
            map.gameObject.name = "Top Auto Brushes - Bottom Manual Sprites";
            for(int y=15;y<18;y++) for(int x=0;x<3;x++) map.SetTile(new Vector3Int(x,y,0),soil);
            for(int y=15;y<18;y++) for(int x=5;x<8;x++) map.SetTile(new Vector3Int(x,y,0),rock);
            map.SetTile(new Vector3Int(3,16,0),soil);
            map.SetTile(new Vector3Int(8,16,0),rock);
            foreach(var sprite in sprites)
            {
                string path=Folder+"/Manual/"+sprite.name+".asset";
                var tile=AssetDatabase.LoadAssetAtPath<Tile>(path);
                if(tile==null) { tile=ScriptableObject.CreateInstance<Tile>(); AssetDatabase.CreateAsset(tile,path); }
                tile.sprite=sprite; tile.colliderType=Tile.ColliderType.Grid; EditorUtility.SetDirty(tile);
                map.SetTile(new Vector3Int((int)sprite.rect.x/16,(int)sprite.rect.y/16,0),tile);
            }
            map.CompressBounds(); PrefabUtility.SaveAsPrefabAsset(root,palettePath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
        Selection.activeObject=soil;
        EditorGUIUtility.PingObject(soil);
        return "Created two terrain RuleTiles, 65 manual tiles, Ground Terrain Palette. PPU=16, Point, uncompressed. Cardinal rules cover every neighbor arrangement.";
    }
    static RuleTile Make(string name, Func<int,int,Sprite> at, int[,] layout)
    {
        string path=Folder+"/"+name+".asset";
        var tile=AssetDatabase.LoadAssetAtPath<RuleTile>(path);
        bool fresh=tile==null;
        if(fresh) tile=ScriptableObject.CreateInstance<RuleTile>();
        tile.name=name; tile.m_DefaultSprite=at(layout[1,2],layout[1,3]);
        tile.m_DefaultColliderType=Tile.ColliderType.Grid; tile.m_TilingRules.Clear();
        var positions=new [] {Vector3Int.up,Vector3Int.right,Vector3Int.down,Vector3Int.left};
        for(int mask=0;mask<16;mask++)
        {
            bool n=(mask&1)!=0,e=(mask&2)!=0,s=(mask&4)!=0,w=(mask&8)!=0;
            int row=!n?0:!s?2:1, col=!w?0:!e?2:1;
            var rule=new RuleTile.TilingRule {m_Sprites=new []{at(layout[row,col*2],layout[row,col*2+1])},m_ColliderType=Tile.ColliderType.Grid,m_Output=RuleTile.TilingRuleOutput.OutputSprite.Single,m_RuleTransform=RuleTile.TilingRuleOutput.Transform.Fixed,m_NeighborPositions=positions.ToList(),m_Neighbors=Enumerable.Range(0,4).Select(i=>(mask&(1<<i))!=0?1:2).ToList()};
            tile.m_TilingRules.Add(rule);
        }
        tile.UpdateNeighborPositions();
        if(fresh) AssetDatabase.CreateAsset(tile,path); else EditorUtility.SetDirty(tile);
        return tile;
    }
}
