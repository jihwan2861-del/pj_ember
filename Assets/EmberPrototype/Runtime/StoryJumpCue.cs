using UnityEngine;

namespace EmberPrototype
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Ember/Story Jump Cue")]
    public sealed class StoryJumpCue : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private Canvas cueCanvas;
        [SerializeField] private Vector2 offset = new Vector2(0f, 2f);
        [SerializeField] private float startX = 52f;
        [SerializeField] private float endX = 76.5f;

        private void LateUpdate()
        {
            if (player == null || cueCanvas == null) return;
            cueCanvas.enabled = player.position.x >= startX && player.position.x < endX;
            transform.position = player.position + (Vector3)offset;
        }
    }
}
