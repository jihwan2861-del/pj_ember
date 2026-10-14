using UnityEngine;
using UnityEngine.InputSystem;

namespace EmberPrototype
{
    internal static class EmberPlayerInput
    {
        internal static Vector2 MovementDirection()
        {
            Keyboard keys = Keyboard.current;
            if (keys == null) return Vector2.zero;
            Vector2 direction = Vector2.zero;
            if (keys.leftArrowKey.isPressed) direction.x -= 1f;
            if (keys.rightArrowKey.isPressed) direction.x += 1f;
            if (keys.upArrowKey.isPressed) direction.y += 1f;
            if (keys.downArrowKey.isPressed) direction.y -= 1f;
            return direction.normalized;
        }

        internal static Vector2 AimDirection(Vector2 fallbackDirection)
        {
            Vector2 direction = MovementDirection();
            return direction.sqrMagnitude > 0.0001f ? direction : fallbackDirection.normalized;
        }

        internal static bool ActionPressed => Keyboard.current != null && Keyboard.current.xKey.wasPressedThisFrame;
        internal static bool ActionReleased => Keyboard.current != null && Keyboard.current.xKey.wasReleasedThisFrame;
        internal static bool IgnitionBurstPressed => Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame;
    }
}
