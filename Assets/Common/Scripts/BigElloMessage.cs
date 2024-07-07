using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "Big Ello/Message", fileName = "New Big Ello Message")]
public class BigElloMessage : ScriptableObject
{
    public static BigElloMessage Create(LocalizedString message, LocalizedAudioClip localizedAudio, string animationTrigger,
        bool lipSync, bool clickDisabled, bool finishWithAudio, bool skipAtFirstRun, bool obscureEverythingButMessage)
    {
        var result = CreateInstance<BigElloMessage>();

        result.Message = message;
        result.LocalizedAudio = localizedAudio;
        result.AnimationTrigger = animationTrigger;
        result.LipSync = lipSync;
        result.ClickDisabled = clickDisabled;
        result.FinishWithAudio = finishWithAudio;
        result.SkipAtFirstRun = skipAtFirstRun;
        result.ObscureEverythingButMessage = obscureEverythingButMessage;

        return result;
    }

    [field: SerializeField]
    public LocalizedString Message { get; private set; }
    [field: SerializeField]
    public LocalizedAudioClip LocalizedAudio { get; set; }
    [field: SerializeField]
    public string AnimationTrigger { get; private set; }
    [field: SerializeField]
    public bool LipSync { get; private set; } = true;
    [field: SerializeField]
    public bool ClickDisabled { get; private set; }
    [field: SerializeField]
    public bool FinishWithAudio { get; private set; }
    [field: SerializeField]
    public bool SkipAtFirstRun { get; private set; }
    [field: SerializeField]
    public bool ObscureEverythingButMessage { get; private set; } //TODO is this equivalent to HighlightObject = Message?
}