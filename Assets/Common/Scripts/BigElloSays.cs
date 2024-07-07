using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DG.Tweening;
using DG.Tweening.Core.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

public class BigElloSays : MonoBehaviour
{
    private const string NextCommand = "next";
    private const string AtomicCommand = "atomic";
    
    public enum Pauses

    {
        SoftPause,
        HardPause
    }
    
    [Serializable]
    public class Config
    {
        public LocalizedString Message;
        public LocalizedAudioClip LocalizedAudio;
        public Vector3 BigElloPosition;
        public Vector3 MessagePosition;
        public string AnimationTrigger;
        public bool LipSync = true;
        public Transform HighlightObject; 
        public bool ClickDisabled;
        public bool FinishWithAudio;
        public bool SkipAtFirstRun;
        public bool ObscureEverythingButMessage;
        public List<GameObject> GameObjectsToActivate = new List<GameObject>(); 
        public List<GameObject> GameobjectsToDeactivate = new List<GameObject>();
        public bool ShowRedButton;
        public bool ShowYellowButton;
        public bool ShowGreenButton;
    }

    private static readonly Dictionary<char, Pauses> pausesCharacters = new()
    {
        ['.'] = Pauses.HardPause,
        ['!'] = Pauses.HardPause,
        ['?'] = Pauses.HardPause,
        [','] = Pauses.SoftPause,
        [';'] = Pauses.SoftPause,
        [':'] = Pauses.HardPause
    };

    [SerializeField]
    public GameObject root;

    [SerializeField]
    private AssetReference modelReference; //since it's duplicated everywhere we load it as addressable

    private GameObject modelGameObject;

    [SerializeField]
    private Transform modelParent;

    [SerializeField]
    private TutorialHighlight highlight;

    [SerializeField]
    public GameObject messageRoot;

    [SerializeField]
    private TextMeshProUGUI messageText;

    [SerializeField]
    private AudioSource source;

    [SerializeField]
    private LipSync lipSync;

    [SerializeField]
    private bool _animateText;

    [SerializeField, Min(0), Tooltip("chars/s")]
    private float _defaultTextAnimationSpeed;
    [SerializeField, Min(0)]
    private float _hardPauseTime;
    [SerializeField, Min(0)]
    private float _softPauseTime;
    [SerializeField, Min(1)]
    private int _maxLinesCount = 1;

    [SerializeField]
    private CanvasGroup buttonGroup;

    private bool _audioPlayed;
    private Coroutine _textCoroutine;
    private bool _pauseTextAnimation;

    public bool IsVisible => root.activeInHierarchy;
    public bool IsPlayingAudio => source.isPlaying;

    public event Action AudioFinished;
    public event Action MessageClick;
    public event Action MessageFinished;
    public event Action MessageInterrupted;
    public event Action NextCommandReceived;

    private Dictionary<GameObject, bool> activeCache = new();

    private AsyncOperationHandle<GameObject> _modelLoadHandle;
    private Animator _bigElloAnimator;

    private IEnumerator Start()
    {
        _modelLoadHandle = modelReference.InstantiateAsync(modelParent, false);
        yield return _modelLoadHandle;
        modelGameObject = _modelLoadHandle.Result;

        if (modelGameObject)
        {
            SetLayerRecursively(modelGameObject.transform, modelParent.gameObject.layer);
            _bigElloAnimator = modelGameObject.GetComponent<Animator>();
            lipSync.MouthRenderer =
                modelGameObject.transform.Find("Ello_Mouth")
                    .GetComponent<SkinnedMeshRenderer>(); //HACK use a script on the prefab
        }
        else
        {
            Debug.LogError("ERROR during load Big Ello Model");
        }
    }

    private static void SetLayerRecursively(Transform transform, int layer)
    {
        transform.gameObject.layer = layer;

        foreach (Transform child in transform)
            SetLayerRecursively(child, layer);
    }

    private void Update()
    {
        if (_audioPlayed && !IsPlayingAudio)
        {
            _audioPlayed = false;

            if (lipSync != null)
                lipSync.enabled = false;

            AudioFinished?.Invoke();
        }
    }

    private void OnDestroy()
    {
        modelReference.ReleaseInstance(modelGameObject);
    }

