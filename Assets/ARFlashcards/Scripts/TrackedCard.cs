using System;
using UnityEngine;
#if ARF_VUFORIA
using Vuforia;
#endif

namespace ARFlashcards
{
    /// <summary>Sits on a Vuforia Image Target; shows its model while tracked and reports found / lost.</summary>
    public sealed class TrackedCard : MonoBehaviour
    {
        public Card card;
        public GameObject model;
        public bool IsTracked { get; private set; }
        public event Action<TrackedCard, bool> TrackingChanged;

#if ARF_VUFORIA
        ObserverBehaviour observer;

        public void Bind(Card c, GameObject modelObject)
        {
            card = c;
            model = modelObject;
            if (model != null) model.SetActive(false);
            observer = GetComponent<ObserverBehaviour>();
            if (observer != null) observer.OnTargetStatusChanged += OnStatus;
        }

        void OnStatus(ObserverBehaviour behaviour, TargetStatus status)
        {
            Apply(status.Status == Status.TRACKED || status.Status == Status.EXTENDED_TRACKED);
        }

        void OnDestroy()
        {
            if (observer != null) observer.OnTargetStatusChanged -= OnStatus;
        }
#else
        public void Bind(Card c, GameObject modelObject)
        {
            card = c; model = modelObject;
            if (model != null) model.SetActive(false);
        }
#endif

        void Apply(bool now)
        {
            if (now == IsTracked) return;
            IsTracked = now;
            if (model != null) model.SetActive(now);
            if (TrackingChanged != null) TrackingChanged(this, now);
        }
    }
}
