using UnityEngine;
using UnityEngine.Events;

public class Timer : MonoBehaviour
{
    public int totalTime = 240;

    public float timeLeft = 0;
    public bool itIsEnd;

    public bool activation = false;

    public UnityEvent<int> timerSecondsCallback;

    public UnityEvent timerCallback;

    void Update()
    {
        if (activation)
        {
            float lastTimeLeft = timeLeft;
            timeLeft = Mathf.Max(timeLeft - Time.deltaTime, 0);

            if ((int)lastTimeLeft != (int)timeLeft)
            {
                if (timerSecondsCallback != null)
                    timerSecondsCallback.Invoke((int)timeLeft);
            }

            if (timeLeft == 0)
            {
                itIsEnd = true;
                timerCallback?.Invoke();
            }
        }
    }

    public void SetTime(int seconds)
    {
        totalTime = seconds;
        timeLeft = seconds;
        itIsEnd = false;

        timerSecondsCallback?.Invoke(seconds);
    }

    public void Reset()
    {
        activation = false;

        timeLeft = totalTime;

        if (timerSecondsCallback != null)
            timerSecondsCallback.Invoke((int)timeLeft);
        itIsEnd = false;
    }

    public float getTimeFactor()
    {
        return Mathf.Clamp01(1f - (timeLeft / totalTime));
    }

    public void SetActivation(bool value)
    {
        activation = value;
    }

}
