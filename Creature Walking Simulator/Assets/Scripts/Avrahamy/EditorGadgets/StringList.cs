using UnityEngine;

namespace Avrahamy.EditorGadgets {
    /// <summary>
    /// These lists can be used to populate [StringListDropdown] attributes.
    /// </summary>
    [CreateAssetMenu(menuName = "Avrahamy/Config/String List", fileName = "StringList")]
    public class StringList : ScriptableObject, IStringList {
        [SerializeField] string[] values;

        public string[] Values {
            get {
                return values;
            }
        }
    }
}