    private int GetMessageAnimatedLength(string text)
    {
        var matches = Regex.Matches(text, @"<\/?[^<>]+>");
        return text.Count(char.IsLetterOrDigit) - 
                matches.Select(m => m.Length - 2 - (m.Value.Contains("/") ? 1 : 0)).Sum();
    }

    private int HandleCommand(string text, StringBuilder builder, StringBuilder commandBuilder, int currentIndex)
    {
        var found = false;
        var i = currentIndex + 1;
        for ( ; i < text.Length; i++)
        {
            if (text[i] == '>')
            {
                found = true;
                break;
            }
            commandBuilder.Append(text[i]);
        }

        if (!found) return currentIndex;
        
        var commandToken = commandBuilder.ToString();
        commandBuilder.Clear();
        
        if (commandToken == NextCommand) NextCommandReceived?.Invoke();
        else if (commandToken.Replace("/", "") == AtomicCommand)
        {
            if (!commandToken.Contains("/"))
            {
                i++;
                for (; i < text.Length && text[i] != '<'; i++) builder.Append(text[i]);
                if (text[i] == '<') i = HandleCommand(text, builder, commandBuilder, i);
            }
        }
        else builder.Append('<').Append(commandToken).Append('>');

        return i;
    }

    private IEnumerator AnimateText(string text, float time)
    {
        var usefulChars = GetMessageAnimatedLength(text);
        var waitSingle = time / usefulChars;
        var builder = new StringBuilder();
        var commandBuilder = new StringBuilder();

        var timer = 0f;
        for (var i = 0; i < text.Length; i++)
        {
            if (_pauseTextAnimation)
            {
                i--;
                yield return null;
                continue;
            }
            
            if (timer < waitSingle)
            {
                i--;
                timer += Time.deltaTime;
                yield return null;
                continue;
            }

            timer -= waitSingle;
            while (i < text.Length && text[i] == '<') i = HandleCommand(text, builder, commandBuilder, i) + 1;

            if (i < text.Length) builder.Append(text[i]);

            var j = i + 1;
            var pauseCharFound = '\0';
            while (j < text.Length && !char.IsLetterOrDigit(text[j]) && text[j] != '<')
            {
                builder.Append(text[j]);
                i++;
                j++;

                if (!pausesCharacters.ContainsKey(text[j - 1])) continue;
                if (!pausesCharacters.ContainsKey(pauseCharFound) ||
                    pausesCharacters.ContainsKey(pauseCharFound) &&
                    pausesCharacters[text[j - 1]] > pausesCharacters[pauseCharFound])
                {
                    pauseCharFound = text[j - 1];
                }
            }

            messageText.text = builder.ToString();
            messageText.ForceMeshUpdate();

            if (messageText.textInfo.lineCount > _maxLinesCount)
            {
                var startIndex = messageText.textInfo.lineInfo[0].firstCharacterIndex;
                builder.Remove(startIndex, messageText.textInfo.lineInfo[0].lastCharacterIndex + 1 - startIndex); 
                messageText.text = builder.ToString();
            }

            if (pausesCharacters.TryGetValue(pauseCharFound, out var pauseType))
            {
                var pauseTime = _softPauseTime;
                if (pauseType == Pauses.HardPause) pauseTime = _hardPauseTime;
                timer -= pauseTime;
            }
        }

        _textCoroutine = null;

        yield return new WaitUntil(() => !source.isPlaying);
        
        MessageFinished?.Invoke();
    }

    public void Show(BigElloMessage bigElloMessage)
    {
        if (_showRoutine != null)
        {
            StopCoroutine(_showRoutine);
        }

        _showRoutine = StartCoroutine(ShowRoutine(bigElloMessage));
    }

    private Coroutine _showRoutine;

