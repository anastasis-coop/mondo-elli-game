using System.Collections;
using UnityEngine;

namespace Room5
{
  public class LightOffAfterDelay : MonoBehaviour
  {
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");    
    
    private Material[] _objMatElements;

    private MeshRenderer _renderer;
    
    // Start is called before the first frame update
    private void Awake()
    {
      _renderer = GetComponentInChildren<MeshRenderer>();
      _objMatElements = _renderer.materials;

      foreach (var mat in _objMatElements)
      {
        mat.SetColor(EmissionColorId, mat.color * 100);
      }

      _renderer.materials = _objMatElements;
      
      StartCoroutine(LightOff());
    }
    IEnumerator LightOff()
    {
      //Create 3s lighting Up
      yield return new WaitForSeconds(3);

      foreach (var mat in _objMatElements)
      {
        mat.SetColor(EmissionColorId, Color.black);
      }
      _renderer.materials = _objMatElements;
    }
  }
}
