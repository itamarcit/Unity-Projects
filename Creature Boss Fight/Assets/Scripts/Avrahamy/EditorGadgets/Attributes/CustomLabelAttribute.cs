namespace Avrahamy.EditorGadgets {
    public class CustomLabelAttribute : CompoundAttributeBase {
        public readonly string labelText;

        public CustomLabelAttribute(string labelText) {
            this.labelText = labelText;
        }
    }
}
