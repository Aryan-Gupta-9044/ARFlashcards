using UnityEngine;

namespace ARFlashcards
{
    /// <summary>
    /// Gives the primitive-built animals life: breathing, head movement, tail wag, trunk sway.
    /// All references are filled in by AnimalFactory.
    /// </summary>
    public sealed class ProceduralAnimator : MonoBehaviour
    {
        public Transform torso;      // scaled slightly to "breathe"
        public Transform head;
        public Transform tail;
        public Transform trunk;
        public Vector3 tailAxis = Vector3.up;
        public float tailAmp = 20f, tailSpeed = 2f;
        public float headAmp = 3f, headSpeed = 1.1f;
        public float trunkAmp = 7f, trunkSpeed = 1.2f;
        public float phase;

        Quaternion headBase = Quaternion.identity, tailBase = Quaternion.identity, trunkBase = Quaternion.identity;
        bool captured;

        void Capture()
        {
            if (head != null) headBase = head.localRotation;
            if (tail != null) tailBase = tail.localRotation;
            if (trunk != null) trunkBase = trunk.localRotation;
            captured = true;
        }

        void OnEnable() { if (!captured) Capture(); }

        void Update()
        {
            float t = Time.time + phase;
            if (torso != null) torso.localScale = new Vector3(1f, 1f + 0.012f * Mathf.Sin(t * 1.8f), 1f);
            if (head != null)
                head.localRotation = headBase * Quaternion.Euler(Mathf.Sin(t * headSpeed) * headAmp,
                                                                  Mathf.Sin(t * headSpeed * 0.6f) * headAmp * 1.6f, 0f);
            if (tail != null)
                tail.localRotation = tailBase * Quaternion.AngleAxis(Mathf.Sin(t * tailSpeed) * tailAmp, tailAxis);
            if (trunk != null)
                trunk.localRotation = trunkBase * Quaternion.Euler(Mathf.Sin(t * trunkSpeed) * trunkAmp,
                                                                    Mathf.Sin(t * trunkSpeed * 0.7f) * trunkAmp, 0f);
        }
    }
}
