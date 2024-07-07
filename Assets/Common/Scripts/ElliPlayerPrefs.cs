using Game;
using UnityEngine;
using static Game.ArrowState;

public static class ElliPrefs
{
    public static ComprendoBackend.UserPrefs currentPrefs = new ComprendoBackend.UserPrefs();
    
    private const string ARROW_TUTORIAL_KEY = "ArrowTutorial";

    public static bool GetArrowTutorialFlag(string userId, ArrowState arrow)
    {
        /*// Default arrows don't need tutorial
        if (arrow is NONE or UP or LEFT or RIGHT) return true;

        return PlayerPrefs.GetInt(userId + ARROW_TUTORIAL_KEY + arrow.ToString()) != default;*/
        
        switch (arrow)
        {
            case DOUBLE_UP:
                return currentPrefs.tutorialArrowDoubleUp;
            case TRIPLE_UP:
                return currentPrefs.tutorialArrowTripleUp;
            case X2:
                return currentPrefs.tutorialArrowX2;
            case X3:
                return currentPrefs.tutorialArrowX3;
            case X4:
                return currentPrefs.tutorialArrowX4;
            default:
                return true;
        }
    }

    public static void SetArrowTutorialFlag(string userId, ArrowState arrow)
    {
        /*PlayerPrefs.SetInt(userId + ARROW_TUTORIAL_KEY + arrow.ToString(), 1);*/
        switch (arrow)
        {
            case DOUBLE_UP:
                currentPrefs.tutorialArrowDoubleUp = true;
                break;
            case TRIPLE_UP:
                currentPrefs.tutorialArrowTripleUp = true;
                break;
            case X2:
                currentPrefs.tutorialArrowX2 = true;
                break;
            case X3:
                currentPrefs.tutorialArrowX3 = true;
                break;
            case X4:
                currentPrefs.tutorialArrowX4 = true;
                break;
            default:
                break;
        }

        SaveUserPrefs();
    }

    private const string ROOM_FIRST_ENTER_KEY = "showScrignoIntro";

    public static bool GetRoomFirstEnterFlag(string userId)
    {
        /*return PlayerPrefs.GetInt(userId + ROOM_FIRST_ENTER_KEY) != default;*/
        return currentPrefs.showScrignoIntro;
    }

    public static void SetRoomFirstEnterFlag(string userId, bool value)
    {
        /*PlayerPrefs.SetInt(userId + ROOM_FIRST_ENTER_KEY, value ? 1 : default);*/
        currentPrefs.showScrignoIntro = value;
        
        SaveUserPrefs();
    }

    public const string MEDIA_LITERACY_EXERCISES_STARTED = "mediaLiteracyExercisesStarted";
    public static string GetMediaLiteracyExercisesStarted(string userId)
    {
        /*return PlayerPrefs.HasKey(userId+MEDIA_LITERACY_EXERCISES_STARTED) ? PlayerPrefs.GetString(userId + MEDIA_LITERACY_EXERCISES_STARTED) : string.Empty;*/
        return currentPrefs.mediaLiteracyExercisesStarted;
    }

    public static void SetMediaLiteracyExercisesStarted(string userId, string exercisesStarted)
    {
        /*PlayerPrefs.SetString(userId+MEDIA_LITERACY_EXERCISES_STARTED, exercisesStarted);*/
        currentPrefs.mediaLiteracyExercisesStarted = exercisesStarted;
        
        SaveUserPrefs();
    }

    private static void SaveUserPrefs()
    {
        GameState.Instance.comprendoBackend.SetUserPrefs(currentPrefs,
            () =>
            {
                
            },
            exception =>
            {
                
            });
    }
}
