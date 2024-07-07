using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using DG.Tweening;

public class RoomEndPopup : MonoBehaviour
{
    [SerializeField]
    private Score score;

    [SerializeField]
    private TextMeshProUGUI answerCountLabel;

    [SerializeField]
    private TextMeshProUGUI comboCountLabel;

    [SerializeField]
    private LocalizedString comboPattern;

    [SerializeField]
    private GameObject[] stars;

    [SerializeField]
    private float[] starThresholds;

    private Action _callback;

    public void Show(Action callback)
    {
        gameObject.SetActive(true);

        _callback = callback;

        answerCountLabel.text = score.RightCounter + "/" + score.TotalAnswers;
        comboCountLabel.text = string.Format(comboPattern.GetLocalizedString(), score.ComboCounter);

        float percentage = score.RightPercentage;

        for (int i = 0; i < stars.Length; i++)
        {
            bool star = percentage >= starThresholds[i];

            stars[i].SetActive(star);

            if (star)
            {
                stars[i].transform.DOScale(1, 1).From(0).SetEase(Ease.OutBack).SetDelay(i);
                GameState.Instance.RoomStars++; // HACK to display the star in session end popup
            }
        }
    }

    public void OnContinueButtonPressed()
    {
        gameObject.SetActive(false);
        _callback?.Invoke();
    }
}
