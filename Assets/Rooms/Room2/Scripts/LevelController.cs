using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Room2
{
    public class LevelController : MonoBehaviour
    {
        public RoomLevel level;
        public DemoController demo;
        public TableController table;
        [SerializeField] private GameObject receptionWindow;
        [SerializeField] private GameObject receptionDoubleWindow;
        [SerializeField] private GameObject receptionContainers;
        [SerializeField] private GameObject airportButton;
        [SerializeField] private List<Transform> containerFeedbackTransforms;
        [SerializeField] private float _wrongAnswerCooldownInSecs = 0.5f;
        public SoundsController soundsController;
        public SoundsController auxSoundsController;
        public FeedbackController feedback;
        public Button singleManager;
        public AudioSource ambience;
        public SelectionController selection;
        public VoiceDemo voiceDemo;
        public FrameController frames;
        public Timer timer;
        public Bomb bomb;
        [SerializeField]
        private TimerBar timerBar;

        public Score score;

        public int rightRingtoneProbability = 50;
        public Vector3 managerFeedbackPosition;

        public float lingeringReceptionistsTime = 2;
        public float lingeringPhonesTime = 1;
        
        private bool canOverlap;
        private string target;
        private bool batteryWithTarget;
        public bool answerGiven;

        private bool manVoiceDemo = false;
        private bool sequential;
        bool levelStarted;
        
        [SerializeField]
        private RoomEndPopup endPopup;

        [SerializeField]
        private BigElloSaysConfig _endExercise;

        [Header("Tutorial")] [SerializeField] private MessageSystem messageSystem;
        
        [SerializeField] private List<BigElloSaysConfig> _tutorial_0_1;
        [SerializeField] private List<BigElloSaysConfig> _tutorial_0_2;
        [SerializeField] private List<BigElloSaysConfig> _tutorial_1_1;
        [SerializeField] private List<BigElloSaysConfig> _tutorial_1_2;
        [SerializeField] private List<BigElloSaysConfig> _replayRingtonePhone_tutorial;
        [SerializeField] private List<BigElloSaysConfig> _replayRingtonePhone2_tutorial;
        [SerializeField] private List<BigElloSaysConfig> _tutorial_2_1;
        [SerializeField] private List<BigElloSaysConfig> _replayOperatorTutorial_2_1;
        [SerializeField] private List<BigElloSaysConfig> _tutorial_2_2;
        [SerializeField] private List<BigElloSaysConfig> _replayOperatorTutorial_2_2;
        [SerializeField] private List<BigElloSaysConfig> _tutorial_3_1;
        [SerializeField] private List<BigElloSaysConfig> _tutorial_3_2;
        [SerializeField] private List<BigElloSaysConfig> _replayOperatorTutorial_3_X;

        public Animation cameraAnimation;

        private IEnumerator Start()
        {
#if UNITY_EDITOR
            if (!GameState.StartedFromThisScene)
#endif
                level = GameState.Instance.levelBackend.roomLevel;

            messageSystem.messageRead.AddListener(OnMessageRead);
            messageSystem.messageShown.AddListener(OnMessageShown);

            switch (level)
            {
                case RoomLevel.LEVEL_01:
                    yield return soundsController.LoadAudioClips("Ringtones/Level01");
                    table.NumberOfObjects = TableController.AllowedNumbers.Phones_6;
                    table.AllowDistractions = false;
                    messageSystem.yellowButtonClicked.AddListener(ReplayPhoneRingtoneSlides);
                    messageSystem.greenButtonClicked.AddListener(StartPhonesGame);
                    messageSystem.SetCurrentMessagesBatch(_tutorial_0_1);
                    messageSystem.ShowNextMessage();
                    break;
                case RoomLevel.LEVEL_02:
                    yield return soundsController.LoadAudioClips("Ringtones/Level02");
                    table.NumberOfObjects = TableController.AllowedNumbers.Phones_8;
                    table.AllowDistractions = true;
                    messageSystem.yellowButtonClicked.AddListener(ReplayPhoneRingtoneSlides);
                    messageSystem.greenButtonClicked.AddListener(StartPhonesGame);
                    messageSystem.SetCurrentMessagesBatch(_tutorial_0_2);
                    messageSystem.ShowNextMessage();
                    break;
                case RoomLevel.LEVEL_11:
                    yield return soundsController.LoadAudioClips("Ringtones/Level11");
                    table.NumberOfObjects = TableController.AllowedNumbers.PhoneTabletsAndLaptops_15;
                    table.AllowDistractions = true;
                    messageSystem.yellowButtonClicked.AddListener(ReplayPhoneRingtoneSlides);
                    messageSystem.greenButtonClicked.AddListener(StartPhonesGame);
                    messageSystem.SetCurrentMessagesBatch(_tutorial_1_1);
                    messageSystem.ShowNextMessage();
                    break;
                case RoomLevel.LEVEL_12:
                    yield return soundsController.LoadAudioClips("Ringtones/Level12");
                    table.NumberOfObjects = TableController.AllowedNumbers.PhoneTabletsAndLaptops_15;
                    table.AllowDistractions = true;
                    messageSystem.yellowButtonClicked.AddListener(ReplayPhoneRingtoneSlides2);
                    messageSystem.greenButtonClicked.AddListener(StartPhonesGame);
                    messageSystem.SetCurrentMessagesBatch(_tutorial_1_2);
                    messageSystem.ShowNextMessage();
                    break;
                case RoomLevel.LEVEL_21:
                    yield return soundsController.LoadAudioClips("Sentences/Level21");
                    messageSystem.greenButtonClicked.AddListener(StartLevel21);
                    messageSystem.SetYellowButtonText("RIASCOLTA OPERATORE");
                    messageSystem.yellowButtonClicked.AddListener(ReplayOperatorTutorial_2_1);
                    messageSystem.SetCurrentMessagesBatch(_tutorial_2_1);
                    messageSystem.ShowNextMessage();
                    break;
                case RoomLevel.LEVEL_22:
                    yield return soundsController.LoadAudioClips("Sentences/Level22");
                    selection.LoadTargetsAndDistractors("Objects/Level22");
                    messageSystem.greenButtonClicked.AddListener(StartLevel22);
                    messageSystem.SetYellowButtonText("RIASCOLTA OPERATORE");
                    messageSystem.yellowButtonClicked.AddListener(ReplayOperatorTutorial_2_2);
                    messageSystem.SetCurrentMessagesBatch(_tutorial_2_2);
                    messageSystem.ShowNextMessage();
                    break;
                case RoomLevel.LEVEL_31:
                    yield return soundsController.LoadAudioClips("Sentences/Level31/Man");
                    yield return auxSoundsController.LoadAudioClips("Sentences/Level31/Woman");
                    selection.LoadTargetsAndDistractors("Objects/Level31");
                    messageSystem.SetYellowButtonText("RIASCOLTA OPERATORE");
                    messageSystem.yellowButtonClicked.AddListener(ReplayOperatorTutorial_3_X);
                    messageSystem.greenButtonClicked.AddListener(StartLevel3x);
                    SetupLevel3X();
                    messageSystem.SetCurrentMessagesBatch(_tutorial_3_1);
                    messageSystem.TryShowNextMessage();
                    break;
                case RoomLevel.LEVEL_32:
                    yield return soundsController.LoadAudioClips("Sentences/Level32/Man");
                    yield return auxSoundsController.LoadAudioClips("Sentences/Level32/Woman");
                    selection.LoadTargetsAndDistractors("Objects/Level32");
                    messageSystem.redButtonClicked.AddListener(ReplayTutorial32FromStart);
                    messageSystem.SetYellowButtonText("RIASCOLTA OPERATORE");
                    messageSystem.yellowButtonClicked.AddListener(ReplayOperatorTutorial_3_X);
                    messageSystem.greenButtonClicked.AddListener(StartLevel3x);
                    SetupLevel3X();
                    messageSystem.SetCurrentMessagesBatch(_tutorial_3_2);
                    messageSystem.TryShowNextMessage();
                    break;
            }
        }

        private void SetCameraToTable()
        {
            cameraAnimation.Play("MainCameraZoomIn");
        }

        private void SetCameraToManager()
        {
            cameraAnimation.Play("MainCameraToManager");
        }

        private void SetCameraToDesk()
        {
            cameraAnimation.Play("MainCameraToDesk");
        }

        private void EnableTimerBar()
        {
            timerBar.gameObject.SetActive(true);
        }

        public void BigElloTalked()
        {
            messageSystem.oneShotRead.RemoveListener(BigElloTalked);

            if (timer.itIsEnd)
            {
                airportButton.GetComponent<Button>().interactable = false;
                endPopup.Show(saveResultsAndExit);
            }
            else
            {
                switch (level)
                {
                    case RoomLevel.LEVEL_01:
                        target = "Phone4";
                        //demo.StartDemo(DemoController.DemoType.DemoPhone4);
                        break;
                    case RoomLevel.LEVEL_02:
                        target = "Phone4";
                        //demo.StartDemo(DemoController.DemoType.DemoPhone4);
                        break;
                    case RoomLevel.LEVEL_11:
                        target = "Phone4";
                        //demo.StartDemo(DemoController.DemoType.DemoPhone4);
                        break;
                    case RoomLevel.LEVEL_12:
                        target = "Phone5";
                        //demo.StartDemo(DemoController.DemoType.DemoPhone5);
                        break;
                    case RoomLevel.LEVEL_21:
                        //demo.StartDemo(DemoController.DemoType.DemoManager21);
                        break;
                    case RoomLevel.LEVEL_22:
                        //demo.StartDemo(DemoController.DemoType.DemoManager22);
                        break;
                    case RoomLevel.LEVEL_31:
                        // SetupLevel3X();
                        // StartTutorial31();
                        break;
                    case RoomLevel.LEVEL_32:
                        // SetupLevel3X();
                        // StartVoicesDemo();
                        break;
                }
            }
        }

        public void PlayFinished()
        {
            switch (level)
            {
                case RoomLevel.LEVEL_01:
                case RoomLevel.LEVEL_02:
                case RoomLevel.LEVEL_11:
                case RoomLevel.LEVEL_12:
                    PlayFinishedForPhonesLevels();
                    break;
                case RoomLevel.LEVEL_21:
                    PlayFinishedForLevel21();
                    break;
                case RoomLevel.LEVEL_22:
                    PlayFinishedForLevel22();
                    break;
                case RoomLevel.LEVEL_31:
                case RoomLevel.LEVEL_32:
                    PlayFinishedForLevel3x();
                    break;
            }
        }

        public void ObjectSelected(int index, string name)
        {
            if (!answerGiven)
            {
                Vector3 feedbackPos;
                if (level == RoomLevel.LEVEL_22)
                {
                    feedbackPos = new Vector3(-878.5f + index * 5.5f, 6f, -12f);
                    if (containerFeedbackTransforms.Count - 1 >= index)
                        feedbackPos = containerFeedbackTransforms[index].position;
                }
                else
                {
                    feedbackPos = new Vector3(-874f + index * 10f, 6f, -12f);
                    if (containerFeedbackTransforms.Count - 1 >= index)
                        feedbackPos = containerFeedbackTransforms[index].position;
                }

                if (name == target)
                {
                    RightAnswer();
                    feedback.ShowRightFeedback(feedbackPos, containerFeedbackTransforms[index].GetChild(0).gameObject);
                }
                else
                {
                    WrongAnswer();
                    feedback.ShowWrongFeedback(feedbackPos, containerFeedbackTransforms[index].GetChild(0).gameObject);
                }
            }
        }

        private void InitAnswer()
        {
            answerGiven = false;
        }

        private void RightAnswer()
        {
            score.RightCounter++;
            answerGiven = true;
        }

        private void WrongAnswer()
        {
            score.WrongCounter++;
            answerGiven = true;
        }

        private void SingleManagerRightAnswer()
        {
            score.RightCounter++;
        }

        private void SingleManagerWrongAnswer()
        {
            score.WrongCounter++;
        }

        private void CheckMissedAnswer()
        {
            if (!answerGiven)
            {
                feedback.ShowWrongFeedback(managerFeedbackPosition);
                score.MissedCounter++;
            }
        }

        private void EndExercise()
        {
            levelStarted = false;

            selection.HidePanel();
            messageSystem.oneShotRead.AddListener(BigElloTalked);
            messageSystem.ShowMessage(_endExercise);
        }

        // THIS PART IS FOR LEVELS 01 02 11 12

        private void InitPhonesLevels()
        {
            SetCameraToTable();
        }

        private void StartPhonesBattery(bool concurrentTones)
        {
            InitAnswer();
            canOverlap = concurrentTones;
            batteryWithTarget = (Random.Range(0, 100) < rightRingtoneProbability);
            if (concurrentTones)
            {
                // Two sounds (second starts after 1 sec. delay)
                if (batteryWithTarget)
                {
                    // Including target
                    soundsController.PlayTwoRandomAudioClipsIncluding(target, 1f);
                }
                else
                {
                    // Excluding target
                    soundsController.PlayTwoRandomAudioClipsExcluding(target, 1f);
                }
            }
            else
            {
                if (batteryWithTarget)
                {
                    // Play target sound
                    soundsController.PlayAudioClip(target);
                }
                else
                {
                    // Play any other sound
                    soundsController.PlayRandomAudioClipExcluding(target);
                }
            }
        }

        public void SelectedPhone(Transform phone)
        {
            if (!answerGiven && levelStarted)
            {
                if (phone.name == target && soundsController.isAudioClipPlaying(target))
                {
                    RightAnswer();
                    feedback.ShowRightFeedback(phone.position, phone.gameObject);
                }
                else
                {
                    WrongAnswer();
                    feedback.ShowWrongFeedback(phone.position, phone.gameObject);
                }
            }
        }

        private void NextPhonesBattery()
        {
            bomb.gameObject.SetActive(false);
            
            if (batteryWithTarget)
            {
                CheckMissedAnswer();
            }

            if (timer.itIsEnd)
            {
                EndExercise();
            }
            else
            {
                table.ShowNextGroup();
                StartPhonesBattery(canOverlap);
            }
        }

        private void PlayFinishedForPhonesLevels()
        {
            bomb.gameObject.SetActive(true);
            bomb.timeoutSeconds = lingeringPhonesTime;
            bomb.StartCountdown();
            Invoke(nameof(NextPhonesBattery), lingeringPhonesTime);
        }

        // THIS PART IS FOR LEVEL 21

        void StartLevel21()
        {
            airportButton.GetComponent<Button>().interactable = true;
            //singleManager.gameObject.SetActive(true);
            timer.activation = true;
            EnableTimerBar();
            ambience.Play();
            ManagerTalksAfterRandomDelay();
        }

        private void InitLevel21()
        {
            SetCameraToManager();
            airportButton.SetActive(true);
            airportButton.GetComponent<Button>().interactable = false;
            receptionContainers.SetActive(false);
            receptionWindow.SetActive(true);
            receptionDoubleWindow.SetActive(false);
        }

        void ManagerTalksAfterRandomDelay()
        {
            Invoke(nameof(ManagerTalks), Random.Range(2.5f, 5f));
        }

        void ManagerTalks()
        {
            InitAnswer();
            soundsController.PlaySequentialAudioClip();
        }

        public void SingleManagerClicked()
        {
            if (!answerGiven)
            {
                if (!soundsController.isPlaying())
                {
                    StartCoroutine(HandleSingleManagerWrongAnswer());
                }
                else
                {
                    answerGiven = true;
                    SingleManagerRightAnswer();
                    feedback.ShowRightFeedback(managerFeedbackPosition);
                }
            }
        }

        private IEnumerator HandleSingleManagerWrongAnswer()
        {
            SingleManagerWrongAnswer();
            feedback.ShowWrongFeedback(managerFeedbackPosition);
            answerGiven = true;
            yield return new WaitForSeconds(_wrongAnswerCooldownInSecs);
            if (!soundsController.isPlaying()) answerGiven = false;
        }

        private void PlayFinishedForLevel21()
        {
            CheckMissedAnswer();
            if (timer.itIsEnd)
            {
                EndExercise();
            }
            else
            {
                ManagerTalksAfterRandomDelay();
            }

            answerGiven = false;
        }

        // THIS PART IS FOR LEVEL 22

        void StartLevel22()
        {
            timer.activation = true;
            EnableTimerBar();
            StartRandomObjectBattery();
        }

        private void InitLevel22()
        {
            airportButton.SetActive(false);
            receptionContainers.SetActive(true);
            receptionWindow.SetActive(true);
            receptionDoubleWindow.SetActive(false);
            SetCameraToManager();
        }

        private void StartRandomObjectBattery()
        {
            InitAnswer();
            target = soundsController.PlayShuffledAudioClip();
            selection.ShowNextGroup(target);
        }

        private void PlayFinishedForLevel22()
        {
            bomb.gameObject.SetActive(true);
            bomb.timeoutSeconds = lingeringReceptionistsTime;
            bomb.StartCountdown();
            Invoke(nameof(NextRandomObjectsBattery), lingeringReceptionistsTime);
        }

        private void NextRandomObjectsBattery()
        {
            bomb.gameObject.SetActive(false);
            CheckMissedAnswer();
            if (timer.itIsEnd)
            {
                EndExercise();
            }
            else
            {
                StartRandomObjectBattery();
            }
        }

        // THIS PART IS COMMON FOR LEVELS 3x

        private void SetupLevel3X()
        {
            SetCameraToDesk();
            airportButton.SetActive(false);
            receptionContainers.SetActive(true);
            receptionWindow.SetActive(false);
            receptionDoubleWindow.SetActive(true);
        }

        void StartVoicesDemo()
        {
            manVoiceDemo = true;
            frames.gameObject.SetActive(true);
            frames.SelectFrame(FrameController.FrameType.Left);
            voiceDemo.PlayManVoiceDemo();
        }

        public void VoiceDemoFinished()
        {
            if (manVoiceDemo)
            {
                frames.SelectFrame(FrameController.FrameType.Right);
                voiceDemo.PlayWomanVoiceDemo();
                manVoiceDemo = false;
                messageSystem.TryShowNextMessage();
            }
            else
            {
                if (level == RoomLevel.LEVEL_32)
                    StartLevel3x();
                else
                {
                    messageSystem.TryShowNextMessage();
                }
            }
        }

        void StartLevel3x()
        {
            timer.activation = true;
            EnableTimerBar();
            switch (level)
            {
                case RoomLevel.LEVEL_31:
                    sequential = false;
                    StartObjectSelectionTwoTalksBattery();
                    break;
                case RoomLevel.LEVEL_32:
                    ambience.Play();
                    sequential = true;
                    StartObjectSelectionTwoTalksBattery();
                    break;
            }
        }

        private void StartObjectSelectionTwoTalksBattery()
        {
            InitAnswer();
            bool targetIsMan = (Random.Range(0, 2) == 0);
            frames.SelectFrame(targetIsMan ? FrameController.FrameType.Left : FrameController.FrameType.Right);
            // Right target starts immediately, distracting voice after 1 second
            float manDelay = targetIsMan ? 0 : 1f;
            float womanDelay = targetIsMan ? 1f : 0;
            string manTarget;
            string womanTarget;
            if (sequential)
            {
                manTarget = soundsController.PlaySequentialAudioClip(manDelay);
                womanTarget = auxSoundsController.PlaySequentialAudioClip(womanDelay);
            }
            else
            {
                manTarget = soundsController.PlayShuffledAudioClip(manDelay);
                womanTarget = auxSoundsController.PlayShuffledAudioClip(womanDelay);
            }

            target = targetIsMan ? manTarget : womanTarget;
            selection.ShowNextGroup(target);
        }

        private void PlayFinishedForLevel3x()
        {
            if (!soundsController.isPlaying() && !auxSoundsController.isPlaying())
            {
                bomb.gameObject.SetActive(true);
                bomb.timeoutSeconds = lingeringReceptionistsTime;
                bomb.StartCountdown();
                Invoke(nameof(NextObjectSelectionTwoTalksBattery), lingeringReceptionistsTime);
            }
        }

        private void NextObjectSelectionTwoTalksBattery()
        {
            bomb.gameObject.SetActive(false);
            CheckMissedAnswer();
            if (timer.itIsEnd)
            {
                EndExercise();
            }
            else
            {
                StartObjectSelectionTwoTalksBattery();
            }
        }

        private void saveResultsAndExit()
        {
            if (GameState.Instance.testMode)
            {
                GameState.Instance.LoadSceneAfterRoom();
            }
            else
            {
                GameState.Instance.levelBackend.ExitRoom(timer.totalTime, score,
                    () => { GameState.Instance.LoadSceneAfterRoom(); },
                    err => { Debug.Log(err.Message); });
            }
        }

        public void OnMessageRead(int readMessageIndex)
        {
            switch (level)
            {
                case RoomLevel.LEVEL_01:
                case RoomLevel.LEVEL_02:
                    switch (readMessageIndex)
                    {
                        case 0:
                            target = "Phone4";
                            demo.Phone4.SetActive(true);
                            messageSystem.ShowNextMessage();
                            break;
                        case 1:
                            demo.PlayAudio(demo.RingtonePhone4, true);
                            messageSystem.ShowNextMessage();
                            break;
                        case 2:
                            demo.DemoEnded();
                            InitPhonesLevels();
                            messageSystem.ShowNextMessage();
                            break;
                        case 3:
                        case 4:
                            messageSystem.ShowNextMessage();
                            break;
                        default:
                            messageSystem.ShowNextMessage();
                            break;
                    }

                    break;
                case RoomLevel.LEVEL_11:
                    switch (readMessageIndex)
                    {
                        case 0:
                            target = "Phone4";
                            demo.Phone4.SetActive(true);
                            messageSystem.ShowNextMessage();
                            break;
                        case 1:
                            demo.PlayAudio(demo.RingtonePhone4, true);
                            messageSystem.ShowNextMessage();
                            break;
                        case 2:
                            demo.DemoEnded();
                            InitPhonesLevels();
                            messageSystem.ShowNextMessage();
                            break;
                        case 3:
                        case 4:
                        case 5:
                        default:
                            messageSystem.ShowNextMessage();
                            break;
                    }
                    break;
                case RoomLevel.LEVEL_12:
                    switch (readMessageIndex)
                    {
                        case 0:
                            target = "Phone5";
                            demo.Phone5.SetActive(true);
                            messageSystem.ShowNextMessage();
                            break;
                        case 1:
                            demo.PlayAudio(demo.RingtonePhone5, true);
                            messageSystem.ShowNextMessage();
                            break;
                        case 2:
                            demo.DemoEnded();
                            messageSystem.ShowNextMessage();
                            break;
                        default:
                            messageSystem.ShowNextMessage();
                            break;
                    }

                    break;
                case RoomLevel.LEVEL_21:
                    switch (readMessageIndex)
                    {
                        case 0:
                            InitLevel21();
                            messageSystem.ShowNextMessage();
                            break;
                        default:
                            messageSystem.TryShowNextMessage();
                            break;
                    }

                    break;
                case RoomLevel.LEVEL_22:
                    switch (readMessageIndex)
                    {
                        case 0:
                            InitLevel22();
                            messageSystem.ShowNextMessage();
                            break;
                        default:
                            messageSystem.TryShowNextMessage();
                            break;
                    }

                    break;
                case RoomLevel.LEVEL_31:
                    switch (readMessageIndex)
                    {
                        default:
                            messageSystem.TryShowNextMessage();
                            break;
                    }
                    break;
                case RoomLevel.LEVEL_32:
                    switch (readMessageIndex)
                    {
                        default:
                            messageSystem.TryShowNextMessage();
                            break;
                    }
                    break;
            }
        }

        private void HandleReplayRingtoneFlow(int closedTutorialMessageIndex)
        {
            switch (closedTutorialMessageIndex)
            {
                case 0:
                    demo.PlayAudio(level == RoomLevel.LEVEL_12 ? demo.RingtonePhone5 : demo.RingtonePhone4, true);
                    messageSystem.ShowNextMessage();
                    break;
                case 1:
                    demo.DemoEnded();
                    messageSystem.ShowNextMessage();
                    break;
                default:
                    messageSystem.ShowNextMessage();
                    break;
            }
        }

        private void StartPhonesGame()
        {
            demo.DemoEnded();
            messageSystem.messageRead.RemoveListener(OnMessageRead);
            messageSystem.messageRead.RemoveListener(HandleReplayRingtoneFlow);

            InitPhonesLevels();
            timer.activation = true;
            levelStarted = true;
            EnableTimerBar();
            StartPhonesBattery(concurrentTones: level is RoomLevel.LEVEL_02 or RoomLevel.LEVEL_12);
        }

        private void ReplayTutorial32FromStart()
        {
            messageSystem.SetCurrentMessagesBatch(_tutorial_3_1);
            messageSystem.RestartMessagesList();
        }

        private void ReplayPhoneRingtoneSlides()
        {
            messageSystem.messageRead.RemoveListener(OnMessageRead);
            messageSystem.messageRead.AddListener(HandleReplayRingtoneFlow);
            messageSystem.SetCurrentMessagesBatch(_replayRingtonePhone_tutorial);
            messageSystem.ShowNextMessage();
            SetCameraForDemo();
            if(target == "Phone4")
            {
                demo.Phone4.SetActive(true);
            }
            else if (target == "Phone5")
            {
                demo.Phone5.SetActive(true);
            }
                
            messageSystem.currentMessagesBatch.Last().ShowRedButton = false;
        }

        private void ReplayPhoneRingtoneSlides2()
        {
            messageSystem.messageRead.RemoveListener(OnMessageRead);
            messageSystem.messageRead.AddListener(HandleReplayRingtoneFlow);
            messageSystem.SetCurrentMessagesBatch(_replayRingtonePhone2_tutorial);
            SetCameraForDemo();
            if (target == "Phone4")
            {
                demo.Phone4.SetActive(true);
            }
            else if (target == "Phone5")
            {
                demo.Phone5.SetActive(true);
            }
            messageSystem.currentMessagesBatch.Last().ShowRedButton = false;
            messageSystem.ShowNextMessage();
        }

        private void OnMessageShown(int messageShownIndex)
        {
            if (messageShownIndex != 3) return;

            if (messageSystem.currentMessagesBatch == _tutorial_0_1
                       || messageSystem.currentMessagesBatch == _tutorial_0_2
                       || messageSystem.currentMessagesBatch == _tutorial_1_1
                       || messageSystem.currentMessagesBatch == _tutorial_1_2)
            {
                InitPhonesLevels();
            }
        }

        private static void SetCameraForDemo()
        {
            Camera.main.transform.transform.position = new Vector3(0, 2000, 0);
            Camera.main.transform.forward = new Vector3(0, 0, 360);
        }

        private void ReplayOperatorTutorial_2_1()
        {
            messageSystem.SetCurrentMessagesBatch(_replayOperatorTutorial_2_1);
            messageSystem.ShowNextMessage();
        }

        private void ReplayOperatorTutorial_2_2()
        {
            messageSystem.SetCurrentMessagesBatch(_replayOperatorTutorial_2_2);
            messageSystem.ShowNextMessage();
        }

        private void ReplayOperatorTutorial_3_X()
        {
            messageSystem.SetCurrentMessagesBatch(_replayOperatorTutorial_3_X);
            if (messageSystem.TryShowNextMessage())
            {
                switch (level)
                {
                    case RoomLevel.LEVEL_31:
                        messageSystem.currentMessagesBatch.Last().ShowRedButton = false;
                        break;
                    case RoomLevel.LEVEL_32:
                        messageSystem.currentMessagesBatch.Last().ShowRedButton = true;
                        break;
                }
            }
        }
    }
}