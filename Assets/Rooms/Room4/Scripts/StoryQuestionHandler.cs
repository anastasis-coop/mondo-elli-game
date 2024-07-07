using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Room4 {
    [Serializable]
    public class StoryQuestion {
        public LocalizedAudioClip localizedAudioClip;
        public Sprite correctImage;
        public Sprite[] wrongImages;
    }

    public class StoryQuestionHandler : BaseQuestionHandler<StoryQuestion> {

        public int numScreens;

        public GameObject[] screenList;

        private void OnValidate() {
            if (screenList.Length != numScreens)
                screenList = new GameObject[numScreens];

            foreach (StoryQuestion question in questionList)
                if (question.wrongImages.Length != numScreens-1)
                    question.wrongImages = new Sprite[numScreens-1];
        }

        public override IEnumerator LoadQuestion() {
            if (questionList[questionIndex].correctImage != null) {
                int randomRightPosition = UnityEngine.Random.Range(0, numScreens);
                SetScreens(randomRightPosition);
            }
            var op = questionList[questionIndex].localizedAudioClip.LoadAssetAsync();

            yield return op;

            audioSource.clip = op.Result;
            audioSource.Play();
            yield return new WaitUntil(() => !audioSource.isPlaying);
            if (questionList[questionIndex].correctImage != null) {
                answerGiven = false;
                controller.bomb.gameObject.SetActive(true);
                controller.bomb.StartCountdown();
            } else {
                Invoke(nameof(NextQuestion), 1f);
            }
        }

        private void SetScreens(int rightScreenIndex) {
            int wrongIndex = 0;
            for (int i = 0; i < screenList.Length; i++)
            {
                var screen = screenList[i];
                screen.SetActive(true);
                screen.GetComponent<Button>().onClick.RemoveAllListeners();

                if (rightScreenIndex == i) {
                    screen.GetComponent<SpriteRenderer>().sprite = questionList[questionIndex].correctImage;
                    screen.GetComponent<Button>().onClick.AddListener(() => CorrectAnswer(screen.transform.position));
                } else {
                    screen.GetComponent<SpriteRenderer>().sprite = questionList[questionIndex].wrongImages[wrongIndex];
                    screen.GetComponent<Button>().onClick.AddListener(() => WrongAnswer(screen.transform.position));
                    wrongIndex++;
                }
            }
        }

        protected override void NextQuestion() {
            questionIndex++;

            if (controller.levelTimer.itIsEnd) {
                LevelEnd();
            }
            else {
                if (questionIndex == questionList.Length) {
                    questionIndex = 0;
                    controller.bomb.timeoutSeconds = 3.5f;
                }

                controller.bomb.StopCountdown();
                StartCoroutine(LoadQuestion());
            }
        }

    }
}