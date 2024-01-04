using UnityEngine;
using Avrahamy.Math;

namespace Avrahamy.Meshes {
    public enum SpaceType {
        XYZ = 0,
        XY = 1,
        XZ = 2,
    };

    public class EditableMesh : UniqueMesh {
        [SerializeField] protected SpaceType space = SpaceType.XY;

        public void SetPoints(Vector3[] points) {
            var vec2Points = space == SpaceType.XY ? points.ToVector2XY() : points.ToVector2XZ();
            // Triangulate points for mesh.
            var triangulator = new Triangulator(vec2Points);
            var indices = triangulator.Triangulate();
            SetPoints(points, indices);
        }

        public void SetPoints(Vector3[] vertices, int[] triangles) {
            // Reset mesh.
            var sharedMesh = Mesh;
            sharedMesh.Clear();
            sharedMesh.vertices = vertices;
            sharedMesh.triangles = triangles;
            MeshFilter.sharedMesh.RecalculateBounds();
            MeshFilter.sharedMesh.RecalculateNormals();
        }
    }
}
