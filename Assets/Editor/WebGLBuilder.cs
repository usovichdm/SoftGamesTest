using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SoftGames.EditorTools
{
    public static class WebGLBuilder
    {
        private const string OutputPath = "Builds/WebGL";

        [MenuItem("SoftGames/Build WebGL")]
        public static void BuildFromMenu()
        {
            var result = Build();
            if (result != BuildResult.Succeeded)
            {
                throw new System.Exception("WebGL build failed: " + result);
            }

            Debug.Log("WebGL build succeeded → " + Path.GetFullPath(OutputPath));
        }

        /// <summary>
        /// Batchmode entry: Unity -batchmode -quit -executeMethod SoftGames.EditorTools.WebGLBuilder.BuildCli
        /// </summary>
        public static void BuildCli()
        {
            var result = Build();
            if (result != BuildResult.Succeeded)
            {
                EditorApplication.Exit(1);
                return;
            }

            EditorApplication.Exit(0);
        }

        public static BuildResult Build()
        {
            var outDir = Path.GetFullPath(OutputPath);
            if (Directory.Exists(outDir))
            {
                Directory.Delete(outDir, true);
            }

            Directory.CreateDirectory(outDir);

            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;

            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outDir,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"WebGL build: {report.summary.result}, size={report.summary.totalSize}");
            return report.summary.result;
        }
    }
}
