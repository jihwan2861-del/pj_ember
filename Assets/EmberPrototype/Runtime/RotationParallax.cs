using UnityEngine;

namespace EmberPrototype
{
    [AddComponentMenu("Ember/Rotation Parallax")]
    public sealed class RotationParallax : MonoBehaviour
    {
        [SerializeField] private Transform source;
        [SerializeField] private float multiplier = 0.4f;
        private Quaternion initialRotation;
        private float previousAngle;
        private float accumulatedAngle;

        private void OnEnable()
        {
            initialRotation = transform.localRotation;
            previousAngle = source != null ? source.localEulerAngles.z : 0f;
            accumulatedAngle = 0f;
        }

        private void LateUpdate()
        {
            if (source == null) return;
            float angle = source.localEulerAngles.z;
            accumulatedAngle += Mathf.DeltaAngle(previousAngle, angle);
            previousAngle = angle;
            transform.localRotation = initialRotation * Quaternion.Euler(0f, 0f, accumulatedAngle * multiplier);
        }
    }
}
