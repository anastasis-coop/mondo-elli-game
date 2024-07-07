using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Room3 {
  public class CardRoller : MonoBehaviour
  {

    public event Action Rolled;

    public float duration = 1f;

    private Transform rotationCenter;
    private bool moving;
    private int count;
    private float startAngle;
    private float targetAngle;
    private int targetIndex = -1;
    private float elapsed;

    // Update is called once per frame
    void Update()
    {
      if (moving)
      {
        elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsed / duration);
        float t = Mathf.Sin(progress * Mathf.PI / 2);
        rotationCenter.eulerAngles = new Vector3(Mathf.Lerp(startAngle, targetAngle, t), 0, 0);
        if (progress == 1)
        {
          while (targetAngle > 360)
          {
            targetAngle -= 360;
          }
          rotationCenter.eulerAngles = new Vector3(targetAngle, 0, 0);
          moving = false;
          Rolled?.Invoke();
        }
      }
    }

    public void createRoller(List<GameObject> cards, float cardHeight)
    {
      count = cards.Count;
      float radius = (cardHeight / 2f) / Mathf.Tan(Mathf.PI / cards.Count);
      GameObject center = new GameObject();
      center.name = "center";
      center.transform.parent = transform;
      center.transform.localPosition = new Vector3(0, 0, radius);
      rotationCenter = center.transform;
      Instantiate(center);
      List<Transform> cardObjects = new List<Transform>();
      for (int i = 0; i < cards.Count; i++)
      {
        GameObject newCard = cards[i];
        newCard.transform.parent = transform;
        newCard.transform.localPosition = Vector3.zero;
        newCard.transform.parent = rotationCenter;
        center.transform.eulerAngles = new Vector3(-i * 360 / cards.Count, 0, 0);
        newCard.transform.parent = transform;
        center.transform.eulerAngles = Vector3.zero;
        cardObjects.Add(newCard.transform);
      }
      foreach (Transform card in cardObjects)
      {
        card.parent = rotationCenter;
      }
    }

    public void startRotationToTarget(int index)
    {
      moving = true;
      elapsed = 0;
      startAngle = rotationCenter.eulerAngles.x;
      targetAngle = 360 + index * 360 / count;
      targetIndex = index;
    }
    public void startRotationAvoiding(int index)
    {
      if (count > 1)
      {
        int i;
        do
        {
          i = Random.Range(0, count);
        } while (i == index);
        startRotationToTarget(i);
      }
    }

    public int getCurrentIndex()
    {
      return moving ? -1 : targetIndex;
    }

  }
}
