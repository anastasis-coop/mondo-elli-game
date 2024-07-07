using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;

namespace Room8
{
    [Serializable]
    public class Number
    {
        public int number;
        public LocalizedAudioClip localizedVoiceM;
        public LocalizedAudioClip localizedVoiceF;
    }

    [Serializable]
    public class Letter
    {
        public string letter;
        public LocalizedAudioClip localizedClip;
    }

    public enum SetType { ODD, EVEN, VOCAL, CONSONANT };

    [Serializable]
    public class SetDescription
    {
        public SetType type;
        public Sprite image;
        public LocalizedAudioClip localizedClip;
    }

    public class Room8_BaseGameController : MonoBehaviour
    {

        public Timer levelTimer;
        public Bomb bomb;
        public Score levelScore;

        public bool helpRequested = false;

        public Number[] numbers;

        public GameObject energyBar;

        // public GameObject helpButton;

        public MessageSystem messagePanel;

        public AudioClip failSound;
        public AudioClip goodSound;
        public AudioClip initSelectionSound;

        public AudioSource voiceSource;
        public AudioSource effectSource;

        [SerializeField]
        protected RoomLevel level;

        [SerializeField]
        private RoomEndPopup endPopup;

        private bool answerTimerStatus;
        private bool levelTimerStatus;

        void Start()
        {
#if UNITY_EDITOR
            if (!GameState.StartedFromThisScene)
#endif
                level = GameState.Instance.levelBackend.roomLevel;

            GoToLevel();
        }

        public virtual void GoToLevel()
        {

        }

        public void RequestHelp()
        {
            helpRequested = true;
            answerTimerStatus = bomb.gameObject.activeSelf;
            levelTimerStatus = levelTimer.activation;
            bomb.gameObject.SetActive(false);
            levelTimer.SetActivation(false);
            messagePanel.messageRead.RemoveAllListeners();
            GoToLevel();
        }

        public void HelpRequestEnded(UnityAction<int> messageHandler)
        {
            messagePanel.messageRead.AddListener(messageHandler);
            bomb.gameObject.SetActive(answerTimerStatus);
            levelTimer.SetActivation(levelTimerStatus);
        }

        public virtual void RightAnswer()
        {
            levelScore.RightCounter++;
            effectSource.clip = goodSound;
            effectSource.Play();
        }

        public virtual void WrongAnswer()
        {
            levelScore.WrongCounter++;
            effectSource.clip = failSound;
            effectSource.Play();
        }

        public void MissedAnswer()
        {
            levelScore.MissedCounter++;
            effectSource.clip = failSound;
            effectSource.Play();
        }

        public void levelEnd()
        {
            endPopup.Show(SaveResultsAndExit);
        }

        private void SaveResultsAndExit()
        {
            if (GameState.Instance.testMode)
            {
                GameState.Instance.LoadSceneAfterRoom();
            }
            else
            {
                GameState.Instance.levelBackend.ExitRoom(Mathf.RoundToInt(levelTimer.totalTime - levelTimer.timeLeft), levelScore,
                () => {
                    GameState.Instance.LoadSceneAfterRoom();
                },
                err => {
                    Debug.Log(err.Message);
                });
            }
        }

        public void ShowMessage(BigElloSaysConfig config)
        {
            messagePanel.ShowMessage(config);
        }
    }

}
