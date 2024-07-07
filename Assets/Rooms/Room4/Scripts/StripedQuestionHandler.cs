﻿using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

namespace Room4 {

    public class StripedQuestionHandler : ShuffledQuestionHandler<Question> {

        public GameObject leftScreen;
        public GameObject rightScreen;

        public float fromPos = 0.2f;
        public float toPos = 4.2f;

        public float animDuration = .3f;
        public float animDelay = .5f;


        public override IEnumerator LoadQuestion() {
            bool stripedImages = (UnityEngine.Random.value > 0.5f);
            bool randomPosition = (UnityEngine.Random.value > 0.5f);
            SetCorrectScreen(randomPosition ? leftScreen : rightScreen, stripedImages);
            SetWrongScreen(randomPosition ? rightScreen : leftScreen, stripedImages);

            var op = questionList[questionIndex].localizedAudioClip.LoadAssetAsync();

            yield return op;

            audioSource.clip = op.Result;
            audioSource.Play();

            var leftParent = leftScreen.transform.parent;
            var leftAlpha = leftParent.GetComponent<SpriteRendererAlphaGroup>();
            leftParent.DOKill();
            leftParent.DOLocalMoveY(toPos, animDuration).From(fromPos).SetDelay(animDelay);
            leftAlpha.DOKill();
            leftAlpha.Alpha = 1;

            var rightParent = rightScreen.transform.parent;
            var rightAlpha = rightParent.GetComponent<SpriteRendererAlphaGroup>();
            rightParent.DOKill();
            rightParent.DOLocalMoveY(toPos, animDuration).From(fromPos).SetDelay(animDelay);
            rightAlpha.DOKill();
            rightAlpha.Alpha = 1;
            

            yield return new WaitUntil(() => !audioSource.isPlaying);
            answerGiven = false;
            controller.bomb.gameObject.SetActive(true);
            controller.bomb.StartCountdown();
        }

        private void OnAnswer(bool correct, GameObject screen)
        {
            if (answerGiven) return;

            answerGiven = true;
            audioSource.Stop();
            controller.bomb.StopCountdown();
            if (correct) controller.RightAnswer(screen.transform.position);
            else controller.WrongAnswer(screen.transform.position);

            var parent = screen.transform.parent;
            parent.DOLocalMoveY(fromPos, animDuration)
                .From(toPos)
                .SetEase(correct ? Ease.InBack : Ease.Linear)
                .OnComplete(() => Invoke(nameof(NextQuestion), 1f));
            if (!correct) parent.GetComponent<SpriteRendererAlphaGroup>().DOAlpha(0, animDuration);
        }

        private void SetCorrectScreen(GameObject screen, bool striped)
        {
            screen.transform.parent.gameObject.SetActive(true);
            if (striped) {
                screen.transform.parent.GetComponent<SpriteRenderer>().sprite = controller.stripedBackground;
                screen.GetComponent<SpriteRenderer>().sprite = questionList[questionIndex].wrongImage;
            } else {
                screen.transform.parent.GetComponent<SpriteRenderer>().sprite = controller.normalBackground;
                screen.GetComponent<SpriteRenderer>().sprite = questionList[questionIndex].correctImage;
            }

            screen.GetComponent<Button>().onClick.RemoveAllListeners();
            screen.GetComponent<Button>().onClick.AddListener(() => OnAnswer(true, screen));
        }

        private void SetWrongScreen(GameObject screen, bool striped)
        {
            screen.transform.parent.gameObject.SetActive(true);
            if (striped) {
                screen.transform.parent.GetComponent<SpriteRenderer>().sprite = controller.stripedBackground;
                screen.GetComponent<SpriteRenderer>().sprite = questionList[questionIndex].correctImage;
            } else {
                screen.transform.parent.GetComponent<SpriteRenderer>().sprite = controller.normalBackground;
                screen.GetComponent<SpriteRenderer>().sprite = questionList[questionIndex].wrongImage;
            }

            screen.GetComponent<Button>().onClick.RemoveAllListeners();
            screen.GetComponent<Button>().onClick.AddListener(() => OnAnswer(false, screen));
        }
    }
}