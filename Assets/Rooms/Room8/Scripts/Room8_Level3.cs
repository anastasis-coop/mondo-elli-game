using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Room8 {

    public class Room8_Level3 : MonoBehaviour {

        public int LEVEL_TIME;
        public int MISS_TIME;
        
        public bool maxSpeed;

        public Calculator Calculator;

        private int[] numberSequence;

        private int numberCount = 0;

        private int expectedResult;

        private Room8_GameController3 controller;

        public GameObject nextBehaviour;

        private bool mult;

        private int SEQUENCE_LENGTH = 2;

        public GameObject sceneObj;

        private bool helpRequestHandle = false;

        private Coroutine selectCoroutine;

        void Start() {
            
            numberSequence = new int[SEQUENCE_LENGTH];
           
            controller = (Room8_GameController3) Room8_CommonRefs.controller;

            controller.levelTimer.SetTime(LEVEL_TIME);
            controller.bomb.timeoutSeconds = MISS_TIME;

            controller.bomb.onExplode.AddListener(AnswerTimerEnded);

            controller.messagePanel.messageRead.RemoveAllListeners();
            controller.messagePanel.messageRead.AddListener(MessageReadHandler);

            Calculator.enabled = true;
            
            Calculator.Submitted -= OnCalculatorConfirm;
            Calculator.Submitted += OnCalculatorConfirm;

            Init();
            selectCoroutine = StartCoroutine(SpellNumber());
        }

        private void Init() {
            controller.energyBar.SetActive(true);

            sceneObj.SetActive(true);

            controller.levelTimer.SetActivation(true);
        }

        public virtual void MessageReadHandler(int _) {

            if (controller.levelTimer.itIsEnd) {
                LevelEnd();
                return;
            }

            controller.levelTimer.SetActivation(true);

            StartRound();
        }

        private void OnCalculatorConfirm(IReadOnlyList<int> sequence)
        {
            var number = sequence.Select((d, i) => d * (int)Mathf.Pow(10, sequence.Count - i - 1)).Sum();

            if (number == expectedResult) controller.RightAnswer();
            else controller.WrongAnswer();
            controller.levelTimer.SetActivation(false);
            StartRound();
        }

        private void Update() {
            if (controller.helpRequested && !helpRequestHandle) {
                helpRequestHandle = true;
                // Controllo che non sia null perché la variabile viene modificata solo quando la coroutine termina
                // per cui in alcuni momenti può essere null (quando chiamata Start_Coroutine prima della terminazione)
                if (selectCoroutine != null)
                    StopCoroutine(selectCoroutine);
            }
            else if (!controller.helpRequested && helpRequestHandle) {
                helpRequestHandle = false;
                controller.HelpRequestEnded(MessageReadHandler);
                Calculator.enabled = true;
                if (!controller.bomb.enabled) StartRound();
            }
        }

        private IEnumerator SpellNumber(bool? mult = null) {
            controller.bomb.StopCountdown();

            if (controller.levelTimer.itIsEnd) {
                LevelEnd();
                yield break;
            }

            Calculator.Clear();
            Calculator.enabled = false;

            int numberIndex = UnityEngine.Random.Range(0, controller.numbers.Length);

            mult ??= Convert.ToBoolean(UnityEngine.Random.Range(0, 2));

            var op = (mult.Value ? controller.numbers[numberIndex].localizedVoiceM :
                controller.numbers[numberIndex].localizedVoiceF).LoadAssetAsync();

            yield return op;

            controller.voiceSource.clip = op.Result;
            controller.voiceSource.Play();

            if (numberCount < SEQUENCE_LENGTH) {
                numberSequence[numberCount] = controller.numbers[numberIndex].number;
                numberCount++;

            } else {
                numberSequence[SEQUENCE_LENGTH - 2] = numberSequence[SEQUENCE_LENGTH - 1];
                numberSequence[SEQUENCE_LENGTH - 1] = controller.numbers[numberIndex].number;
            }

            if (numberCount < SEQUENCE_LENGTH) {
                if (maxSpeed)
                    yield return new WaitForSeconds(0.5f);
                else
                    yield return new WaitForSeconds(2);
                yield return new WaitUntil(() => !controller.voiceSource.isPlaying);
                selectCoroutine = StartCoroutine(SpellNumber(mult));
            } else {   
                expectedResult = mult.Value ? numberSequence.Aggregate(1, (acc, value) => acc * value): numberSequence.Sum();
                Debug.Log(expectedResult);

                if (!maxSpeed) {
                    yield return new WaitForSeconds(3);

                    yield return new WaitUntil(() => !controller.effectSource.isPlaying);

                    controller.effectSource.clip = controller.initSelectionSound;
                    controller.effectSource.Play();
                }

                Calculator.enabled = true;
                controller.bomb.gameObject.SetActive(true);
                controller.bomb.SetVisibleWhenReaches(5);
                controller.bomb.StartCountdown();
                controller.levelTimer.SetActivation(true);
            }
            
            Calculator.Clear();
        }

        public void LevelEnd() {
            sceneObj.SetActive(false);
            controller.energyBar.SetActive(false);
            gameObject.SetActive(false);
        }


        /* Callback per gestire il miss di una risposta perché finito il tempo */
        private void AnswerTimerEnded() {
            controller.levelTimer.SetActivation(false);
            controller.bomb.gameObject.SetActive(false);
            controller.bomb.StopCountdown();

            controller.MissedAnswer();

            Invoke(nameof(StartRound), 1.5f);
        }

        private void StartRound() => selectCoroutine = StartCoroutine(SpellNumber());

        private void OnDisable() {
            nextBehaviour.SetActive(true);
        }
    }
}
