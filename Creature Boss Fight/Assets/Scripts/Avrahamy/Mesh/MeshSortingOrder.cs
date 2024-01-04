using UnityEngine;
using Avrahamy.EditorGadgets;

namespace Avrahamy.Meshes {
    [RequireComponent(typeof(Renderer))]
    public class MeshSortingOrder : MonoBehaviour {
        [SortingLayer]
        [SerializeField] int sortingLayer;
        [SerializeField] int sortingOrder;

        protected void Awake() {
            var rendererComponent = GetComponent<Renderer>();
            rendererComponent.sortingLayerID = sortingLayer;
            rendererComponent.sortingOrder = sortingOrder;
        }
    }
}
