using UnityEngine;
using UnityEngine.SceneManagement;

namespace EmberPrototype
{
    // Reset temporary mechanics, not checkpoints, unlocked abilities or story progress.
    public static class MapReset
    {
        public static void ResetScene(Scene scene, bool deferWaves = false)
        {
            foreach (var shot in Object.FindObjectsByType<FireFlowerProjectile>(FindObjectsSortMode.None))
            {
                if (shot.gameObject.scene != scene) continue;
                shot.gameObject.SetActive(false);
                Object.Destroy(shot.gameObject);
            }
            foreach (var flower in Object.FindObjectsByType<FireFlower>(FindObjectsSortMode.None))
                if (flower.gameObject.scene == scene) flower.ResetForRespawn();
            foreach (var pollution in Object.FindObjectsByType<PollutionObstacle>(FindObjectsSortMode.None))
                if (pollution.gameObject.scene == scene) pollution.ResetForRespawn();
            foreach (var pollutionTiles in Object.FindObjectsByType<PurifiableSpikeTilemap>(FindObjectsSortMode.None))
                if (pollutionTiles.gameObject.scene == scene) pollutionTiles.ResetForRespawn();
            foreach (var berry in Object.FindObjectsByType<StrawberryPickup>(FindObjectsSortMode.None))
                if (berry.gameObject.scene == scene) berry.ResetForRespawn();
            foreach (var spirit in Object.FindObjectsByType<WaterSpirit>(FindObjectsSortMode.None))
                if (spirit.gameObject.scene == scene) spirit.ResetForRespawn(deferWaves);
            foreach (var ember in Object.FindObjectsByType<EmberProjectile>(FindObjectsSortMode.None))
            {
                if (ember.gameObject.scene != scene) continue;
                ember.gameObject.SetActive(false);
                Object.Destroy(ember.gameObject);
            }
            // Reset receivers before torches reapply their initial burning state/events.
            foreach (var block in Object.FindObjectsByType<PathMovingBlock>(FindObjectsSortMode.None))
                if (block.gameObject.scene == scene) block.ResetForRespawn();
            foreach (var platform in Object.FindObjectsByType<WeightPlatform>(FindObjectsSortMode.None))
                if (platform.gameObject.scene == scene) platform.ResetForRespawn();
            foreach (var rope in Object.FindObjectsByType<BurningRope>(FindObjectsSortMode.None))
                if (rope.gameObject.scene == scene) rope.ResetMotion();
            foreach (var wave in Object.FindObjectsByType<WaterWave>(FindObjectsSortMode.None))
                if (wave.gameObject.scene == scene) wave.ResetWave(deferWaves);
            foreach (var tile in Object.FindObjectsByType<FlammableTile>(FindObjectsSortMode.None))
                if (tile.gameObject.scene == scene) tile.ResetFire();
            Physics2D.SyncTransforms();
            WaterContact.SyncScene(scene);
        }

        public static void ResumeWaves(Scene scene)
        {
            foreach (var spirit in Object.FindObjectsByType<WaterSpirit>(FindObjectsSortMode.None))
                if (spirit.gameObject.scene == scene) spirit.ResumeAfterRespawn();
            foreach (var wave in Object.FindObjectsByType<WaterWave>())
                if (wave.gameObject.scene == scene) wave.ResumeAfterRespawn();
        }
    }
}
