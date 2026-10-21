using System;
using System.Collections.Generic;
using UnityEngine;

namespace EmberPrototype
{
    [AddComponentMenu("Ember Prototype/Parallax Layer")]
    [DisallowMultipleComponent]
    // The room camera moves in LateUpdate; sample its final position afterward.
    [DefaultExecutionOrder(1000)]
    public sealed class ParallaxLayer : MonoBehaviour
    {
        [Header("Camera")]
        [Tooltip("Assign Main Camera. When empty, Camera.main is resolved once on enable.")]
        [SerializeField] private Transform cameraTransform;

        [Header("Follow Ratio")]
        [Tooltip("1 follows the camera exactly; 0 stays fixed in world space.")]
        [SerializeField, Range(0f, 1f)] private float followX = 0.8f;
        [SerializeField, Range(0f, 1f)] private float followY = 0.8f;

        [Header("Horizontal Repeat")]
        [Tooltip("Repeat the SpriteRenderer on this object horizontally. Copies contain only a renderer, not scripts or colliders.")]
        [SerializeField] private bool repeatX;

        public bool RepeatsHorizontally => repeatX;

        private const int MaxCopiesPerSide = 32;
        private Vector3 layerOrigin;
        private Vector3 cameraOrigin;
        private Camera viewCamera;
        private SpriteRenderer sourceRenderer;
        private MaterialPropertyBlock properties;
        private readonly List<SpriteRenderer> copies = new List<SpriteRenderer>();
        private bool warnedAboutRepeat;

        private void OnEnable()
        {
            if (cameraTransform == null)
            {
                Camera mainCamera = Camera.main;
                if (mainCamera != null) cameraTransform = mainCamera.transform;
            }

            if (cameraTransform == null)
            {
                Debug.LogWarning("ParallaxLayer: assign a camera before enabling this component.", this);
                enabled = false;
                return;
            }

            if (transform == cameraTransform || transform.IsChildOf(cameraTransform))
            {
                Debug.LogWarning("ParallaxLayer must not be attached to the camera or its children.", this);
                enabled = false;
                return;
            }

            // Capture authored placement before the camera's startup snap.
            // Re-enabling resumes from the current placement without a jump.
            layerOrigin = transform.position;
            cameraOrigin = cameraTransform.position;
            viewCamera = cameraTransform.GetComponent<Camera>();
            sourceRenderer = GetComponent<SpriteRenderer>();
            warnedAboutRepeat = false;
        }

        private void LateUpdate()
        {
            if (cameraTransform == null) return;

            Vector3 displacement = cameraTransform.position - cameraOrigin;
            // Absolute displacement avoids cumulative drift and handles respawn snaps.
            Vector3 position = layerOrigin + new Vector3(
                displacement.x * followX, displacement.y * followY, 0f);

            if (!repeatX)
            {
                ClearCopies();
                transform.position = position;
                return;
            }

            if (!TryGetRepeatGeometry(out float localWidth, out float worldWidth, out float centerOffset))
            {
                ClearCopies();
                transform.position = position;
                return;
            }

            float halfViewWidth = viewCamera.orthographicSize * viewCamera.aspect;
            double requiredRadius = Math.Max(1d, Math.Ceiling(halfViewWidth / worldWidth));
            if (double.IsNaN(requiredRadius) || requiredRadius > MaxCopiesPerSide)
            {
                WarnRepeat("The sprite is too small to cover this camera. Increase its scale or lower the zoom-out.");
                ClearCopies();
                transform.position = position;
                return;
            }

            warnedAboutRepeat = false;
            // Keep the original tile nearest the view. Recompute from the authored
            // origin, not last frame's wrapped position, so teleports cannot drift.
            double tileIndex = Math.Floor(((double)cameraTransform.position.x - position.x - centerOffset) / worldWidth + 0.5d);
            position.x += (float)(tileIndex * worldWidth);
            transform.position = position;

            UpdateCopies((int)requiredRadius, localWidth);
        }

