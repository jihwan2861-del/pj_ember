using UnityEngine;
using UnityEngine.Tilemaps;

namespace EmberPrototype
{
    [AddComponentMenu("Ember/Shared Level Tilemaps")]
    [DisallowMultipleComponent]
    public sealed class LevelTilemaps : MonoBehaviour
    {
        [SerializeField] private Grid grid;
        [SerializeField] private Tilemap terrain;
        [SerializeField] private Tilemap spikes;
        [SerializeField] private Tilemap decorations;

        public Grid Grid => grid;
        public Tilemap Terrain => terrain;
        public Tilemap Spikes => spikes;
        public Tilemap Decorations => decorations;
        public DoodleTerrainSkin Skin => GetComponent<DoodleTerrainSkin>();
        public Tilemap[] Maps => new[] { terrain, spikes, decorations };

        public void Configure(Grid sharedGrid, Tilemap solidTerrain, Tilemap hazardSpikes, Tilemap decorativeTiles)
        {
            grid = sharedGrid;
            terrain = solidTerrain;
            spikes = hazardSpikes;
            decorations = decorativeTiles;
        }
    }
}
