using UnityEngine;

public class TimerBar : MonoBehaviour
{
    [SerializeField]
    private Timer timer;

    [SerializeField]
	private RectTransform fillTransform;

	private void Update()
	{
		Vector2 anchorMax = fillTransform.anchorMax;
		anchorMax.x = timer.getTimeFactor();
		fillTransform.anchorMax = anchorMax;
	}
}
