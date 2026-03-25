using UnityEngine;
namespace KairaDigitalArts {
    public class UCC_CollisionHandler : MonoBehaviour
    {
        public MeshFilter[] meshFilter;
        private float deformRadius = 1f;
        public float deformStrength = 1f;

        void OnCollisionEnter(Collision collision)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                Vector3 contactPoint = contact.point;
                foreach (var mesh in meshFilter)
                {
                    DeformMesh(contactPoint, contact.normal, collision.impulse.magnitude, mesh.mesh);
                }
            }
        }

        void DeformMesh(Vector3 contactPoint, Vector3 normal, float force, Mesh mesh)
        {
            if (meshFilter == null)
            {
                Debug.LogWarning("MeshFilter not assigned.");
                return;
            }

            Vector3[] vertices = mesh.vertices;

            contactPoint = transform.InverseTransformPoint(contactPoint);

            float normalizedForce = Mathf.Min(force / 1000f, 1f);

            for (int i = 0; i < vertices.Length; i++)
            {
                float distance = Vector3.Distance(vertices[i], contactPoint);
                if (distance < deformRadius)
                {
                    float deformAmount = Mathf.Clamp((deformRadius - distance) * normalizedForce * deformStrength, 0, 0.1f);
                    vertices[i] += normal * deformAmount;
                }
            }
            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            MeshCollider collider = GetComponent<MeshCollider>();
            if (collider != null)
            {
                collider.sharedMesh = null;
                collider.sharedMesh = mesh;
            }
        }
    }
}
