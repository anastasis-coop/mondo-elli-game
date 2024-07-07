using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Game
{
    public struct GameEvent
    {
        public GameEventType eventType;
        public GameObject triggerObject;
    }

    public enum ArrowState { NONE, UP, LEFT, RIGHT, DOUBLE_UP, TRIPLE_UP, X2, X3, X4 };

    public enum GameEventType { CODING_LIST_CHANGED, UNUSED_1, UNUSED_2, UNUSED_3, PLAY, GHOST, HELP, COLLECTIBLE, POWER, MESSAGE_READ, PHASE_START, PHASE_END, WRONG_PATH, CORRECT_PATH, UNUSED_4, SESSION_END };

    [Serializable]
    public class GameObjectGroup
    {
        public GameObject[] group;
    }

    [Serializable]
    public class PlayerLocation
    {
        public Transform transform;
        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;
    }

    [Serializable]
    public class PlayerLocationGroup
    {
        public PlayerLocation[] group;
    }

    public class GameController : MonoBehaviour
    {
        private enum GamePhase
        {
            /// <summary>
            /// Bridge animation when starting an island, 3 steps forward then COMPLEX_PATH
            /// </summary>
            INTRO,

            /// <summary>
            /// Coding tutorial logic.
            /// </summary>
            TUTORIAL,

            /// <summary>
            /// State triggered by pressing help button.
            /// Backs up the current phase then shows a message. It used to trigger highlights and path coloring.
            /// After the message is read, it goes back to the last phase.
            /// </summary>
            HELP,

            /// <summary>
            /// Main coding logic
            /// </summary>
            CODING,

            /// <summary>
            /// Cleans path, shows messages (in the past it also rotated Ello towards the room).
            /// After the last message is read, updates progress and goes to RoomStart
            /// </summary>
            ENTER_ROOM,

            /// <summary>
            /// First state after coming back from a room, shows messages then goes to COMPLEX_PATH. 
            /// </summary>
            EXIT_ROOM,

            /// <summary>
            /// Last state before a scene load, it does nothing.
            /// </summary>
            SCENE_END,

            /// <summary>
            /// Shows a message and goes to End scene (or RoomTutorial if it's in testMode)
            /// </summary>
            GAME_END,
        };

        // Costanti
        private const int TRIAL_NUMBER = 2;

        public static GameController Instance { get; private set; }

        // Riferimenti pubblici a oggetti nascosti
        public Button helpButton;

        public GameObject[] bridges;

        // Proprietà pubbliche
        public QuizPanelHandler quizPanelGroup;

        public PlayerLocation[] firstLoadPlayerPositions;

        public AudioClip collectibleSound;

        [SerializeField]
        private TotalProgressionCircle progressionCircle;

        [SerializeField]
        private ProgressBarController progressBar;

        [SerializeField]
        private RectTransform timerFill;

        [SerializeField]
        private CodingEndPopup endPopup;

        [Header("Big Ello Says")]

        [SerializeField]
        private BigElloSays bigElloSays;

        [SerializeField]
        private MessageSystem messageSystem;
        
        [SerializeField]
        private BigElloSaysConfig[] _tutorialConfigs;
        
        [FormerlySerializedAs("_firstIslandTutorialConfig")]
        [SerializeField]
        private BigElloSaysConfig[] _interferenceIslandTutorialConfig;

        [SerializeField]
        private BigElloSaysConfig[] _inhibitionIslandTutorialConfig;

        [SerializeField]
        private BigElloSaysConfig[] _memoryIslandTutorialConfig;

        [SerializeField]
        private BigElloSaysConfig[] _flexibilityIslandTutorialConfig;

        [SerializeField]
        private BigElloSaysConfig[] _mediaLiteracyIslandTutorialConfig;

        [SerializeField]
        private BigElloSaysConfig _pathErrorConfig;

        [SerializeField]
        private BigElloSaysConfig _repeatedPathErrorConfig;

        [SerializeField]
        private BigElloSaysConfig _complexPathEndingConfig;

        [SerializeField]
        private BigElloSaysConfig _endSessionConfig;

        [SerializeField]
        private BigElloSaysConfig _powerConfig;

        [SerializeField]
        private BigElloSaysConfig[] _enterRoomConfigs;

        [SerializeField]
        private BigElloSaysConfig[] _exitRoomConfigs;

        [SerializeField]
        private BigElloSaysConfig _complexPathConfig;

        [SerializeField]
        private BigElloSaysConfig _ghostHelpConfig;

        [SerializeField]
        private BigElloSaysConfig _tileHelpConfig;

        [Serializable]
        private class TileTutorial
        {
            public ArrowState Tile;
            public BigElloSaysConfig[] tutorial;
        }

        [SerializeField]
        private TileTutorial[] tileTutorials;

        [SerializeField]
        private ElloCustomization customization;

        [HideInInspector]
        public Color currentUnlightedColor;

        public GameObject flag;

        private int currentPathIndex = 0;  // Definisce l'indice del gruppo di percorsi correnti

        // Stati di gioco
        private int currentAreaIndex;
        public int CurrentArea => currentAreaIndex;
        private RoomChannel currentChannel;

        private GamePhase currentGamePhase;

        public AudioSource envAudioSource;

        private int explorationTime;

        private DateTime initPathTime;
        private long initPathTimestamp;

        // Indica se la parte più importante della sessione del giorno è terminata:
        // - finito il tutorial fino al trofeo o allo scadere del timer
        // - finito un quartiere completo delle due stanze scrigno (fino al trofeo se non già ottenuto)
        private bool powerUnlocked = false;

        [Serializable]
        private class CodingIsland
        {
            public Island Island;
            public List<CodingLevel> Levels;
        }

        [Serializable]
        private class CodingLevel
        {
            public List<CodingPath> Paths;
        }

        [Header("Coding")]

        [SerializeField]
        private CodingUI codingUI;

        [SerializeField]
        private CodingPath[] tutorialPaths;

        [SerializeField]
        private CodingIsland[] CodingIslands;

        private CodingPath currentPath;

        [HideInInspector]
        public CodingSubPath[] currentSubPaths;
        public CodingSubPath CurrentSubPath => currentSubPaths[currentSubPathIndex];

        private int currentLevelIndex;

        public int currentSubPathIndex;

        [Serializable]
        private class TileSet
        {
            public List<ArrowState> Tiles;
        }

        [SerializeField]
        private List<TileSet> TileSets;

        [Header("Scene Objects")]

        public GameObject player;
        public GameObject shadow;

        [SerializeField]
        private GameObject pathPanel;

        [SerializeField]
        private GameObject powerPanel;

        [SerializeField]
        private GameObject firstPowerPanel;

        [SerializeField]
        private GameObject ghostButton;

        [Header("Components")]

        [SerializeField]
        public PathHandler pathHandler;

        [SerializeField]
        public MovementHandler movementHandler;

        [SerializeField]
        private CameraHandler cameraHandler;

        private List<ArrowState> currentTileSet;

        private bool _showingStartOfIslandMessages;
        private ArrowState _tileTutorial;
        private float _timer;
        private int helpLevel;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            GameState gameState = GameState.Instance;
            LevelBackend backend = gameState.levelBackend;

            Island island = backend.island;
            List<Island> completedIslands = backend.completedIslands;

            explorationTime = backend.getMaxExplorationMinutes() * 60;
            
            currentAreaIndex = Array.FindIndex(CodingIslands, i => i.Island == island);

            customization.ToggleHairColors(completedIslands, true);
            customization.SetAccessories(gameState.accessorio1, gameState.accessorio2, gameState.accessorio3);

            if (backend.areaCompleted)
                customization.ToggleHairColor(island, true);

            bool useMediaLiteracy = backend.MediaLiteracyEnabled;
            bool tutorialCompleted = backend.completedIslands.Contains(Island.INTRODUZIONE);

            if (island == Island.INTRODUZIONE)
            {
                currentPathIndex = 0;
                //HACK since we replaced path randomization with level progression, to randomize intro to coding we need extra logic
                currentLevelIndex = tutorialCompleted ? Random.Range(0, CodingIslands[currentAreaIndex].Levels.Count) : 0;
            }
            else
            {
                currentLevelIndex = backend.CodingLevel;
                currentPathIndex = gameState.pathNumber;
            }

            int pathCount = CodingIslands[currentAreaIndex].Levels[currentLevelIndex].Paths.Count;
            int roomCount = (island > Island.CONTROLLO_INTERFERENZA && useMediaLiteracy) ? pathCount : pathCount - 1;

            bool codingCompleted = currentPathIndex >= pathCount;

            float pathProgress = codingCompleted ? 1 : (float)currentPathIndex / pathCount;
            progressionCircle.Init(island, completedIslands, useMediaLiteracy, pathProgress);

            codingUI.SetPoolVisible(false);
            helpButton.interactable = false;
            timerFill.anchorMax = Vector2.up;

            if (island == Island.INTRODUZIONE && !tutorialCompleted)
            {
                currentPath = tutorialPaths[currentPathIndex];
                currentSubPaths = new[] { currentPath.ToSubPath() };

                player.transform.position = CurrentSubPath.Start.Value;
                player.transform.forward = CurrentSubPath.StartDirection;

                currentGamePhase = GamePhase.TUTORIAL;

                codingUI.SetPoolVisible(false);
            }
            else if (codingCompleted) // Just completed a room but there is no more coding
            {
                progressBar.Init(currentAreaIndex, roomCount, pathCount);
                UpdateProgressBar();
                currentGamePhase = GamePhase.GAME_END;
                
                currentPath = CodingIslands[currentAreaIndex].Levels[currentLevelIndex].Paths[^1];
                currentSubPaths = new[] { currentPath.ToSubPath() };
                
                player.transform.position = CurrentSubPath.End.Value;
                player.transform.forward = CurrentSubPath.EndDirection;
            }
            else
            {
                progressBar.Init(currentAreaIndex, roomCount, pathCount);

                currentChannel = backend.roomChannel;

                currentPath = CodingIslands[currentAreaIndex].Levels[currentLevelIndex].Paths[currentPathIndex];

                currentTileSet = new();

                for (int i = 0; i <= backend.CodingTileSetLevel; i++)
                {
                    codingUI.SetPoolSlotsVisible(TileSets[i].Tiles, true);
                    currentTileSet.AddRange(TileSets[i].Tiles);
                }

                if (!currentPath.TrySplitPathForArrows(currentTileSet, ref currentSubPaths))
                {
                    Debug.LogError("The current path is unsolvable with the provided set");
                    return;
                }

                if (gameState.firstLoad)
                {
                    if (backend.areaCompleted)
                    {
                        player.transform.position = CurrentSubPath.Start.Value;
                        player.transform.forward = CurrentSubPath.StartDirection;

                        currentGamePhase = GamePhase.CODING;
                        //messageSystem.ShowMessage(_complexPathConfig);
                    }
                    else
                    {
                        // Animazione
                        // TODO maybe always assume firstload is always 1 tile back
                        PlayerLocation location = firstLoadPlayerPositions[currentAreaIndex];

                        Vector3 newPosition = location.Position;
                        newPosition.y = player.transform.position.y;

                        player.transform.SetPositionAndRotation(newPosition, location.Rotation);

                        currentGamePhase = GamePhase.INTRO;
                    }
                }
                else
                {
                    player.transform.position = CurrentSubPath.Start.Value;
                    player.transform.forward = CurrentSubPath.StartDirection;

                    currentGamePhase = GamePhase.EXIT_ROOM;
                }

                UpdateProgressBar();
            }

            movementHandler.InitCurrentGround();
        }

        IEnumerator SessionTimer()
        {
            _timer = 0;
            while (_timer < explorationTime)
            {
                yield return null;
                _timer += Time.deltaTime;

                Vector2 anchorMax = timerFill.anchorMax;
                anchorMax.x = _timer / explorationTime;
                timerFill.anchorMax = anchorMax;
            }

            // Se scade il timer nel quartiere tutorial segnalo al server di passare a quello successivo,
            // segnalo l'area completata nel levelBackend perché venga visualizzato il cambiamento nel caso di modalità test,
            // imposto la fine della sessione minima in modo che in GameLogic venga poi chiusa la sessione come nel caso di 
            // completamento corretto del tutorial
            // TODO if the tutorial has it's own week, then it shouldn't be completed with a timeout
            if (currentAreaIndex == 0)
            {
                MarkAreaCompleted();
                GameState.Instance.levelBackend.areaCompleted = true;
                powerUnlocked = true;
            }

            GameEvent ev;
            ev.eventType = GameEventType.SESSION_END;
            ev.triggerObject = null;
            GameLogic(ev);
        }

        public void GameStart()
        {
            GameEvent ev;
            ev.eventType = GameEventType.PHASE_START;
            ev.triggerObject = null;

            GameLogic(ev);
        }

        public void StopAudio() => bigElloSays.StopAudio();

        private void MarkAreaCompleted()
        {
            if (GameState.Instance.testMode) return;

            GameState.Instance.levelBackend.SaveAreaCompleted(GameState.Instance.comprendoBackend.username,
                () => { },
                err => {
                    Debug.Log(err.Message);
                });
        }

        private void StartExploration()
        {
            if (GameState.Instance.testMode) return;

            GameState.Instance.levelBackend.Exploration(res => {
                Debug.Log("Timestamp inizio percorso: " + res.inizio);
                initPathTimestamp = res.inizio;
                initPathTime = DateTime.Now;
            },
            err => {
                Debug.Log(err.Message);
            });
        }

        public void TutorialEndPopupClose()
        {
            firstPowerPanel.SetActive(false);
            SceneManager.LoadScene("End");
        }

        public void SaveExplorationResult(bool correct, bool last)
        {
            if (correct && last) // Room reached
                GameState.Instance.CodingStars++;

            // HACK to avoid counting intro paths towards tile unlock
            if (GameState.Instance.levelBackend.island == Island.INTRODUZIONE) last = false;

            if (GameState.Instance.testMode) return;

            EndExplorationParams param = new EndExplorationParams();

            TimeSpan pathTime = DateTime.Now.Subtract(initPathTime);
            param.inizio = initPathTimestamp;
            param.quartiere = GameState.Instance.levelBackend.island.ToString();
            param.compito = pathHandler.GetExpectedPath();
            param.esecuzione = pathHandler.GetUserPath();
            param.corretto = correct;
            param.tempoImpiegato = (int)Math.Floor(pathTime.TotalSeconds);
            param.ombra = pathHandler.IsShadowEnabled();
            param.prospettiva = cameraHandler.GetCurentCamera();
            param.finale = correct && last;

            GameState.Instance.levelBackend.EndExploration(
                param,
                () => {
                    Debug.Log("Percorso: " + initPathTimestamp + " salvato con successo");

                    if (correct && !last)
                        StartExploration();
                },
                err => {
                    Debug.LogError(err.Message);
                }
            );
        }

        private bool TutorialPhaseLogic(GameEvent ev)
        {
            GameEventType eventType = ev.eventType;

            if (eventType == GameEventType.PHASE_START)
            {
                StartCoroutine(nameof(SessionTimer));

                progressBar.gameObject.SetActive(false);

                StartExploration();

                codingUI.SetPoolVisible(false);
                codingUI.SetInteractable(false);
                codingUI.SetPoolSlotVisible(ArrowState.UP, true);
                codingUI.SetPoolSlotVisible(ArrowState.RIGHT, true);

                cameraHandler.PanEnabled = false;
                cameraHandler.ToggleFrontCamera(false);

                helpButton.interactable = false;

                pathPanel.SetActive(true);

                ShowTutorialPath();

                messageSystem.SetCurrentMessagesBatch(_tutorialConfigs);
                messageSystem.ShowNextMessage();

                return false;
            }
            else if (eventType == GameEventType.HELP && messageSystem.LastShownMessageIndex > 5)
            {
                messageSystem.ShowCurrentMessage();
                return false;
            }

            int messageShownIndex = messageSystem.LastShownMessageIndex;

            switch (messageShownIndex)
            {
                case 0: // Welcome
                case 1: // Ello
                case 2: // Flag
                case 3: // Coding Pool
                case 4: // Coding List
                    if (eventType == GameEventType.MESSAGE_READ)
                        messageSystem.ShowNextMessage();
                    break;

                case 5: // Help Button
                    if (eventType == GameEventType.MESSAGE_READ)
                    {
                        helpButton.interactable = true;
                        codingUI.SetPoolSlotInteractable(ArrowState.UP, true);
                        codingUI.SetListInteractable(true);
                        messageSystem.ShowNextMessage();
                    }
                    break;

                case 6: // Drag UP
                    if (eventType == GameEventType.CODING_LIST_CHANGED)
                    {
                        //TODO check coding list
                        pathHandler.CodingListChanged();
                        codingUI.SetInteractable(false);
                        codingUI.SetPlayButtonInteractable(true);
                        messageSystem.ShowNextMessage();
                    }
                    break;

                case 7: // Play Button
                    if (eventType == GameEventType.PLAY)
                    {
                        bigElloSays.Hide();
                        cameraHandler.PanEnabled = false;
                        cameraHandler.ToggleFrontCamera(false);
                        cameraHandler.AttachCamera();
                        codingUI.SetInteractable(false);
                        helpButton.interactable = false;

                        pathHandler.PlaySequence();
                    }
                    else if (eventType == GameEventType.CORRECT_PATH)
                    {
                        currentPathIndex++;

                        ShowTutorialPath();
                        
                        StartExploration();

                        codingUI.SetPoolSlotInteractable(ArrowState.UP, true);
                        codingUI.SetListInteractable(true);
                        codingUI.SetPlayButtonInteractable(true);
                        helpButton.interactable = true;

                        messageSystem.ShowNextMessage();
                    }

                    break;

                case 8: // Drag UP UP Play
                    if (eventType == GameEventType.CODING_LIST_CHANGED)
                    {
                        pathHandler.CodingListChanged();
                    }
                    else if (eventType == GameEventType.PLAY)
                    {
                        bigElloSays.Hide();
                        cameraHandler.PanEnabled = false;
                        cameraHandler.ToggleFrontCamera(false);
                        cameraHandler.AttachCamera();
                        codingUI.SetInteractable(false);
                        helpButton.interactable = false;

                        pathHandler.PlaySequence();
                    }
                    else if (eventType == GameEventType.WRONG_PATH)
                    {
                        pathHandler.ResetPath();
                        pathHandler.ResetArrows();

                        codingUI.SetPoolSlotInteractable(ArrowState.UP, true);
                        codingUI.SetListInteractable(true);
                        codingUI.SetPlayButtonInteractable(true);
                        helpButton.interactable = true;

                        messageSystem.ShowMessage(_pathErrorConfig);
                    }
                    else if (eventType == GameEventType.CORRECT_PATH)
                    {
                        currentPathIndex++;

                        ShowTutorialPath();
                        
                        StartExploration();

                        codingUI.SetPoolSlotInteractable(ArrowState.RIGHT, true);
                        codingUI.SetListInteractable(true);
                        helpButton.interactable = true;

                        messageSystem.ShowNextMessage();
                    }
                    break;

                case 9: // Drag RIGHT
                    if (eventType == GameEventType.CODING_LIST_CHANGED)
                    {
                        pathHandler.CodingListChanged();

                        codingUI.SetPoolSlotInteractable(ArrowState.RIGHT, false);
                        codingUI.SetPoolSlotInteractable(ArrowState.UP, true);
                        codingUI.SetPlayButtonInteractable(true);

                        messageSystem.ShowNextMessage();
                    }
                    break;

                case 10: // Drag UP Play
                    if (eventType == GameEventType.CODING_LIST_CHANGED)
                    {
                        pathHandler.CodingListChanged();
                    }
                    else if (eventType == GameEventType.PLAY)
                    {
                        bigElloSays.Hide();
                        cameraHandler.PanEnabled = false;
                        cameraHandler.ToggleFrontCamera(false);
                        cameraHandler.AttachCamera();
                        codingUI.SetInteractable(false);
                        helpButton.interactable = false;

                        pathHandler.PlaySequence();
                    }
                    else if (eventType == GameEventType.WRONG_PATH)
                    {
                        pathHandler.ResetPath();
                        pathHandler.ResetArrows();

                        _ = codingUI.TryAddTileToList(ArrowState.RIGHT);

                        codingUI.SetPoolSlotInteractable(ArrowState.RIGHT, true);
                        codingUI.SetPoolSlotInteractable(ArrowState.UP, true);
                        codingUI.SetPlayButtonInteractable(true);
                        codingUI.SetListInteractable(true);
                        helpButton.interactable = true;

                        messageSystem.ShowMessage(_pathErrorConfig);
                    }
                    else if (eventType == GameEventType.MESSAGE_READ)
                    {
                        //We read the wrong path one shot message
                        messageSystem.TryShowCurrentMessage();
                    }
                    else if (eventType == GameEventType.CORRECT_PATH)
                    {
                        currentPathIndex++;

                        ShowTutorialPath();
                        
                        StartExploration();

                        codingUI.SetPoolSlotVisible(ArrowState.LEFT, true);

                        messageSystem.ShowNextMessage();
                    }
                    break;

                case 11: // Last path
                case 12: // LEFT
                case 13: // Clear Button
                case 14: // Camera Button
                    if (eventType == GameEventType.MESSAGE_READ)
                        messageSystem.ShowNextMessage();
                    break;

                case 15: // Ghost button
                    if (eventType == GameEventType.MESSAGE_READ)
                    {
                        codingUI.SetInteractable(true);
                        messageSystem.ShowNextMessage();
                    }
                    break;

                case 16: // Drag Last path Play
                    if (eventType == GameEventType.CODING_LIST_CHANGED)
                    {
                        pathHandler.CodingListChanged();
                    }
                    else if (eventType == GameEventType.GHOST && movementHandler.IsNotMoving())
                    {
                        pathHandler.EnableShadow();
                        pathHandler.MoveShadow();
                    }
                    else if (eventType == GameEventType.PLAY)
                    {
                        bigElloSays.Hide();
                        cameraHandler.PanEnabled = false;
                        cameraHandler.ToggleFrontCamera(false);
                        cameraHandler.AttachCamera();
                        codingUI.SetInteractable(false);
                        helpButton.interactable = false;

                        pathHandler.PlaySequence();
                    }
                    else if (eventType == GameEventType.WRONG_PATH)
                    {
                        pathHandler.ResetPath();

                        codingUI.SetInteractable(true);
                        helpButton.interactable = true;

                        messageSystem.ShowMessage(_pathErrorConfig);
                    }
                    else if (eventType == GameEventType.MESSAGE_READ)
                    {
                        //We read the wrong path one shot message
                        messageSystem.TryShowCurrentMessage();
                    }
                    else if (eventType == GameEventType.CORRECT_PATH)
                    {
                        cameraHandler.ToggleFrontCamera(true);
                        flag.SetActive(false);
                        pathHandler.ResetArrows();
                        pathHandler.DisableShadow();

                        messageSystem.ShowNextMessage();
                    }
                    break;

                case 17: // You will unlock new tiles to move
                    //if (eventType == GameEventType.MESSAGE_READ)
                    //    messageSystem.ShowNextMessage();
                       MarkAreaCompleted();
                       if (eventType == GameEventType.MESSAGE_READ)
                           return true;
                    break;
                //case 18: // Let's go to the first island
                //    MarkAreaCompleted();
                //    if (eventType == GameEventType.MESSAGE_READ)
                //        return true;
                //    break;
            }
            return false;
        }

        private void ShowTutorialPath()
        {
            pathErrorNumber = 0;

            cameraHandler.ToggleFrontCamera(false);
            pathHandler.ResetArrows();
            pathHandler.DisableShadow();

            currentPath = tutorialPaths[currentPathIndex];
            currentSubPaths = new[] { currentPath.ToSubPath() };

            pathHandler.ShowNextPath(false);
        }

        private void PowerHandler(GameObject trigger)
        {
            if (currentAreaIndex > 0)
            {
                powerPanel.SetActive(false);
                quizPanelGroup.SetQuiz(trigger, OnQuizClosed);
                cameraHandler.PanEnabled = false;
            }
            else
            {
                firstPowerPanel.SetActive(true);
            }
        }

        private void OnQuizClosed()
        {
            StartCoroutine(nameof(SessionTimer));
            StartCoroutine(ShowNextCodingPath(_complexPathConfig));
        }

        private int pathErrorNumber = 0;

        IEnumerator ShowNextCodingPath(BigElloSaysConfig config)
        {
            if (config != null)
                messageSystem.ShowMessage(config);

            UpdateProgressBar();

            yield return new WaitUntil(movementHandler.IsNotMoving);

            pathPanel.SetActive(true);

            codingUI.SetInteractable(true);
            helpButton.interactable = true;

            cameraHandler.PanEnabled = true;

            bool lastSubPath = currentSubPathIndex == currentSubPaths.Length - 1;

            pathHandler.ShowNextPath(/*lastSubPath*/);

            StartExploration();
        }

        private bool CodingPhaseLogic(GameEvent ev)
        {
            switch (ev.eventType)
            {
                case GameEventType.PHASE_START:
                    pathHandler.DisableShadow();
                    helpButton.interactable = false;
                    ghostButton.SetActive(true);

                    if (powerUnlocked && !GameState.Instance.levelBackend.areaCompleted)
                    {
                        powerPanel.SetActive(true);
                        customization.ToggleHairColor((Island)currentAreaIndex, true);

                        MarkAreaCompleted();

                        messageSystem.ShowMessage(_powerConfig);

                    }
                    else if (currentPathIndex == 0)
                    {
                        var batch = (Island)currentAreaIndex switch
                        {
                            Island.INTRODUZIONE => new[] { _complexPathConfig },
                            Island.CONTROLLO_INTERFERENZA => _interferenceIslandTutorialConfig,
                            Island.INIBIZIONE_RISPOSTA => _inhibitionIslandTutorialConfig,
                            Island.MEMORIA_LAVORO => _memoryIslandTutorialConfig,
                            Island.FLESSIBILITA_COGNITIVA => _flexibilityIslandTutorialConfig,
                            Island.MEDIA_LITERACY => _mediaLiteracyIslandTutorialConfig,
                            _ => null
                        };

                        if (batch != null && !GameState.Instance.levelBackend.areaCompleted)
                        {
                            _showingStartOfIslandMessages = true;
                            messageSystem.SetCurrentMessagesBatch(batch);
                            messageSystem.ShowNextMessage();
                        }
                        else if (!TryShowNextTileTutorial(out _tileTutorial))
                        {
                            StartCoroutine(nameof(SessionTimer));
                            StartCoroutine(ShowNextCodingPath(_complexPathConfig));
                        }
                    }
                    else if (!TryShowNextTileTutorial(out _tileTutorial))
                    {
                        StartCoroutine(nameof(SessionTimer));
                        StartCoroutine(ShowNextCodingPath(_complexPathConfig));
                    }

                    break;

                case GameEventType.MESSAGE_READ:
                    // In questo caso è stato visualizzato il messaggio che segnala di aver preso il trofeo
                    if (powerUnlocked && !GameState.Instance.levelBackend.areaCompleted)
                    {
                        GameState.Instance.levelBackend.areaCompleted = true;

                        // Moved this to on quiz closed?
                        //StartCoroutine(ShowNextCodingPath());
                    }
                    else if (_showingStartOfIslandMessages) //HACK flag to avoid having a full fsm state for this
                    {
                        if (!messageSystem.TryShowNextMessage())
                        {
                            _showingStartOfIslandMessages = false;

                            if (!TryShowNextTileTutorial(out _tileTutorial))
                            {
                                // Solo quando ricevuto dalla chiusura del pannello
                                if (ev.triggerObject != gameObject)
                                    cameraHandler.ToggleFrontCamera(false);

                                bigElloSays.Hide();

                                StartCoroutine(nameof(SessionTimer));
                                StartCoroutine(ShowNextCodingPath(_complexPathConfig));
                            }
                        }
                    }
                    else if (_tileTutorial != ArrowState.NONE)
                    {
                        if (!messageSystem.TryShowNextMessage())
                        {

                            ElliPrefs.SetArrowTutorialFlag(GameState.Instance.comprendoBackend.id.ToString(), _tileTutorial);

                            if (!TryShowNextTileTutorial(out _tileTutorial))
                            {
                                StartCoroutine(nameof(SessionTimer));
                                StartCoroutine(ShowNextCodingPath(_complexPathConfig));
                            }
                        }
                    }
                    else
                    {
                        // Solo quando ricevuto dalla chiusura del pannello
                        if (ev.triggerObject != gameObject)
                            cameraHandler.ToggleFrontCamera(false);

                        bigElloSays.Hide();
                    }
                    break;

                case GameEventType.GHOST:

                    if (movementHandler.IsNotMoving())
                    {
                        pathHandler.EnableShadow();
                        pathHandler.MoveShadow();
                    }

                    break;

                case GameEventType.WRONG_PATH:
                    // Camera da davanti
                    cameraHandler.ToggleFrontCamera(true);

                    pathErrorNumber++;

                    bool repeated = pathErrorNumber >= TRIAL_NUMBER && ghostButton.activeSelf;
                    messageSystem.ShowMessage(repeated ? _repeatedPathErrorConfig : _pathErrorConfig);

                    UpdateProgressBar();

                    pathHandler.ResetPath();
                    UpdateProgressBar();

                    StartExploration(); //TODO we need to wait this one, a new wrong path could trigger before this is over

                    codingUI.SetInteractable(true);
                    helpButton.interactable = true;
                    cameraHandler.PanEnabled = true;

                    break;

                case GameEventType.CORRECT_PATH:
                    // Camera da davanti
                    cameraHandler.ToggleFrontCamera(true);
                    pathErrorNumber = 0;
                    helpLevel = 0;
                    currentSubPathIndex++;

                    flag.SetActive(false);

                    if (currentSubPathIndex == currentSubPaths.Length)
                        return true;

                    UpdateProgressBar();

                    pathHandler.DisableShadow();

                    StartCoroutine(ShowNextCodingPath(_complexPathEndingConfig));
                    break;

                case GameEventType.CODING_LIST_CHANGED:
                    pathHandler.CodingListChanged();
                    break;

                case GameEventType.PLAY:
                    cameraHandler.ToggleFrontCamera(false);
                    cameraHandler.PanEnabled = false;
                    cameraHandler.AttachCamera();
                    codingUI.SetInteractable(false);
                    helpButton.interactable = false;

                    pathHandler.PlaySequence();
                    StopAudio();

                    GameEvent newev;
                    newev.eventType = GameEventType.MESSAGE_READ;
                    newev.triggerObject = gameObject;
                    HandleEvent(newev);
                    break;

                case GameEventType.HELP:
                    HelpLogic();
                    break;

                case GameEventType.POWER:
                    PowerHandler(ev.triggerObject);
                    break;

                case GameEventType.SESSION_END:
                    flag.SetActive(false);
                    pathHandler.DisableShadow();
                    pathHandler.ClearPath();

                    codingUI.SetInteractable(false);
                    helpButton.interactable = false;

                    player.transform.position = currentPath.End.Value;
                    movementHandler.InitCurrentGround();

                    return true;

            }

            return false;
        }

        private void HelpLogic()
        {
            float percentage = _timer / explorationTime;

            if (helpLevel == 0 || percentage < 0.33f)
            {
                messageSystem.ShowMessage(_ghostHelpConfig);

                pathHandler.EnableShadow();
                pathHandler.MoveShadow();

                helpLevel = 1;
            }
            else if (helpLevel == 1 || percentage < 0.66f)
            {
                pathHandler.HighlightPath();

                helpLevel = 2;
            }
            else
            {
                if (!CodingUtility.TryGetOptimalSolution(
                    CurrentSubPath, currentTileSet, out List<ArrowState> solution))
                {
                    Debug.LogError("Help failed to find a solution to the current path");
                    return;
                }

                StringBuilder builder = new();

                for (int i = 0; i < helpLevel && i < solution.Count; i++)
                {
                    builder.AppendFormat("<sprite name={0}>", solution[i]);
                }

                // Creating copies to avoid changes to the original or the asset
                BigElloSaysConfig instance = _tileHelpConfig.Clone();
                instance.Message = Instantiate(instance.Message);

                Dictionary<string, string> arguments = new() { { "sequence", builder.ToString() } };
                instance.Message.Message.Arguments = new[] { arguments };

                messageSystem.ShowMessage(instance);

                helpLevel++;
            }
        }

        private bool EnterRoomPhaseLogic(GameEvent ev, int messageIndex)
        {
            if (ev.eventType == GameEventType.PHASE_START)
            {
                if (currentChannel == RoomChannel.VERBALE)
                {
                    pathHandler.ClearPath();
                }

                messageSystem.ShowMessage(_enterRoomConfigs[messageIndex]);

            }
            else if (ev.eventType == GameEventType.MESSAGE_READ)
            {
                return true;
            }


            return false;
        }

        private bool ExitRoomPhaseLogic(GameEvent ev)
        {
            if (ev.eventType == GameEventType.PHASE_START)
            {
                int messageIndex = (2 * (currentAreaIndex - 1)) + currentPathIndex - 1;

                messageSystem.ShowMessage(_exitRoomConfigs[messageIndex]);
            }
            else if (ev.eventType == GameEventType.MESSAGE_READ)
            {
                return true;
            }

            return false;
        }

        private bool EndGamePhaseLogic(GameEvent ev)
        {
            if (ev.eventType == GameEventType.PHASE_START)
            {
                // In case we end the game but the timer is not expired
                StopCoroutine(nameof(SessionTimer));

                cameraHandler.ToggleFrontCamera(true);
                messageSystem.ShowMessage(_endSessionConfig);
            }
            else if (ev.eventType == GameEventType.MESSAGE_READ)
            {
                if (GameState.Instance.testMode)
                    SceneManager.LoadScene("RoomStart");
                else if (currentAreaIndex == 0)
                    firstPowerPanel.SetActive(true);
                else
                    endPopup.Show(() => SceneManager.LoadScene("End"));
            }

            return false;
        }

        private bool IntroPhaseLogic(GameEvent ev)
        {
            if (ev.eventType == GameEventType.PHASE_START)
            {
                cameraHandler.ToggleFrontCamera(false);
                StartCoroutine(movementHandler.MoveUpSequence(3));
            }
            else if (ev.eventType == GameEventType.PHASE_END)
            {
                return true;
            }

            return false;
        }

        public void HandleEvent(GameEvent ev)
        {
            GameLogic(ev);
        }

        private void GameLogic(GameEvent ev)
        {
            bool phaseEnd = false;
            int messageIndex = 0;

            int pathCount = CodingIslands[currentAreaIndex].Levels[currentLevelIndex].Paths.Count;
            bool lastCoding = currentPathIndex >= pathCount;

            if (ev.eventType == GameEventType.SESSION_END && lastCoding)
            {
                currentGamePhase = GamePhase.GAME_END;

                ev.eventType = GameEventType.PHASE_START;
                ev.triggerObject = null;
            }
            
            do
            {
                switch (currentGamePhase)
                {
                    case GamePhase.INTRO:
                        phaseEnd = IntroPhaseLogic(ev);
                        if (phaseEnd) currentGamePhase = GamePhase.CODING;
                        break;
                    case GamePhase.TUTORIAL:
                        phaseEnd = TutorialPhaseLogic(ev);
                        if (phaseEnd) currentGamePhase = GamePhase.GAME_END;
                        break;

                    case GamePhase.CODING:
                        phaseEnd = CodingPhaseLogic(ev);
                        if (phaseEnd)
                        {
                            currentPathIndex++;
                            messageIndex = 2 * (currentAreaIndex - 1) + currentPathIndex - 1;

                            if (currentPathIndex >= CodingIslands[currentAreaIndex].Levels[currentLevelIndex].Paths.Count)
                            {
                                if (GameState.Instance.levelBackend.MediaLiteracyEnabled && currentAreaIndex > (int)Island.CONTROLLO_INTERFERENZA)
                                {
                                    currentGamePhase = GamePhase.ENTER_ROOM;
                                    messageIndex = 8;
                                }
                                else currentGamePhase = GamePhase.GAME_END;
                            }
                            else currentGamePhase = GamePhase.ENTER_ROOM;
                        }
                        break;

                    case GamePhase.ENTER_ROOM:
                        phaseEnd = EnterRoomPhaseLogic(ev, messageIndex);
                        if (phaseEnd)
                        {
                            // Lascio finire il gestore eventi corrente
                            currentGamePhase = GamePhase.SCENE_END;

                            // Salvo le info persistenti
                            GameState.Instance.firstLoad = false;

                            GameState.Instance.levelBackend.roomChannel = currentChannel;

                            GameState.Instance.playerPosition = player.transform.position;
                            GameState.Instance.playerRotation = player.transform.rotation;

                            GameState.Instance.pathNumber = currentPathIndex;

                            cameraHandler.SaveCameraInfo();

                            // Passo nella scena per la selezione della stanza
                            SceneManager.LoadScene("RoomStart");
                        }
                        break;

                    case GamePhase.EXIT_ROOM:
                        phaseEnd = ExitRoomPhaseLogic(ev);
                        if (phaseEnd)
                        {
                            currentGamePhase = GamePhase.CODING;

                            //messageSystem.ShowMessage(_complexPathConfig);

                            currentChannel++;

                            LevelBackend backend = GameState.Instance.levelBackend;

                            backend.lastExercizeResult = EndExercizeSaveResponse.NONE;

                            bool mlChannelEnabled = backend.MediaLiteracyEnabled && currentAreaIndex > (int)Island.CONTROLLO_INTERFERENZA;

                            if (currentChannel > RoomChannel.VERBALE)
                            {
                                powerUnlocked = true;

                                if (!mlChannelEnabled || currentChannel > RoomChannel.MEDIA_LITERACY)
                                    currentChannel = RoomChannel.VISIVO;
                            }
                        }
                        break;

                    case GamePhase.GAME_END:
                        phaseEnd = EndGamePhaseLogic(ev);
                        break;

                    case GamePhase.SCENE_END:
                        phaseEnd = false;
                        break;
                }

                if (phaseEnd)
                {
                    ev.eventType = GameEventType.PHASE_START;
                    ev.triggerObject = null;
                }

            } while (phaseEnd);
        }

        public void UpdateProgressBar()
        {
            bool tutorialCompleted = GameState.Instance.levelBackend.completedIslands.Contains(Island.INTRODUZIONE);

            //HACK to fix HACK in movementHandler :)
            if (currentAreaIndex == (int)Island.INTRODUZIONE && !tutorialCompleted) return;

            float fillAmount;

            //HACK for Exit from ML room (= entering coding with no current path)
            if (currentPath != null)
            {
                float totalDistance = Vector3.Distance(CurrentSubPath.Start.Value, CurrentSubPath.End.Value);
                float distance = Vector3.Distance(player.transform.position, CurrentSubPath.End.Value);
                float subPathProgress = 0.01f + Mathf.InverseLerp(totalDistance, 0, distance);
                fillAmount = (currentSubPathIndex + subPathProgress) / currentSubPaths.Length;
            }
            else
            {
                fillAmount = 1;
            }

            progressBar.Fill(currentPathIndex, fillAmount);
        }

        private bool TryShowNextTileTutorial(out ArrowState tile)
        {
            tile = ArrowState.NONE;

            foreach (ArrowState entry in currentTileSet)
            {
                if (ElliPrefs.GetArrowTutorialFlag(GameState.Instance.comprendoBackend.id.ToString(), entry))
                    continue;

                tile = entry;

                var tutorial = Array.Find(tileTutorials, t => t.Tile == entry)?.tutorial;

                if (tutorial != null)
                {
                    messageSystem.SetCurrentMessagesBatch(tutorial);
                    messageSystem.ShowNextMessage();

                    return true;
                }
            }

            return false;
        }
    }
}
