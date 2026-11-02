using UnityEngine;

namespace ARFlashcards
{
    /// <summary>
    /// Builds each animal from Unity primitives (no external 3D asset needed).
    /// Convention: origin on the ground, +Y up, the animal faces +Z, roughly 1 unit long.
    /// To use real models instead, put a prefab at Resources/ARFlashcards/Models/&lt;id&gt; - it is
    /// picked up automatically (see Build).
    /// </summary>
    public static class AnimalFactory
    {
        public static GameObject Build(string id)
        {
            // Optional: a real model dropped into Resources/ARFlashcards/Models/<id>.prefab wins.
            var prefab = Resources.Load<GameObject>("ARFlashcards/Models/" + id);
            if (prefab != null)
            {
                var g = Object.Instantiate(prefab);
                g.name = id;
                return g;
            }

            var root = new GameObject(id);
            var torso = Pivot(root.transform, "Torso", Vector3.zero);
            var anim = root.AddComponent<ProceduralAnimator>();
            anim.torso = torso;
            anim.phase = Random.value * 6f;

            switch (id)
            {
                case "lion": Lion(root.transform, torso, anim); break;
                case "elephant": Elephant(root.transform, torso, anim); break;
                case "dog": Dog(root.transform, torso, anim); break;
                case "cat": Cat(root.transform, torso, anim); break;
                case "horse": Horse(root.transform, torso, anim); break;
                default: Prim(torso, PrimitiveType.Sphere, "Body", new Vector3(0, 0.5f, 0), new Vector3(0.6f, 0.6f, 0.9f), Color.gray); break;
            }
            return root;
        }

        // ----------------------------------------------------------------------------- animals

        static void Lion(Transform root, Transform torso, ProceduralAnimator a)
        {
            Color gold = C(0.90f, 0.63f, 0.24f), mane = C(0.47f, 0.24f, 0.08f), cream = C(0.99f, 0.90f, 0.72f), dark = C(0.12f, 0.08f, 0.07f);
            Prim(torso, PrimitiveType.Sphere, "Body", V(0, 0.62f, -0.02f), V(0.56f, 0.50f, 0.98f), gold);
            Prim(torso, PrimitiveType.Sphere, "Chest", V(0, 0.60f, 0.30f), V(0.50f, 0.50f, 0.50f), gold);
            Prim(torso, PrimitiveType.Sphere, "Hind", V(0, 0.60f, -0.30f), V(0.50f, 0.50f, 0.55f), gold);
            Prim(torso, PrimitiveType.Sphere, "ChestMane", V(0, 0.55f, 0.42f), V(0.40f, 0.45f, 0.35f), mane);
            foreach (float sx in new[] { -1f, 1f })
            {
                Limb(root, "FrontLeg", V(sx * 0.17f, 0.55f, 0.30f), V(sx * 0.17f, 0.07f, 0.32f), 0.085f, gold);
                Limb(root, "BackLeg", V(sx * 0.17f, 0.55f, -0.30f), V(sx * 0.17f, 0.07f, -0.28f), 0.09f, gold);
                Prim(root, PrimitiveType.Sphere, "Paw", V(sx * 0.17f, 0.05f, 0.35f), V(0.19f, 0.10f, 0.27f), gold);
                Prim(root, PrimitiveType.Sphere, "Paw", V(sx * 0.17f, 0.05f, -0.25f), V(0.19f, 0.10f, 0.27f), gold);
            }
            var head = Pivot(root, "Head", V(0, 0.86f, 0.46f));
            Prim(head, PrimitiveType.Sphere, "Mane", V(0, 0, -0.06f), V(0.72f, 0.72f, 0.50f), mane);
            Prim(head, PrimitiveType.Sphere, "Face", V(0, -0.02f, 0.10f), V(0.42f, 0.40f, 0.40f), gold);
            Prim(head, PrimitiveType.Sphere, "Muzzle", V(0, -0.10f, 0.30f), V(0.22f, 0.17f, 0.20f), cream);
            Prim(head, PrimitiveType.Sphere, "Nose", V(0, -0.04f, 0.395f), V(0.07f, 0.05f, 0.05f), dark);
            foreach (float sx in new[] { -1f, 1f })
            {
                Prim(head, PrimitiveType.Sphere, "Eye", V(sx * 0.095f, 0.06f, 0.285f), V(0.055f, 0.065f, 0.04f), dark);
                Prim(head, PrimitiveType.Sphere, "Ear", V(sx * 0.19f, 0.20f, 0.05f), V(0.13f, 0.13f, 0.07f), gold);
            }
            var tail = Pivot(root, "Tail", V(0, 0.68f, -0.50f));
            Chain(tail, gold, 0.035f, 0.03f, V(0, 0, 0), V(0, -0.10f, -0.20f), V(0, -0.28f, -0.28f));
            Prim(tail, PrimitiveType.Sphere, "Tuft", V(0, -0.31f, -0.30f), V(0.11f, 0.17f, 0.11f), mane);
            a.head = head; a.tail = tail; a.tailAmp = 18f; a.tailSpeed = 1.6f;
        }

