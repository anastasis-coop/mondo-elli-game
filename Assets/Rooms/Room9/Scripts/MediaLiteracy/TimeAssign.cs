using System;
using UnityEngine;

namespace Room9
{
    public class TimeAssign : MonoBehaviour
    {
        [SerializeField]
        private Timer timer;

        [SerializeField]
        private TimeEntry snippetEntry;

        [SerializeField]
        private Vector2Int snippetMinutesRange;

        [SerializeField]
        private TimeEntry productionEntry;

        [SerializeField]
        private Vector2Int productionMinutesRange;

        [SerializeField]
        private TimeEntry revisionEntry;

        [SerializeField]
        private Vector2Int revisionMinutesRange;

        public delegate void TimeAssignCallback(int snippetMinutes, int productionMinutes, int revisionMinutes);

        private TimeAssignCallback _callback;

        public void Show(TimeAssignCallback callback)
        {
            _callback = callback;

            snippetEntry.Init(snippetMinutesRange, snippetMinutesRange.x, OnEntryValueChanged);
            productionEntry.Init(productionMinutesRange, productionMinutesRange.x, OnEntryValueChanged);
            revisionEntry.Init(revisionMinutesRange, revisionMinutesRange.x, OnEntryValueChanged);

            UpdateTimerLabel();

            gameObject.SetActive(true);
        }

        private void OnEntryValueChanged() => UpdateTimerLabel();
        
        private void UpdateTimerLabel()
        {
            int minutes = snippetEntry.Value + productionEntry.Value + revisionEntry.Value;

            timer.SetTime(minutes * 60);
        }

        public void OnContinuePressed()
        {
            int snippetMinutes = snippetEntry.Value;
            int productionMinutes = productionEntry.Value;
            int revisionMinutes = revisionEntry.Value;

            _callback?.Invoke(snippetMinutes, productionMinutes, revisionMinutes);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}