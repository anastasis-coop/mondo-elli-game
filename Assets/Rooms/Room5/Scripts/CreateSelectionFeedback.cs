using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Room5
{
    public class CreateSelectionFeedback : MonoBehaviour
    {
        public Timer timer;
        public Bomb bomb;
        public Score score;
        public MessageSystem messageSystem;
        public LampsCableController LampsController;

        [Space]
        public AudioSource effectsSource;
        public AudioClip clickSound;
        public AudioClip answerTimeSound;
        public AudioClip SequenceShowPingIn;
        public AudioClip SequenceShowPingOut;

        //Variables For creating Objects
        public float ObjsMargin;
        public Highlighter HighlightObject;
        public GameObject FirstposForObjFlr1, FirstposForObjFlr2;
        public LayerMask ShelfLayer;
        
        private GameObject instantiateObject1;
        private bool startTrial;
        private int numOfLightUpOrAppear;
        public Transform parent1stFloorObjs, parent2DFloorObjs;

        private readonly List<GameObject> _sequence = new();
        private readonly List<int> _answer = new();
        private readonly List<GameObject> _answerLamps = new();
        private bool _canAnswer;
        private bool _isLampsPhase;
        
        private float timeForGearSL;
        private readonly float timeForObjSL = 3f;
        private int demoStep;
        private bool checkobjAnswer, modalMessage;
        public int numOfObjsForLV0 = 3, numOfObjsForLV123 = 2;
        public int numOfCorrectSeq, CheckNumIf3times, multiplyWaitNum;
        
        //Variables For Feedbacks
        
        public AudioClip CorrectSLSound, WrongSLSound;
        public Material SuccessMaterial;
        public ParticleSystem SuccessParticles;
        public Material FailureMaterial;
        public ParticleSystem FailureParticles;
        public Transform FeedbackParent;

        //Generally
        public bool endGame;
        private bool Level0102, Level3132, Level021112, Level122232;
        private int _numOfHitsOnLamps;

        //Messages
        [SerializeField]
        private BigElloSaysConfig _wrongOrderMessage;

        [SerializeField]
        private BigElloSaysConfig _gearDemoMessage;

        [SerializeField]
        private BigElloSaysConfig _reverseAnswerMessage;

        [SerializeField]
        private BigElloSaysConfig _endExerciseMessage;

        [SerializeField]
        private RoomEndPopup endPopup;

        public bool BigElloPresenting;

        private readonly List<ParticleSystem> _successParticles = new();
        private readonly List<ParticleSystem> _failureParticles = new();

        private readonly List<GameObject> ObjsOn1stFlr = new();
        private readonly List<GameObject> ObjsOn2dFlr = new();
        private readonly List<GameObject> CopyObjsOn1stFlr = new();
        private readonly List<GameObject> CopyObjsOn2DFlr = new();
        
        private Camera _mainCamera;

        private bool _bombIsToResume;

        public LevelsConfig Config { get; set; }

        public ItemsSet ItemsSet => Config != null ? Config.ItemsSet : null;

        private void Start()
        {
            bomb.SetAlwaysInvisible(!Config.ShowTimer);
            
            _mainCamera = Camera.main;
            
            if (Config.Level is RoomLevel.LEVEL_31 or RoomLevel.LEVEL_32)
            {
                timeForGearSL = 3;
                demoStep = 1;
            }
            else
            {
                timeForGearSL = 0;
                demoStep = 0;
                
                StartGame();
            }

            //Set Integerate Levels
            SetIntegrationOfLevels();
            modalMessage = false;
            messageSystem.oneShotRead.AddListener(BigElloTalked);
        }

        private void Update()
        {
            //Repeat these steps if ello is not talking and is not doing Demo and game is not ended.
            if (BigElloPresenting || modalMessage || demoStep != 0 || endGame) return;
            
            //Set and update Some features based on level
            UpdateAndStartBasedOnlevel();

            // TODO please use buttons
            if (!_canAnswer || !Input.GetMouseButtonDown(0)) return;

            var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, 100)) return;
                
            var go = hit.transform.gameObject;
            var itemIndex = ObjsOn2dFlr.IndexOf(go);
            if (itemIndex >= 0 && !_answer.Contains(itemIndex))
            {
                FeedbackOnClickedObject(go.transform);
                
                _answer.Add(itemIndex);

                if (_answer.Count != _sequence.Count) return;
                
                var sequenceEquals = true;
                for (var i = 0; i < _sequence.Count && sequenceEquals; i++)
                    sequenceEquals &= _sequence[i] == ObjsOn1stFlr[_answer[i]];
                    
                FeedbackItems(sequenceEquals);

                if (sequenceEquals)
                {
                    _isLampsPhase = LampsController.ActiveLampsCount > 0;
                    if (!_isLampsPhase) EndTrial(true);
                }
                else EndTrial(false);
            }
            else if (LampsController.Contains(go) && !_answerLamps.Contains(go))
            {
                if (!_isLampsPhase)
                {
                    _numOfHitsOnLamps++;

                    if (_numOfHitsOnLamps > 1 && _numOfHitsOnLamps < 4)
                    {
                        ModalMessage(_wrongOrderMessage);
                    }
                }
                else
                {
                    FeedbackOnClickedObject(go.transform);
                    
                    _answerLamps.Add(go);

                    if (_answerLamps.Count == LampsController.InactiveLampsCount)
                    {
                        var contentEquals = true;
                        foreach (var lamp in _answerLamps)
                        {
                            contentEquals &=
                                LampsController.InactiveLamps.Contains(
                                    lamp.GetComponent<CompositeBooleanSwapper>());
                            if (!contentEquals) break;
                        }
                        
                        FeedbackLamps(contentEquals);
                        EndTrial(contentEquals);
                    }
                }
            }
        }

        private void EndTrial(bool success)
        {
            bomb.StopCountdown();

            if (success) score.RightCounter++;
            else score.WrongCounter++;
            Invoke(nameof(AfterDelay), 0.5f);
            _canAnswer = false;
            if (success) numOfCorrectSeq++;
        }

        private void SetIntegrationOfLevels()
        {
            //Set compact variables for combination of levels that will be used multiple time in the code.
            Level0102 = Config.Level is RoomLevel.LEVEL_01 or RoomLevel.LEVEL_02;
            Level3132 = Config.Level is RoomLevel.LEVEL_31 or RoomLevel.LEVEL_32;
            Level021112 = Config.Level is RoomLevel.LEVEL_02 or RoomLevel.LEVEL_11 or RoomLevel.LEVEL_12;
            Level122232 = Config.Level is RoomLevel.LEVEL_12 or RoomLevel.LEVEL_22 or RoomLevel.LEVEL_32;
        }

        private void UpdateAndStartBasedOnlevel()
        {
            if (startTrial)
            {
                startTrial = false;
                //Control this initiation with "startTrial" variable to be ready at the start of each trial.
                switch (Config.Level)
                {
                    case RoomLevel.LEVEL_01:
                    case RoomLevel.LEVEL_02:
                        MainFunction(numOfObjsForLV0);
                        break;
                    case RoomLevel.LEVEL_11:
                    case RoomLevel.LEVEL_12:
                    case RoomLevel.LEVEL_21:
                    case RoomLevel.LEVEL_22:
                        MainFunction(numOfObjsForLV123);
                        break;
                    case RoomLevel.LEVEL_31:
                    case RoomLevel.LEVEL_32:
                        //The Function Will be Called with 2, 3, 4, 5, 6, 7 objects, set the gears to be with Movements, and the position change of objects
                        MainFunction(numOfObjsForLV123);
                        break;
                }
            }
        }

        public void MainFunction(int numberObjs)
        {
            ///Summary
            ///Stop the answer part of current trial if there is a soon answer
            // Increase the number of trials that have happend till now
            // Reset the variables
            // Set the steps for a new trial
            CheckNumIf3times++;
            SetTheVariablesAtStart();
            StartCoroutine(SetSteps(numberObjs));
        }

        private void SetTheVariablesAtStart()
        {
            _isLampsPhase = false;
            startTrial = false;
        }

        private IEnumerator SetSteps(int numberObjs)
        {
            ///Summary
            ///Set the number of light up objects
            ///1. Create all the objects
            ///2. Light up or show one by one one first floor (in level 0) or the only floor in other levels
            ///3. Appear all on the second floor (in level 0) or the only floor in other levels
            ///4. Create the answering status for each trial
            numOfLightUpOrAppear = Level0102 ? numberObjs - 1 : numberObjs;

            yield return CreateObjectsAtOnce(numberObjs);

            for (var obj = 0; obj < numOfLightUpOrAppear; obj++) yield return ShowOneByoneOn1stFloor();
            yield return AppearAllOnSecondLevel();

            if (Level122232) _sequence.Reverse();
            
            //This coroutine should be stopped with a soon answer 
            ContinueForAnswer();
        }

        private IEnumerator CreateObjectsAtOnce(int numberOfObjs)
        {
            CopyObjsOn1stFlr.Clear();
            yield return null;
            ///summary
            //Creating them but invisible in the positions based on the number of objs in each trial
      		//DistractList = new(ItemsSet.Values.Select(item => item.Prefab));
            CreateInvisibleMainObjsFor1st2dFloor(numberOfObjs);

            //Check If The Level is 0, Make The Objs on the 1st floor visible.
            if (Level0102) MakeObjsVisible(ObjsOn1stFlr);
        }

        private void CreateInvisibleMainObjsFor1st2dFloor(int numberOfObjs)
        {
            var objs = ItemsSet.Take(numberOfObjs).ToList();
            var result = 
                    SpawnAtFloor(objs, FirstposForObjFlr1.transform.position, parent1stFloorObjs, true);
            ObjsOn1stFlr.AddRange(result);
            CopyObjsOn1stFlr.AddRange(ObjsOn1stFlr);

            var pos = Config.ShelfConfiguration == LevelsConfig.ShelfConfig.Single ? FirstposForObjFlr1 : FirstposForObjFlr2;
            
            result = SpawnAtFloor(objs, pos.transform.position, parent2DFloorObjs, false);
            ObjsOn2dFlr.AddRange(result);
            // if (!Level0102) AddTheDistractors(numberOfObjs);
            CopyObjsOn2DFlr.AddRange(ObjsOn2dFlr);
        }

        private List<GameObject> SpawnAtFloor(ICollection<Item> items, Vector3 start, Transform parent, bool removeFromDistractions)
        {
            var result = new List<GameObject>();
            
            var occupation = items.Sum(item => item.Prefab.GetComponent<BoxCollider>()?.size.x ?? 0) +
                             ObjsMargin * (items.Count - 1);
            start -= occupation / 2 * Vector3.right;
            var offset = 0f;
            RaycastHit shelfHit;
            foreach (var item in items)
            {
                ///Summary
                //Instantiate the main objs from the list of objects
                //Set Some Properties For The Instantiated Obj (Scale, Angle, Name, Position, Visibility)
                //Put objs on floor but all invisible

                //The position of fisrt instantiated object is the position of "FirstposForObjFlr1", while the y value is influenced by "ChangePosOfYForObjFl1" based on level.
                //Next ones have the position of "FirstposForObjFlr1" but the "X" value is plussed with 2.5 unit.

                var collider = item.Prefab.GetComponent<BoxCollider>();
                
                if (offset != 0) offset+= ObjsMargin;
                offset += collider.size.x / 2;

                var pos = start + new Vector3(offset, 0, 0);

                if (Physics.Raycast(pos, Vector3.down, out shelfHit, 10, ShelfLayer))
                {
                    pos = shelfHit.point + (collider.size.y / 2 - collider.center.y) * Vector3.up;
                }
                
                instantiateObject1 = Instantiate(item.Prefab, pos, Quaternion.identity);
                instantiateObject1.transform.SetParent(parent);
                instantiateObject1.SetActive(false);

                //Adding each instantiated obj to a list, a copy of that list and the list of objects on second floor
                result.Add(instantiateObject1);

                //Eliminate this object from the list of distractors
                //if (removeFromDistractions) DistractList.Remove(item.Prefab);
                
                offset += collider.size.x / 2;
            }

            return result;
        }

        private void MakeObjsVisible(List<GameObject> objsVisible)
        {
            for (var a = 0; a < objsVisible.Count; a++) objsVisible[a].SetActive(true);
        }

        private IEnumerator ShowOneByoneOn1stFloor()
        {
            ///Summary
            // Waiting for the period needs
            // Creating a random index to choose a random object from the list of objects created for the 1st floor
            // Giving it lighting Up feature if we are in level 0, or visibility feature if not.
            // Light off or disappear after a delay
            // Removing it from the list of objects on the 1st floor

            var randObj = ObjsOn1stFlr[Random.Range(0, ObjsOn1stFlr.Count)];
            
            effectsSource.PlayOneShot(SequenceShowPingIn);
            
            if (Level0102)
            {
                // Set the wait time: 4 means (3 (the period for each object) + 1 (the period to lighting up the next one))
                
                HighlightObject.Highlight(randObj);

                yield return new WaitForSeconds(1.5f);
                
                HighlightObject.Hide();
                
            }
            else
            {
                randObj.SetActive(true);
                
                yield return new WaitForSeconds(1.5f);

                randObj.SetActive(false);
            }
            
            effectsSource.PlayOneShot(SequenceShowPingOut);
            
            yield return new WaitForSeconds(1);
            
            _sequence.Add(randObj);
            ObjsOn1stFlr.Remove(randObj);
        }

        private IEnumerator AppearAllOnSecondLevel()
        {
            // Set the wait time: 4 means (3 (the period for last object) + 1 (the period for starting the answer part))
            yield return new WaitForSeconds(1.5f);

            ObjsOn1stFlr.Clear();
            ObjsOn1stFlr.AddRange(CopyObjsOn1stFlr);
            ObjsOn2dFlr.Clear();
            ObjsOn2dFlr.AddRange(CopyObjsOn2DFlr);

            effectsSource.PlayOneShot(answerTimeSound);
            
            MakeObjsVisible(ObjsOn2dFlr);

            if (Level021112)
            {
                foreach (var obj in ObjsOn1stFlr)
                    obj.SetActive(false);
            }

            LampsController.Pause();

            _canAnswer = true;
        }

        private void ContinueForAnswer()
        {
            // Set "multiplyWaitNum" based on the level for answer part (in average each object or gear will have "3s" time to be selected)
            multiplyWaitNum = Level0102 ? numOfObjsForLV0 : numOfObjsForLV123;
            
            bomb.gameObject.SetActive(true);
            bomb.timeoutSeconds = multiplyWaitNum * timeForObjSL + (multiplyWaitNum - 1) * timeForGearSL;
            bomb.StartCountdown();
            bomb.onExplode.AddListener(OnExplode);
        }

        private void FeedbackOnClickedObject(Transform clickedObj)
        {
            effectsSource.PlayOneShot(clickSound);
            clickedObj.GetComponent<MaterialHighlighter>()?.Emphasize();
        }

        private void OnExplode()
        {
            _canAnswer = false;
            AfterDelay();
        }

        private void AfterDelay()
        {
            bomb.StopCountdown();
            bomb.gameObject.SetActive(false);
            bomb.onExplode.RemoveListener(OnExplode);
            CheckForWrongReverseAnswer();

            if (_answer.Count < _sequence.Count && _answerLamps.Count == 0)
            {
                CheckNumIf3times = 0;
                effectsSource.PlayOneShot(WrongSLSound);
                score.MissedCounter++;
            }

            ClearAll();
            Invoke(nameof(ResetAndProgress), 1.3f);
        }

        private void ResetAndProgress()
        {
            ClearAll();
            
            //Check if the game is finished
            if (timer.itIsEnd && !endGame)
                Invoke(nameof(EndGame), 2f);
            else StartGame();
            
            if (CheckNumIf3times == 3) //If the number of trials is equal to 3, go for evaluate the three experienced trials.
            {
                CheckNumIf3times = 0;
                if (numOfCorrectSeq is 2 or 3)
                {
                    numOfObjsForLV0++;
                    numOfObjsForLV123++;
                    // answerSeqIndex = -1;
                }
                numOfCorrectSeq = 0;
            }
        }

        private void ClearAll()
        {
            DestroyEveryObjs();
            ObjsOn1stFlr.Clear();
            ObjsOn2dFlr.Clear();
            CopyObjsOn2DFlr.Clear();
            _sequence.Clear();
            _answer.Clear();
            _answerLamps.Clear();
            CopyObjsOn1stFlr.Clear();
            
            foreach (var lamp in LampsController) 
                lamp.GetComponent<MaterialHighlighter>()?.DeEmphasize();
        }

        private void PrepareFeedbackParticles(List<ParticleSystem> particles, ParticleSystem prefab, int count)
        {
            while (particles.Count < count)
            {
                var instance = Instantiate(prefab, FeedbackParent);
                instance.Stop();
                particles.Add(instance);
            }
        }

        private void ApplyFeedback(GameObject obj, Material material, ParticleSystem particles, AudioClip clip)
        {
            var highlighter = obj.GetComponent<MaterialHighlighter>();
            highlighter.ExternalOverride = material;
            highlighter.Emphasize();
            particles.transform.position = obj.transform.position;
            particles.Play();
            effectsSource.PlayOneShot(clip);
        }

        private void Feedback(Func<int, GameObject> supplyAtIndex, int count, bool isSuccess)
        {
            var particlePrefab = isSuccess ? SuccessParticles : FailureParticles;
            var particlesList = isSuccess ? _successParticles : _failureParticles;
            var material = isSuccess ? SuccessMaterial : FailureMaterial;
            var sound = isSuccess ? CorrectSLSound : WrongSLSound;
            
            PrepareFeedbackParticles(particlesList, particlePrefab, count);

            for (var i = 0; i < count; i++)
                ApplyFeedback(supplyAtIndex(i), material, particlesList[i], sound);
        }

        private void FeedbackItems(bool isSuccess)
            => Feedback(i => ObjsOn2dFlr[_answer[i]], _answer.Count, isSuccess);

        private void FeedbackLamps(bool isSuccess)
            => Feedback(i => LampsController.InactiveLamps[i].gameObject, LampsController.InactiveLampsCount, isSuccess);

        private void CheckForWrongReverseAnswer()
        {  
            var isReversed = _answer.Count == _sequence.Count;
            for (var i = 0; i < _answer.Count && isReversed; i++)
            {
                isReversed &= ObjsOn1stFlr[_answer[i]] == _sequence[^(i + 1)];
            }

            if (isReversed && numOfObjsForLV123 <= 3 && !Level3132)
                ModalMessage(_reverseAnswerMessage);
        }

        private void DestroyEveryObjs()
        {
            //Destroy all the Main Objs On the First and second floor and Distracting Objs On the Second Floor
            DestroyObjs(ObjsOn2dFlr);
            DestroyObjs(ObjsOn1stFlr);
        }

        private void DestroyObjs(List<GameObject> objsList)
        {
            foreach (var obj in objsList)
            {
                Destroy(obj);
            }
        }

        private void EndGame()
        {
            messageSystem.ShowMessage(_endExerciseMessage);
            endGame = true;
        }

        public void StartGame()
        {
            startTrial = true;

            if (Config.ShelfConfiguration != LevelsConfig.ShelfConfig.Single || Config.ActiveLights <= 0) return;
            
            var inactiveCount = LampsController.AvailableLampsCount - Config.ActiveLights;
            if (inactiveCount < 0) 
                Debug.LogError($"Configuration {Config.name} requires too many active lamps ({Config.ActiveLights}/{LampsController.AvailableLampsCount})");
            LampsController.ConfigureLights(inactiveCount, Config.OnOffLightsPeriod);
        }

        public void StartTimer()
        {
            timer.Reset();
            timer.SetActivation(true);
        }

        public void BigElloTalked()
        {
            if (modalMessage && !endGame)
            {
                modalMessage = false;
            }
            else if (endGame)
            {
                endPopup.Show(saveResultsAndExit);
            }
            else switch (demoStep)
            {
                case 1:
                    messageSystem.ShowMessage(_gearDemoMessage);
                    demoStep++;
                    BigElloPresenting = false;
                    break;
                case 2:
                    LampsController.ExecuteMechanicTrial(() =>
                    {
                        StartGame();
                        StartTimer();
                        demoStep = 0;
                    });
                    break;
            }

            if (BigElloPresenting && demoStep == 0)
            {
                BigElloPresenting = false;
                StartTimer();
            }

            if (timer.itIsEnd && !endGame) EndGame();
            
            timer.SetActivation(true);
            bomb.enabled = _bombIsToResume;
        }

        private void ModalMessage(BigElloSaysConfig message)
        {
            modalMessage = true;
            messageSystem.ShowMessage(message);
            timer.SetActivation(false);
            _bombIsToResume = bomb.enabled;
            bomb.enabled = false;
        }

        private void saveResultsAndExit()
        {
            if (GameState.Instance.testMode)
                GameState.Instance.LoadSceneAfterRoom();
            else
            {
                GameState.Instance.levelBackend.ExitRoom(timer.totalTime, score,
                    () => { GameState.Instance.LoadSceneAfterRoom(); },
                    err => { Debug.Log(err.Message); });
            }
        }
    }
}