        static void Elephant(Transform root, Transform torso, ProceduralAnimator a)
        {
            Color grey = C(0.58f, 0.63f, 0.72f), dkgrey = C(0.46f, 0.51f, 0.60f), pink = C(0.93f, 0.68f, 0.72f), ivory = C(0.99f, 0.97f, 0.90f), dark = C(0.10f, 0.09f, 0.10f);
            Prim(torso, PrimitiveType.Sphere, "Body", V(0, 0.82f, -0.04f), V(0.84f, 0.80f, 1.15f), grey);
            Prim(torso, PrimitiveType.Sphere, "Rump", V(0, 0.80f, -0.34f), V(0.80f, 0.76f, 0.70f), grey);
            foreach (float sx in new[] { -1f, 1f })
            {
                Limb(root, "FrontLeg", V(sx * 0.26f, 0.70f, 0.38f), V(sx * 0.26f, 0.06f, 0.38f), 0.14f, grey);
                Limb(root, "BackLeg", V(sx * 0.26f, 0.70f, -0.40f), V(sx * 0.26f, 0.06f, -0.40f), 0.14f, grey);
                Prim(root, PrimitiveType.Cylinder, "Foot", V(sx * 0.26f, 0.04f, 0.38f), V(0.31f, 0.04f, 0.31f), dkgrey);
                Prim(root, PrimitiveType.Cylinder, "Foot", V(sx * 0.26f, 0.04f, -0.40f), V(0.31f, 0.04f, 0.31f), dkgrey);
            }
            var head = Pivot(root, "Head", V(0, 1.00f, 0.66f));
            Prim(head, PrimitiveType.Sphere, "Skull", V(0, 0, 0), V(0.60f, 0.58f, 0.58f), grey);
            Prim(head, PrimitiveType.Sphere, "Forehead", V(0, 0.14f, 0.02f), V(0.46f, 0.38f, 0.44f), grey);
            foreach (float sx in new[] { -1f, 1f })
            {
                Prim(head, PrimitiveType.Sphere, "Ear", V(sx * 0.36f, 0.02f, -0.06f), V(0.07f, 0.56f, 0.50f), dkgrey);
                Prim(head, PrimitiveType.Sphere, "EarInner", V(sx * 0.395f, 0.0f, -0.05f), V(0.03f, 0.40f, 0.35f), pink);
                Prim(head, PrimitiveType.Sphere, "Eye", V(sx * 0.19f, 0.08f, 0.25f), V(0.06f, 0.07f, 0.05f), dark);
                Limb(head, "Tusk", V(sx * 0.15f, -0.14f, 0.24f), V(sx * 0.22f, -0.30f, 0.52f), 0.032f, ivory);
            }
            var trunk = Pivot(head, "Trunk", V(0, -0.08f, 0.27f));
            ChainR(trunk, grey,
                new[] { V(0, 0, 0), V(0, -0.20f, 0.07f), V(0, -0.42f, 0.10f), V(0, -0.64f, 0.08f), V(0, -0.80f, 0.14f) },
                new[] { 0.12f, 0.10f, 0.085f, 0.07f, 0.06f });
            var tail = Pivot(root, "Tail", V(0, 0.92f, -0.66f));
            Chain(tail, dkgrey, 0.03f, 0.025f, V(0, 0, 0), V(0, -0.20f, -0.05f), V(0, -0.42f, -0.06f));
            Prim(tail, PrimitiveType.Sphere, "Tuft", V(0, -0.46f, -0.06f), V(0.07f, 0.10f, 0.07f), dark);
            a.head = head; a.trunk = trunk; a.tail = tail; a.tailAmp = 10f; a.tailSpeed = 1.2f; a.headAmp = 2f; a.headSpeed = 0.7f;
        }

