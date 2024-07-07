using System;
using TMPro;
using UnityEngine;

namespace Room9
{
    public class TimerLabel : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI valueLabel;

        [SerializeField]
        private string timeFormat;

        public void SetValue(int seconds)
        {
            valueLabel.text = string.Format(timeFormat, TimeSpan.FromSeconds(seconds));
        }
    }
}