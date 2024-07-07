using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndDemoBehaviour : MonoBehaviour
{
    public void OnChiamaciButtonPressed()
    {
        Application.OpenURL("https://www.example.com/");
    }

    public void OnLinkPressed()
    {
        Application.OpenURL("https://www.example.com/");
    }
}
