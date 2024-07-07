using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class ImportTimelineWizard : ScriptableWizard
{
    [Header("Imported")]
    public TextAsset timeline;
    public GameObject model;

    [MenuItem("Tools/Import timeline")]
    private static void Open()
    {
        var wizard = DisplayWizard<ImportTimelineWizard>(nameof(ImportTimelineWizard), "Import");
        wizard.OnValidate();
    }

    private void OnValidate()
    {
        isValid = timeline && model;
    }

    private void OnWizardCreate()
    {
        Import(AssetDatabase.GetAssetPath(model),
            AssetDatabase.GetAssetPath(timeline));
    }

    static void Import(string modelPath, string timelinePath)
    {
        EditorUtility.DisplayProgressBar(nameof(Import), "Loading assets", 0.1f);

        var modelImporter = AssetImporter.GetAtPath(modelPath) as ModelImporter;
        var clipImporters = new List<ModelImporterClipAnimation>();

        var timeline = AssetDatabase.LoadAssetAtPath<TextAsset>(timelinePath);
        var lines = timeline.text.Split('\n');

        EditorUtility.DisplayProgressBar(nameof(Import), "Importing clips", 0.25f);

        var invalidLines = new StringBuilder();

        for (int i = 0; i < lines.Length; i++)
        {
            EditorUtility.DisplayProgressBar(nameof(Import), "Importing clips", Mathf.Lerp(0.25f, 1, i / (float)lines.Length));

            // Removing invalid characters
            var clean = Regex.Replace(lines[i], @"[^a-zA-Z0-9:=_\/-]", "");

            if (string.IsNullOrEmpty(clean) || lines[i].StartsWith("//"))
            {
                continue;
            }

            // The correct line format is: start-end=name but sometimes = is :
            var match = Regex.Match(clean, @"^(?<start>\d+)(-(?<end>\d+))?(=|:)(?<name>.*)$");

            if (!match.Success)
            {
                invalidLines.AppendLine(lines[i]);
                continue;
            }

            var start = int.Parse(match.Groups["start"].Value);
            var end = match.Groups["end"].Success ? int.Parse(match.Groups["end"].Value) : start;
            var name = match.Groups["name"].Value;

            var clip = Array.Find(modelImporter.clipAnimations, c => c.name == name);

            if (clip == null)
            {
                clip = new ModelImporterClipAnimation();
            }

            clip.firstFrame = start;
            clip.lastFrame = end;
            clip.name = name;
            clip.lockRootHeightY = true;
            clip.lockRootPositionXZ = true;
            clip.lockRootRotation = true;
            clip.keepOriginalOrientation = true;
            clip.keepOriginalPositionXZ = true;
            clip.keepOriginalPositionY = true;
            clip.loopTime = name.ToLower().Contains("loop") || name.ToLower().Contains("idle");
            clip.loop = clip.loopTime;

            clipImporters.Add(clip);
        }

        modelImporter.clipAnimations = clipImporters.ToArray();
        modelImporter.SaveAndReimport();
        EditorUtility.ClearProgressBar();

        string errors = invalidLines.ToString();

        if (!string.IsNullOrEmpty(errors))
        {
            EditorUtility.DisplayDialog("Could not parse these lines:", errors, "Ok");
        }

        AssetDatabase.Refresh();

    }
}
