using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Acciaio.Editor;
using Acciaio.Editor.Extensions;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Common.Scripts
{
    public static class Tools
    {
        private static string GetRoot(bool askUser = true)
        {
            const string rootKey = "TOOLS.ANASTASIS.CONFIGS_TO_BIG_ELLO_MESSAGES.ROOT";

            var root = EditorPrefs.GetString(rootKey) ?? Application.dataPath;

            if (!askUser) return root;
            
            var lastNodeIndex = root.LastIndexOf("/", StringComparison.Ordinal) + 1;
            var folderName = root[lastNodeIndex..];
            root = EditorUtility.SaveFolderPanel("Select Root Folder", root[..lastNodeIndex], folderName);
            if (!string.IsNullOrEmpty(root)) EditorPrefs.SetString(rootKey, root);

            return root;
        }
        
        private static BigElloMessage AnalyzeProperty(string root, MonoBehaviour parent, SerializedProperty property)
        {
            var value = property.GetValue<BigElloSays.Config>();
            if (value == null) return null;

            var result = BigElloMessage.Create(value.Message, value.LocalizedAudio, value.AnimationTrigger, value.LipSync,
                value.ClickDisabled, value.FinishWithAudio, value.SkipAtFirstRun, value.ObscureEverythingButMessage);

            var fileName = property.propertyPath.Replace("[", "_").Replace("]", "_").Replace("Array.", "");
            var path = $"{root}/{fileName}.asset";
            if (!Directory.Exists(root)) Directory.CreateDirectory(root);
            AssetDatabase.CreateAsset(result, path.Replace(Application.dataPath, "Assets").Trim());
            return result;
        }

        private static void AnalyzeObject(string root, GameObject obj)
        {
            var pathToObject = $"{root}/{obj.name.Trim()}";
            foreach (var component in obj.GetComponents<MonoBehaviour>())
            {
                var serialized = new SerializedObject(component);

                var property = serialized.GetIterator();
                while (property.Next(true)) AnalyzeProperty(pathToObject, component, property);
            }
            
            foreach (Transform t in obj.transform) AnalyzeObject(pathToObject, t.gameObject);
        }

        private static void AnalyzeScene(string root, SceneAsset asset)
        {
            var pathToScene = $"{root}/{asset.name}";
            
            var scene = SceneManager.GetActiveScene().name == asset.name ? SceneManager.GetActiveScene() : 
                    EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(asset), OpenSceneMode.Additive);

            foreach (var obj in scene.GetRootGameObjects())
            {
                AnalyzeObject(pathToScene, obj);
            }

            if (SceneManager.GetActiveScene().name != asset.name) EditorSceneManager.CloseScene(scene, true);
        }

        public static SerializedProperty FindOther(SerializedProperty property)
        {
            var tokens = property.propertyPath.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var builderUnderscore = new StringBuilder();
            var builderUpper = new StringBuilder();
            var builderLower = new StringBuilder();

            for (var i = 0; i < tokens.Length; i++)
            {
                builderUnderscore.Clear();
                builderUpper.Clear();
                builderLower.Clear();
                
                for (var j = 0; j < tokens.Length; j++)
                {
                    var token = tokens[j];
                    if (i == j)
                    {
                        builderUnderscore.Append("_" + token).Append('.');
                        builderUpper.Append(char.ToUpper(token[0]) + token[1..]).Append('.');
                        builderLower.Append(char.ToLower(token[0]) + token[1..]).Append('.');
                    }
                    else
                    {
                        builderUnderscore.Append(token).Append('.');
                        builderUpper.Append(token).Append('.');
                        builderLower.Append(token).Append('.');
                    }
                }
                
                builderUnderscore.Remove(builderUnderscore.Length - 1, 1);
                builderUpper.Remove(builderUpper.Length - 1, 1);
                builderLower.Remove(builderLower.Length - 1, 1);
                
                var other = property.serializedObject.FindProperty(builderUnderscore.ToString());
                if (other != null && other.propertyPath != property.propertyPath) return other;
                other = property.serializedObject.FindProperty(builderUpper.ToString());
                if (other != null && other.propertyPath != property.propertyPath) return other;
                other = property.serializedObject.FindProperty(builderLower.ToString());
                if (other != null && other.propertyPath != property.propertyPath) return other;
            }
            
            return null;
        }

        public static void MigrateConfig(SerializedObject obj, SerializedProperty property)
        {
            static string GetObjectPath(GameObject obj) 
                => obj.transform.parent == null ? obj.name : $"{GetObjectPath(obj.transform.parent.gameObject)}/{obj.name}";

            var other = FindOther(property);
            if (other == null || other.GetPropertyType() != typeof(BigElloSaysConfig))
            {
                Debug.LogError($"Property {property.propertyPath} must be converted, but no compatible container was found");
                return;
            }

            var oldV = property.GetValue<BigElloSays.Config>();
            var newV = other.GetValue<BigElloSaysConfig>();

            newV.HighlightObject = oldV.HighlightObject;
            newV.GameobjectsToDeactivate = oldV.GameobjectsToDeactivate.ToList();
            newV.GameObjectsToActivate = oldV.GameObjectsToActivate.ToList();
            newV.MessagePosition = oldV.MessagePosition;
            newV.BigElloPosition = oldV.BigElloPosition;
            newV.ShowGreenButton = oldV.ShowGreenButton;
            newV.ShowRedButton = oldV.ShowRedButton;
            newV.ShowYellowButton = oldV.ShowYellowButton;

            var gameObject = (obj.targetObject as MonoBehaviour).gameObject;
            var pathForFile = property.propertyPath.Replace(".Array", "")
                    .Replace("[", "_")
                    .Replace("]", "_");
            var root = $"{GetRoot(false)}/{GetObjectPath(gameObject)}/{gameObject.name.Trim()}";
            var filePath = $"{root}/{pathForFile}";

            var asset = AssetDatabase.LoadAssetAtPath<BigElloMessage>(filePath.Replace(Application.dataPath, "Assets"));
            if (asset == null)
                asset = AnalyzeProperty(root, property.serializedObject.targetObject as MonoBehaviour, property);

            newV.Message = asset;

            obj.ApplyModifiedProperties();
        }
        
        [MenuItem ("CONTEXT/MonoBehaviour/Migrate Big Ello Configs")]
        public static void MigrateBigElloConfigs(MenuCommand command)
        {
            var serialized = new SerializedObject(command.context);
            serialized.Update();

            var property = serialized.GetIterator();
            
            if (!property.Next(true)) return;
            
            do
            {
                var type = property.GetPropertyType();
                if (type == typeof(BigElloSays.Config))
                {
                    MigrateConfig(serialized, property);
                }
                else if (typeof(IEnumerable<BigElloSays.Config>).IsAssignableFrom(type))
                {
                    var other = FindOther(property);
                    if (other != null && other.isArray) other.arraySize = property.arraySize;
                    else
                    {
                        Debug.LogError(
                            $"Property{property.propertyPath} is an array to be converted, but no compatible container was found");
                        continue;
                    }

                    for (var i = 0; i < property.arraySize; i++)
                    {
                        MigrateConfig(serialized, property.GetArrayElementAtIndex(i));
                    }
                }
            } while (property.Next(false));
            
            serialized.ApplyModifiedProperties();
        }
        
        [MenuItem("Tools/Anastasis/Collect Configs In ACTIVE Scene In Big Ello Messages")]
        public static void ConfigsToBigElloMessagesActive()
        {
            AnalyzeScene(GetRoot(), AssetDatabase.LoadAssetAtPath<SceneAsset>(SceneManager.GetActiveScene().path));
            AssetDatabase.Refresh();
        }
        
        [MenuItem("Tools/Anastasis/Collect Configs In Big Ello Messages")]
        public static void ConfigsToBigElloMessages()
        {
            var rootFolder = GetRoot();

            if (!rootFolder.Contains(Application.dataPath))
            {
                Debug.LogError("Cannot save outside the Assets folder");
                return;
            }


            var scenes = AssetDatabase.FindAssets("t:Scene")
                    .Select(s => AssetDatabase.LoadAssetAtPath<SceneAsset>(AssetDatabase.GUIDToAssetPath(s)))
                    .ToList();
            
            scenes.ForEach(s => AnalyzeScene(rootFolder, s));
            
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Assign Audio Content to Big Ello Messages")]
        public static void AssignAudioToBigElloMessages()
        {
            string GetKeyFrom(BigElloMessage message)
            {
                var collection = LocalizationEditorSettings.GetStringTableCollection(message.Message.TableReference);
                return collection == null ? null : message.Message.TableEntryReference.ResolveKeyName(collection.SharedData);
            }
            
            var folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            var audioClips = AssetDatabase.FindAssets($"t:{nameof(AudioClip)}", new[] { folderPath })
                .Select(guid => AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToList();

            var messagesRoot = GetRoot(false);
            if (string.IsNullOrEmpty(messagesRoot)) messagesRoot = GetRoot();
            messagesRoot = "Assets" + messagesRoot.Replace(Application.dataPath, "");

            var messages = AssetDatabase.FindAssets($"t:{nameof(BigElloMessage)}", new [] { messagesRoot })
                .Select(guid => AssetDatabase.LoadAssetAtPath<BigElloMessage>(AssetDatabase.GUIDToAssetPath(guid)))
                .GroupBy(GetKeyFrom)
                .Where(g => g.Key != null)
                .ToDictionary(group => group.Key, msg => msg.ToList());
            
            var audioProperty = typeof(BigElloMessage).GetProperty("Audio",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            var csvFile = AssetDatabase.LoadAssetAtPath<TextAsset>($"{folderPath}/csv.csv");
            var duplicatesDictionary = new Dictionary<string, string>();
            if (csvFile != null)
            {
                var csv = Csv.WithFirstLineAsHeaders(true).Parse(csvFile.text);
                foreach (var value in csv.GetRows())
                {
                    var cell = value["Duplicato di"];
                    if (string.IsNullOrEmpty(cell.StringValue)) continue;
                    duplicatesDictionary.Add(value[0].StringValue, cell.StringValue);
                }
            }
            
            foreach (var clip in audioClips)
            {
                if (!messages.TryGetValue(clip.name, out var msgs)) continue;
                foreach (var msg in msgs)
                {
                    var audio = clip;
                    while (duplicatesDictionary.ContainsKey(audio.name))
                        audio = audioClips.Find(c => c.name == duplicatesDictionary[audio.name]);

                    audioProperty.SetValue(msg, audio);
                    EditorUtility.SetDirty(msg);
                    AssetDatabase.SaveAssetIfDirty(msg);
                }
            }
        }

        [MenuItem("Assets/Assign Audio Content to Big Ello Messages", true)]
        public static bool EnableAssignAudioToBigElloMessages()
        {
            var path = Application.dataPath[..(Application.dataPath.LastIndexOf("/", StringComparison.Ordinal) + 1)];
            path += AssetDatabase.GetAssetPath(Selection.activeObject);
            return Directory.Exists(path);
        }
    }
}