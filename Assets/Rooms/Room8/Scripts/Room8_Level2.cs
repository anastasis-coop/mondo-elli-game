using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Room8 {

    public class Room8_Level2 : MonoBehaviour {

        public int LEVEL_TIME;
        public int MISS_TIME;

        public bool ExplicitTimeOut;

        public int PRESENTATION_DELAY;

        public GameObject sceneObj;
        public GameObject sceneUI;

        public GameObject nextBehaviour;

        public Button redButton;
        public Button greenButton;

        public bool moreDifficult;

        private Room8_GameController2 controller;

        private bool selectionAllowed = false;
        private bool correctAnswer = false;

        private List<string> vocalList = new List<string> { "a", "e", "i", "o", "u" };

        private SetDescription[] clonedSetDescriptionList;


        private bool helpRequestHandle = false;

        private Coroutine selectCoroutine;

        void Start() {
            controller = (Room8_GameController2) Room8_CommonRefs.controller;

            controller.levelTimer.SetTime(LEVEL_TIME);
            controller.bomb.timeoutSeconds = MISS_TIME;

            controller.bomb.onExplode.AddListener(AnswerTimerEnded);

            controller.messagePanel.messageRead.RemoveAllListeners();
            controller.messagePanel.messageRead.AddListener(MessageReadHandler);
            controller.messagePanel.oneShotRead.AddListener(() => MessageReadHandler(default));

            redButton.onClick.AddListener(RedButtonClick);
            greenButton.onClick.AddListener(GreenButtonClick);

            clonedSetDescriptionList = (SetDescription[]) controller.setDescriptions.Clone();

            SetScreens();

            Init();
            StartMatch();
        }

        private void Init() {
            controller.energyBar.SetActive(true);

            sceneObj.SetActive(true);
            sceneUI.SetActive(true);

            // controller.helpButton.SetActive(true);

            controller.levelTimer.SetActivation(true);
        }

        private void GreenButtonClick() {
            if (selectionAllowed && !controller.helpRequested) {
                if (correctAnswer)
                    controller.RightAnswer();
                else
                    controller.WrongAnswer();

                StartMatch();
            
                selectionAllowed = false;
            }

        }

        private void RedButtonClick() {
            if (selectionAllowed && !controller.helpRequested) {
                if (!correctAnswer)
                    controller.RightAnswer();
                else
                    controller.WrongAnswer();

                StartMatch();
                
                selectionAllowed = false;
            }
        }

        private void SetScreens() {
            for (int i = 0; i < controller.screenList.Length; i++) {
                SetType t = clonedSetDescriptionList[i].type;

                controller.screenList[i].GetComponent<Image>().sprite = clonedSetDescriptionList[i].image;
            }
        }

        public virtual void MessageReadHandler(int _) {

            if (controller.levelTimer.itIsEnd) {
                LevelEnd();
                return;
            }

            controller.levelTimer.SetActivation(true);

            StartMatch();
        }

        protected void Shuffle(SetDescription[] list) {
            int n = list.Length;
            while (n > 1) {
                n--;
                int k = UnityEngine.Random.Range(0, n + 1);
                SetDescription value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public IEnumerator SelectSet() {
            controller.bomb.gameObject.SetActive(false);
            controller.bomb.StopCountdown();

            if (controller.levelTimer.itIsEnd) {
                LevelEnd();
                yield break;
            }

            selectionAllowed = false;

            if (moreDifficult) {
                // Mescolo le posizioni degli schermi
                Shuffle(clonedSetDescriptionList);
                SetScreens();
            }

            // Scelgo uno schermo casuale e attivo il marker
            int screenIndex = UnityEngine.Random.Range(0, controller.screenList.Length);

            for (int i = 0; i < controller.screenList.Length; i++) {
                if (i == screenIndex) {
                    controller.screenList[i].transform.GetChild(0).gameObject.SetActive(true);
                } else {
                    controller.screenList[i].transform.GetChild(0).gameObject.SetActive(false);
                }
            }

            // Scelgo una lettera e un numero casuale
            int letterIndex = UnityEngine.Random.Range(0, controller.letters.Length);
            int numberIndex = UnityEngine.Random.Range(0, controller.numbers.Length);

            if (!moreDifficult)
                yield return new WaitForSeconds(PRESENTATION_DELAY);

            var op = controller.letters[letterIndex].localizedClip.LoadAssetAsync();

            yield return op;

            controller.voiceSource.clip = op.Result;
            controller.voiceSource.Play();

            yield return new WaitUntil(() => !controller.voiceSource.isPlaying);

            if (!moreDifficult)
                yield return new WaitForSeconds(PRESENTATION_DELAY);

            op = controller.numbers[numberIndex].localizedVoiceF.LoadAssetAsync();

            yield return op;

            controller.voiceSource.clip = op.Result;
            controller.voiceSource.Play();

            yield return new WaitUntil(() => !controller.voiceSource.isPlaying);

            // Le liste degli schermi e dei tipi hanno corrispondenza 1-1
            switch (clonedSetDescriptionList[screenIndex].type) {
                case SetType.EVEN:
                    if (controller.numbers[numberIndex].number % 2 == 0)
                        correctAnswer = true;
                    else
                        correctAnswer = false;
                    break;
                case SetType.ODD:
                    if (controller.numbers[numberIndex].number % 2 != 0)
                        correctAnswer = true;
                    else
                        correctAnswer = false;
                    break;
                case SetType.VOCAL:
                    if (vocalList.Contains(controller.letters[letterIndex].letter))
                        correctAnswer = true;
                    else
                        correctAnswer = false;
                    break;
                case SetType.CONSONANT:
                    if (!vocalList.Contains(controller.letters[letterIndex].letter))
                        correctAnswer = true;
                    else
                        correctAnswer = false;
                    break;
            }

            selectionAllowed = true;

            controller.bomb.gameObject.SetActive(true);
            if (!ExplicitTimeOut) controller.bomb.SetVisibleWhenReaches(5);
            else controller.bomb.SetAlwaysVisible();
            controller.bomb.StartCountdown();
        } 

        /* Callback per gestire il miss di una risposta perché finito il tempo */
        private void AnswerTimerEnded() {
            controller.bomb.StopCountdown();

            selectionAllowed = false;

            controller.MissedAnswer();
            
            Invoke(nameof(StartMatch), 1.5f);
        }

        private void Update() {
            if (controller.helpRequested && !helpRequestHandle) {
                helpRequestHandle = true;
                if (selectCoroutine != null)
                    StopCoroutine(selectCoroutine);
            } else if (!controller.helpRequested && helpRequestHandle) {
                helpRequestHandle = false;
                controller.HelpRequestEnded(MessageReadHandler);
                if (!controller.bomb.enabled) StartMatch();
            }
        }

        public void LevelEnd() {
            sceneObj.SetActive(false);
            sceneUI.SetActive(false);

            StopAllCoroutines();

            controller.energyBar.SetActive(false);
            gameObject.SetActive(false);
        }

        private void StartMatch() => selectCoroutine = StartCoroutine(SelectSet());

        private void OnDisable() {
            nextBehaviour.SetActive(true);
        }
    }
}
