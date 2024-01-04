using UnityEngine;
using Avrahamy.Math;

namespace Avrahamy.Meshes {
    [ExecuteInEditMode]
    public class EditablePolygonMesh : EditableMesh {
        protected PolygonCollider2D polygon;

        protected void Awake() {
            polygon = GetComponent<PolygonCollider2D>();
        }

#if UNITY_EDITOR
        protected void Update() {
            if (Application.isPlaying) return;
            if (polygon == null) {
                polygon = GetComponent<PolygonCollider2D>();
                return;
            }
            SetPoints(polygon.points);
        }
#endif

        protected virtual void SetPoints(Vector2[] points) {
            SetPoints(points.ToVector3XY());
        }
    }
}
