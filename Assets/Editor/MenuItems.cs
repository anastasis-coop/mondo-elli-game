using System.Collections.Generic;
using System.IO;
using System.Linq;
using AStar;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization.Tables;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class MenuItems
{
    private const string menu = "Tools/";

    //[MenuItem(menu + nameof(Foo))]
    //private static void Foo() { }

    [MenuItem(menu + nameof(MoveToAveragePositionOfChildren))]
    private static void MoveToAveragePositionOfChildren()
    {
        Transform selected = Selection.activeTransform;

        if (selected == null || selected.childCount == 0) return;

        Vector3 sum = Vector3.zero;

        foreach (Transform child in selected)
        {
            sum += child.position;
        }

        Vector3 average = sum / selected.childCount;
        Vector3 delta = selected.position - average;

        foreach (Transform child in selected)
        {
            Undo.RecordObject(child, nameof(MoveToAveragePositionOfChildren));

            child.position += delta;
        }

        Undo.RecordObject(selected, nameof(MoveToAveragePositionOfChildren));

        selected.position = average;
    }

    

    #region Scene shortcuts

    [MenuItem(menu + "Play From Start")]
    private static void PlayFromStart()
    {
        GoToScene(EditorBuildSettings.scenes[0].path);
        EditorApplication.EnterPlaymode();
    }

    const string gotoMenu = "Go To Scene/";
    
    private static void GoToScene(string path)
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
    }

    [MenuItem(gotoMenu + "Start")]
    private static void GoToStart() => GoToScene("Assets/Rooms/Start/Scenes/Start.unity");

    [MenuItem(gotoMenu + "RoomStart")]
    private static void GoToRoomStart() => GoToScene("Assets/Rooms/RoomStart/RoomStart.unity");

    [MenuItem(gotoMenu + "(Town) Coding")]
    private static void GoToTown() => GoToScene("Assets/Rooms/Game/Scenes/Town.unity");

    [MenuItem(gotoMenu + "(Cutscene) Video")]
    private static void GoToCutscene() => GoToScene("Assets/Cutscene/Scenes/Cutscene.unity");

    private const string ROOM_PATH = "Assets/Rooms/Room{0}/Scenes/Room{0}_Levels{1}.unity";

    [MenuItem(gotoMenu + "(Room 1) Interference Visual/Levels 1")]
    private static void GoToRoom1Levels1() => GoToScene(string.Format(ROOM_PATH, 1, 1));

    [MenuItem(gotoMenu + "(Room 1) Interference Visual/Levels 2")]
    private static void GoToRoom1Levels2() => GoToScene(string.Format(ROOM_PATH, 1, 2));

    [MenuItem(gotoMenu + "(Room 2) Interference Audio")]
    private static void GoToRoom2() => GoToScene(string.Format(ROOM_PATH, 2, ""));

    [MenuItem(gotoMenu + "(Room 3) Inhibition Visual")]
    private static void GoToRoom3() => GoToScene(string.Format(ROOM_PATH, 3, ""));

    [MenuItem(gotoMenu + "(Room 4) Inhibition Audio")]
    private static void GoToRoom4() => GoToScene(string.Format(ROOM_PATH, 4, ""));

    [MenuItem(gotoMenu + "(Room 5) Memory Visual")]
    private static void GoToRoom5() => GoToScene(string.Format(ROOM_PATH, 5, ""));

    [MenuItem(gotoMenu + "(Room 6) Memory Audio")]
    private static void GoToRoom6() => GoToScene(string.Format(ROOM_PATH, 6, ""));

    [MenuItem(gotoMenu + "(Room 7) Flexibility Visual/Levels 1")]
    private static void GoToRoom7Levels1() => GoToScene(string.Format(ROOM_PATH, 7, 1));

    [MenuItem(gotoMenu + "(Room 7) Flexibility Visual/Levels 2")]
    private static void GoToRoom7Levels2() => GoToScene(string.Format(ROOM_PATH, 7, 2));

    [MenuItem(gotoMenu + "(Room 8) Flexibility Audio/Levels 1")]
    private static void GoToRoom8Levels1() => GoToScene(string.Format(ROOM_PATH, 8, 1));

    [MenuItem(gotoMenu + "(Room 8) Flexibility Audio/Levels 2")]
    private static void GoToRoom8Levels2() => GoToScene(string.Format(ROOM_PATH, 8, 2));

    [MenuItem(gotoMenu + "(Room 8) Flexibility Audio/Levels 3")]
    private static void GoToRoom8Levels3() => GoToScene(string.Format(ROOM_PATH, 8, 3));

    [MenuItem(gotoMenu + "(Room 9) Media Literacy")]
    private static void GoToRoom9() => GoToScene(string.Format(ROOM_PATH, 9, ""));

    #endregion

    #region Utility

    private static string GetSceneName(Island island, RoomChannel channel, RoomLevel level)
    {
        if (island is Island.MEDIA_LITERACY || channel is RoomChannel.MEDIA_LITERACY)
            return "Room9";

        return island switch
        {
            Island.CONTROLLO_INTERFERENZA =>
                channel is RoomChannel.VISIVO ?
                    level is RoomLevel.LEVEL_22 or RoomLevel.LEVEL_32 ? "Room1_Levels2" : "Room1_Levels1" :
                    "Room2_Levels",

            Island.INIBIZIONE_RISPOSTA =>
                channel is RoomChannel.VISIVO ? "Room3_Levels" : "Room4_Levels",

            Island.MEMORIA_LAVORO =>
                channel is RoomChannel.VISIVO ? "Room5_Levels" : "Room6_Levels",

            Island.FLESSIBILITA_COGNITIVA =>
                channel is RoomChannel.VISIVO ?
                    level < RoomLevel.LEVEL_21 ? "Room7_Levels1" : "Room7_Levels2" :

                    level < RoomLevel.LEVEL_21 ? "Room8_Levels1" :
                    level < RoomLevel.LEVEL_31 ? "Room8_Levels2" : "Room8_Levels3",

            _ => string.Empty
        };
    }

    private static Position ToPosition(this Vector2Int vector2Int)
        => new Position(vector2Int.x, vector2Int.y);

    private static Vector2Int ToVector(this Position position)
        => new Vector2Int(position.Row, position.Column);

    private static IEnumerable<T> IterateWithProgressBar<T>(this T[] array, string title = "")
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (!EditorUtility.DisplayCancelableProgressBar(title, "", (float)i/array.Length))
            {
                yield return array[i];
            }
        }

        EditorUtility.ClearProgressBar();

        yield break;
    }

    private static IEnumerable<T> IterateWithProgressBar<T>(this List<T> list, string title = "")
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (!EditorUtility.DisplayCancelableProgressBar(title, "", (float)i / list.Count))
            {
                yield return list[i];
            }
        }

        EditorUtility.ClearProgressBar();

        yield break;
    }

    private static void LogToFile(object log, string filename = "log")
    {
        string path = EditorUtility.SaveFilePanel("Save Log", "", filename, "txt");

        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, log.ToString());
        }
    }

    private static void SaveToFile(this Texture2D tex, string filename = "tex")
    {
        string path = EditorUtility.SaveFilePanel("Save Texture", "", filename, "png");

        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllBytes(path, tex.EncodeToPNG());
        }
    }

    private static IEnumerable<string> FindAssetPaths(string filter, params string[] folders)
    {
        foreach (string guid in AssetDatabase.FindAssets(filter, folders))
        {
            yield return AssetDatabase.GUIDToAssetPath(guid);
        }
    }

    private static IEnumerable<T> LoadAssets<T>(string filter, params string[] folders) where T : Object
    {
        foreach (string path in FindAssetPaths(filter, folders))
        {
            yield return AssetDatabase.LoadAssetAtPath<T>(path);
        }
    }

    private static T GetComponent<T>(this Scene scene, bool includeInactive = true) where T : Component
    {
        return GetComponents<T>(scene, includeInactive).FirstOrDefault();
    }

    private static IEnumerable<T> GetComponents<T>(this Scene scene, bool includeInactive = true) where T : Component
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (T component in root.GetComponentsInChildren<T>(includeInactive))
            {
                yield return component;
            }
        }
    }

    private static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();

        return component == null ? gameObject.AddComponent<T>() : component;
    }

    public static void Shuffle<T>(this IList<T> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }

    public static string GetLocalizedString(string tableName, SerializedProperty property)
    {
        var collection = LocalizationEditorSettings.GetStringTableCollection(tableName);
        var table = collection.Tables[0].asset as StringTable;
        var key = property.FindPropertyRelative("m_TableEntryReference.m_KeyId").longValue;
        var entry = table?.GetEntry(key);

        return entry?.GetLocalizedString();
    }


    [MenuItem(menu + "Remap materials on selection")]
    private static void RemapMaterialsOnSelectedModels()
    {
        var models = Selection.GetFiltered<GameObject>(SelectionMode.DeepAssets);

        foreach (var model in models.IterateWithProgressBar())
        {
            var modelPath = AssetDatabase.GetAssetPath(model);
            var modelImporter = AssetImporter.GetAtPath(modelPath) as ModelImporter;
            modelImporter.SearchAndRemapMaterials(ModelImporterMaterialName.BasedOnMaterialName, ModelImporterMaterialSearch.Everywhere);
            modelImporter.SaveAndReimport();
        }
    }


    #endregion
}