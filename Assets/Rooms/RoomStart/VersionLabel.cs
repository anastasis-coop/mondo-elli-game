using UnityEngine;

public class VersionLabel : MonoBehaviour
{
    [SerializeField]
    private TMPro.TextMeshProUGUI label;

    private void Start()
    {
        label.text = $"Il Mondo degli Elli   v{Application.version}";
    }
}
