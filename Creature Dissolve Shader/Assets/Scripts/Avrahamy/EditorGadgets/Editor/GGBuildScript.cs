using BitStrap;
using UnityEngine;

namespace Avrahamy.EditorGadgets {
    [CreateAssetMenu(menuName = "Avrahamy/Build/GG Build Script", fileName = "GGBuildScript")]
    public class GGBuildScript : UMakeBuildAction {
        public enum BuildType {
            Release,
            Demo,
            Beta,
        }

        public BuildType buildType;

        public override void Execute(UMake umake, UMakeTarget target) {
            Debug.Log("Setting define symbols");
            ScriptDefinesHelper.SetDefined( target.buildTarget, "DEBUG_LOG", false);
            ScriptDefinesHelper.SetDefined( target.buildTarget, "DEBUG_DRAW", false);
            ScriptDefinesHelper.SetDefined( target.buildTarget, "GG_CONSOLE", false);
            ScriptDefinesHelper.SetDefined( target.buildTarget, "GG_CONSOLE_LOCKED", true);
            ScriptDefinesHelper.SetDefined( target.buildTarget, "GG_DEMO", buildType == BuildType.Demo);
            ScriptDefinesHelper.SetDefined( target.buildTarget, "GG_BETA", buildType == BuildType.Beta);
            Debug.Log("Done setting define symbols");
        }
    }
}
