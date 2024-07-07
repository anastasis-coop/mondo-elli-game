using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game {
    public class QuizPanelHandler : MonoBehaviour {

        public GameObject quizPanel;

        public Text titleText;
        public Text questionText;
        public ToggleGroup answerGroup;
        public Toggle answerPrefab;

        public GameObject scorePanel;
        public Text resultText;
        public Text scoreNumberText;

        [TextArea(0, 20)]
        public string correctQuizText;

        [TextArea(0, 20)]
        public string wrongQuizText;

        [TextArea (0,40)]
        public string[] questions;
  
        private Toggle correctToggle;
        private int id;
        private DateTime initQuizTime;

        private Button triggerButton;

        public Button sendAnswerButton;

        private Action _callback;

        public void SetQuiz(GameObject trigger, Action callback) {
            _callback = callback;
            sendAnswerButton.interactable = false;
            GameState.Instance.levelBackend.GetQuiz(res => {
                id = res.id;

                int currentArea = (int) GameState.Instance.levelBackend.island - 1; // Non considero il tutorial

                if (currentArea >= questions.Length) {
                    Debug.LogError("Quiz non disponibile per questa area");
                    return;
                }

                string title;

                switch (GameState.Instance.levelBackend.island) {
                    case Island.CONTROLLO_INTERFERENZA:
                        title = "Controllo delle interferenze";
                        break;
                    case Island.INIBIZIONE_RISPOSTA:
                        title = "Inibizione della risposta";
                        break;
                    case Island.MEMORIA_LAVORO:
                        title = "Memoria di lavoro";
                        break;
                    case Island.FLESSIBILITA_COGNITIVA:
                        title = "Flessibilità cognitiva";
                        break;
                    default:
                        title = "";
                        break;
                }

                titleText.text = title;
                questionText.text = questions[currentArea];

                foreach (string answer in res.situazioni) {
                    Toggle wobj = Instantiate(answerPrefab);
                    wobj.transform.SetParent(answerGroup.transform, false);
                    wobj.transform.GetComponentInChildren<Text>().text = answer;
                    wobj.group = answerGroup;
                    wobj.onValueChanged.AddListener(OnToggle);
                }

                int correctIndex = UnityEngine.Random.Range(0, res.situazioni.Count);

                correctToggle = Instantiate(answerPrefab);
                correctToggle.transform.SetParent(answerGroup.transform, false);
                correctToggle.transform.GetComponentInChildren<Text>().text = res.corretta;
                correctToggle.group = answerGroup;
                correctToggle.onValueChanged.AddListener(OnToggle);

                correctToggle.transform.SetSiblingIndex(correctIndex);

                initQuizTime = DateTime.Now;

                if (!trigger.TryGetComponent<Button>(out triggerButton))
                    Debug.LogError("Non è stato trovato un Button nell'oggetto trigger dei quiz");

                answerGroup.SetAllTogglesOff();

                quizPanel.SetActive(true);

            }, err => {
                Debug.Log(err.Message);
            });
        }

        public void OnToggle(bool _)
        {
            sendAnswerButton.interactable = answerGroup.AnyTogglesOn();
        }

        public void CompleteQuiz() {
            if (!answerGroup.AnyTogglesOn())
                return;
            sendAnswerButton.interactable = false;

            string answer = "";

            if (correctToggle.isOn) {
                Debug.Log("corretta");
                resultText.text = correctQuizText;
            } else {
                Debug.Log("non corretta");
                resultText.text = wrongQuizText;
            }                

            foreach (Transform child in answerGroup.transform) {
                if (child.GetComponent<Toggle>().isOn)
                    answer = child.GetComponentInChildren<Text>().text;
            }

            TimeSpan pathTime = DateTime.Now.Subtract(initQuizTime);
            int tempoImpiegato = (int)Math.Floor(pathTime.TotalSeconds);

            Debug.Log("tempo impiegato: " + tempoImpiegato);
            
            GameState.Instance.levelBackend.SaveQuiz(id, tempoImpiegato, answer, 
                res => {
                    quizPanel.SetActive(false);
                    scorePanel.SetActive(true);
                    scoreNumberText.text = res.score.ToString();
                    
                    if (triggerButton != null)
                        triggerButton.interactable = false;

                }, err => {
                      Debug.Log(err.Message);
                });
        }

        public void CloseQuiz() {
            scorePanel.SetActive(false);
            quizPanel.SetActive(false);

            foreach (Transform child in answerGroup.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            _callback?.Invoke();
        }

#if UNITY_EDITOR
        [ContextMenu(nameof(TestQuiz))]
        private void TestQuiz()
        {
            SetQuiz(gameObject, null);
        }
#endif
    }
}
