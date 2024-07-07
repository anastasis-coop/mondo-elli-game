using UnityEngine;

public class ExplosionAutoExtinguish : MonoBehaviour
{

	public float lifeTime = 1f;
	public float dieTime = 1f;

	private float elapsed = 0;

	void Update() {
		elapsed += Time.deltaTime;
		if (elapsed > lifeTime) {
			float progress = Mathf.Clamp01((elapsed - lifeTime) / dieTime);
			GetComponent<Light>().intensity = 1f - progress;
			if (progress == 1f) {
				Destroy(gameObject);
			}
		}
	}
}
