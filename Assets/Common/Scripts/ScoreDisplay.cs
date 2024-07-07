using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField]
    private Score score;

    [SerializeField]
    private TextMeshProUGUI totalScoreLabel;

    [SerializeField]
    private GameObject[] coins;

    [SerializeField]
    private Image coinImage;

    [SerializeField]
    private Sprite elloCoinSprite;

    [SerializeField]
    private Sprite comboCoinSprite;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private string coinTrigger;

    [SerializeField]
    private string comboTrigger;

    private void Awake()
    {
        score.TotalScoreChanged += OnTotalScoreChanged;
        score.RightAnswer += OnRightAnswer;
        score.WasReset += OnWasReset;
        score.Combo += OnCombo;
    }

    private void OnTotalScoreChanged(int totalScore)
    {
        totalScoreLabel.text = totalScore.ToString();

        for (int i = 0; i < coins.Length; i++)
        {
            coins[i].SetActive(i < totalScore);
        }
    }

    private void OnRightAnswer(int _)
    {
        coinImage.sprite = elloCoinSprite;
        animator.SetTrigger(coinTrigger);
    }

    private void OnCombo(int _)
    {
        coinImage.sprite = comboCoinSprite;
        animator.SetTrigger(comboTrigger);
    }

    private void OnWasReset()
    {
        totalScoreLabel.text = 0.ToString();

        for (int i = 0; i < coins.Length; i++)
        {
            coins[i].SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (score == null) return;

        score.TotalScoreChanged -= OnTotalScoreChanged;
        score.RightAnswer -= OnRightAnswer;
        score.WasReset -= OnWasReset;
        score.Combo -= OnCombo;
    }
}
