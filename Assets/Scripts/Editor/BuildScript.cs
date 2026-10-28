using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SlotGame.EditorTools
{
    /// <summary>
    /// Command-line / menu builds. Batch mode:
    /// Unity -batchmode -quit -projectPath . -buildTarget Android -executeMethod SlotGame.EditorTools.BuildScript.BuildAndroidApk
    /// Output goes to Builds/Android (ignored by git).
    /// </summary>
    public static class BuildScript
    {
        private const string Scene = "Assets/Scenes/Classic3x5.unity";

        [MenuItem("SlotGame/Build/Android APK")]
        public static void BuildAndroidApk()
        {
            Build("Builds/Android/ClassicSlots3x5.apk", false);
        }

        [MenuItem("SlotGame/Build/Android App Bundle (AAB)")]
        public static void BuildAndroidBundle()
        {
            Build("Builds/Android/ClassicSlots3x5.aab", true);
        }

        private static void Build(string path, bool bundle)
        {
            ProjectSetup.Apply();
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            EditorUserBuildSettings.buildAppBundle = bundle;
            EditorUserBuildSettings.androidBuildType = AndroidBuildType.Release;

            var options = new BuildPlayerOptions
            {
                scenes = new[] { Scene },
                locationPathName = path,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None,
            };

            BuildSummary summary = BuildPipeline.BuildPlayer(options).summary;
            Debug.Log("Build " + summary.result + ": " + summary.outputPath + " (" + (summary.totalSize / 1024 / 1024) + " MB, " + summary.totalTime + ")");

            if (summary.result != BuildResult.Succeeded && Application.isBatchMode)
                EditorApplication.Exit(1);
        }
    }
}
