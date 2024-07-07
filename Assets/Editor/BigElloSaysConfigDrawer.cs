using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Tables;
using static UnityEditor.EditorGUIUtility;

[CustomPropertyDrawer(typeof(BigElloSays.Config))]
public class BigElloSaysConfigDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var parent = GetParent(property);

        if (parent != null && parent.isArray)
        {
            var message = property.FindPropertyRelative(nameof(BigElloSays.Config.Message));
            string localizedString = GetLocalizedString(message);

            if (!string.IsNullOrEmpty(localizedString))
            {
                label.text += ": " + localizedString;
            }
        }

        Rect propertyRect = position;

        EditorGUI.PropertyField(propertyRect, property, label, true);

        if (property.isExpanded)
        {
            Rect buttonRect = propertyRect;
            buttonRect.y += EditorGUI.GetPropertyHeight(property, label) + standardVerticalSpacing;
            buttonRect.width /= 2;
            buttonRect.height = singleLineHeight + standardVerticalSpacing;

            if (GUI.Button(buttonRect, "Position: Scene → Config"))
            {
                CopyBigElloPos(property);
                CopyMessagePos(property);
            }

            buttonRect.x += buttonRect.width;

            if (GUI.Button(buttonRect, "Position: Config → Scene (!)"))
            {
                SetBigElloPos(property);
                SetMessagePos(property);
            }

            position.height += buttonRect.height;
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUI.GetPropertyHeight(property, label);

        if (property.isExpanded)
        {
            height += singleLineHeight + standardVerticalSpacing * 2;
        }

        return height;
    }

    private void CopyBigElloPos(SerializedProperty property)
    {
        BigElloSays instance = Object.FindObjectOfType<BigElloSays>();

        if (instance == null)
        {
            Debug.LogError("Could not find an instance of BigElloSays");
            return;
        }

        const string rootName = nameof(BigElloSays.root);

        var root = new SerializedObject(instance).FindProperty(rootName).objectReferenceValue as GameObject;

        if (root != null)
        {
            const string positionName = nameof(BigElloSays.Config.BigElloPosition);

            SerializedProperty position = property.FindPropertyRelative(positionName);
            position.vector3Value = root.transform.localPosition;

            property.serializedObject.ApplyModifiedProperties();
        }
    }

    private void CopyMessagePos(SerializedProperty property)
    {
        BigElloSays instance = Object.FindObjectOfType<BigElloSays>();

        if (instance == null)
        {
            Debug.LogError("Could not find an instance of BigElloSays");
            return;
        }

        const string messageRootName = nameof(BigElloSays.messageRoot);

        var root = new SerializedObject(instance).FindProperty(messageRootName).objectReferenceValue as GameObject;

        if (root != null)
        {
            const string messagePositionName = nameof(BigElloSays.Config.MessagePosition);

            SerializedProperty position = property.FindPropertyRelative(messagePositionName);
            position.vector3Value = root.transform.localPosition;

            property.serializedObject.ApplyModifiedProperties();
        }
    }

    private void SetBigElloPos(SerializedProperty property)
    {
        BigElloSays instance = Object.FindObjectOfType<BigElloSays>();

        if (instance == null)
        {
            Debug.LogError("Could not find an instance of BigElloSays");
            return;
        }

        const string rootName = nameof(BigElloSays.root);

        var root = new SerializedObject(instance).FindProperty(rootName).objectReferenceValue as GameObject;

        if (root != null)
        {
            const string positionName = nameof(BigElloSays.Config.BigElloPosition);

            SerializedProperty position = property.FindPropertyRelative(positionName);

            Undo.RecordObject(root.transform, nameof(SetBigElloPos));

            root.transform.localPosition = position.vector3Value;

            EditorUtility.SetDirty(root.transform);
        }
    }

    private void SetMessagePos(SerializedProperty property)
    {
        BigElloSays instance = Object.FindObjectOfType<BigElloSays>();

        if (instance == null)
        {
            Debug.LogError("Could not find an instance of BigElloSays");
            return;
        }

        const string messageRootName = nameof(BigElloSays.messageRoot);

        var root = new SerializedObject(instance).FindProperty(messageRootName).objectReferenceValue as GameObject;

        if (root != null)
        {
            const string messagePositionName = nameof(BigElloSays.Config.MessagePosition);

            SerializedProperty position = property.FindPropertyRelative(messagePositionName);

            Undo.RecordObject(root.transform, nameof(SetMessagePos));

            root.transform.localPosition = position.vector3Value;

            EditorUtility.SetDirty(root.transform);
        }
    }

    public static SerializedProperty GetParent(SerializedProperty property)
    {
        if (!property.propertyPath.Contains('.'))
            return null;

        var parentPropertyPath = GetSubstringBeforeLast(property.propertyPath, '.');
        if (parentPropertyPath.EndsWith(".Array"))
        {
            parentPropertyPath = parentPropertyPath.Substring(0, parentPropertyPath.Length - 6);
        }

        if (string.IsNullOrEmpty(parentPropertyPath))
            return null;

        return property.serializedObject.FindProperty(parentPropertyPath);
    }

    public static string GetSubstringBeforeLast(string text, char character)
    {
        int lastCharIndex = text.LastIndexOf(character);
        return lastCharIndex == -1 ? text : text.Substring(0, lastCharIndex);
    }

    public static string GetLocalizedString(SerializedProperty property)
    {
        var collection = LocalizationEditorSettings.GetStringTableCollection("Strings");
        var table = collection.Tables[0].asset as StringTable;
        var key = property.FindPropertyRelative("m_TableEntryReference.m_KeyId").longValue;
        var entry = table?.GetEntry(key);

        return entry?.GetLocalizedString();
    }
}
