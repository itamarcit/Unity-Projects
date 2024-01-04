using UnityEngine;
using Avrahamy;
using Avrahamy.EditorGadgets;

namespace Extra {
    public class GradientDescentTest : MonoBehaviour {
        [SerializeField] int childrenCount = 2;
        [SerializeField] float delta = 0.1f;
        [SerializeField] float learningRate = 0.1f;
        [SerializeField] float desiredRadius = 1f;
        [Range(0, 1f)]
        [SerializeField] float distanceWeight = 1f;
        [Range(0, 1f)]
        [SerializeField] float centerWeight = 1f;
        [SerializeField] float minScore = 0.001f;
        [ReadOnly]
        [SerializeField] float score;
        [ReadOnly]
        [SerializeField] int iterations;

        private Vector2[] origins;
        private Vector2[] points;
        private Vector2[] gradient;
        private Vector2 originCenter;
        private Vector2 center;

        protected void OnEnable() {
            origins = new Vector2[Mathf.Min(transform.childCount, childrenCount)];
            points = new Vector2[origins.Length];
            gradient = new Vector2[origins.Length];
            // Calculate the bounding box to get the center of the points.
            var originBottomLeft = Vector2.positiveInfinity;
            var originTopRight = Vector2.negativeInfinity;
            for (int i = 0; i < points.Length; i++) {
                origins[i] = transform.GetChild(i).position;
                points[i] = origins[i];
                originBottomLeft.x = Mathf.Min(originBottomLeft.x, origins[i].x);
                originBottomLeft.y = Mathf.Min(originBottomLeft.y, origins[i].y);
                originTopRight.x = Mathf.Max(originTopRight.x, origins[i].x);
                originTopRight.y = Mathf.Max(originTopRight.y, origins[i].y);
            }
            originCenter = (originTopRight + originBottomLeft) / 2;
            iterations = 0;
        }

        protected void OnValidate() {
            OnEnable();
        }

        protected void Update() {
            score = Score();
            if (score <= minScore) {
                // Draw result.
                for (int i = 0; i < points.Length; i++) {
                    DebugDraw.DrawLine(origins[i], points[i], Color.magenta);
                    DebugDraw.DrawCross2D(points[i], 0.25f, Color.green);
                }
                return;
            }
            DebugDraw.DrawCross2D(originCenter, 0.25f, Color.green);
            DebugDraw.DrawCross2D(center, 0.25f, Color.yellow);
            for (int i = 0; i < points.Length; i++) {
                var originalPoint = points[i];
                // Partial derivative of xi
                points[i].x += delta;
                gradient[i].x = Score() - score;
                points[i] = originalPoint;
                // Partial derivative of yi
                points[i].y += delta;
                gradient[i].y = Score() - score;
                points[i] = originalPoint;
            }

            // Normalize the gradient and multiply be the learning rate.
            var sumSqr = 0f;
            for (int i = 0; i < points.Length; i++) {
                sumSqr += gradient[i].sqrMagnitude;
            }
            var multiplier = sumSqr > float.Epsilon ? learningRate / Mathf.Sqrt(sumSqr) : 0f;

            for (int i = 0; i < points.Length; i++) {
                // Subtract the gradient because it points up the slope.
                points[i] -= gradient[i] * multiplier;
                DebugDraw.DrawLine(origins[i], points[i], Color.magenta);
                DebugDraw.DrawCross2D(points[i], 0.25f, Color.red);
            }

            ++iterations;
        }

        private float Score() {
            var score = 0f;
            var bottomLeft = Vector2.positiveInfinity;
            var topRight = Vector2.negativeInfinity;
            for (int i = 0; i < points.Length; i++) {
                var point = points[i];
                score += Score(i);
                bottomLeft.x = Mathf.Min(bottomLeft.x, point.x);
                bottomLeft.y = Mathf.Min(bottomLeft.y, point.y);
                topRight.x = Mathf.Max(topRight.x, point.x);
                topRight.y = Mathf.Max(topRight.y, point.y);
            }

            center = (topRight + bottomLeft) / 2;
            var centerOffset = center - originCenter;
            return score / points.Length
                   + (Mathf.Abs(centerOffset.x) + Mathf.Abs(centerOffset.y)) * centerWeight;
        }

        private float Score(int i) {
            var minDistance = float.MaxValue;
            for (int j = 0; j < points.Length; j++) {
                if (j == i) continue;
                var distance = (points[j] - points[i]).sqrMagnitude;
                if (distance < minDistance) {
                    minDistance = distance;
                }
            }

            var distanceT = Mathf.InverseLerp(0f, desiredRadius * desiredRadius, minDistance);
            // This is -log((x+0.001)/(2.001-x))/3.3. At x=0 it is 1 and at x=1 is it 0.
            var distanceScore = -Mathf.Log10((distanceT + 0.001f) / (2.001f - distanceT)) / 3.3f;
            return distanceScore * distanceWeight;
        }
    }
}
