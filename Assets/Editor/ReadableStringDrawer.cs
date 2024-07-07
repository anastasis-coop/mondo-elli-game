using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUIUtility;

namespace Room9
{

    [CustomPropertyDrawer(typeof(ReadableString))]
    public class ReadableStringDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            const int MAX_LENGTH = 50;

            if (label.text.Length > MAX_LENGTH)
                label.text = label.text[..MAX_LENGTH] + "...\"";

            Rect propertyRect = position;

            EditorGUI.PropertyField(propertyRect, property, label, true);
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
    }

}