        static void Dog(Transform root, Transform torso, ProceduralAnimator a)
        {
            Color tan = C(0.82f, 0.60f, 0.35f), cream = C(0.98f, 0.91f, 0.78f), brown = C(0.42f, 0.25f, 0.14f), dark = C(0.09f, 0.07f, 0.07f), pink = C(0.93f, 0.38f, 0.46f);
            Prim(torso, PrimitiveType.Sphere, "Body", V(0, 0.50f, -0.02f), V(0.38f, 0.40f, 0.80f), tan);
            Prim(torso, PrimitiveType.Sphere, "Chest", V(0, 0.50f, 0.22f), V(0.40f, 0.42f, 0.40f), tan);
            Prim(torso, PrimitiveType.Sphere, "Neck", V(0, 0.64f, 0.32f), V(0.28f, 0.30f, 0.30f), tan);
            foreach (float sx in new[] { -1f, 1f })
            {
                Limb(root, "FrontLeg", V(sx * 0.12f, 0.45f, 0.25f), V(sx * 0.12f, 0.05f, 0.27f), 0.055f, tan);
                Limb(root, "BackLeg", V(sx * 0.12f, 0.45f, -0.27f), V(sx * 0.12f, 0.05f, -0.25f), 0.06f, tan);
                Prim(root, PrimitiveType.Sphere, "Paw", V(sx * 0.12f, 0.04f, 0.30f), V(0.12f, 0.07f, 0.17f), cream);
                Prim(root, PrimitiveType.Sphere, "Paw", V(sx * 0.12f, 0.04f, -0.21f), V(0.12f, 0.07f, 0.17f), cream);
            }
            var head = Pivot(root, "Head", V(0, 0.78f, 0.42f));
            Prim(head, PrimitiveType.Sphere, "Skull", V(0, 0, 0), V(0.34f, 0.32f, 0.34f), tan);
            Prim(head, PrimitiveType.Sphere, "Snout", V(0, -0.06f, 0.20f), V(0.19f, 0.15f, 0.22f), cream);
            Prim(head, PrimitiveType.Sphere, "Nose", V(0, -0.03f, 0.31f), V(0.07f, 0.05f, 0.06f), dark);
            Prim(head, PrimitiveType.Sphere, "Tongue", V(0, -0.14f, 0.27f), V(0.06f, 0.02f, 0.11f), pink);
            foreach (float sx in new[] { -1f, 1f })
            {
                Prim(head, PrimitiveType.Sphere, "Eye", V(sx * 0.09f, 0.06f, 0.15f), V(0.05f, 0.055f, 0.04f), dark);
                Prim(head, PrimitiveType.Sphere, "Ear", V(sx * 0.18f, 0.0f, -0.02f), V(0.07f, 0.25f, 0.15f), brown, V(0, 0, sx * 12f));
            }
            var tail = Pivot(root, "Tail", V(0, 0.62f, -0.40f));
            Chain(tail, tan, 0.035f, 0.028f, V(0, 0, 0), V(0, 0.12f, -0.10f), V(0, 0.26f, -0.14f));
            Prim(tail, PrimitiveType.Sphere, "Tip", V(0, 0.28f, -0.145f), V(0.06f, 0.07f, 0.06f), cream);
            a.head = head; a.tail = tail; a.tailAmp = 38f; a.tailSpeed = 7f; a.headAmp = 4f; a.headSpeed = 1.4f;
        }

