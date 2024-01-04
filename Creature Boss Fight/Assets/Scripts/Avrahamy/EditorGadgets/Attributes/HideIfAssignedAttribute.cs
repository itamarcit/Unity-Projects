using System;

namespace Avrahamy.EditorGadgets {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public class HideIfAssignedAttribute : CompoundAttributeBase {
        // Empty on purpose.
    }
}