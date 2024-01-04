using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Diagnostics;
using System.IO;
using BitStrap;

namespace Avrahamy.EditorGadgets {
    [CreateAssetMenu(menuName = "Avrahamy/Build/GG Post Build Script", fileName = "GGPostBuildScript")]
    public class GGPostBuildScript : UMakeBuildAction {
        public string appName = "Spiritfall";
        public bool waitForZipToFinish = true;

        public override void Execute(UMake umake, UMakeTarget target) {
            Debug.Log($"Running {name} for {target.name}");
            var buildPath = UMake.GetBuildPath();
            var path = target.GetTargetPath(umake.version, buildPath);
            string pathToArchive;
            if (target.buildTarget == BuildTarget.StandaloneOSX) {
                pathToArchive = PrepareMacOSBuild(path);
            } else {
                pathToArchive = PrepareWindowsBuild(path);
            }

            var currentDirectory = Directory.GetCurrentDirectory();
            var originalDirectoryPath = Path.GetRelativePath(currentDirectory, path.directoryPath);
            //Debug.Log($"currentDirectory = {currentDirectory}\npath.directoryPath = {path.directoryPath}\noriginalDirectoryPath = {originalDirectoryPath}");
            var originalExePath = Path.GetRelativePath(currentDirectory, path.path);

            var targetWithoutExt = Path.GetFileNameWithoutExtension(path.targetName);
            var archivePath = target.buildTarget == BuildTarget.StandaloneOSX
                ? $"{originalExePath}.zip"
                : Path.Combine(originalDirectoryPath, "..", $"{targetWithoutExt}.zip");

            var zipProcess = new Process();
#if UNITY_EDITOR_WIN
            zipProcess.StartInfo.FileName = "tar.exe";
            zipProcess.StartInfo.Arguments = $"-cf \"{archivePath}\" \"{pathToArchive}\"";
#elif UNITY_EDITOR_OSX
            zipProcess.StartInfo.FileName = "zip";
            zipProcess.StartInfo.Arguments = $"-r -X \"{archivePath}\" \"{pathToArchive}\"";
#endif

            Debug.Log($"Executing {zipProcess.StartInfo.FileName} {zipProcess.StartInfo.Arguments}");
            zipProcess.OutputDataReceived += ( sender, msg ) => {
                if( msg != null )
                    Debug.Log( msg.Data );
            };
            zipProcess.Start();
            zipProcess.BeginOutputReadLine();

            Debug.Log("Restoring define symbols");
            ScriptDefinesHelper.SetDefined( target.buildTarget, "DEBUG_LOG", true);
            ScriptDefinesHelper.SetDefined( target.buildTarget, "DEBUG_DRAW", true);
            ScriptDefinesHelper.SetDefined( target.buildTarget, "GG_CONSOLE", true);
            ScriptDefinesHelper.SetDefined( target.buildTarget, "GG_CONSOLE_LOCKED", false);

            if (waitForZipToFinish) {
                zipProcess.WaitForExit();
                Debug.Log("ZIP ready");
            }
        }

        public string PrepareWindowsBuild(UMakeTarget.Path path) {
            /*
             * currentDirectory = C:\Users\nadav\OneDrive\Documents\Repositories\Yoyo
             * path.directoryPath = C:/Users/nadav/OneDrive/Documents/CodenameGame Docs/Builds\WindowsDemo
             */
            var newPath = Path.Combine(path.directoryPath, $"{appName}.exe");
            var exeName = newPath;
            if (File.Exists(newPath)) {
                Debug.Log($"EXE {newPath} already exists");
            } else if (File.Exists(path.path)) {
                Debug.Log($"Renaming EXE {path.path} to {newPath}");
                File.Move(path.path, newPath);
            }

            var targetWithoutExt = Path.GetFileNameWithoutExtension(path.targetName);
            var baseName = Path.Combine(path.directoryPath, targetWithoutExt);
            newPath = Path.Combine(path.directoryPath, $"{appName}_Data");
            if (Directory.Exists(newPath)) {
                Debug.Log($"Directory {newPath} already exists");
            } else if (Directory.Exists($"{baseName}_Data")) {
                Debug.Log($"Renaming directory {baseName}_Data to {newPath}");
                Directory.Move($"{baseName}_Data", newPath);
            }

            newPath = Path.Combine(path.directoryPath, "..", $"{targetWithoutExt}_BurstDebugInformation_DoNotShip");
            if (!Directory.Exists($"{baseName}_BurstDebugInformation_DoNotShip")) {
                Debug.LogError($"Directory {baseName}_BurstDebugInformation_DoNotShip does not exist");
            } else {
                Debug.Log($"Moving directory {baseName}_BurstDebugInformation_DoNotShip to {newPath}");
                Directory.Move($"{baseName}_BurstDebugInformation_DoNotShip", newPath);
            }

            var currentDirectory = Directory.GetCurrentDirectory();
            var originalDirectoryPath = Path.GetRelativePath(currentDirectory, path.directoryPath);
            return $"{Path.Combine(originalDirectoryPath, "MonoBleedingEdge")}\" "
                   + $"\"{Path.Combine(originalDirectoryPath, $"{appName}_Data")}\" "
                   + $"\"{Path.GetRelativePath(currentDirectory, exeName)}\" "
                   + $"\"{Path.Combine(originalDirectoryPath, "UnityCrashHandler64.exe")}\" "
                   + $"\"{Path.Combine(originalDirectoryPath, "UnityPlayer.dll")}";
        }

        public string PrepareMacOSBuild(UMakeTarget.Path path) {
            var exeName = $"{appName}.app";
            var currentDirectory = Directory.GetCurrentDirectory();
            var originalDirectoryPath = Path.GetRelativePath(currentDirectory, path.directoryPath);
            var newExePath = Path.Combine(originalDirectoryPath, exeName);
            var originalExePath = Path.GetRelativePath(currentDirectory, path.path);
            Debug.Log($"Renaming {originalExePath} to {newExePath}");
            if (!Directory.Exists(originalExePath)) {
                Debug.LogError($"App {originalExePath} not found");
                return null;
            }
            Directory.Move(originalExePath, newExePath);
            return newExePath;
        }
    }
}
