using System;
using System.Collections;
using Cutscene;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Start
{
    public class GameController : MonoBehaviour
    {

        public AssetReference town;

        // PUBLIC 
        public GameObject cheatChecker;

        public DemoDemon demoDemonPrefab;

        // Inputs

        public InputField userField;
        public InputField passwordField;
        public InputField nameField;

        // TMP_InputField does not work on WEBGL mobile
        // 3.0.6 does not bring up soft keyboard
        // 3.2.0-pre.5 crashes on submit
        // 4.0.0-pre.2 not tested but is a drastic change

        //public TMP_InputField userField;
        //public TMP_InputField passwordField;

        //public TMP_InputField nameField;

        // Texts
        public TextMeshProUGUI loginErrorText;
        public TextMeshProUGUI sessionErrorText;
        public TextMeshProUGUI completedProgramErrorText;
        public TextMeshProUGUI notStartedYetErrorText;

        public GameObject BigEllo;
        public GameObject Ello;

        [SerializeField]
        private ElloCustomization customization;
        [SerializeField]
        private ElloCustomization loadingCustomization;

        private string url = string.Empty;

        // Buttons
        public Button loginButton;

        // Screens
        public GameObject initialLoading;
        public GameObject loginScreen;
        public GameObject customizationScreen;
        public LoadingScreen loadingScreen;
        
        [SerializeField]
        private CutsceneLoader cutsceneLoader;

        [SerializeField]
        private CanvasGroup fader;

        private Tween elloRotatorTween = null;

        public IEnumerator Start()
        {
            SetFader(true);
            BigEllo.SetActive(false);
            initialLoading.SetActive(true);
            
            // Waiting for localization asset bundle to be loaded
            if (!LocalizationSettings.InitializationOperation.IsDone)
                yield return LocalizationSettings.InitializationOperation;

            yield return new WaitForSeconds(2f);
            
            checkURLParametersAndLogin();
        }

        private void ShowLoginUI(bool show)
        {
            SetFader(false);
            BigEllo.SetActive(show);
            initialLoading.SetActive(false);
            loginScreen.SetActive(show);
        }
        
        private void ShowCustomizationScreenUI(bool show)
        {
            SetFader(false);
            customizationScreen.SetActive(show);
            Ello.SetActive(show);
            if(elloRotatorTween != null)
                elloRotatorTween.Kill();
            if(show)
                elloRotatorTween = Ello.transform.DORotate(Vector3.up * 359, 5, RotateMode.LocalAxisAdd).SetEase(Ease.InOutSine).SetLoops(-1);
        }
        
        public void GetElloSkin()
        {
            LevelBackend backend = GameState.Instance.levelBackend;

            customization.UnlockAccessories(Island.INTRODUZIONE, true);
            loadingCustomization.UnlockAccessories(Island.INTRODUZIONE, true);

            if (backend.Demo || GameState.Instance.testMode)
            {
                ShowLoginUI(false);
                ShowCustomizationScreenUI(true);
                return;
            }

            backend.GetElloParams(
                res => {
                    nameField.text = res.nomeEllo;
                    customization.SetAccessories(res.accessorio1, res.accessorio2, res.accessorio3);
                    loadingCustomization.SetAccessories(res.accessorio1, res.accessorio2, res.accessorio3);

                    foreach (Island island in GameState.Instance.levelBackend.completedIslands)
                    {
                        customization.ToggleHairColor(island, true);
                        loadingCustomization.ToggleHairColor(island, true);
                        customization.UnlockAccessories(island, true);
                        loadingCustomization.UnlockAccessories(island, true);
                    }

                    ShowLoginUI(false);
                    ShowCustomizationScreenUI(true);
                },
                err => {
                    Debug.LogError(err.Message);
                    ShowLoginUI(false);
                    ShowCustomizationScreenUI(true);
                });

        }
        
        private void SetFader(bool enabled)
        {
            fader.alpha = enabled ? 1f : 0f;
            fader.blocksRaycasts = enabled;
        }

        public void SetElloSkin(int accessorio1, int accessorio2, int accessorio3)
        {
            string name = nameField.text.Trim();

            LevelBackend backend = GameState.Instance.levelBackend;

            if (backend.Demo || GameState.Instance.testMode)
            {
                StartGame();
                return;
            }

            backend.SaveElloParams(name, accessorio1, accessorio2, accessorio3,
            () => {
                loadingCustomization.SetAccessories(accessorio1, accessorio2, accessorio3);
                StartGame();
            },
            err => {
                Debug.Log(err.Message);
                StartGame();
            });
        }

        private void StartGame()
        {
            Ello.transform.DOKill();
            Ello.SetActive(false);
            customizationScreen.SetActive(false);

            LevelBackend backend = GameState.Instance.levelBackend;

#if UNITY_EDITOR
            backend.MediaLiteracyEnabled |= GameState.Instance.forceMediaLiteracy;
#endif
            if (!backend.gameCompleted)
            {
                bool demo = backend.Demo;

#if UNITY_EDITOR
                demo |= GameState.Instance.forceDemo;
#endif

                if (demo)
                    SceneManager.LoadScene("RoomStart");
                else
                {
                    loadingScreen.gameObject.SetActive(true);
                    loadingScreen.LoadIsland(backend.island);
                }
            }
        }

        public void LoginTestDemo() 
        {
            userField.SetTextWithoutNotify("demo");
            passwordField.SetTextWithoutNotify("demo");
            LoginTest();
        }

        /* Funzione per la gestione del login */
        public void LoginTest()
        {
            // Disabilito temporaneamente l'interazione con l'utente
            loginButton.interactable = false;
            userField.interactable = false;
            passwordField.interactable = false;

            // Recupero username e password
            string userText = userField.text.Trim();
            string passwordText = passwordField.text.Trim();

            ResetErrorMessages();

            // Richiesta di login
            GameState.Instance.comprendoBackend.Login(userText, passwordText,
            res => {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
                GameState.Instance.comprendoBackend.GetUserPrefs((prefs) =>
                {
                    ElliPrefs.currentPrefs = prefs;
                    StartSession(() => {
                        ShowLoginUI(false);
                        ShowCustomizationScreenUI(false);
                        cheatChecker.SetActive(true);
                        GetElloSkin();
                    });
                }, (err) =>
                {
                    Debug.Log(err.Message);
                    ResetLoginUIAfterError(loginErrorText.gameObject);
                });
            },
            err => {
                Debug.Log(err.Message);
                ResetLoginUIAfterError(loginErrorText.gameObject);
            });
        }
        
        private void checkURLParametersAndLogin()
        {
            url = Application.absoluteURL;
            if (url.Contains("?token="))
            {
                var  urlParams =url.Split("?token=");
                if (urlParams.Length == 2)
                {
                    var token = urlParams[1];
                    
                    GameState.Instance.comprendoBackend.CheckToken(token, tokenResponse =>
                    {
                        if (tokenResponse != null)
                        {
                            // In caso di successo salvo i dati di accesso e faccio partire sessione
                            GameState.Instance.comprendoBackend.id = tokenResponse.id;
                            GameState.Instance.comprendoBackend.token = token;
                            GameState.Instance.comprendoBackend.username = tokenResponse.username;
                            GameState.Instance.comprendoBackend.SetTokenHeader();
                            
                            GameState.Instance.comprendoBackend.GetUserPrefs((prefs) =>
                            {
                                ElliPrefs.currentPrefs = prefs;
                                Screen.sleepTimeout = SleepTimeout.NeverSleep;
                                StartSession(() => {
                                    initialLoading.SetActive(false);
                                    ShowLoginUI(false);
                                    ShowCustomizationScreenUI(false);
                                    cheatChecker.SetActive(true);
                                    GetElloSkin();
                                });
                            }, (err) =>
                            {
                                Debug.Log(err.Message);
                                ResetLoginUIAfterError(loginErrorText.gameObject);
                            });
                        }
                        else
                        {
                            ResetLoginUIAfterError(loginErrorText.gameObject);
                        }
                    });
                }
                else
                {
                    ResetLoginUIAfterError(null);
                }
            }
            else
            {
                ResetLoginUIAfterError(null);
            }
        }

        private void ResetLoginUIAfterError(GameObject errorToEnable = null)
        {
            ShowLoginUI(true);
            ShowCustomizationScreenUI(false);
            // Reset dei campi e del bottone
            ResetErrorMessages();
            
            loginButton.interactable = true;
            userField.interactable = true;
            passwordField.interactable = true;
            userField.text = null;
            passwordField.text = null;
            
            // Mostro errore all'utente
            if (errorToEnable)
                errorToEnable.SetActive(true);
        }

        private void ResetErrorMessages()
        {
            loginErrorText.gameObject.SetActive(false);
            sessionErrorText.gameObject.SetActive(false);
            completedProgramErrorText.gameObject.SetActive(false);
            notStartedYetErrorText.gameObject.SetActive(false);
        }

        private void StartSession(Action onStarted)
        {
            GameState.Instance.levelBackend.StartSession(
            res => {

                LevelBackend backend = GameState.Instance.levelBackend;

                if (backend.gameCompleted || backend.notStartedYet)
                {
                    loginButton.interactable = true;
                    userField.interactable = true;
                    passwordField.interactable = true;
                    
                    ResetErrorMessages();
                    completedProgramErrorText.gameObject.SetActive(backend.gameCompleted);
                    notStartedYetErrorText.gameObject.SetActive(backend.notStartedYet);

                    return;
                }

                bool demo = backend.Demo;

#if UNITY_EDITOR
                demo |= GameState.Instance.forceDemo;
#endif

                if (demo)
                {
                    GameState.Instance.testMode = true;
                    Instantiate(demoDemonPrefab);

                    BigEllo.SetActive(false);
                    string id = backend.island.ToString().ToLower();
                    cutsceneLoader.Load(id, onStarted);

                    return; // Don't ask backend for video
                }

                var playVideo = !backend.gameCompleted;

                backend.MustShowVideo(showVideo =>
                {
                    playVideo &= showVideo;
#if UNITY_EDITOR
                    if (GameState.Instance.forceVideo) playVideo = true;
#endif
                    if (playVideo)
                    {
                        string id = backend.island.ToString().ToLower();

                        BigEllo.SetActive(false);

                        Debug.Log("Video introduttivo " + id);
                        cutsceneLoader.Load(id, onStarted);
                    }
                    else
                    {
                        onStarted();
                    }
                },
                    err => {
                        Debug.LogError(err.Message);
                        onStarted();
                    });
            },
            err => {
                Debug.Log(err.Message);
                ResetLoginUIAfterError(sessionErrorText.gameObject);
            });
        }

        public void ConfirmSkin()
        {
            GameState gameState = GameState.Instance;
            gameState.accessorio1 = customization.Slot1Index;
            gameState.accessorio2 = customization.Slot2Index;
            gameState.accessorio3 = customization.Slot3Index;

            SetElloSkin(gameState.accessorio1, gameState.accessorio2, gameState.accessorio3);
        }

        void Update()
        {
            if (fader.blocksRaycasts) return;

            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (loginScreen.activeSelf)
                    LoginTest();
                else if (customizationScreen.activeSelf)
                    ConfirmSkin();
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                EventSystem system = EventSystem.current;
                GameObject selected = system.currentSelectedGameObject;

                if (selected != null && selected.GetComponent<Selectable>() != null)
                {
                    Selectable next = selected.GetComponent<Selectable>().FindSelectableOnDown();

                    if (next != null)
                    {

                        system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));
                    }
                }
            }
        }

        public void OnShowPasswordButtonPressed()
        {
            bool passwordShown = passwordField.contentType == InputField.ContentType.Standard;

            passwordField.DeactivateInputField();
            passwordField.contentType = passwordShown ? InputField.ContentType.Password : InputField.ContentType.Standard;
            passwordField.ActivateInputField();
        }
    }
}
