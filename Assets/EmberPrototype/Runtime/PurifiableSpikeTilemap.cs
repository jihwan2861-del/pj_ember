using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace EmberPrototype
{
    [RequireComponent(typeof(Tilemap), typeof(TilemapCollider2D))]
    [DisallowMultipleComponent]
    public sealed class PurifiableSpikeTilemap : MonoBehaviour
    {
        private Tilemap tiles;
        private readonly Dictionary<Vector3Int, TileBase> removedTiles = new Dictionary<Vector3Int, TileBase>();
        public bool TryPurifyAt(Vector2 hitPoint, Vector2 direction)
        {
            if (!isActiveAndEnabled) return false;
            if (tiles == null) tiles = GetComponent<Tilemap>();
            var cell = tiles.WorldToCell(hitPoint + direction.normalized * .02f);
            var tile = tiles.GetTile(cell);
            if (tile == null) return false;
            if (!removedTiles.ContainsKey(cell)) removedTiles.Add(cell, tile);
            tiles.SetTile(cell, null);
            GetComponent<TilemapCollider2D>().ProcessTilemapChanges();
            return true;
        }
        public void ResetForRespawn()
        {
            if (tiles == null) tiles = GetComponent<Tilemap>();
            foreach (var pair in removedTiles) tiles.SetTile(pair.Key, pair.Value);
            removedTiles.Clear();
            GetComponent<TilemapCollider2D>().ProcessTilemapChanges();
        }
    }
}
