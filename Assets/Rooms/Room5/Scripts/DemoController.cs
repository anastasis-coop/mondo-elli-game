using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Room5
{
  public class DemoController : MonoBehaviour
  {
    public UnityEvent randomGearsSelected = new UnityEvent();
    public GameObject Conveyor;
    public GameObject Arrow1, Arrow2;
    public GameObject mainCamera;
    public AudioSource clickSound;
    private RaycastHit hit;
    private int numOfCorrectGearAnswer;
    private bool IfTwoRandomGearInit;
    private Vector3 arrow1Pos, arrow2Pos;
    public List<string> gearSelected;

    public void StartDemo() {
      ///Summary
      ///Refresh the gearSelected list
      ///Set the message (text and voice)
      ///Change the position of camera
      ///Make comic and conveyor observable
      ///set the Number of counterclockwise gears and the speed

      gearSelected.Clear();
      mainCamera.transform.position = new Vector3(0, 1010.23f, -55f);
      Conveyor.SetActive(true);
    }

    private void Update() {
      //Then, user can select two defined gears.
      if (IfTwoRandomGearInit) {
        if (Input.GetMouseButtonDown(0)) {
          if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100)) {
            //When two gears are selected correctly and they are not the same, makes things hidden and change the position of camera.
            if (numOfCorrectGearAnswer == 2) {
              Conveyor.SetActive(false);
              Arrow1.SetActive(false);
              Arrow2.SetActive(false);
              mainCamera.transform.position = new Vector3(0, 11.5f, -55f);
              randomGearsSelected.Invoke();
            }
          }
        }
      }
    }

    Vector3 CalArrowPos(GameObject arrow) {
      return new Vector3(arrow.transform.position.x, arrow.transform.position.y - 3.5f, arrow.transform.position.z);
    }

  }
}
