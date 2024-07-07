using System.Collections.Generic;
using UnityEngine;

namespace Room1
{
	public class InterferenceforLevel1232 : MonoBehaviour
	{
		public bool allowInterferences = false;

		public Timer timer;
		private AudioSource rainSource;
		public AudioClip rainSound;

		private AudioSource smokeSource;
		public AudioClip smokeSound;

		private AudioSource sparkleSource;
		public AudioClip sparkleSound;

		private AudioSource bubbleSource;
		public AudioClip bubbleSound;

		const int INTERFERENCES_COUNT = 10;

		private enum interferenceType
		{
			None, BubblesTop, SmokeTop, SparklesTop, BubblesLeft, SmokeLeft, SparklesLeft, Snow, Rain, BouncingBalls
		};
		public bool canDoSnowing = false;
		public bool canDoRain = false;
		public bool canDoBubbles = false;
		public bool canDoSmoke = false;
		public bool canDoSparkles = false;
		public bool canDoLight = false;
		public bool canDoBouncingBalls = false;

		public bool nothing = false;

		public float minPause = 0.5f;
		public float maxPause = 2f;
		public float minDuration = 5f;
		public float maxDuration = 10f;

		public ParticlesController Snow;
		public ParticlesController Rain;
		public ParticlesController bubblesTop;
		public ParticlesController smokeTop;
		public ParticlesController sparklesTop;
		public ParticlesController bubblesLeft;
		public ParticlesController smokeLeft;
		public ParticlesController sparklesLeft;
		public List<BouncingBall> bouncingBalls;

		private float pause = 0;
		private float duration = 0;
		private float elapsed;
		private interferenceType interference;

		void Start()
		{
			Rain.StopParticles();
			Snow.StopParticles();

			rainSource = gameObject.AddComponent<AudioSource>();
			bubbleSource = gameObject.AddComponent<AudioSource>();
			smokeSource = gameObject.AddComponent<AudioSource>();
			sparkleSource = gameObject.AddComponent<AudioSource>();


			rainSource.playOnAwake = false;
			bubbleSource.playOnAwake = false;
			smokeSource.playOnAwake = false;
			sparkleSource.playOnAwake = false;

			rainSource.loop = true;
			bubbleSource.loop = true;
			smokeSource.loop = true;
			sparkleSource.loop = true;

			bubbleSource.pitch = 0.5f;

			rainSource.clip = rainSound;
			bubbleSource.clip = bubbleSound;
			smokeSource.clip = smokeSound;
			sparkleSource.clip = sparkleSound;

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
					// Debug.Log(pause);
					if (pause == 0)
					{
						duration = Random.Range(minDuration, maxDuration);
						elapsed = 0;
						Debug.Log("START NEW INTERF");
						StartNewInterference();
					}
				}
				else
				{
					elapsed += Time.deltaTime;
					float progress = Mathf.Clamp01(elapsed / duration);
					UpdateInterference(progress);
					if (progress == 1)
					{
						StopCurrentInterference();
						pause = Random.Range(minPause, maxPause);
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
					return canDoBubbles;
				case 2:
					return canDoSmoke;
				case 3:
					return canDoSparkles;
				case 4:
					return canDoBubbles;
				case 5:
					return canDoSmoke;
				case 6:
					return canDoSparkles;
				case 7:
					return canDoSnowing;
				case 8:
					return canDoRain;
				case 9:
					return canDoBouncingBalls;
			}
			return false;
		}
		private bool allInterfencesDisabled()
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
			switch (newInterference)
			{
				case 0:
					interference = interferenceType.None;
					break;
				case 1:
					interference = interferenceType.BubblesTop;
					bubblesTop.StartParticles();
					bubbleSource.Play();
					break;
				case 2:
					interference = interferenceType.SmokeTop;
					smokeTop.StartParticles();
					smokeSource.Play();
					break;
				case 3:
					interference = interferenceType.SparklesTop;
					sparklesTop.StartParticles();
					sparkleSource.Play();
					break;
				case 4:
					interference = interferenceType.BubblesLeft;
					bubblesLeft.StartParticles();
					bubbleSource.Play();
					break;
				case 5:
					interference = interferenceType.SmokeLeft;
					smokeLeft.StartParticles();
					smokeSource.Play();
					break;
				case 6:
					interference = interferenceType.SparklesLeft;
					sparklesLeft.StartParticles();
					sparkleSource.Play();
					break;
				case 7:
					interference = interferenceType.Snow;
					Snow.StartParticles();
					break;
				case 8:
					interference = interferenceType.Rain;
					Rain.StartParticles();
					rainSource.Play();
					break;
				case 9:
					interference = interferenceType.BouncingBalls;
					bouncingBalls.ForEach(ball => ball.activation = true);
					break;
			}
		}

		private void UpdateInterference(float progress)
		{
			switch (interference)
			{
				case interferenceType.BubblesTop:
					break;
				case interferenceType.SmokeTop:
					break;
				case interferenceType.SparklesTop:
					break;
				case interferenceType.BubblesLeft:
					break;
				case interferenceType.SmokeLeft:
					break;
				case interferenceType.SparklesLeft:
					break;
				case interferenceType.Snow:
					break;
				case interferenceType.Rain:
					break;
				case interferenceType.BouncingBalls:
					break;
			}
		}

		private void StopCurrentInterference()
		{
			switch (interference)
			{
				case interferenceType.None:
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
					sparkleSource.Stop();
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
					sparkleSource.Stop();
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
			}
			interference = interferenceType.None;
		}
	}
}
