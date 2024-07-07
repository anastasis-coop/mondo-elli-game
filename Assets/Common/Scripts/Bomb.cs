using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Bomb replaces all the RoomN_Bomb scripts into one
/// since their behaviour has been uniformed.
/// </summary>
public class Bomb : MonoBehaviour
{
    public float timeoutSeconds = 3;
    public float explosionSeconds = 2;
    public bool showCountUp;

    [Space]
    [SerializeField]
    private Image background;
    [SerializeField]
    private Image timerFill;

    [SerializeField]
    private Text timerLabel;
    
    [Space]
    public UnityEvent onExplode;

    private float elapsedSeconds;
    private bool isInvisible;
    private float visibleWhenRemaining = -1;

    private void Update()
    {
        elapsedSeconds += Time.deltaTime;
        var progress = Mathf.Clamp01(elapsedSeconds / timeoutSeconds);

        timerFill.fillAmount = showCountUp ? progress : 1 - progress;
        timerLabel.text = (showCountUp ? Mathf.FloorToInt(elapsedSeconds) : Mathf.CeilToInt(timeoutSeconds - elapsedSeconds)).ToString();

        var visible = !isInvisible && (visibleWhenRemaining < 0 || timeoutSeconds - elapsedSeconds <= visibleWhenRemaining);
        timerLabel.enabled = visible;
        timerFill.enabled = visible;
        background.enabled = visible;

        if (progress < 1) return;

        enabled = false;
        Explode();
    }

    private void Explode()
    {
        onExplode.Invoke();
    }

    public void StartCountdown()
    {
        enabled = true;
        elapsedSeconds = 0;
        timerFill.fillAmount = showCountUp ? 0 : 1;
        timerLabel.text = (showCountUp ? Mathf.FloorToInt(elapsedSeconds) : Mathf.CeilToInt(timeoutSeconds - elapsedSeconds)).ToString();
    }

    public void StopCountdown()
    {
        enabled = false;
        elapsedSeconds = 0;
        timerFill.fillAmount = 0;
        timerLabel.text = 0.ToString();
    }

    public void SetVisibleWhenReaches(float timeStampSeconds)
    {
        var visible = timeoutSeconds - elapsedSeconds <= visibleWhenRemaining;
        timerLabel.enabled = visible;
        timerFill.enabled = visible;
        background.enabled = visible;
        visibleWhenRemaining = timeStampSeconds;
    }

    public void SetAlwaysVisible() => SetVisibleWhenReaches(-1);

    public void SetAlwaysInvisible(bool alwaysInvisible) => isInvisible = alwaysInvisible;
}