        static void Cat(Transform root, Transform torso, ProceduralAnimator a)
        {
            Color gin = C(0.93f, 0.60f, 0.28f), stripe = C(0.74f, 0.40f, 0.14f), cream = C(0.99f, 0.93f, 0.82f), pink = C(0.96f, 0.62f, 0.68f), green = C(0.42f, 0.83f, 0.38f), dark = C(0.08f, 0.07f, 0.07f), white = C(0.96f, 0.96f, 0.96f);
            Prim(torso, PrimitiveType.Sphere, "Body", V(0, 0.40f, -0.04f), V(0.30f, 0.32f, 0.66f), gin);
            Prim(torso, PrimitiveType.Sphere, "Chest", V(0, 0.40f, 0.18f), V(0.30f, 0.33f, 0.30f), gin);
            Prim(torso, PrimitiveType.Sphere, "Stripe", V(0, 0.55f, -0.04f), V(0.04f, 0.03f, 0.45f), stripe);
            foreach (float sx in new[] { -1f, 1f })
            {
                Limb(root, "FrontLeg", V(sx * 0.09f, 0.36f, 0.19f), V(sx * 0.09f, 0.04f, 0.21f), 0.045f, gin);
                Limb(root, "BackLeg", V(sx * 0.10f, 0.34f, -0.22f), V(sx * 0.10f, 0.04f, -0.20f), 0.05f, gin);
                Prim(root, PrimitiveType.Sphere, "Paw", V(sx * 0.09f, 0.035f, 0.235f), V(0.09f, 0.06f, 0.13f), cream);
                Prim(root, PrimitiveType.Sphere, "Paw", V(sx * 0.10f, 0.035f, -0.175f), V(0.09f, 0.06f, 0.13f), cream);
            }
            var head = Pivot(root, "Head", V(0, 0.62f, 0.32f));
            Prim(head, PrimitiveType.Sphere, "Skull", V(0, 0, 0), V(0.32f, 0.28f, 0.28f), gin);
            Prim(head, PrimitiveType.Sphere, "Muzzle", V(0, -0.07f, 0.11f), V(0.15f, 0.10f, 0.10f), cream);
            Prim(head, PrimitiveType.Sphere, "Nose", V(0, -0.03f, 0.155f), V(0.04f, 0.03f, 0.03f), pink);
            foreach (float sx in new[] { -1f, 1f })
            {
                Prim(head, PrimitiveType.Sphere, "Eye", V(sx * 0.085f, 0.03f, 0.115f), V(0.07f, 0.08f, 0.04f), green);
                Prim(head, PrimitiveType.Sphere, "Pupil", V(sx * 0.085f, 0.03f, 0.135f), V(0.02f, 0.07f, 0.02f), dark);
                Cone(head, "Ear", V(sx * 0.095f, 0.11f, 0.0f), new Vector3(0.15f, 0.15f, 0.12f), V(0, 0, -sx * 10f), gin);
                Cone(head, "EarInner", V(sx * 0.095f, 0.115f, 0.022f), new Vector3(0.09f, 0.10f, 0.06f), V(0, 0, -sx * 10f), pink);
                Prim(head, PrimitiveType.Cylinder, "Whisker", V(sx * 0.17f, -0.04f, 0.12f), V(0.006f, 0.12f, 0.006f), white, V(0, 0, 90f));
                Prim(head, PrimitiveType.Cylinder, "Whisker", V(sx * 0.17f, -0.07f, 0.11f), V(0.006f, 0.12f, 0.006f), white, V(0, 0, 90f + sx * -14f));
            }
            Prim(head, PrimitiveType.Sphere, "Mark", V(0, 0.13f, 0.07f), V(0.025f, 0.02f, 0.07f), stripe);
            var tail = Pivot(root, "Tail", V(0, 0.46f, -0.34f));
            Chain(tail, gin, 0.032f, 0.026f, V(0, 0, 0), V(0, 0, -0.14f), V(0, 0.07f, -0.25f), V(0, 0.19f, -0.30f), V(0, 0.33f, -0.28f), V(0, 0.44f, -0.22f));
            Prim(tail, PrimitiveType.Sphere, "Tip", V(0, 0.45f, -0.215f), V(0.06f, 0.06f, 0.06f), stripe);
            a.head = head; a.tail = tail; a.tailAmp = 14f; a.tailSpeed = 1.8f; a.tailAxis = Vector3.right + Vector3.up * 0.4f;
        }

        static void Horse(Transform root, Transform torso, ProceduralAnimator a)
        {
            Color brown = C(0.56f, 0.34f, 0.18f), dkbrown = C(0.20f, 0.12f, 0.08f), muzzle = C(0.38f, 0.24f, 0.15f), hoof = C(0.14f, 0.11f, 0.10f), dark = C(0.07f, 0.06f, 0.06f), white = C(0.98f, 0.96f, 0.92f);
            Prim(torso, PrimitiveType.Sphere, "Body", V(0, 0.98f, -0.02f), V(0.50f, 0.56f, 1.10f), brown);
            Prim(torso, PrimitiveType.Sphere, "Rump", V(0, 1.00f, -0.32f), V(0.50f, 0.56f, 0.55f), brown);
            Prim(torso, PrimitiveType.Sphere, "Chest", V(0, 0.98f, 0.34f), V(0.46f, 0.56f, 0.50f), brown);
            foreach (float sx in new[] { -1f, 1f })
            {
                Limb(root, "FrontLeg", V(sx * 0.15f, 0.80f, 0.36f), V(sx * 0.15f, 0.08f, 0.38f), 0.062f, brown);
                Limb(root, "BackLeg", V(sx * 0.15f, 0.88f, -0.38f), V(sx * 0.15f, 0.08f, -0.34f), 0.07f, brown);
                Prim(root, PrimitiveType.Cylinder, "Hoof", V(sx * 0.15f, 0.04f, 0.385f), V(0.15f, 0.04f, 0.15f), hoof);
                Prim(root, PrimitiveType.Cylinder, "Hoof", V(sx * 0.15f, 0.04f, -0.335f), V(0.15f, 0.04f, 0.15f), hoof);
            }
            Limb(root, "Neck", V(0, 1.12f, 0.40f), V(0, 1.52f, 0.62f), 0.15f, brown);
            Limb(root, "Mane", V(0, 1.22f, 0.30f), V(0, 1.64f, 0.55f), 0.045f, dkbrown);
            var head = Pivot(root, "Head", V(0, 1.56f, 0.66f));
            Limb(head, "HeadBone", V(0, 0.04f, -0.02f), V(0, -0.20f, 0.27f), 0.125f, brown);
            Prim(head, PrimitiveType.Sphere, "Muzzle", V(0, -0.25f, 0.325f), V(0.19f, 0.20f, 0.22f), muzzle);
            Limb(head, "Blaze", V(0, 0.02f, 0.115f), V(0, -0.19f, 0.33f), 0.026f, white);
            foreach (float sx in new[] { -1f, 1f })
            {
                Prim(head, PrimitiveType.Sphere, "Eye", V(sx * 0.105f, 0.0f, 0.06f), V(0.045f, 0.055f, 0.045f), dark);
                Cone(head, "Ear", V(sx * 0.075f, 0.12f, -0.04f), new Vector3(0.08f, 0.16f, 0.07f), V(-8f, 0, -sx * 6f), brown);
            }
            var tail = Pivot(root, "Tail", V(0, 1.10f, -0.62f));
            ChainR(tail, dkbrown, new[] { V(0, 0, 0), V(0, -0.15f, -0.10f), V(0, -0.38f, -0.14f), V(0, -0.60f, -0.12f) },
                new[] { 0.05f, 0.055f, 0.05f, 0.035f });
            a.head = head; a.tail = tail; a.tailAmp = 12f; a.tailSpeed = 1.5f; a.headAmp = 5f; a.headSpeed = 0.9f;
        }

