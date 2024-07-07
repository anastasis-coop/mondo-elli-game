using System.Collections;
using Common;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.PostProcessing;

namespace Room1
{
    public class ConveyorRoomforLevel1232 : MonoBehaviour
	{
		public bool grayScale;
		public Timer timer;
		public Score score;
		public SpawnerforLevel1232 spawner;
		public InterferenceforLevel1232 interferences;
		public TimerBar timerBar;
		public MessageSystem bigElloTalking;
        public BigElloSaysConfig _endDemo;
        public RestartButton restartButton;

		public float minDistance = 1.5f;

		[SerializeField] private Color rightColor;
		[SerializeField] private Color wrongColor;
		
		[SerializeField] private AudioClip rightAnswerClip;
		[SerializeField] private AudioClip wrongAnswerClip;
		private AudioSource WrongSelectionSource;
		
		[SerializeField] private ParticleSystem rightAnswerPrefab;
		[SerializeField] private ParticleSystem wrongAnswerPrefab;

		public AudioClip missSound;
		public GameObject missRoot;
		public GameObject missPrefab;
		
		public UnityEvent<bool> onItemFunneledEvent;

        [SerializeField]
        private RoomEndPopup endPopup;

        private Plane plane;
		
		private bool stopInput;
		private bool gameEnded;

		public ItemsSet ItemsSet { get; set; }

		// Called from LevelController start
		public void Prepare() {
			Debug.Log("Conveyor Room prepare");

			spawner.Prepare();

			WrongSelectionSource = gameObject.AddComponent<AudioSource>();
			WrongSelectionSource.playOnAwake = false;

			//Removed for graphical glitch in build
			//Camera.main.GetComponent<PostProcessVolume>().enabled = grayScale;
			//Camera.main.GetComponent<PostProcessLayer>().enabled = grayScale;

			score.Reset();
			gameEnded = false;

			float z = spawner.transform.position.z;
			plane = new Plane(new Vector3(0, 10, z), new Vector3(-20, -10, z), new Vector3(20, -10, z));
		}

		public void startAfterInstructions() {
			//TODO Consider a delay
			
			timer.activation = true;
			timerBar.gameObject.SetActive(true);
			spawner.activation = true;
			interferences.allowInterferences = true;
		}

		// Update is called once per frame
		void Update() {
			if (timer.itIsEnd && !gameEnded) {
				endGame();
			}
			if (Input.GetMouseButtonDown(0) && !stopInput) {
				// create a ray from the mousePosition
				var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				float dist;
				if (plane.Raycast(ray, out dist)) {
					Vector3 hitPoint = ray.GetPoint(dist);
					Transform nearest = null;
					float minSqrMagnitude = 1000000f;
					foreach (Transform child in spawner.root.transform) {
						Vector3 vec = child.position - hitPoint;
						float sqrMagnitude = Vector3.SqrMagnitude(vec);
						if (sqrMagnitude < minSqrMagnitude) {
							minSqrMagnitude = sqrMagnitude;
							nearest = child;
						}
					}
					if (nearest != null && Vector3.Magnitude(nearest.position - hitPoint) < minDistance) {
						onObjectClick(nearest.GetComponent<ObjectProperties1232>());
					}
				}
			}
		}

		private void onObjectClick(ObjectProperties1232 obj) {
			if (isTargetNotTaken(obj.transform)) {
				if (obj.isTarget) {
					onRightObjectClick(obj.transform);
				} else {
					onWrongObjectClick(obj.transform);
				}
			}
		}

		private bool isTargetNotTaken(Transform obj) {
			return (obj.GetComponent<RightTarget>() == null);
		}

		private void onRightObjectClick(Transform obj) {
			score.RightCounter++;
			// Attach the RightTarget script to object (to make it turn and disappear)
			obj.GetComponent<Collider>().enabled = false;
			
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

		public void onObjectExit(ObjectProperties1232 obj) {
			var isMissed = isTargetNotTaken(obj.transform) && obj.isTarget;
			
			if (isMissed) {
				score.MissedCounter++;
				var pos = new Vector3(obj.transform.position.x, -14, 0);
				GameObject newExplosion = Instantiate(missPrefab);
				newExplosion.transform.parent = missRoot.transform;
				newExplosion.transform.position = pos;
				StartCoroutine(playMissSound());
			}
			
			obj.Despawn();
			
			onItemFunneledEvent.Invoke(!isMissed);
		}

		IEnumerator playMissSound() {
			AudioSource missSource = gameObject.AddComponent<AudioSource>();
			missSource.clip = missSound;
			missSource.Play();
			yield return new WaitForSeconds(missSound.length);
			Destroy(missSource);
		}

		public void bigElloTalked() {
			bigElloTalking.oneShotRead.RemoveListener(bigElloTalked);
			if (gameEnded) {
				endPopup.Show(SaveResultsAndExit);
			} else {
				startAfterInstructions();
			}
		}

		private void ClearAllObjects() {
			foreach (Transform child in spawner.root.transform) 
			{
				child.gameObject.SetActive(false);
			}
		}

		private void endGame() {
			stopInput = true;
			spawner.activation = false;
			ClearAllObjects();
			bigElloTalking.oneShotRead.AddListener(bigElloTalked);
			bigElloTalking.ShowMessage(_endDemo);
			gameEnded = true;
		}

		private void SaveResultsAndExit() {
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
