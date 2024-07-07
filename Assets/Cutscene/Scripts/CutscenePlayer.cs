using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Cutscene
{
    public class CutscenePlayer : MonoBehaviour
    {
        [Serializable]
        public class Cutscene
        {
            public string Id;
            public AssetReference Scene;
        }

        public static CutscenePlayer Instance { get; private set; }

#if UNITY_EDITOR
        [SerializeField]
        private string _debugPlayCutscene;
#endif

        [SerializeField]
        private CutsceneMessageSystem messageSystem;

        [SerializeField]
        private RectTransform _animationsParent;

        [SerializeField]
        private RectTransform _animationAlternativeParent;

        [SerializeField]
        private RollingDownUI _parentAnimatedObject;
        
        [SerializeField]
        private Button _skipButton;

        [SerializeField]
        private ToggleButton _pauseButton;

        [SerializeField]
        private Image _fadePanel;

        [SerializeField, Min(0)]
        private float _fadeDuration;

        [SerializeField]
        private List<Cutscene> cutscenes;

        private Action _cachedCallback;
        private List<Binding> _currentBindings;
        private SceneInstance _currentAssetScene;
        private CutsceneAnimation _currentAnimation;

        private void Awake()
        {
            Instance = this;
            _skipButton.interactable = false;
            _pauseButton.interactable = false;
        }

        public void Play(string id, Action callback)
        {
            Cutscene cutscene = cutscenes.Find(c => c.Id == id);

#if UNITY_EDITOR
            if (cutscene == null)
            {
                Debug.LogError("Could not find cutscene: " + id);
                callback();
                return;
            }
#endif

            void PlayCutsceneRoutine()
            {
                StartCoroutine(PlayCutscene(cutscene, callback));
            }

            if (_fadeDuration > 0)
            {
                _fadePanel.color = new Color(0, 0, 0, 1);
                _fadePanel.DOFade(0, _fadeDuration).OnComplete(PlayCutsceneRoutine);
            }
            else
            {
                _fadePanel.color = new Color(0, 0, 0, 0);
                PlayCutsceneRoutine();
            }
        }

        public Tween ShowWhiteboard(bool show) => show ? _parentAnimatedObject.Show() : _parentAnimatedObject.MoveUp();

        private void OnSafeToRemoveChanged(bool isSafeToRemove)
        {
            if (isSafeToRemove) messageSystem.RemoveInhibition();
            else messageSystem.RequestInhibition();
        }

        private IEnumerator ActionBindingExecution(ActionBinding binding)
        {
            var enumerator = binding.Execute(this);
            OnSafeToRemoveChanged(false);
            while (enumerator.MoveNext()) yield return enumerator.Current;
            OnSafeToRemoveChanged(true);
        }

        private void OnMessageChange(int index)
        {
            var b = _currentBindings[index];

            if (b is ActionBinding actionBinding)
            {
                StartCoroutine(ActionBindingExecution(actionBinding));
                return;
            }

            var binding = b as CutsceneBinding;

            if (_currentAnimation != null && (binding.CutsceneAnimation == null || _currentAnimation.name != binding.CutsceneAnimation.name))
            {
                _currentAnimation.SafeToRemoveChanged -= OnSafeToRemoveChanged;
                Destroy(_currentAnimation.gameObject);
                _currentAnimation = null;
            }

            if (binding.CutsceneAnimation == null || _currentAnimation != null && _currentAnimation.name == binding.CutsceneAnimation.name) return;

            var parent = binding.UseAlternativeParent ? _animationAlternativeParent : _animationsParent;

            //TODO TEMPORARY, SHOULD GIVE THE POSSIBILITY TO ANIMATE THE WHITEBOARD
            _parentAnimatedObject.gameObject.SetActive(!binding.UseAlternativeParent);

            _currentAnimation = Instantiate(binding.CutsceneAnimation, parent);
            _currentAnimation.name = binding.CutsceneAnimation.name;
            _currentAnimation.ShowAndStart();
            _currentAnimation.SafeToRemoveChanged += OnSafeToRemoveChanged;
        }

        private void OnNext()
        {
            if (_currentAnimation == null) return;
            _currentAnimation.NextCutsceneStep();
        }

        private void OnMessageRead(int _)
        {
            if (!messageSystem.TryShowNextMessage())
            {
                StartCoroutine(CleanupAndCallback());
            }
        }

        private void OnAutoplayEnd()
        {
            StartCoroutine(CleanupAndCallback());
        }

        public void OnSkipButtonPressed()
        {
            StopAllCoroutines();

            StartCoroutine(CleanupAndCallback());
        }

        public void OnPauseButtonPressed(bool pause) => messageSystem.SetPause(pause);

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;

            if (messageSystem != null)
            {
                messageSystem.messageChange.RemoveListener(OnMessageChange);
                messageSystem.messageRead.RemoveListener(OnMessageRead);
                messageSystem.BigElloSays.NextCommandReceived -= OnNext;
            }
        }

        private IEnumerator PlayCutscene(Cutscene cutscene, Action callback)
        {
            _fadePanel.raycastTarget = false;
            _cachedCallback = callback;

            var op = Addressables.LoadSceneAsync(cutscene.Scene, LoadSceneMode.Additive);

            yield return op;
            
            _skipButton.interactable = true;
            _pauseButton.interactable = true;

            _currentAssetScene = op.Result;

            foreach (GameObject root in _currentAssetScene.Scene.GetRootGameObjects())
            {
                _currentBindings = root.GetComponent<CutsceneRoot>()?.Bindings;

                if (_currentBindings != null) break;
            }

            // Calling GetLocalizedString before init is done makes the system use await => not supported on WebGL

            if (!LocalizationSettings.InitializationOperation.IsDone)
                yield return LocalizationSettings.InitializationOperation;

            messageSystem.SetCurrentMessagesBatch(_currentBindings.Select(b => b.Message));
            messageSystem.messageChange.AddListener(OnMessageChange);
            messageSystem.messageRead.AddListener(OnMessageRead);
            messageSystem.BigElloSays.NextCommandReceived += OnNext;

            if (!messageSystem.TryShowNextMessage())
            {
                yield return CleanupAndCallback();
            }
        }

        private IEnumerator CleanupAndCallback()
        {
            if (messageSystem != null)
            {
                messageSystem.messageChange.RemoveListener(OnMessageChange);
                messageSystem.messageRead.RemoveListener(OnMessageRead);
                messageSystem.BigElloSays.NextCommandReceived -= OnNext;
            }

            if (_fadeDuration > 0)
            {
                var tween = _fadePanel.DOFade(1, _fadeDuration);

                yield return tween.AsyncWaitForCompletion();
            }
            
            _currentBindings?.Clear();

            if (_currentAssetScene.Scene.isLoaded)
            {
                yield return Addressables.UnloadSceneAsync(_currentAssetScene);
            }
            
            _cachedCallback?.Invoke();
        }

#if UNITY_EDITOR
        [ContextMenu("Play Debug Cutscene")]
        private void DebugPlay()
        {
            Play(_debugPlayCutscene, null);

            if (!gameObject.GetComponent<AudioListener>())
                gameObject.AddComponent<AudioListener>();
        }
#endif
    }
}