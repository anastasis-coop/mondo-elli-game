using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Room3
{
    public class AnimalsController : MonoBehaviour
  {

    public int rightProbability = 50;
    public int secondsForEachTarget = 30;
    public float timeout = 1.5f;
    public Bomb bomb;
    public FeedbackController feedback;
    public Sprite[] animals;
    public Sprite backgroundWhite;
    public Sprite backgroundRed;
    [SerializeField] private Collider _animalsSkipButtonCollider;
    [SerializeField] private Collider _animalsConfirmButtonCollider;
    public CardRoller cardRoller;
    public Image targetImage;
    public Timer timer;
    public Score score;
    public bool useBackground;
    public UnityEvent onGameOver = new UnityEvent();

    private int targetIndex;
    private bool selected;
    private float elapsed = 0;
    private bool waiting = false;
    private int targetStep;

    void Start() {
      selected = false;
      waiting = false;
      elapsed = 0;
    }

    void Update() {
      if (waiting) {
        elapsed += Time.deltaTime;

        if (!bomb.gameObject.activeSelf && elapsed >= 2 * timeout / 3f)
        {
          bomb.showCountUp = false;
          bomb.timeoutSeconds = timeout - elapsed;
          bomb.gameObject.SetActive(true);
          bomb.enabled = true;
          bomb.StartCountdown();
        }
        
        if (elapsed > timeout) {
          elapsed = 0;
          waiting = false;
          onTimeout();
        }
      }
    }

    private void enableTimeout() {
      elapsed = 0;
      waiting = true;
    }

    private void disableTimeout() {
      elapsed = 0;
      waiting = false;
    }

    public void prepareGame(bool useBG) {
      useBackground = useBG;
      List<Sprite> backgrounds = new List<Sprite>();
      backgrounds.Add(backgroundWhite);
      if (useBG) {
        backgrounds.Add(backgroundRed);
      }
      // lever.gameObject.SetActive(true);
      // redButton.gameObject.SetActive(true);
      // redButton.onPress.AddListener(ConfirmPressed);
      extractRandomTarget();
      prepareCardRoller(backgrounds);
    }

    public void startGame() {
      targetStep = (int)((timer.timeLeft - 0.5f) / secondsForEachTarget);
      Roll();
    }

    private void prepareCardRoller(List<Sprite> backgrounds) {
      List<GameObject> cards = new List<GameObject>();
      cardRoller.gameObject.SetActive(true);
      foreach (Sprite background in backgrounds) {
        foreach (Sprite animal in animals) {
          GameObject newCard = new GameObject();
          newCard.transform.localScale = new Vector3(1.5f, 1f, 1f);
          newCard.name = animal.name + " (" + background.name + ")";
          newCard.AddComponent<SpriteRenderer>().sprite = background;
          GameObject overlay = new GameObject();
          overlay.name = animal.name;
          overlay.transform.parent = newCard.transform;
          overlay.transform.localPosition = new Vector3(0, 0, -0.1f);
          overlay.transform.localScale = new Vector3(0.3f, 0.45f, 0.45f);
          overlay.AddComponent<SpriteRenderer>().sprite = animal;
          cards.Add(newCard);
        }
      }
      cardRoller.createRoller(cards, 5.12f);
    }

    private void Roll() {
      void Rolled()
      {
        cardRoller.Rolled -= Rolled;
        _animalsSkipButtonCollider.enabled = true;
        _animalsConfirmButtonCollider.enabled = true;
      }
      
      selected = false;
      int currTargetStep = (int)((timer.timeLeft - 0.5f) / secondsForEachTarget);
      if (currTargetStep != targetStep) {
        targetStep = currTargetStep;
        extractRandomTarget();
      }
      if (Random.Range(0, 100) < rightProbability) 
      {
        cardRoller.startRotationToTarget(targetIndex);
      }
      else
      {
        if ((useBackground) && (Random.Range(0, 2) == 0))
        {
          cardRoller.startRotationToTarget(targetIndex + animals.Length);
        }
        else
        {
          cardRoller.startRotationAvoiding(targetIndex);
        }
      }

      cardRoller.Rolled += Rolled;
      enableTimeout();
      //lever.Pull();
    }

    public void SkipPressed() {
      if (!selected) {
        if (isRightImage()) {
          wrongAnswer();
        } else feedback.rightAnswerFeedback();
        selected = true;
        _animalsSkipButtonCollider.enabled = false;
        _animalsConfirmButtonCollider.enabled = false;
        next();
      }
    }

    public void ConfirmPressed() {
      if (!selected) {
        selected = true;
        if (isRightImage()) {
          rightAnswer();
        } else {
          wrongAnswer();
        }
        _animalsSkipButtonCollider.enabled = false;
        _animalsConfirmButtonCollider.enabled = false;
        next();
      }
    }

    private void rightAnswer() {
      feedback.rightAnswerFeedback();
      score.RightCounter++;
    }

    private void wrongAnswer() {
      feedback.wrongAnswerFeedback();
      score.WrongCounter++;
    }

    private void missedAnswer() {
      feedback.wrongAnswerFeedback();
      score.MissedCounter++;
    }

    public void extractRandomTarget() {
      targetIndex = Random.Range(0, animals.Length);
      
      var sequence = DOTween.Sequence(targetImage);

      var scaleDown = targetImage.transform.DOScale(Vector3.zero, .1f)
          .SetEase(Ease.OutCubic)
          .OnComplete(() => targetImage.sprite = animals[targetIndex]);
      var scaleUp = targetImage.transform.DOScale(Vector3.one, .15f)
        .SetEase(Ease.OutElastic);
      sequence.Append(scaleDown);
      sequence.Append(scaleUp);
    }

    private bool isRightImage() {
      return (cardRoller.getCurrentIndex() == targetIndex);
    }

    private void onTimeout() {
      if (!selected) {
        missedAnswer();
      }
      next();
    }

    private void next() {
      disableTimeout();
      if (timer.itIsEnd) {
        onGameOver.Invoke();
      } else
      {
        bomb.enabled = false;
        bomb.gameObject.SetActive(false);
        Roll();
      }
    }

  }
}