        private bool TryGetRepeatGeometry(out float localWidth, out float worldWidth, out float centerOffset)
        {
            localWidth = worldWidth = centerOffset = 0f;
            if (sourceRenderer == null || sourceRenderer.sprite == null)
            {
                WarnRepeat("Repeat X needs a SpriteRenderer with a sprite on the same object.");
                return false;
            }

            if (viewCamera == null || !viewCamera.orthographic)
            {
                WarnRepeat("Repeat X needs Camera Transform to reference an orthographic Camera.");
                return false;
            }

            if (sourceRenderer.drawMode != SpriteDrawMode.Simple
                || Quaternion.Angle(transform.rotation, Quaternion.identity) > 0.01f)
            {
                WarnRepeat("Repeat X supports unrotated sprites with Draw Mode set to Simple.");
                return false;
            }

            Sprite sprite = sourceRenderer.sprite;
            // Use the full sprite rect rather than rendered bounds so spacing
            // includes the transparent borders authored for the wrap seam.
            localWidth = sprite.rect.width / sprite.pixelsPerUnit;
            float scaleX = transform.lossyScale.x;
            worldWidth = localWidth * Mathf.Abs(scaleX);
            float localCenter = localWidth * 0.5f - sprite.pivot.x / sprite.pixelsPerUnit;
            centerOffset = localCenter * scaleX * (sourceRenderer.flipX ? -1f : 1f);
            if (worldWidth < 0.0001f || float.IsNaN(worldWidth) || float.IsInfinity(worldWidth))
            {
                WarnRepeat("Repeat X needs a non-zero sprite width and Transform scale.");
                return false;
            }

            return true;
        }

        private void UpdateCopies(int radius, float localWidth)
        {
            int requiredCopies = radius * 2;
            while (copies.Count < requiredCopies)
            {
                // Never clone the original GameObject: it may contain gameplay
                // components or colliders that must not be repeated.
                GameObject copyObject = new GameObject(name + " (Parallax Copy " + (copies.Count + 1) + ")");
                copyObject.hideFlags = HideFlags.DontSave;
                copyObject.transform.SetParent(transform, false);
                copies.Add(copyObject.AddComponent<SpriteRenderer>());
            }

            if (properties == null) properties = new MaterialPropertyBlock();
            sourceRenderer.GetPropertyBlock(properties);
            for (int i = 0; i < copies.Count; i++)
            {
                SpriteRenderer copy = copies[i];
                if (copy == null) continue;
                bool needed = i < requiredCopies;
                if (copy.gameObject.activeSelf != needed) copy.gameObject.SetActive(needed);
                if (!needed) continue;

                int tileOffset = (i / 2 + 1) * (i % 2 == 0 ? -1 : 1);
                copy.transform.localPosition = new Vector3(tileOffset * localWidth, 0f, 0f);
                copy.gameObject.layer = gameObject.layer;
                copy.sprite = sourceRenderer.sprite;
                copy.sharedMaterial = sourceRenderer.sharedMaterial;
                copy.color = sourceRenderer.color;
                copy.flipX = sourceRenderer.flipX;
                copy.flipY = sourceRenderer.flipY;
                copy.sortingLayerID = sourceRenderer.sortingLayerID;
                copy.sortingOrder = sourceRenderer.sortingOrder;
                copy.maskInteraction = sourceRenderer.maskInteraction;
                copy.spriteSortPoint = sourceRenderer.spriteSortPoint;
                copy.enabled = sourceRenderer.enabled;
                copy.SetPropertyBlock(properties);
            }
        }

        private void WarnRepeat(string reason)
        {
            if (warnedAboutRepeat) return;
            warnedAboutRepeat = true;
            Debug.LogWarning("ParallaxLayer: " + reason, this);
        }

        private void OnDisable()
        {
            ClearCopies();
        }

        private void ClearCopies()
        {
            for (int i = 0; i < copies.Count; i++)
            {
                if (copies[i] == null) continue;
                copies[i].enabled = false;
                if (Application.isPlaying) Destroy(copies[i].gameObject);
                else DestroyImmediate(copies[i].gameObject);
            }
            copies.Clear();
        }
    }
}
