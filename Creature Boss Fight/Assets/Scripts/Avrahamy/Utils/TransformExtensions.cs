using UnityEngine;
using System.Text;

namespace Avrahamy.Utils {
    public static class TransformExtensions {
        /// <summary>
        /// When something is destroyed it won't be null. The only way to check
        /// this is by calling Equals(null).
        /// </summary>
        public static bool IsNullOrDestroyed(this Object obj) {
            return obj == null || obj.Equals(null);
        }

        /// <summary>
        /// Sets the X and Y values of the position without changing the Z value.
        /// </summary>
        public static void SetXY(this Transform transform, Vector2 value) {
            var position = transform.position;
            position.x = value.x;
            position.y = value.y;
            transform.position = position;
        }

        public static void SetAnchoredX(this RectTransform transform, float value) {
            var position = transform.anchoredPosition;
            position.x = value;
            transform.anchoredPosition = position;
        }

        public static void SetAnchoredY(this RectTransform transform, float value) {
            var position = transform.anchoredPosition;
            position.y = value;
            transform.anchoredPosition = position;
        }

        public static void SetX(this Transform transform, float value) {
            var position = transform.position;
            position.x = value;
            transform.position = position;
        }

        public static void SetY(this Transform transform, float value) {
            var position = transform.position;
            position.y = value;
            transform.position = position;
        }

        public static void SetZ(this Transform transform, float value) {
            var position = transform.position;
            position.z = value;
            transform.position = position;
        }

        public static void SetXRotation(this Transform transform, float value) {
            var rotation = transform.eulerAngles;
            rotation.x = value;
            transform.eulerAngles = rotation;
        }

        public static void SetYRotation(this Transform transform, float value) {
            var rotation = transform.eulerAngles;
            rotation.y = value;
            transform.eulerAngles = rotation;
        }
        public static void SetZRotation(this Transform transform, float value) {
            var rotation = transform.eulerAngles;
            rotation.z = value;
            transform.eulerAngles = rotation;
        }

        public static void SetLocalXY(this Transform transform, Vector2 value) {
            var position = transform.localPosition;
            position.x = value.x;
            position.y = value.y;
            transform.localPosition = position;
        }

        /// <summary>
        /// Assigns x to localPosition.x and y to localPosition.z.
        /// </summary>
        public static void SetLocalXZ(this Transform transform, Vector2 value) {
            var position = transform.localPosition;
            position.x = value.x;
            position.z = value.y;
            transform.localPosition = position;
        }

        /// <summary>
        /// Assigns x to localPosition.x and z to localPosition.z, keeping the Y
        /// value as is.
        /// </summary>
        public static void SetLocalXZ(this Transform transform, Vector3 value) {
            var position = transform.localPosition;
            position.x = value.x;
            position.z = value.z;
            transform.localPosition = position;
        }

        public static void SetLocalX(this Transform transform, float value) {
            var position = transform.localPosition;
            position.x = value;
            transform.localPosition = position;
        }

        public static void SetLocalY(this Transform transform, float value) {
            var position = transform.localPosition;
            position.y = value;
            transform.localPosition = position;
        }

        public static void SetLocalZ(this Transform transform, float value) {
            var position = transform.localPosition;
            position.z = value;
            transform.localPosition = position;
        }

        public static void SetScale(this Transform transform, float value) {
            var scale = transform.localScale;
            scale.x = value;
            scale.y = value;
            scale.z = value;
            transform.localScale = scale;
        }

        public static void SetScaleX(this Transform transform, float value) {
            var scale = transform.localScale;
            scale.x = value;
            transform.localScale = scale;
        }

        public static void AbsScaleX(this Transform transform, bool negate = false) {
            var scale = transform.localScale;
            scale.x *= Mathf.Sign(transform.lossyScale.x);
            if (negate) {
                scale.x *= -1f;
            }
            transform.localScale = scale;
        }

        public static void FlipScaleX(this Transform transform) {
            var scale = transform.localScale;
            scale.x = -scale.x;
            transform.localScale = scale;
        }

        public static void SetScaleY(this Transform transform, float value) {
            var scale = transform.localScale;
            scale.y = value;
            transform.localScale = scale;
        }

        public static void AbsScaleY(this Transform transform, bool negate = false) {
            var scale = transform.localScale;
            scale.y *= Mathf.Sign(transform.lossyScale.y);
            if (negate) {
                scale.y *= -1f;
            }
            transform.localScale = scale;
        }

        public static void SetScaleZ(this Transform transform, float value) {
            var scale = transform.localScale;
            scale.z = value;
            transform.localScale = scale;
        }

        public static void AbsScaleZ(this Transform transform, bool negate = false) {
            var scale = transform.localScale;
            scale.z *= Mathf.Sign(transform.lossyScale.z);
            if (negate) {
                scale.z *= -1f;
            }
            transform.localScale = scale;
        }

        public static void AbsScale(this Transform transform, bool negate = false) {
            transform.AbsScaleX(negate);
            transform.AbsScaleY(negate);
            transform.AbsScaleZ(negate);
        }

        public static void SetLocalXRotation(this Transform transform, float value) {
            var rotation = transform.localEulerAngles;
            rotation.x = value;
            transform.localEulerAngles = rotation;
        }

        public static void SetLocalYRotation(this Transform transform, float value) {
            var rotation = transform.localEulerAngles;
            rotation.y = value;
            transform.localEulerAngles = rotation;
        }

        public static void SetLocalZRotation(this Transform transform, float value) {
            var rotation = transform.localEulerAngles;
            rotation.z = value;
            transform.localEulerAngles = rotation;
        }

        public static void ResetTransform(this Transform transform) {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        public static string PathInHierarchy(this Transform obj, int maxLevels = 5) {
            var sb = new StringBuilder();
            PathInHierarchy(obj, maxLevels, sb);
            return sb.ToString();
        }

        private static void PathInHierarchy(this Transform obj, int maxLevels, StringBuilder sb) {
            if (maxLevels <= 1 || obj.parent == null) {
                sb.Append(obj.name);
                return;
            }
            PathInHierarchy(obj.parent, maxLevels - 1, sb);
            sb.Append(".");
            sb.Append(obj.name);
        }

        public static string PathInHierarchy(this Transform obj, string stopAtName) {
            var sb = new StringBuilder();
            PathInHierarchy(obj, stopAtName, sb);
            return sb.ToString();
        }

        private static void PathInHierarchy(this Transform obj, string stopAtName, StringBuilder sb) {
            if (obj.parent == null || obj.parent.name == stopAtName) {
                sb.Append(obj.name);
                return;
            }
            PathInHierarchy(obj.parent, stopAtName, sb);
            sb.Append(".");
            sb.Append(obj.name);
        }

        public static void DestroyChildren(this Transform parent, int fromIndex = 0) {
            for (int i = parent.childCount - 1; i >= fromIndex; i--) {
                var child = parent.GetChild(i);
                // Detach child so the parent won't have "zombie" children that
                // are being destroyed.
                child.SetParent(null);
                Object.Destroy(child.gameObject);
            }
        }

        public static void DestroyChildrenImmediate(this Transform parent) {
            while (parent.childCount > 0) {
                Object.DestroyImmediate(parent.GetChild(0).gameObject);
            }
        }
    }
}
