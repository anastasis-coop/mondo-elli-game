using System.Collections.Generic;
using System.Text;
using Game;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CodingPath))]
public class CodingPathEditor : Editor
{
    private Vector3Int[] directions = { Vector3Int.forward, Vector3Int.back, Vector3Int.left, Vector3Int.right };
    private string[] options = { "▲ (North)", "▼ (South)", "◀ (West)", "▶ (East)" };

    private bool sceneValid = false;

    public override void OnInspectorGUI()
    {
        CodingPath path = (CodingPath)target;

        EditorGUI.BeginChangeCheck();

        int selectedIndex = ArrayUtility.IndexOf(directions, path.StartDirection);

        selectedIndex = EditorGUILayout.Popup("Start Direction", selectedIndex, options);

        if (EditorGUI.EndChangeCheck())
        {
            var property = serializedObject.FindProperty(nameof(path.StartDirection));
            property.vector3IntValue = directions[selectedIndex];
        }

        var suggestedPath = serializedObject.FindProperty(nameof(path.SuggestedPath));

        EditorGUILayout.PropertyField(suggestedPath);

        var minSubPathCount = serializedObject.FindProperty(nameof(path.MinSubPathCount));

        EditorGUILayout.PropertyField(minSubPathCount);

        if (GUILayout.Button("Add Selected"))
        {
            Transform selectedTransform = Selection.activeTransform;

            if (selectedTransform != null)
            {
                int index = suggestedPath.arraySize;

                suggestedPath.InsertArrayElementAtIndex(index);
                suggestedPath.GetArrayElementAtIndex(index).vector3Value = selectedTransform.position;
            }
        }

        serializedObject.ApplyModifiedProperties();

        try
        {
            if (path.StartDirection.magnitude != 1)
                throw new System.Exception("Select a Start Direction");

            if (path.Count < 2)
                throw new System.Exception("Path needs at least two points to be valid.");

            if (!sceneValid)
                throw new System.Exception("Could not find valid path block in scene.");

            const int MAX_ARROWS = 7;

            List<ArrowState> arrows = CodingUtility.CodingPathToArrowStates(path);
            int arrowCount = arrows.Count;
            string info = $"Arrows ({arrowCount}): {CodingUtility.ArrowStatesToString(arrows)}";

            if (arrowCount > MAX_ARROWS)
            {
                string warning = $"\nArrow count is over {MAX_ARROWS}, the path might be subdivided for some players.";
                EditorGUILayout.HelpBox(info + warning, MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox(info, MessageType.Info);
            }
        }
        catch (System.Exception e)
        {
            EditorGUILayout.HelpBox(e.Message, MessageType.Error);
        }
    }

    private void OnEnable() => SceneView.duringSceneGui += OnDuringSceneGui;

    // OnSceneGUI is not called for ScriptableObject Editors
    private void OnDuringSceneGui(SceneView sceneView)
    {
        CodingPath path = (CodingPath)target;

        sceneValid = path.Start != null && path.End != null;

        if (!sceneValid) return;

        Handles.DrawAAPolyLine(10, path.SuggestedPath);

        foreach (Vector3 point in path.SuggestedPath)
        {
            bool pointValid = Physics.Raycast(point + Vector3.up * 3, Vector3.down, out RaycastHit hit, 20)
                && hit.transform.gameObject.CompareTag("PathBlock");

            Handles.color = pointValid ? Color.white : Color.red;

            Handles.SphereHandleCap(0, point, Quaternion.identity, 1, EventType.Repaint);

            sceneValid &= pointValid;
        }
    }

    private void OnDisable() => SceneView.duringSceneGui -= OnDuringSceneGui;
}
