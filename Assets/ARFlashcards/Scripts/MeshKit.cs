using UnityEngine;

namespace ARFlashcards
{
    /// <summary>Small procedural meshes that Unity has no primitive for.</summary>
    public static class MeshKit
    {
        static Mesh cone;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { cone = null; }

        /// <summary>Unit cone: base radius 0.5 at y=0, apex at y=1. Scale = (diameter, height, diameter).</summary>
        public static Mesh Cone()
        {
            if (cone != null) return cone;
            const int seg = 20;
            var v = new Vector3[seg + 2];
            for (int i = 0; i < seg; i++)
            {
                float a = (float)i / seg * Mathf.PI * 2f;
                v[i] = new Vector3(Mathf.Cos(a) * 0.5f, 0f, Mathf.Sin(a) * 0.5f);
            }
            int apex = seg, centre = seg + 1;
            v[apex] = new Vector3(0f, 1f, 0f);
            v[centre] = Vector3.zero;

            var t = new int[seg * 6];
            for (int i = 0; i < seg; i++)
            {
                int a = i, b = (i + 1) % seg;
                // side (outward) and base cap (downward) - winding checked in the unit tests
                t[i * 6 + 0] = a; t[i * 6 + 1] = apex; t[i * 6 + 2] = b;
                t[i * 6 + 3] = centre; t[i * 6 + 4] = a; t[i * 6 + 5] = b;
            }
            cone = new Mesh();
            cone.name = "ARF_Cone";
            cone.vertices = v;
            cone.triangles = t;
            cone.RecalculateNormals();
            cone.RecalculateBounds();
            return cone;
        }
    }
}
