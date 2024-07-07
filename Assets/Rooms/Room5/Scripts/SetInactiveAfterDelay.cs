using System.Collections;
using UnityEngine;

namespace Room5
{
  public class SetInactiveAfterDelay : MonoBehaviour
  {
    private void Start()
    {
      StartCoroutine(LightOff());
    }
    IEnumerator LightOff()
    {
      //Create 2s showing up
      yield return new WaitForSeconds(1f);
      gameObject.SetActive(false);
    }
  }
}
