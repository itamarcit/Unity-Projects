using UnityEngine;
using Avrahamy.Meshes;

namespace MeshTutorial {
    [ExecuteAlways]
    public class QuadMesh : EditableMesh
    {
        [SerializeField] private Vector3 vec = new (1, 1, 1);
        protected void Update()
        {
            var vertices = new Vector3[]
            {
                vec,
                new(1, 2, 1),
                new(2, 2, 1),
                new(2, 1, 1),
            };
            var triangles = new int[]
            {
                0, 1, 2,
                2, 3, 0
            };
            Mesh.vertices = vertices;
            Mesh.triangles = triangles;
            Vector2[] uvs = new []
            {
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 0f),
            };
            Mesh.uv = uvs;
        }
    }
}
