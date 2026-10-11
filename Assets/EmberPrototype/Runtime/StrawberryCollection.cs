using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace EmberPrototype
{
    [DisallowMultipleComponent]
    public sealed class StrawberryCollection : MonoBehaviour
    {
        [SerializeField] private UnityEvent<int> onCountChanged = new UnityEvent<int>();
        private readonly List<StrawberryPickup> followers = new List<StrawberryPickup>();
        public int CollectedCount { get; private set; }
        public int PendingCount => followers.Count;
        public UnityEvent<int> OnCountChanged => onCountChanged;

        internal void Attach(StrawberryPickup berry) { if (!followers.Contains(berry)) followers.Add(berry); }
        internal Transform LeaderFor(StrawberryPickup berry)
        {
            int index = followers.IndexOf(berry);
            return index > 0 ? followers[index - 1].transform : transform;
        }
        internal void Detach(StrawberryPickup berry) => followers.Remove(berry);
        internal void Confirm(StrawberryPickup berry)
        {
            if (!followers.Remove(berry)) return;
            CollectedCount++;
            onCountChanged.Invoke(CollectedCount);
        }
        public void ReturnPending()
        {
            while (followers.Count > 0) followers[followers.Count - 1].ResetForRespawn();
        }
        private void OnDisable() => ReturnPending();
    }
}
