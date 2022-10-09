using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;


public class BuildScript
{
	static string s_appName = Application.productName;
	static string s_outDir = "../Out";

	static string[] SCENES = FindEnabledEditorScenes();


	[MenuItem("Game/Build")]
	static void Build()
	{
		string path = Path.GetFullPath($"{Application.dataPath}/{s_outDir}");
		Debug.Log($"Build Path : {path}");


		// 버전에 날자+시간 넣자
		var nowTime = System.DateTime.Now;
		string timestamp = nowTime.ToString("yyMMdd_hhmmss");

		PlayerSettings.bundleVersion = $"{Application.version}.{timestamp}";

		GenericBuild(SCENES, $"{path}/{s_appName}.exe", BuildTarget.StandaloneWindows, BuildOptions.None);
	}


	static void GenericBuild(string[] scenes, string target_dir, BuildTarget build_target, BuildOptions build_options)
	{
		//EditorUserBuildSettings.SwitchActiveBuildTarget(build_target);
		BuildReport report = BuildPipeline.BuildPlayer(scenes, target_dir, build_target, build_options);
		BuildResult ret = report.summary.result;

		if (ret != BuildResult.Succeeded)
		{
			// 이유는 나중에 파싱.
			throw new Exception($"BuildPlayer failure.");
		}
	}


	private static string[] FindEnabledEditorScenes()
	{
		List<string> EditorScenes = new List<string>();
		foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
		{
			if (!scene.enabled) continue;
			EditorScenes.Add(scene.path);
		}
		return EditorScenes.ToArray();
	}


}



