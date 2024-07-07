using System.Collections.Generic;
using UnityEngine;

namespace Room5
{
  public class CreateConveyor : MonoBehaviour
  {
    public LevelController levelController;
    private float radius = 0.37f;
    public GameObject gear1, gear2;
    public GameObject module;
    public List<Material> material;
    private GameObject pieces;
    private List<GameObject> items;
    public List<GameObject> gears, totalGears;
    public List<GameObject> tempGears;
    public List<GameObject> randomSelectedGears;
    private float step;
    private int horizSteps;
    private float halfCircle;
    private float distance; // distance between gears 
    private float checkPoint1;
    private float checkPoint2;
    private float checkPoint3;
    private float startOffset; // current offset
    private float totalLength; // total length of the conveyor
    public float speed;
    public int numOfRequestedGears;
    public float GearNumSet = 1;
    private float ConveyerPosPlus = 0;
    private float GearScaleset = 1;
    public int numOfCtrClockGears;
    private void Start()
    {
      //Get initial values for these variables from level controller base on level
      //Variables of SetGearNum, PlusConveyor, muLtipleGearScale will be set for changing the number of gears, getting the top and buttom surface of conveyor wider, and changing the size of each gear
      // GearNumSet = levelController.SetGearNum;
      // ConveyerPosPlus = levelController.PlusConveyor;
      // GearScaleset = levelController.muLtipleGearScale;

      halfCircle = radius * Mathf.PI;
      step = halfCircle / 12;
      horizSteps = Mathf.RoundToInt((gear2.transform.localPosition.x - gear1.transform.localPosition.x) / step);
      if (horizSteps % 2 != 0)
      {
        horizSteps++;
      }
      distance = horizSteps * step;
      checkPoint1 = halfCircle;
      checkPoint2 = checkPoint1 + distance;
      checkPoint3 = checkPoint2 + halfCircle;

      totalLength = 2 * (distance + halfCircle);

      CreateGears();
      CreatePiecesParent();
      CreateAllPieces();
      UpdatePieces();
    }
    private void CreateGears()
    {
      //Give gears size and scale
      Vector3 gearPos = gear1.transform.localPosition;
      Vector3 gearScale = new Vector3(gear1.transform.localScale.x * GearScaleset, gear1.transform.localScale.y * GearScaleset, gear1.transform.localScale.z);
      gear1.transform.localScale = gearScale;

      //Make gear list an a copy of that
      gears.Add(gear1);
      tempGears.Add(gear1);
      int extraGears = Mathf.RoundToInt(distance / (2 * GearNumSet));
      for (int i = 0; i < extraGears; i++)
      {
        GameObject newGear = Instantiate(gear1);
        newGear.transform.parent = transform;
        newGear.name = "extragear" + (i + 1);
        newGear.AddComponent<BoxCollider>();
        Vector3 pos = gearPos;
        pos.x += (i + 1) * distance / (extraGears + 1);
        newGear.transform.localPosition = pos;
        newGear.transform.localEulerAngles = Vector3.zero;
        gears.Add(newGear);
        tempGears.Add(newGear);
      }
      gearPos.x += distance;
      gear2.transform.localPosition = gearPos;
      gear2.transform.localScale = gearScale;
      gears.Add(gear2);
      tempGears.Add(gear2);
      totalGears = new List<GameObject>(gears);
    }
    private void CreateAllPieces()
    {
      items = new List<GameObject>();
      int numPieces = (12 + horizSteps) * 2;
      for (int i = 0; i < numPieces; i++)
      {
        GameObject piece = Instantiate(module);
        piece.transform.parent = pieces.transform;
        piece.GetComponent<MeshRenderer>().material = material[items.Count % 4];
        items.Add(piece);
        piece.name = "piece" + items.Count;
      }
    }
    private void CreatePiecesParent()
    {
      pieces = new GameObject();
      pieces.name = "pieces";
      pieces.transform.parent = transform;
      pieces.transform.position = transform.position;
      pieces.transform.Translate(new Vector3(0, 0, -2));
      pieces.transform.eulerAngles = transform.eulerAngles;
    }
    private float advanceOffset(float offset, float delta)
    {
      offset += delta;
      if (offset < 0)
      {
        offset += totalLength;
      }
      else if (offset >= totalLength)
      {
        offset -= totalLength;
      }
      return offset;
    }
    private void UpdatePieces()
    {
      float current = startOffset;
      UpdatePiece(items[0], current);
      foreach (GameObject item in items)
      {
        UpdatePiece(item, current);
        current = advanceOffset(current, step);
      }
    }
    private void UpdatePiece(GameObject piece, float pieceOffset)
    {
      Vector3 pos = Vector3.zero;
      Vector3 euler = Vector3.zero;

      if (pieceOffset < checkPoint1)
      {
        float angle = pieceOffset / radius;
        pos.x = gear1.transform.localPosition.x - Mathf.Sin(angle) * radius;
        pos.y = gear1.transform.localPosition.y + Mathf.Cos(angle) * radius;
        euler.z = angle * Mathf.Rad2Deg;
      }
      else if (pieceOffset < checkPoint2)
      {
        pos.x = gear1.transform.localPosition.x + pieceOffset - checkPoint1;
        pos.y = gear1.transform.localPosition.y - ConveyerPosPlus - radius;
        euler.z = 180;
      }
      else if (pieceOffset < checkPoint3)
      {
        float angle = (pieceOffset - checkPoint2) / radius + Mathf.PI;
        pos.x = gear2.transform.localPosition.x - Mathf.Sin(angle) * radius;
        pos.y = gear2.transform.localPosition.y + Mathf.Cos(angle) * radius;
        euler.z = angle * Mathf.Rad2Deg;
      }
      else
      {
        pos.x = gear2.transform.localPosition.x - (pieceOffset - checkPoint3);
        pos.y = gear2.transform.localPosition.y + ConveyerPosPlus + radius;
      }
      piece.transform.localPosition = pos;
      piece.transform.localEulerAngles = euler;
    }
    private void Update()
    {
      float delta = -speed * Time.deltaTime;
      startOffset = advanceOffset(startOffset, delta);
      delta = delta * Mathf.Rad2Deg / radius;

      //Select which gears turn clockwise and which contour clockwise
      if (numOfCtrClockGears < numOfRequestedGears)
      {
        CreateTheGearsSelection();
        numOfCtrClockGears++;
      }
      // Control trun clockwise and counterclockwise
      // foreach (GameObject ctrClockWiseElem in randomSelectedGears)
      // ctrClockWiseElem.transform.Rotate(new Vector3(0, 0, -delta));
      foreach (GameObject clockWiseElem in gears)
        clockWiseElem.transform.Rotate(new Vector3(0, 0, delta));
    }
    private void CreateTheGearsSelection()
    {
      int randomIndex = Random.Range(0, gears.Count);
      randomSelectedGears.Add(gears[randomIndex]);
      gears.Remove(gears[randomIndex]);
    }
  }
}
