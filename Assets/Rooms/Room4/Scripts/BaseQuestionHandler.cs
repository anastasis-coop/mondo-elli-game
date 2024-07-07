using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;

namespace Room4 {

    [Serializable]
    public class Question {
        public LocalizedAudioClip localizedAudioClip;
        public Sprite correctImage;
        public Sprite wrongImage;
    }


    public class BaseQuestionHandler<T> : MonoBehaviour {
        protected int questionIndex;
        protected GameController controller;
        protected AudioSource audioSource;

        public int questionTime;
        public int levelTime;

        public bool useBomb;

        public T[] questionList;

        public GameObject nextBehaviour;
        public GameObject screens;

        protected bool answerGiven;

        protected virtual void Start() {
            questionIndex = 0;

            answerGiven = true;

            controller = GameObject.Find("GameController").GetComponent<GameController>();
            audioSource = Camera.main.GetComponent<AudioSource>();

            controller.energyBar.SetActive(true);

            screens.SetActive(true);

            controller.bomb.SetAlwaysInvisible(!useBomb);
            controller.bomb.timeoutSeconds = questionTime;
            controller.bomb.gameObject.SetActive(false);
            controller.bomb.onExplode.AddListener(BombExploded);

            controller.levelTimer.SetTime(levelTime);
            controller.levelTimer.SetActivation(true);

            StartCoroutine(LoadQuestion());
        }

        public virtual IEnumerator LoadQuestion() {
            yield return 0;
        }

        public void LevelEnd() {
            screens.SetActive(false);
            controller.bomb.gameObject.SetActive(false);
            controller.energyBar.SetActive(false);
            nextBehaviour.SetActive(true);
            gameObject.SetActive(false);
        }

        protected virtual void CorrectAnswer(Vector3 position) {
            if (!answerGiven) {
                answerGiven = true;
                audioSource.Stop();
                controller.bomb.StopCountdown();
                controller.bomb.gameObject.SetActive(false);
                controller.RightAnswer(position);
                Invoke(nameof(NextQuestion), 1f);
            }
        }

        protected virtual void WrongAnswer(Vector3 position) {
            if (!answerGiven) {
                answerGiven = true;
                audioSource.Stop();
                controller.bomb.StopCountdown();
                controller.bomb.gameObject.SetActive(false);
                controller.WrongAnswer(position);
                Invoke(nameof(NextQuestion), 1f);
            }
        }

        protected void Shuffle(T[] list) {
            int n = list.Length;
            while (n > 1) {
                n--;
                int k = UnityEngine.Random.Range(0, n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        protected virtual void NextQuestion() {
            questionIndex++;

            if (controller.levelTimer.itIsEnd) {
                LevelEnd();
            } else {
                if (questionIndex == questionList.Length) {    
                    questionIndex = 0;
                    Shuffle(questionList);
                }

                controller.bomb.StopCountdown();
                controller.bomb.gameObject.SetActive(false);
                StartCoroutine(LoadQuestion());
            }
        }

        private void BombExploded() {
            answerGiven = true;
            controller.MissedAnswer();
            controller.bomb.gameObject.SetActive(false);
            Invoke(nameof(NextQuestion), controller.bomb.explosionSeconds);
        }
    }
}