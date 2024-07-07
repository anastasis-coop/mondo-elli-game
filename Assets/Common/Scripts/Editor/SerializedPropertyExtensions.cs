using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

//Origin: https://github.com/ACCIAI0/acciaio
namespace Acciaio.Editor.Extensions
{
    public static class SerializedPropertyExtensions
    {
		private const BindingFlags InstanceAny = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
        private const string PathElementArray = "Array";

		private static void SetDefaultConstructorValue(SerializedProperty property)
		{
			var parent = GetObject(property, true);
			var field = parent.GetType().GetField(property.name,  InstanceAny);
			field.SetValue(parent, Activator.CreateInstance(field.FieldType, true));
		}

        private static object GetObject(SerializedProperty property, bool stopAtParent)
        {
            var sObj = property.serializedObject;
            sObj.ApplyModifiedProperties();
            object @object = sObj.targetObject;

            var elements = property.propertyPath.Split('.');
            for (var i = 0; i < elements.Length - Convert.ToInt32(stopAtParent); i++)
            {
                var pathElement = elements[i];

                if (pathElement == PathElementArray)
                {
	                if (i + 1 >= elements.Length || elements[i + 1] == "size") return null;
	                
                    var index = int.Parse(elements[i + 1].Replace("data[", "").Replace("]", ""));
                    var j = -1;
                    var enumerator = ((IEnumerable)@object).GetEnumerator();
                    while(j < index && enumerator.MoveNext()) j++;
                    if (j == index) @object = enumerator.Current;
                    else 
                    {
                        Debug.LogError("Serialized object is referencing an item outside of a collection's size");
                        return null;
                    }
                    i++;
                }
                else
                {
					var type = @object.GetType();
					FieldInfo field;
					do
					{
						field = type.GetField(pathElement, InstanceAny);
						if (field == null) type = type.BaseType;
					} while (field == null && type != typeof(object));
                    @object = field?.GetValue(@object);
                }

                if (@object == null) return null;
            }
            return @object;
        }

		public static Type GetPropertyType(this SerializedProperty property) => GetObject(property, false)?.GetType();

        public static T GetValue<T>(this SerializedProperty property)
        {
            var @object = GetObject(property, false);
            if (@object is T tObject) return tObject;
            return default;
        }
    }
}
