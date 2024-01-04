using UnityEngine;
using System.Collections.Generic;
using Avrahamy.Meshes;

namespace CreatureGeneration
{
    [ExecuteAlways]
    public class LimbMesh : EditableMesh
    {
        public struct Pivot
        {
            public Transform pivot;
            public Vector3 originalPositionInMesh;
        }

        public struct PivotInfluenceOnVertex
        {
            public int pivotIndex;
            public float weight;
        }

        public struct MeshVertex
        {
            public Vector3 originalPositionInMesh;
            public PivotInfluenceOnVertex[] meshVerticesWeights;

            public Vector3 CalculateCurrentPosition(Vector3 rootPosition, List<Pivot> pivots)
            {
                var weightSum = 0f;
                var position = Vector3.zero;
                foreach (var influence in meshVerticesWeights)
                {
                    // Position of vertex relative to influencing pivot.
                    var pivot = pivots[influence.pivotIndex];
                    var relativePosition = originalPositionInMesh - pivot.originalPositionInMesh;
                    // Position of vertex in world space if it was 100% influenced by this pivot.
                    var positionAccordingToPivot = pivot.pivot.TransformPoint(relativePosition);
                    // Sum in order to calculate weighted average.
                    position += positionAccordingToPivot * influence.weight;
                    weightSum += influence.weight;
                }

                // World position to mesh position.
                return position / weightSum - rootPosition;
            }
        }

        private const float X_DEFAULT = -0.15f;
        private const float Y_DEFAULT = -4f;
        private const float Z_DEFAULT = -0.15f;
        private const float DIFF_DEFAULT = 1f;
        private const float RADIUS_DEFAULT = 0.3f;

        private const float X_TAIL = -0.1f;
        private const float Y_TAIL = -4f;
        private const float Z_TAIL = -0.1f;
        private const float DIFF_TAIL = 2f;
        private const float RADIUS_TAIL = 0.3f;

        private const float X_NECK = -0.1f;
        private const float Y_NECK = -4f;
        private const float Z_NECK = -0.1f;
        private const float DIFF_NECK = 1f;
        private const float RADIUS_NECK = 0.5f;

        [SerializeField] Transform root;
        [SerializeField] Transform tip;

        private List<Pivot> pivots;
        private Vector3[] vertices;
        private int[] triangles;
        private Vector2[] uvs;
        private MeshVertex[] meshVertices;

        protected void Awake()
        {
            pivots = new List<Pivot>();
            var joint = tip;
            // Go from the tip up the hierarchy until we reach the root.
            while (joint != null && joint != root)
            {
                pivots.Add(new Pivot
                {
                    pivot = joint,
                    originalPositionInMesh = joint.position - root.position
                });
                joint = joint.parent;
            }

            pivots.Add(new Pivot
            {
                pivot = root,
                originalPositionInMesh = Vector3.zero
            });
            var meshCollider = GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                meshCollider.sharedMesh = Mesh;
            }
        }

        // NOTE: Using OnEnable so it will be easy to reset the mesh.
        protected void OnEnable()
        {
            BuildMesh();
        }

        protected void Update()
        {
            MatchVerticesToSkin();
            SetPoints(vertices, triangles);
            Mesh.uv = uvs;
        }

        // Creates a cuboid starting around the calling GameObject starting from bottomCenter as Y.
        private void AddChestAround(List<Vector3> verticesList, List<int> trianglesList,
            Vector3 bottomCenter)
        {
            float x;
            if (gameObject.CompareTag("Left Legs"))
            {
                x = X_DEFAULT - 1; // Gets the relative location of the left leg from the pelvis.
            }
            else // Tag is "Right Legs" or not a leg.
            {
                x = X_DEFAULT + 1; // Gets the relative location of the right leg from the pelvis.
            }
            float y = bottomCenter.y + Y_DEFAULT;
            float z = Z_DEFAULT;
            float diff = DIFF_DEFAULT;
            float radius = RADIUS_DEFAULT;
            int triangleIndex = verticesList.Count;

            if (gameObject.CompareTag("Tail"))
            {
                x = X_TAIL;
                y = bottomCenter.y + Y_TAIL;
                z = Z_TAIL;
                diff = DIFF_TAIL;
                radius = RADIUS_TAIL;
            }

            if (gameObject.CompareTag("Neck"))
            {
                x = X_NECK;
                y = bottomCenter.y + Y_NECK;
                z = Z_NECK;
                diff = DIFF_NECK;
                radius = RADIUS_NECK;
            }

            verticesList.Add(new Vector3(x, y, z)); // 0
            verticesList.Add(new Vector3(x + radius, y, z)); // 1
            verticesList.Add(new Vector3(x, y, z + radius)); // 2
            verticesList.Add(new Vector3(x + radius, y, z + radius)); // 3
            verticesList.Add(new Vector3(x, y + diff, z)); // 4
            verticesList.Add(new Vector3(x + radius, y + diff, z)); // 5
            verticesList.Add(new Vector3(x, y + diff, z + radius)); // 6
            verticesList.Add(new Vector3(x + radius, y + diff, z + radius)); // 7

            int[] triangleOffsets =
            {
                0, 4, 1, 4, 5, 1, 1, 5, 3, 3, 5, 7,
                3, 7, 2, 7, 6, 2, 4, 0, 2, 2, 6, 4,
                2, 0, 1, 1, 3, 2, 4, 6, 5, 5, 6, 7
            };

            for (int i = 0; i < triangleOffsets.Length; i++)
            {
                trianglesList.Add(triangleIndex + triangleOffsets[i]);
            }
        }

