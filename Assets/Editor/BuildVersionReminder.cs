using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;

public class BuildVersionReminder : IPreprocessBuildWithReport
{
    private const string prefKey = nameof(BuildVersionReminder);

    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) =>
        EditorPrefs.SetString(prefKey, Application.version);

    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (EditorPrefs.HasKey(prefKey) && EditorPrefs.GetString(prefKey) == Application.version
               && !EditorUtility.DisplayDialog($"Warning ({nameof(BuildVersionReminder)})",
                   $"You have already used the build version {Application.version}, did you mean to do that?", "Yes", "No"))
        {
            throw new BuildFailedException("Build was canceled by the user.");
        }
    }
}