    private IEnumerator ShowRoutine(BigElloMessage bigElloMessage)
    {
        root.SetActive(true);
        messageRoot.SetActive(true);

        root.transform.DOKill();
        messageRoot.transform.DOKill();

        if (_textCoroutine != null)
        {
            StopCoroutine(_textCoroutine);
            MessageInterrupted?.Invoke();
        }

        var message = !string.IsNullOrEmpty(bigElloMessage.Message.TableReference.TableCollectionName) ? bigElloMessage.Message.GetLocalizedString() : string.Empty;

        bool isTextAnimated = _animateText && !string.IsNullOrEmpty(message)
            && !bigElloMessage.LocalizedAudio.IsEmpty;

        messageText.SetText(isTextAnimated ? string.Empty : message);

        AudioClip audio = null;
        if (bigElloMessage.LocalizedAudio != null)
        {
            var op = bigElloMessage.LocalizedAudio.LoadAssetAsync<AudioClip>();
            yield return op;

            audio = op.Result;    
        }
        

        var time = 0f;
        if (audio != null && bigElloMessage.FinishWithAudio)
            time = audio.length;
        else if (_defaultTextAnimationSpeed != 0)
            time = GetMessageAnimatedLength(message) / _defaultTextAnimationSpeed;

        if (isTextAnimated && time > 0)
            _textCoroutine = StartCoroutine(AnimateText(message, time));
        else
            messageText.SetText(message);

        if (!string.IsNullOrEmpty(bigElloMessage.AnimationTrigger) && _bigElloAnimator != null)
            _bigElloAnimator.SetTrigger(bigElloMessage.AnimationTrigger);

        if (audio != null)
        {
            source.clip = audio;
            source.Play();
            _audioPlayed = true;

            if (lipSync != null)
                lipSync.enabled = bigElloMessage.LipSync;
        }

        if (string.IsNullOrEmpty(message) && audio == null) MessageFinished?.Invoke();

    }

    public void Show(BigElloSaysConfig cfg, bool skipAnimation = false)
    {
        const float duration = 0.5f;
        
        Show(cfg.Message);

        SetClickEnabled(false);

        if (!skipAnimation)
        {
            root.transform.DOLocalMove(cfg.BigElloPosition, duration).SetEase(Ease.OutQuad);
            messageRoot.transform
                .DOLocalMove(cfg.MessagePosition, duration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => SetClickEnabled(!cfg.Message.ClickDisabled));
        }
        else
        {
            root.transform.localPosition = cfg.BigElloPosition;
            messageRoot.transform.localPosition = cfg.MessagePosition;

            SetClickEnabled(!cfg.Message.ClickDisabled);
        }
        
        foreach (GameObject gameObject in cfg.GameObjectsToActivate)
        {
            activeCache[gameObject] = gameObject.activeSelf;
            gameObject.SetActive(true);
        }
        
        foreach (GameObject gameObject in cfg.GameobjectsToDeactivate)
        {
            activeCache[gameObject] = gameObject.activeSelf;
            gameObject.SetActive(false);
        }

        if (highlight == null) return;
        
        if (cfg.HighlightObject != null)
            highlight.MaskTransform(cfg.HighlightObject, cfg.IgnoreHighlightEnbledState, 0.5f);
        else
        {
            if (cfg.Message.ObscureEverythingButMessage)
                highlight.ObscureEverything();
            else 
                highlight.ClearMask();
        }
    }

    public void Hide()
    {
        if (buttonGroup != null)
        {
            buttonGroup.interactable = false;
            buttonGroup.blocksRaycasts = false;
        }

        root.transform.DOKill();
        messageRoot.transform.DOKill();

        if (highlight != null) highlight.ClearMask();

        foreach (var pair in activeCache)
        {
            if (pair.Key == null) continue;

            pair.Key.SetActive(pair.Value);
        }

        activeCache.Clear();

        StopAudio();

        root.SetActive(false);
        messageRoot.SetActive(false);
    }

    public void StopAudio()
    {
        if (source.isPlaying)
        {
            source.Stop();
            source.clip = null;
        }

        if (_audioPlayed)
        {
            if (lipSync != null)
                lipSync.enabled = false;

            AudioFinished?.Invoke();
            _audioPlayed = false;
        }
    }

    public void PauseRead(bool pause)
    {
        _pauseTextAnimation = pause;
        if (pause) source.Pause();
        else source.UnPause();
    }

    public void SetClickEnabled(bool enabled)
    {
        if (buttonGroup == null) return;

        buttonGroup.interactable = enabled;
        buttonGroup.blocksRaycasts = enabled;
    }

    public void OnMessageClick() => MessageClick?.Invoke();
}