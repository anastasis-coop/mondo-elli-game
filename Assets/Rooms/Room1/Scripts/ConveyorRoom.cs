using System.Collections.Generic;
using Common;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Serialization;

namespace Room1
{
    public class ConveyorRoom : MonoBehaviour
	{
		[SerializeField] private LevelController levelController;
		
		public int rightTargetsForChange = 5;
		public int missedTargetsForChange = 4;
		
		public float changeAnimationTime = 5;

		public bool grayScale;
		[FormerlySerializedAs("animateTargetScreen")]
		[FormerlySerializedAs("moveTargetScreen")]
		public bool highlightChangeOfTarget;

		public Spawner spawner;
		public Interferences interferences;
		public TargetScreen showFirstTarget;
		public TargetScreen showSecondTarget;
		public MessageSystem bigElloTalking;
        public BigElloSaysConfig _endDemo;
        public BigElloSaysConfig _changeTaskAll;
        public RestartButton restartButton;

		public TimerBar timerBar;
		public List<ConveyorBelt> _conveyorBelts;
		public Timer timer;
		public Score score;
		
		[SerializeField] private Color rightColor;
		[SerializeField] private Color wrongColor;

		[SerializeField] private AudioClip rightAnswerClip;
		[SerializeField] private AudioClip wrongAnswerClip;
		private AudioSource WrongSelectionSource;
		
		[SerializeField] private ParticleSystem rightAnswerPrefab;
		[SerializeField] private ParticleSystem wrongAnswerPrefab;
		
		private AudioSource missSource;

		public GameObject missRoot;
		public GameObject missPrefab;

		private int rightCounterOnCurrentTarget;  // TODO DA RESETTARE SUL CAMBIO TARGET
		private int missedCounterOnCurrentTarget;

		public float trackSpeedMultiplier = 1f;
		public bool useSound = false;

		public ItemsSet ItemsSet { get; set; }
		
		public AudioClip conveyorSound;

		public UnityEvent startEvent;

		public UnityEvent<bool> onItemFunneledEvent;

		private bool changeWhenAnimating;
		private bool firstLoop = true;
		private bool stopInput;

		private AudioSource conveyorSource;

		private bool gameEnded;

		[SerializeField]
		private RoomEndPopup endPopup;

		// Called from LevelController start
		public void Prepare() {
			showFirstTarget.Prepare(ItemsSet);
			showSecondTarget.Prepare(ItemsSet);
			spawner.Prepare();

			conveyorSource = gameObject.AddComponent<AudioSource>();
			conveyorSource.playOnAwake = false;
			conveyorSource.pitch = .07f;
			conveyorSource.clip = conveyorSound;

			WrongSelectionSource = gameObject.AddComponent<AudioSource>();
			WrongSelectionSource.playOnAwake = false;

			//Removed for graphical glitch in build
			//Camera.main.GetComponent<PostProcessVolume>().enabled = grayScale;
			//Camera.main.GetComponent<PostProcessLayer>().enabled = grayScale;

			score.Reset();
			resetPartialCounters();

			changeWhenAnimating = false;
			gameEnded = false;
		}

		private void resetPartialCounters() {
			rightCounterOnCurrentTarget = 0;
			missedCounterOnCurrentTarget = 0;
		}
		
