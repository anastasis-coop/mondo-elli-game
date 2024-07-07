using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Room3
{
  public class DifferencesController : MonoBehaviour
  {

    public int rightProbability = 50;
    public FeedbackController feedback;

    [SerializeField] private Button _differencesSkipButtonCollider;
    [SerializeField] private Button _differencesConfirmButtonCollider;
    
    [SerializeField] private Transform transitionPanel;
    [SerializeField] private float transitionDurationInSeconds = 2f;
    [SerializeField] private Transform transitionPanelStartingPoint;
    [SerializeField] private Transform transitionPanelMiddlePoint;
    [SerializeField] private Transform transitionPanelEndingPoint;

    [Header("Left Ello accessories")] 
    [SerializeField] private List<GameObject> _leftElloGlasses;
    [SerializeField] private List<GameObject> _leftElloWristbands;
    [SerializeField] private List<GameObject> _leftElloClocks;
    [SerializeField] private List<GameObject> _leftElloLeftShoes;
    [SerializeField] private List<GameObject> _leftElloRightShoes;
    
    [Header("Right Ello accessories")] 
    [SerializeField] private List<GameObject> _rightElloGlasses;
    [SerializeField] private List<GameObject> _rightElloWristbands;
    [SerializeField] private List<GameObject> _rightElloClocks;
    [SerializeField] private List<GameObject> _rightElloLeftShoes;
    [SerializeField] private List<GameObject> _rightElloRightShoes;

    public GameObject viewLeft;
    public GameObject viewRight;

    public Timer timer;
    public Score score;
    public Bomb bomb;
    public UnityEvent onGameOver;

    private bool completelyDifferent;
    private bool selected;
    private float elapsed;
    private bool waiting;

    private GameObject currentElloGlasses;
    private GameObject currentElloClock;
    private GameObject currentElloWristBand;
    private GameObject currentElloLeftShoe;
    private GameObject currentElloRightShoe;
    
    private GameObject currentBigElloGlasses;
    private GameObject currentBigElloClock;
    private GameObject currentBigElloWristBand;
    private GameObject currentBigElloLeftShoe;
    private GameObject currentBigElloRightShoe;

    public void prepareGame(float timeout) {
      // lever.gameObject.SetActive(true);
      // redButton.gameObject.SetActive(true);
      // redButton.onPress.AddListener(ConfirmPressed);
      // prepareCardRollers(rollerLeft);
      // prepareCardRollers(rollerRight);
      bomb.timeoutSeconds = timeout;
      setBombMode(); 
    }

    private void setBombMode() {
      transform.localPosition = new Vector3(-250f, 0, 0);
      bomb.gameObject.SetActive(true);
      bomb.onExplode.AddListener(onExplode);
      // Vector3 buttonPos = redButton.transform.localPosition;
      // buttonPos.x -= 1.2f;
      // redButton.transform.localPosition = buttonPos;
      // Vector3 buttonRot = redButton.transform.localEulerAngles;
      // buttonRot.y = -17.5f;
      // redButton.transform.localEulerAngles = buttonRot;
      Vector3 bombPos = bomb.transform.localPosition;
      bombPos.x += 0.25f;
      bombPos.y -= 0.4f;
      bomb.transform.localPosition = bombPos;
    }

    public void Roll() {
      selected = false;
      completelyDifferent = (Random.Range(0, 100) < rightProbability);

      StartCoroutine(DifferenceTransition());
    }
    
    private IEnumerator DifferenceTransition()
    {
      
      float timer = 0; 
      
      while (timer < transitionDurationInSeconds/2)
      {
        transitionPanel.position = Vector3.Lerp(transitionPanel.position, transitionPanelMiddlePoint.position, timer / transitionDurationInSeconds/2);
        yield return null;
        timer += Time.deltaTime;
      }

      CalculateAndEnableAccessories(completelyDifferent);
      
      timer = 0; 
      while (timer < transitionDurationInSeconds/2)
      {
        transitionPanel.position = Vector3.Lerp(transitionPanel.position, transitionPanelEndingPoint.position, timer / transitionDurationInSeconds/2);
        yield return null;
        timer += Time.deltaTime;
      }

      _differencesSkipButtonCollider.interactable = true;
      _differencesConfirmButtonCollider.interactable = true;
      transitionPanel.position = transitionPanelStartingPoint.position;

      bomb.StartCountdown();
    }

    private void CalculateAndEnableAccessories(bool isCompletelyDifferent)
    {
      EnableDisableCurrentAccessories(false);

      if (isCompletelyDifferent)
      {
        CalculateCompletelyDifferentAccessories();
      }
      else
      {
        CalculateRandomAccessories();
      }

      EnableDisableCurrentAccessories(true);
    }

    private void CalculateCompletelyDifferentAccessories()
    {
      int elloGlassesIndex = Random.Range(0, _leftElloGlasses.Count);
      if (elloGlassesIndex < _leftElloGlasses.Count)
        currentElloGlasses = _leftElloGlasses[elloGlassesIndex];

      int elloClockIndex = Random.Range(0, _leftElloClocks.Count);
      if (elloClockIndex < _leftElloClocks.Count)
        currentElloClock = _leftElloClocks[elloClockIndex];

      int elloWristBandIndex = Random.Range(0, _leftElloWristbands.Count);
      if (elloWristBandIndex < _leftElloWristbands.Count)
        currentElloWristBand = _leftElloWristbands[elloWristBandIndex];

      int elloShoesIndex = Random.Range(0, _leftElloLeftShoes.Count);
      if (elloShoesIndex < _leftElloLeftShoes.Count)
        currentElloLeftShoe = _leftElloLeftShoes[elloShoesIndex];
      if (elloShoesIndex < _leftElloRightShoes.Count)
        currentElloRightShoe = _leftElloRightShoes[elloShoesIndex];

      List<GameObject> _bigElloGlassesCopy = new List<GameObject>(_rightElloGlasses);
      _bigElloGlassesCopy.RemoveAt(elloGlassesIndex);
      List<GameObject> _bigElloClocksCopy = new List<GameObject>(_rightElloClocks);
      _bigElloClocksCopy.RemoveAt(elloClockIndex);
      List<GameObject> _bigElloWristBandsCopy = new List<GameObject>(_rightElloWristbands);
      _bigElloWristBandsCopy.RemoveAt(elloWristBandIndex);

      List<GameObject> _bigElloLeftShoesCopy = new List<GameObject>(_rightElloLeftShoes);
      List<GameObject> _bigElloRightShoesCopy = new List<GameObject>(_rightElloRightShoes);
      _bigElloLeftShoesCopy.RemoveAt(elloShoesIndex);
      _bigElloRightShoesCopy.RemoveAt(elloShoesIndex);
      
      int bigElloGlassesIndex = Random.Range(0, _bigElloGlassesCopy.Count);
      if (bigElloGlassesIndex < _bigElloGlassesCopy.Count)
        currentBigElloGlasses = _bigElloGlassesCopy[bigElloGlassesIndex];

      int bigElloClockIndex = Random.Range(0, _bigElloClocksCopy.Count);
      if (bigElloClockIndex < _bigElloClocksCopy.Count)
        currentBigElloClock = _bigElloClocksCopy[bigElloClockIndex];

      int bigElloWristBandIndex = Random.Range(0, _bigElloWristBandsCopy.Count);
      if (bigElloWristBandIndex < _bigElloWristBandsCopy.Count)
        currentBigElloWristBand = _bigElloWristBandsCopy[bigElloWristBandIndex];

      int bigElloShoesIndex = Random.Range(0, _bigElloLeftShoesCopy.Count);
      if (bigElloShoesIndex < _bigElloLeftShoesCopy.Count)
        currentBigElloLeftShoe = _bigElloLeftShoesCopy[bigElloShoesIndex];
      if (bigElloShoesIndex < _bigElloRightShoesCopy.Count)
        currentBigElloRightShoe = _bigElloRightShoesCopy[bigElloShoesIndex];
    }
    
    private void CalculateRandomAccessories()
    {
      int elloGlassesIndex = Random.Range(0, _leftElloGlasses.Count);
      int elloClockIndex = Random.Range(0, _leftElloClocks.Count);
      int elloWristBandIndex = Random.Range(0, _leftElloWristbands.Count);
      int elloShoesIndex = Random.Range(0, _leftElloLeftShoes.Count);
      
      int bigElloGlassesIndex = Random.Range(0, _rightElloGlasses.Count);
      int bigElloClockIndex = Random.Range(0, _rightElloClocks.Count);
      int bigElloWristBandIndex = Random.Range(0, _rightElloWristbands.Count);
      int bigElloShoesIndex = Random.Range(0, _rightElloLeftShoes.Count);

      if (elloGlassesIndex != bigElloGlassesIndex
          && elloClockIndex != bigElloClockIndex
          && elloWristBandIndex != bigElloWristBandIndex
          && elloShoesIndex != bigElloShoesIndex
         )
      {
        int accessoryToChange = Random.Range(0, 4);
        switch (accessoryToChange)
        {
          case 0:
            bigElloGlassesIndex = elloGlassesIndex;
            break;
          case 1:
            bigElloClockIndex= elloClockIndex;
            break;
          case 2:
            bigElloWristBandIndex = elloWristBandIndex;
            break;
          case 3:
            bigElloShoesIndex = elloShoesIndex;
            break;
        }
      }

      if (elloGlassesIndex < _leftElloGlasses.Count)
        currentElloGlasses = _leftElloGlasses[elloGlassesIndex];

      if (elloClockIndex < _leftElloClocks.Count)
        currentElloClock = _leftElloClocks[elloClockIndex];

      if (elloWristBandIndex < _leftElloWristbands.Count)
        currentElloWristBand = _leftElloWristbands[elloWristBandIndex];

      if (elloShoesIndex < _leftElloLeftShoes.Count)
        currentElloLeftShoe = _leftElloLeftShoes[elloShoesIndex];
      if (elloShoesIndex < _leftElloRightShoes.Count)
        currentElloRightShoe = _leftElloRightShoes[elloShoesIndex];
      
      if (bigElloGlassesIndex < _rightElloGlasses.Count)
        currentBigElloGlasses = _rightElloGlasses[bigElloGlassesIndex];

      if (bigElloClockIndex < _rightElloClocks.Count)
        currentBigElloClock = _rightElloClocks[bigElloClockIndex];

      if (bigElloWristBandIndex < _rightElloWristbands.Count)
        currentBigElloWristBand = _rightElloWristbands[bigElloWristBandIndex];

      if (bigElloShoesIndex < _rightElloLeftShoes.Count)
        currentBigElloLeftShoe = _rightElloLeftShoes[bigElloShoesIndex];
      if (bigElloShoesIndex < _rightElloRightShoes.Count)
        currentBigElloRightShoe = _rightElloRightShoes[bigElloShoesIndex];
    }
    
    private void EnableDisableCurrentAccessories(bool toEnable)
    {
      if(currentElloGlasses != null)
        currentElloGlasses.SetActive(toEnable);
      if(currentElloClock != null)
        currentElloClock.SetActive(toEnable);
      if(currentElloWristBand != null)
        currentElloWristBand.SetActive(toEnable);
      if(currentElloLeftShoe != null) 
        currentElloLeftShoe.SetActive(toEnable);
      if(currentElloRightShoe != null)
        currentElloRightShoe.SetActive(toEnable);
      
      if(currentBigElloGlasses != null)
        currentBigElloGlasses.SetActive(toEnable);
      if(currentBigElloClock != null)
        currentBigElloClock.SetActive(toEnable);
      if(currentBigElloWristBand != null)
        currentBigElloWristBand.SetActive(toEnable);
      if(currentBigElloLeftShoe != null)
        currentBigElloLeftShoe.SetActive(toEnable);
      if(currentBigElloRightShoe != null)
        currentBigElloRightShoe.SetActive(toEnable);
    }

    public void SkipPressed()
    {
      if (selected || !enabled) return;
      selected = true;
      _differencesSkipButtonCollider.interactable = false;
      _differencesConfirmButtonCollider.interactable = false;
      if (completelyDifferent) {
        wrongAnswer();
      } else rightAnswer();
      next();
    }

    public void ConfirmPressed() {
      if (selected || !enabled) return;
      selected = true;
      if (completelyDifferent) {
        rightAnswer();
      } else {
        wrongAnswer();
      }
      _differencesSkipButtonCollider.interactable = false;
      _differencesConfirmButtonCollider.interactable = false;
      next();
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

    private void onExplode() {
      missedAnswer();
      viewLeft.SetActive(false);
      viewRight.SetActive(false);
      _differencesSkipButtonCollider.interactable = false;
      _differencesConfirmButtonCollider.interactable = false;
      Invoke(nameof(next), bomb.explosionSeconds);
    }

    private void next() {
      bomb.StopCountdown();
      if (timer.itIsEnd) {
        onGameOver.Invoke();
      } else {
        Roll();
      }
    }

  }
}
