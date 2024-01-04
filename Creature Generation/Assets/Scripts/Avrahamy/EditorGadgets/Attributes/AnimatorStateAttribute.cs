namespace Avrahamy.EditorGadgets {
    public class AnimatorStateAttribute : CompoundAttributeBase {
        public readonly string animatorFieldName;

        public AnimatorStateAttribute(string animatorFieldName = "animatorComponent") {
            this.animatorFieldName = animatorFieldName;
        }
    }
}