		public void prepareFirstTarget(string target) {
			showFirstTarget.ChangeTarget(target);
			stopAllConveyors();
		}
		public void startMovingAfterInstructions() {
			timer.activation = true;
			timerBar.gameObject.SetActive(true);
			restartAllConveyors();
			if (useSound) {
				conveyorSource.Play();
			}
			interferences.allowInterferences = true;
			if (this.highlightChangeOfTarget) {
				changeWhenAnimating = false;
				animateFirstTargetAboutToChange();
			} else {
				spawner.activation = true;
			}
		}
		// Update is called once per frame
		void Update() {
			if (timer.itIsEnd && !gameEnded) {
				endGame();
			}
			if (Input.GetMouseButtonDown(0) && !stopInput) {
				RaycastHit hit;
				// Check if an object is clicked
				if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100, LayerMask.GetMask("Objects"))) {
					onObjectClick(hit.transform);
				}
			}
		}
		private bool checkValidity(Transform transform) {
			ObjectProperties properties = transform.gameObject.GetComponent<ObjectProperties>();
			if (properties != null) {
				return (properties.objectName == spawner.firstTarget) || (properties.objectName == spawner.secondTarget);
			} else {
				return false;
			}
		}

		private void onObjectClick(Transform obj) {
			if (isTargetNotTaken(obj)) {
				if (checkValidity(obj)) {
					onRightObjectClick(obj);
				} else {
					onWrongObjectClick(obj);
				}
			}
		}

		private bool isTargetNotTaken(Transform obj) {
			return (obj.GetComponent<RightTarget>() == null);
		}

		private void onRightObjectClick(Transform obj) {
			score.RightCounter++;
			rightCounterOnCurrentTarget++;
			if (rightCounterOnCurrentTarget >= rightTargetsForChange) {
				changeTarget();
			}
			
			//VFX
			rightAnswerPrefab.transform.position = obj.position;
			rightAnswerPrefab.Play();
			
			//SFX
			WrongSelectionSource.PlayOneShot(rightAnswerClip);
			
			//Animation
			RightTarget anim = obj.gameObject.AddComponent<RightTarget>();
			anim.overlayColor = rightColor;
		}

		private void onWrongObjectClick(Transform obj) {
			score.WrongCounter++;
			
			//VFX
			wrongAnswerPrefab.transform.position = obj.position;
			wrongAnswerPrefab.Play();
			
			//SFX
			WrongSelectionSource.PlayOneShot(wrongAnswerClip);
			
			//Animation
			RightTarget anim = obj.gameObject.AddComponent<RightTarget>();
			anim.overlayColor = wrongColor;
		}

		public void onObjectExit(Transform obj)
		{
			var isMissed = isTargetNotTaken(obj) && checkValidity(obj);
			
			if (isMissed) {
				score.MissedCounter++;
				missedCounterOnCurrentTarget++;
				if (missedCounterOnCurrentTarget >= missedTargetsForChange) {
					changeTarget();
				}
				var pos = new Vector3(8, -10, -8);
				GameObject newExplosion = Instantiate(missPrefab);
				missSource = newExplosion.AddComponent<AudioSource>();
				missSource.clip = wrongAnswerClip;
				newExplosion.transform.parent = missRoot.transform;
				newExplosion.transform.position = pos;
				missSource.Play();
			}
			
			Destroy(obj.gameObject);
			
			onItemFunneledEvent.Invoke(!isMissed);
		}

		private void changeTarget() {
			resetPartialCounters();

			if (highlightChangeOfTarget) {
				spawner.activation = false;
				conveyorSource.Stop();
				stopAllConveyors();
			}
			changeTargetAfterDelay();
		}

		private void changeTargetAfterDelay() {
			changeWhenAnimating = true;
			interferences.allowInterferences = false;
			animateFirstTargetAboutToChange();
		}

		private void stopAllConveyors() {
			_conveyorBelts.ForEach(track => track.IsStopped = true);
		}

		private void restartAllConveyors() {
			_conveyorBelts.ForEach(track => track.IsStopped = false);
		}

		private void animateFirstTargetAboutToChange() {
			showFirstTarget.PulseForTime(changeAnimationTime, AfterAnimateFirstTarget);
			if (showSecondTarget.gameObject.activeSelf)
			{
				showSecondTarget.PulseForTime(changeAnimationTime, null);
			}
		}

		private void AfterAnimateFirstTarget() {
			if (changeWhenAnimating) {
				doTargetChange();
				restartAfterPause();
			} else {
				spawner.activation = true;
			}
		}

		public void bigElloTalked()
        {
            bigElloTalking.oneShotRead.RemoveListener(bigElloTalked);

            if (gameEnded) {
				endPopup.Show(saveResultsAndExit);
			}
			//TODO check if it works only for halftime one shot
			else if (!timer.activation)
			{
				restartAfterPause();
			}
		}

		void doTargetChange() {
			spawner.nextTargets();
			showFirstTarget.ChangeTarget(spawner.firstTarget);
			if (spawner.secondTarget != "") {
				showSecondTarget.gameObject.SetActive(true);
				Invoke(nameof(displaySecondTarget), 0.01f);
			}
		}

		private void displaySecondTarget() {
			if (firstLoop) {
				pauseGame();
                bigElloTalking.oneShotRead.AddListener(bigElloTalked);
                bigElloTalking.ShowMessage(_changeTaskAll);
				firstLoop = false;
			}
			showSecondTarget.ChangeTarget(spawner.secondTarget);
		}

		private void pauseGame() {
			timer.activation = false;
			spawner.activation = false;
			stopInput = true;
			interferences.allowInterferences = false;
			stopAllConveyors();
			if (useSound) {
				conveyorSource.Stop();
			}
		}

		public void restartAfterPause()
		{
			if (timer.itIsEnd) return;
			
			timer.activation = true;
			spawner.activation = true;
			stopInput = false;
			interferences.allowInterferences = true;
			restartAllConveyors();
			if (useSound) {
				conveyorSource.Play();
			}
		}

		private void endGame() {
			pauseGame();
            bigElloTalking.oneShotRead.AddListener(bigElloTalked);
            bigElloTalking.ShowMessage(_endDemo);
			gameEnded = true;
		}

		private void saveResultsAndExit() {
			if (GameState.Instance.testMode) {
				GameState.Instance.LoadSceneAfterRoom();
			} else {
				GameState.Instance.levelBackend.ExitRoom(timer.totalTime, score,
				() => {
					GameState.Instance.LoadSceneAfterRoom();
				},
				err => {
					Debug.Log(err.Message);
				});
			}
		}

	}

}
