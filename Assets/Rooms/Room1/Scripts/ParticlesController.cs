using UnityEngine;

namespace Room1
{
	public class ParticlesController: MonoBehaviour
	{
		public void StartParticles() {
			foreach (ParticleSystem particles in GetComponentsInChildren<ParticleSystem>() as ParticleSystem[]) {
				particles.Play();
			}
		}
		public void StopParticles() {
			foreach (ParticleSystem particles in GetComponentsInChildren<ParticleSystem>() as ParticleSystem[]) {
				if (particles.isEmitting) {
					particles.Stop();
				}
			}
		}
	}
}
