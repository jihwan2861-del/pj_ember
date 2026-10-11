using System;
using System.Collections.Generic;
using UnityEngine;

namespace EmberPrototype
{
    [AddComponentMenu("Ember Prototype/Water Contact")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class WaterContact : MonoBehaviour
    {
        private static readonly HashSet<WaterContact> sources = new HashSet<WaterContact>();
        private readonly HashSet<FlammableTile> contacts = new HashSet<FlammableTile>();
        private readonly HashSet<FlammableTile> nextContacts = new HashSet<FlammableTile>();
        private Collider2D[] shapes;
        private Collider2D[] overlaps = new Collider2D[16];
        private RaycastHit2D[] sweepHits = new RaycastHit2D[16];
        private bool contactActive = true;
        private static ContactFilter2D Filter => new ContactFilter2D { useTriggers = true };

        private void OnEnable() { shapes = GetComponents<Collider2D>(); sources.Add(this); }
        private void OnDisable() { sources.Remove(this); ClearContacts(); }
        private void FixedUpdate() => SyncContacts();

        public void SetContactActive(bool value)
        {
            contactActive = value;
            if (!value) ClearContacts();
            else if (isActiveAndEnabled) { Physics2D.SyncTransforms(); SyncContacts(); }
        }

        private void ClearContacts()
        {
            foreach (FlammableTile tile in contacts)
                if (tile != null) tile.SetWaterContact(this, false);
            contacts.Clear(); nextContacts.Clear();
        }

        public void SyncContacts()
        {
            if (!isActiveAndEnabled || !contactActive) { ClearContacts(); return; }
            if (shapes == null) shapes = GetComponents<Collider2D>();
            nextContacts.Clear();
            foreach (Collider2D shape in shapes)
            {
                if (shape == null || !shape.enabled) continue;
                int count;
                while ((count = shape.Overlap(Filter, overlaps)) == overlaps.Length)
                    Array.Resize(ref overlaps, overlaps.Length * 2);
                for (int i = 0; i < count; i++)
                {
                    FlammableTile tile = overlaps[i].GetComponentInParent<FlammableTile>();
                    if (tile != null && tile.isActiveAndEnabled) nextContacts.Add(tile);
                }
            }
            foreach (FlammableTile tile in contacts)
                if (tile != null && !nextContacts.Contains(tile)) tile.SetWaterContact(this, false);
            contacts.Clear();
            foreach (FlammableTile tile in nextContacts) contacts.Add(tile);
            // Events may deactivate this water, so don't enumerate a mutable contact set while notifying.
            foreach (FlammableTile tile in new List<FlammableTile>(contacts))
                if (tile != null && isActiveAndEnabled && contactActive) tile.SetWaterContact(this, true);
        }

        public static void RefreshTarget(FlammableTile tile)
        {
            if (tile == null || !tile.isActiveAndEnabled) return;
            Physics2D.SyncTransforms();
            Collider2D[] targets = tile.GetComponentsInChildren<Collider2D>();
            foreach (WaterContact water in new List<WaterContact>(sources))
            {
                bool touching = false;
                if (water != null && water.isActiveAndEnabled && water.contactActive)
                    foreach (Collider2D shape in water.shapes)
                    {
                        if (shape == null || !shape.enabled) continue;
                        foreach (Collider2D target in targets)
                            if (target.enabled && target.GetComponentInParent<FlammableTile>() == tile && shape != target &&
                                shape.Distance(target).isOverlapped) touching = true;
                    }
                if (water == null) continue;
                if (touching) water.contacts.Add(tile); else water.contacts.Remove(tile);
                tile.SetWaterContact(water, touching);
            }
        }

        public void ExtinguishSweep(Vector2 displacement)
        {
            if (!isActiveAndEnabled || !contactActive || displacement.sqrMagnitude <= 0f) return;
            foreach (Collider2D shape in shapes)
            {
                if (shape == null || !shape.enabled) continue;
                int count;
                while ((count = shape.Cast(displacement.normalized, Filter, sweepHits, displacement.magnitude)) == sweepHits.Length)
                    Array.Resize(ref sweepHits, sweepHits.Length * 2);
                for (int i = 0; i < count; i++)
                {
                    FlammableTile tile = sweepHits[i].collider.GetComponentInParent<FlammableTile>();
                    if (tile != null && tile.isActiveAndEnabled) tile.Extinguish();
                }
            }
        }

        public static void SyncScene(UnityEngine.SceneManagement.Scene scene)
        {
            foreach (WaterContact source in new List<WaterContact>(sources))
                if (source != null && source.gameObject.scene == scene) source.SyncContacts();
        }
    }
}
