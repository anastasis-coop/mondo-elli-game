using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Room1
{
	public class Interferences : MonoBehaviour
	{
		public bool allowInterferences = false;

		public Timer timer;
		private AudioSource rainSource;
		public AudioClip rainSound;

		private AudioSource smokeSource;
		public AudioClip smokeSound;

		private AudioSource sparckleSource;
		public AudioClip sparckleSound;

		private AudioSource bubbleSource;
		public AudioClip bubbleSound;

		public Material grayMaterial;

		const int INTERFERENCES_COUNT = 15;
		private enum interferenceType
		{
			None, Accelerate, Decelerate, BubblesTop, SmokeTop, SparklesTop, LightsTop, BubblesLeft, SmokeLeft, SparklesLeft, LightsLeft, Snow, Rain, BouncingBalls, Flickering
		};

		public bool canDoSnowing = false;
		public bool canDoRain = false;
		public bool canAccelerate = false;
		public bool canDecelerate = false;
		public bool canDoBubbles = false;
		public bool canDoSmoke = false;
		public bool canDoSparkles = false;
		public bool canDoLight = false;
		public bool canDoBouncingBalls = false;
		public bool canDoFlickering = false;
		public bool nothing = false;

		public bool grayScale = false;

		public float minPause = 0.5f;
		public float maxPause = 2f;
		public float minDuration = 5f;
		public float maxDuration = 10f;
		public List<ConveyorBelt> ConveyorBelts;

		public ParticlesController Snow;
		public ParticlesController Rain;
		public ParticlesController bubblesTop;
		public ParticlesController smokeTop;
		public ParticlesController sparklesTop;
		public Light[] lightsTop;

		public ParticlesController bubblesLeft;
		public ParticlesController smokeLeft;
		public ParticlesController sparklesLeft;
		public Light[] lightsLeft;
		public List<BouncingBall> bouncingBalls;

		private float pause = 0;
		private float duration = 0;
		private float elapsed;
		private interferenceType interference;

		private int conveyorIndexToIncline;
		private float conveyorDirection;


		void Start()
		{
			Rain.StopParticles();
			Snow.StopParticles();

			rainSource = gameObject.AddComponent<AudioSource>();
			bubbleSource = gameObject.AddComponent<AudioSource>();
			smokeSource = gameObject.AddComponent<AudioSource>();
			sparckleSource = gameObject.AddComponent<AudioSource>();

			rainSource.playOnAwake = false;
			bubbleSource.playOnAwake = false;
			smokeSource.playOnAwake = false;
			sparckleSource.playOnAwake = false;

			rainSource.loop = true;
			bubbleSource.loop = true;
			smokeSource.loop = true;
			sparckleSource.loop = true;

			bubbleSource.pitch = 0.5f;

			rainSource.clip = rainSound;
			bubbleSource.clip = bubbleSound;
			smokeSource.clip = smokeSound;
			sparckleSource.clip = sparckleSound;

			pause = Random.Range(minPause, maxPause);
		}
		void Update()
		{
			if (timer.itIsEnd)
			{
				allowInterferences = false;
			}
			if (allowInterferences)
			{
				if (pause > 0)
				{
					pause = Mathf.Max(0, pause - Time.deltaTime);
					if (pause == 0)
					{
						duration = Random.Range(minDuration, maxDuration);
						elapsed = 0;
						StartNewInterference();
					}
				}
				else
				{
					float saveElapsed = elapsed;
					elapsed += Time.deltaTime;
					float progress = Mathf.Clamp01(elapsed / duration);
					if (UpdateInterference(progress))
					{
						if (progress == 1)
						{
							StopCurrentInterference();
							pause = Random.Range(minPause, maxPause);
						}
					}
					else
					{
						elapsed = saveElapsed;
					}
				}
			}
			else
			{
				StopCurrentInterference();
			}
		}
		private bool isInterferenceEnabled(int interference)
		{

			switch (interference)
			{
				case 1:
					return canAccelerate;
				case 2:
					return canDecelerate;
				case 3:
					return canDoBubbles;
				case 4:
					return canDoSmoke;
				case 5:
					return canDoSparkles;
				case 6:
					return canDoLight;
				case 7:
					return canDoBubbles;
				case 8:
					return canDoSmoke;
				case 9:
					return canDoSparkles;
				case 10:
					return canDoLight;
				case 11:
					return canDoSnowing;
				case 12:
					return canDoRain;
				case 13:
					return canDoBouncingBalls;
				case 14:
					return canDoFlickering;
			}
			return false;
		}

		public bool allInterfencesDisabled()
		{
			for (int i = 1; i <= INTERFERENCES_COUNT; i++)
			{
				if (isInterferenceEnabled(i))
				{
					return false;
				}
			}
			return true;
		}

		private void StartNewInterference()
		{
			int newInterference;
			if (allInterfencesDisabled())
				newInterference = 0;
			else
			{
				do
				{
					newInterference = Random.Range(1, INTERFERENCES_COUNT);
				} while (!(isInterferenceEnabled(newInterference)));
			}
			if(allowInterferences)
				switch (newInterference)
				{
					case 0:
						interference = interferenceType.None;
						break;
					case 1:
						interference = interferenceType.Accelerate;
						conveyorIndexToIncline = Random.Range(0, ConveyorBelts.Count);
						conveyorDirection = ConveyorBelts[conveyorIndexToIncline].Speed;
						conveyorDirection /= Mathf.Abs(conveyorDirection); // -1 o 1
						break;
					case 2:
						interference = interferenceType.Decelerate;
						conveyorIndexToIncline = Random.Range(0, ConveyorBelts.Count);
						conveyorDirection = ConveyorBelts[conveyorIndexToIncline].Speed;
						conveyorDirection /= Mathf.Abs(conveyorDirection); // -1 o 1
						break;
					case 3:
						interference = interferenceType.BubblesTop;
						bubblesTop.StartParticles();
						bubbleSource.Play();
						break;
					case 4:
						interference = interferenceType.SmokeTop;
						smokeTop.StartParticles();
						smokeSource.Play();
						break;
					case 5:
						interference = interferenceType.SparklesTop;
						sparklesTop.StartParticles();
						sparckleSource.Play();
						break;
					case 6:
						interference = interferenceType.LightsTop;
						break;
					case 7:
						interference = interferenceType.BubblesLeft;
						bubblesLeft.StartParticles();
						bubbleSource.Play();
						break;
					case 8:
						interference = interferenceType.SmokeLeft;
						smokeLeft.StartParticles();
						smokeSource.Play();
						break;
					case 9:
						interference = interferenceType.SparklesLeft;
						sparklesLeft.StartParticles();
						sparckleSource.Play();
						break;
					case 10:
						interference = interferenceType.LightsLeft;
						break;
					case 11:
						interference = interferenceType.Snow;
						Snow.StartParticles();
						break;
					case 12:
						interference = interferenceType.Rain;
						Rain.StartParticles();
						rainSource.Play();
						break;
					case 13:
						interference = interferenceType.BouncingBalls;
						bouncingBalls.ForEach(ball =>
						{
							if (grayScale)
							{
								ball.GetComponent<Renderer>().sharedMaterial = grayMaterial;
							}
							ball.activation = true;
						});
						break;
					case 14:
						interference = interferenceType.Flickering;
						ConveyorBelts.ForEach(track => track.StartFlickering(grayScale ? Color.white : Color.yellow));
						break;
				}
		}

		private bool UpdateInterference(float progress)
		{
			switch (interference)
			{
				case interferenceType.Accelerate:
					if (ConveyorBelts[conveyorIndexToIncline].Speed == 0)
					{
						return false;
					}
					float inclinazioneFast = -5f * conveyorDirection;
					float speed1 = ConveyorBelts[conveyorIndexToIncline].Speed;
					float speedFast = speed1 * 2f;
					if (progress < 0.25)
					{
						ConveyorBelts[conveyorIndexToIncline].transform.eulerAngles = new Vector3(0, 0, inclinazioneFast * progress * 4);
						ConveyorBelts[conveyorIndexToIncline].SpeedOverride = Mathf.Lerp(speed1, speedFast, progress * 4);
					}
					else if (progress < 0.75)
					{
						ConveyorBelts[conveyorIndexToIncline].transform.eulerAngles = new Vector3(0, 0, inclinazioneFast);
						ConveyorBelts[conveyorIndexToIncline].SpeedOverride = speedFast;
					}
					else
					{
						ConveyorBelts[conveyorIndexToIncline].transform.eulerAngles = new Vector3(0, 0, inclinazioneFast * (1f - progress) * 4);
						ConveyorBelts[conveyorIndexToIncline].SpeedOverride = Mathf.Lerp(speed1, speedFast, (1f - progress) * 4);
					}
					break;
				case interferenceType.Decelerate:
					if (ConveyorBelts[conveyorIndexToIncline].Speed == 0)
					{
						return false;
					}
					float inclinazioneSlow = 5f * conveyorDirection;
					float speed2 = ConveyorBelts[conveyorIndexToIncline].Speed;
					float speedSlow = speed2 / 2f;
					if (progress < 0.25)
					{
						ConveyorBelts[conveyorIndexToIncline].transform.eulerAngles = new Vector3(0, 0, inclinazioneSlow * progress * 4);
						ConveyorBelts[conveyorIndexToIncline].SpeedOverride = Mathf.Lerp(speed2, speedSlow, progress * 4);
					}
					else if (progress < 0.75)
					{
						ConveyorBelts[conveyorIndexToIncline].transform.eulerAngles = new Vector3(0, 0, inclinazioneSlow);
						ConveyorBelts[conveyorIndexToIncline].SpeedOverride = speedSlow;
					}
					else
					{
						ConveyorBelts[conveyorIndexToIncline].transform.eulerAngles = new Vector3(0, 0, inclinazioneSlow * (1f - progress) * 4);
						ConveyorBelts[conveyorIndexToIncline].SpeedOverride = Mathf.Lerp(speed2, speedSlow, (1f - progress) * 4);
					}
					break;
				case interferenceType.BubblesTop:
					break;
				case interferenceType.SmokeTop:
					break;
				case interferenceType.SparklesTop:
					break;
				case interferenceType.LightsTop:
					foreach (Light light in lightsTop)
					{
						light.intensity = Mathf.Sin(progress * 10 * Mathf.PI);
					}
					break;
				case interferenceType.BubblesLeft:
					break;
				case interferenceType.SmokeLeft:
					break;
				case interferenceType.SparklesLeft:
					break;
				case interferenceType.LightsLeft:
					foreach (Light light in lightsLeft)
					{
						light.intensity = Mathf.Sin(progress * 10 * Mathf.PI);
					}
					break;
				case interferenceType.Snow:
					break;
				case interferenceType.Rain:
					break;
				case interferenceType.BouncingBalls:
					break;
				case interferenceType.Flickering:
					break;
			}
			return true;
		}

		public void StopCurrentInterference()
		{
			switch (interference)
			{
				case interferenceType.None:
					break;
				case interferenceType.Accelerate:
					
					break;
				case interferenceType.Decelerate:
					break;
				case interferenceType.BubblesTop:
					bubblesTop.StopParticles();
					bubbleSource.Stop();
					break;
				case interferenceType.SmokeTop:
					smokeTop.StopParticles();
					smokeSource.Stop();
					break;
				case interferenceType.SparklesTop:
					sparklesTop.StopParticles();
					sparckleSource.Stop();
					break;
				case interferenceType.BubblesLeft:
					bubblesLeft.StopParticles();
					bubbleSource.Stop();
					break;
				case interferenceType.SmokeLeft:
					smokeLeft.StopParticles();
					smokeSource.Stop();
					break;
				case interferenceType.SparklesLeft:
					sparklesLeft.StopParticles();
					sparckleSource.Stop();
					break;
				case interferenceType.Snow:
					Snow.StopParticles();
					break;
				case interferenceType.Rain:
					Rain.StopParticles();
					rainSource.Stop();
					break;
				case interferenceType.BouncingBalls:
					bouncingBalls.ForEach(ball => ball.activation = false);
					break;
				case interferenceType.Flickering:
					ConveyorBelts.ForEach(track => track.StopFlickering());
					break;
			}
			interference = interferenceType.None;
		}

	}
}
 