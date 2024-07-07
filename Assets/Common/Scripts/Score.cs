using System;
using UnityEngine;

public class Score : MonoBehaviour
{
    [SerializeField]
    private int comboToPoint = 5;

    [SerializeField]
    private float secondsForCombo = 5;

    private int _rightCounter;
    private float _lastRightTime;

    public int RightCounter
    {
        get => _rightCounter;
        set
        {
            _rightCounter = value;

            RightAnswer?.Invoke(value);
            TotalScoreChanged?.Invoke(TotalScore);

            if (Time.time - _lastRightTime <= secondsForCombo)
                IncrementCombo();

            _lastRightTime = Time.time;
        }
    }

    public event Action<int> RightAnswer;

    private int _currentCombo;

    private void IncrementCombo()
    {
        _currentCombo++;

        if (_currentCombo >= comboToPoint)
        {
            _currentCombo = 0;
            ComboCounter++;
        }
    }

    private int _comboCounter;

    public int ComboCounter
    {
        get => _comboCounter;
        private set
        {
            _comboCounter = value;
            Combo?.Invoke(value);
            TotalScoreChanged?.Invoke(TotalScore);
        }
    }

    public event Action<int> Combo;

    private int _wrongCounter;

    public int WrongCounter
    {
        get => _wrongCounter;
        set
        {
            _currentCombo = 0;
            _wrongCounter = value;
            WrongAnswer?.Invoke(value);
        }
    }

    public event Action<int> WrongAnswer;

    private int _missedCounter;

    public int MissedCounter
    {
        get => _missedCounter;
        set
        {
            _currentCombo = 0;
            _missedCounter = value;
            MissedAnswer?.Invoke(value);
        }
    }

    public event Action<int> MissedAnswer;

    public int TotalScore => RightCounter + ComboCounter;

    public event Action<int> TotalScoreChanged;

    public int TotalAnswers => RightCounter + WrongCounter + MissedCounter;

    public float RightPercentage => RightCounter / (float)TotalAnswers;

    public void Reset()
    {
        _comboCounter = 0;
        _currentCombo = 0;
        _lastRightTime = 0;
        _missedCounter = 0;
        _rightCounter = 0;
        _wrongCounter = 0;

        WasReset?.Invoke();
    }

    public event Action WasReset;
}

