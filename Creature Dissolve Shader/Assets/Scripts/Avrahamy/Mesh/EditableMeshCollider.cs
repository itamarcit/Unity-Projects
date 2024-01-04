using UnityEngine;

namespace Avrahamy.Meshes {
    public class EditableMeshCollider : UniqueMeshCollider {
        public void SetPoints(Vector3[] vertices, int[] triangles) {
            // Reset mesh.
            var sharedMesh = Mesh;
            sharedMesh.Clear();
            sharedMesh.vertices = vertices;
            sharedMesh.triangles = triangles;
            MeshCollider.sharedMesh.RecalculateBounds();
            MeshCollider.sharedMesh.RecalculateNormals();
        }
    }
}
