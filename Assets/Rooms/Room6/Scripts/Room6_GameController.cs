using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Room6
{
    public enum CategoryType { FRUTTA, ANIMALI, VESTITI, PARTI_DEL_CORPO, OGGETTI_DELLA_SCUOLA };

    [Serializable]
    public class VisualObject
    {
        public string name;
        public GameObject prefab;
        public Sprite image;
        public LocalizedAudioClip localizedAudioDescription;
    }

    [Serializable]
    public class Category
    {
        public CategoryType type;
        public Sprite image;
        public LocalizedAudioClip localizedAudioDescription;
        public LocalizedAudioClip localizedSelectionClip;
        public LocalizedAudioClip localizedDoubleSelectionClip;
        public VisualObject[] objectList;
    }

    public class Room6_GameController : MonoBehaviour
    {
        // Suoni di uso generale
        public AudioClip failSound;
        public AudioClip goodSound;
        public AudioClip selectionSound;
        public AudioClip answerTimeChime;

        // Pulsante per il riascolto
        public Button relistenButton;

        // Livello attuale
        public RoomLevel level;

        // Oggetto che gestisce il punteggio
        public Score score;

        // Timer e durata predefinita dei livelli (in secondi)
        public Timer timer;
        public Bomb Bomb;

        public GameObject energyBar;

        public RestartButton restartButton;

        public MessageSystem messagePanel;

        // Parametri di gioco
        public int NUM_TRIAL_PER_SPAN = 3;
        public int NUM_TRIAL_TO_WIN = 2;

        public GameObject level0_1;
        public GameObject level0_2;
        public GameObject level1_1;
        public GameObject level1_2;
        public GameObject level2_1;
        public GameObject level2_2;
        public GameObject level3_1;
        public GameObject level3_2;

        public GameObject sceneObj;

        // Messaggio da mostrare in caso di miss
        public BigElloSaysConfig WrongSelectionDirMessage;
        
        public VisualObject[] genericObjList;
        public VisualObject[] alfanumObjList;
        public Category[] categoryObjList;

        [HideInInspector]
        public Room6_InteractionHandler interactionHandler;
        [HideInInspector]
        public Room6_ObjectHandler objHandler;

        public AudioSource effectSource;
        public AudioSource mainSource;

        void Start()
        {
            interactionHandler = gameObject.GetComponent<Room6_InteractionHandler>();
            objHandler = gameObject.GetComponent<Room6_ObjectHandler>();

            if (NUM_TRIAL_TO_WIN > NUM_TRIAL_PER_SPAN)
                Debug.LogError("Impossibile vincere con numero di tentativi corretti maggiore di quello dei tentativi totali");

#if UNITY_EDITOR
            if (!GameState.StartedFromThisScene)
#endif
                level = GameState.Instance.levelBackend.roomLevel;

            switch (level)
            {
                case RoomLevel.LEVEL_01:
                    level0_1.SetActive(true);
                    break;
                case RoomLevel.LEVEL_02:
                    level0_2.SetActive(true);
                    break;
                case RoomLevel.LEVEL_11:
                    level1_1.SetActive(true);
                    break;
                case RoomLevel.LEVEL_12:
                    level1_2.SetActive(true);
                    break;
                case RoomLevel.LEVEL_21:
                    level2_1.SetActive(true);
                    break;
                case RoomLevel.LEVEL_22:
                    level2_2.SetActive(true);
                    break;
                case RoomLevel.LEVEL_31:
                    level3_1.SetActive(true);
                    break;
                case RoomLevel.LEVEL_32:
                    level3_2.SetActive(true);
                    break;
            }
        }

        public void SetHelpButtonState(bool state)
        {
            relistenButton.gameObject.SetActive(state);
        }

        public void SaveResultsAndExit()
        {
            if (GameState.Instance.testMode)
            {
                GameState.Instance.LoadSceneAfterRoom();
            }
            else
            {
                GameState.Instance.levelBackend.ExitRoom(timer.totalTime, score,
                () => {
                    GameState.Instance.LoadSceneAfterRoom();
                },
                err => {
                    Debug.Log(err.Message);
                });
            }
        }

        public void PlayAnswerTimeChime() => effectSource.PlayOneShot(answerTimeChime);

        public void PlayGood()
        {
            effectSource.clip = goodSound;
            effectSource.Play();
        }

        public void PlayWrong()
        {
            effectSource.clip = failSound;
            effectSource.Play();
        }

        public void PlaySelection()
        {
            effectSource.clip = selectionSound;
            effectSource.Play();
        }

    }

}
