using System.Timers;
using UnityEngine;
using UnityEngine.SceneManagement;
using SystemTimer = System.Timers.Timer;

public class DemoDemon : MonoBehaviour
{
    [SerializeField, Min(0)]
    private int _demoMinutes;

    [SerializeField]
    private string _demoEndScene;

    private double _endSeconds;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        _endSeconds = Time.unscaledTimeAsDouble + (_demoMinutes * 60);
    }

    private void Update()
    {
        if (Time.unscaledTimeAsDouble < _endSeconds) return;

        SceneManager.LoadScene(_demoEndScene);
        Destroy(gameObject);
    }
}