        // ----------------------------------------------------------------------------- helpers

        static Vector3 V(float x, float y, float z) { return new Vector3(x, y, z); }
        static Color C(float r, float g, float b) { return new Color(r, g, b, 1f); }

        static Transform Pivot(Transform parent, string name, Vector3 pos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            return go.transform;
        }

        static GameObject Prim(Transform parent, PrimitiveType type, string name, Vector3 pos, Vector3 scale, Color c)
        {
            return Prim(parent, type, name, pos, scale, c, Vector3.zero);
        }

        static GameObject Prim(Transform parent, PrimitiveType type, string name, Vector3 pos, Vector3 scale, Color c, Vector3 euler)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(euler);
            go.transform.localScale = scale;
            Style(go, c);
            return go;
        }

        /// <summary>Cone with its base at pos, apex pointing up (rotated by euler). scale = (diameter, height, diameter).</summary>
        static GameObject Cone(Transform parent, string name, Vector3 pos, Vector3 scale, Vector3 euler, Color c)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(euler);
            go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = MeshKit.Cone();
            go.AddComponent<MeshRenderer>();
            Style(go, c);
            return go;
        }

        /// <summary>A capsule stretched between two points (parent-local space).</summary>
        static GameObject Limb(Transform parent, string name, Vector3 from, Vector3 to, float radius, Color c)
        {
            Vector3 d = to - from;
            float len = d.magnitude;
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = name;
            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = (from + to) * 0.5f;
            go.transform.localRotation = Quaternion.FromToRotation(Vector3.up, d.normalized);
            // Unity's capsule is 2 units tall (incl. caps) and 1 unit wide
            go.transform.localScale = new Vector3(radius * 2f, Mathf.Max(len * 0.5f, radius), radius * 2f);
            Style(go, c);
            return go;
        }

        static void Chain(Transform parent, Color c, float r0, float r1, params Vector3[] pts)
        {
            for (int i = 0; i < pts.Length - 1; i++)
            {
                float t = pts.Length > 2 ? (float)i / (pts.Length - 2) : 0f;
                Limb(parent, "Seg" + i, pts[i], pts[i + 1], Mathf.Lerp(r0, r1, t), c);
            }
        }

        static void ChainR(Transform parent, Color c, Vector3[] pts, float[] radii)
        {
            for (int i = 0; i < pts.Length - 1; i++)
                Limb(parent, "Seg" + i, pts[i], pts[i + 1], radii[i], c);
            // round off the end
            Prim(parent, PrimitiveType.Sphere, "End", pts[pts.Length - 1],
                 V(radii[radii.Length - 1] * 2f, radii[radii.Length - 1] * 2f, radii[radii.Length - 1] * 2f), c);
        }

        static void Style(GameObject go, Color c)
        {
            var r = go.GetComponent<Renderer>();
            if (r == null) return;
            r.sharedMaterial = MatLib.Get(c);
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
        }
    }
}