        private void BuildMesh()
        {
            List<Vector3> verticesLst = new List<Vector3>();
            List<int> trianglesLst = new List<int>();
            //Add two chests.
            AddChestAround(verticesLst, trianglesLst,
                pivots[0].pivot.position);
            AddChestAround(verticesLst, trianglesLst,
                pivots[1].pivot.position);
            //Extra chest for the tail - 3 parts.
            if (gameObject.CompareTag("Tail"))
            {
                AddChestAround(verticesLst, trianglesLst,
                    pivots[2].pivot.position);
            }

            vertices = verticesLst.ToArray();
            triangles = trianglesLst.ToArray();

            meshVertices = new MeshVertex[vertices.Length];
            
            // The for loop indices describe the rings of the skin for each cuboid made from BuildMesh() and sets
            // their weights accordingly.
            
            for (int i = 12; i < 16; i++) 
            {
                meshVertices[i] = CreateNewMeshVertex(0, 0, 0, 1, i);
            }

            for (int i = 4; i < 12; i++)
            {
                meshVertices[i] = CreateNewMeshVertex(0, 0.5f, 0.5f, 0, i);
            }

            for (int i = 0; i < 4; i++)
            {
                meshVertices[i] = CreateNewMeshVertex(0, 1, 0, 0, i);
            }

            if (gameObject.CompareTag("Tail"))
            {
                for (int i = 16; i < 20; i++)
                {
                    meshVertices[i] = CreateNewMeshVertex(0, 0, 0.5f, 0.5f, i);
                }
                for (int i = 20; i < 24; i++)
                {
                    meshVertices[i] = CreateNewMeshVertex(0, 0, 0, 1, i);
                }
            }

            List<Vector2> uvsLst = new();
            if (gameObject.CompareTag("Tail"))
            {
                GenerateUVs(3, 3, uvsLst);
            }
            else
            {
                GenerateUVs(2, 3, uvsLst);
            }

            uvs = uvsLst.ToArray();
        }

        // Generates the given uvsLst to fit the cuboids made prior to calling this function.
        private void GenerateUVs(int numRows, int numCols, List<Vector2> uvsLst)
        {
            float stepU = 1f / numCols;
            float stepV = 1f / numRows;
            float u, v;

            for (int row = 0; row <= numRows; row++)
            {
                for (int col = 0; col <= numCols; col++)
                {
                    u = stepU * col;
                    v = stepV * row;

                    uvsLst.Add(new Vector2(u, v));
                }

                if (row != 0 && row != numRows)
                {
                    for (int col = 0; col <= numCols; col++)
                    {
                        u = stepU * col;
                        v = stepV * row;

                        uvsLst.Add(new Vector2(u, v));
                    }
                }
            }
        }


        private MeshVertex CreateNewMeshVertex(float weightZero, float weightOne, float weightTwo, float weightThree,
            int index)
        {
            return new MeshVertex
            {
                originalPositionInMesh = vertices[index],
                meshVerticesWeights = new[] // Each pivot index represents the mesh's bone pivot, where 0 is the 
                                            // bottom bone, going up from there.
                {
                    new PivotInfluenceOnVertex
                    {
                        pivotIndex = 3,
                        weight = weightThree,
                    },
                    new PivotInfluenceOnVertex
                    {
                        pivotIndex = 2,
                        weight = weightTwo,
                    },
                    new PivotInfluenceOnVertex
                    {
                        pivotIndex = 1,
                        weight = weightOne,
                    },
                    new PivotInfluenceOnVertex
                    {
                        pivotIndex = 0,
                        weight = weightZero,
                    }
                }
            };
        }


        private void MatchVerticesToSkin()
        {
            var rootPosition = root.position;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = meshVertices[i].CalculateCurrentPosition(
                    rootPosition,
                    pivots);
            }
        }
    }
}