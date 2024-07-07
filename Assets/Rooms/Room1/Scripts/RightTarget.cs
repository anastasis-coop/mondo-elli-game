using UnityEngine;

namespace Room1
{
	public class RightTarget : MonoBehaviour
	{
		public Color overlayColor;

		private float elapsed = 0;
		private float rotationSpeed = 0;

		private void Start()
		{
			MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer meshRenderer in renderers)
			{
				foreach (Material material in meshRenderer.materials)
				{
					material.color = overlayColor;  
				}
            
			}
		}

		private void Update()
		{
			elapsed += Time.deltaTime;
			if (elapsed is > 0 and < 0.15f)
			{
				transform.localScale += Vector3.one * 0.004f;
			}

			if (elapsed is >= 0.15f)
			{
				transform.localScale -= Vector3.one * 0.01f;
			}

			if (transform.localScale.x < 0.2f)
			{
				if (TryGetComponent<ObjectProperties1232>(out var obj)) obj.Despawn();
				else Destroy(gameObject);
			}
		}
	}
}
