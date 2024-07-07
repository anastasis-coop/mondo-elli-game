using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Room9
{
    public class MemoryTitles : MonoBehaviour
    {
        [SerializeField]
        private InhibitionMemoryTitleEntry titleEntryPrefab;

        [SerializeField]
        private Transform titleEntriesRoot;

        [SerializeField]
        private TextMeshProUGUI yesNoPopupLabel;

        [SerializeField]
        private ReadableLabel yesNoPopupReadable;

        [SerializeField]
        private GameObject yesNoPopupPanel;

        [SerializeField]
        private GameObject continueButtonRoot;

        [SerializeField]
        private TextMeshProUGUI progressAmountLabel;

        private ReadableString[] _titles;

        private Action<Dictionary<int, bool>> _callback;
        private Action _solutionCallback;

        private InhibitionMemoryTitleEntry _selectedEntry;
        private Dictionary<int, bool> _answers = new();
        private List<InhibitionMemoryTitleEntry> _titleEntries = new();

        public void Show(ReadableString[] titles, Action<Dictionary<int, bool>> callback)
        {
            _titles = titles;
            _callback = callback;

            for (int i = 0; i < titles.Length; i++)
            {
                var title = titles[i];

                var titleEntry = Instantiate(titleEntryPrefab, titleEntriesRoot);
                titleEntry.Init(i, title, OnTitleEntrySelect);

                _titleEntries.Add(titleEntry);
            }

            progressAmountLabel.text = $"0/{_titles.Length}";

            gameObject.SetActive(true);
        }

        private void OnTitleEntrySelect(InhibitionMemoryTitleEntry entry)
        {
            _selectedEntry = entry;

            yesNoPopupLabel.text = _titles[entry.TitleIndex]?.String;
            yesNoPopupReadable.AudioClip = _titles[entry.TitleIndex]?.AudioClip;
            yesNoPopupPanel.SetActive(true);
        }

        public void OnTitleEntryRelevance(bool answer)
        {
            yesNoPopupPanel.SetActive(false);
            _answers.Add(_selectedEntry.TitleIndex, answer);

            _selectedEntry.gameObject.SetActive(false);

            progressAmountLabel.text = $"{_answers.Count}/{_titles.Length}";

            if (_answers.Count == _titleEntries.Count)
                OnGameCompleted();
        }

        public void OnTimerExpired() => OnGameCompleted();

        private void OnGameCompleted()
        {
            _callback?.Invoke(_answers);
        }

        public void ShowSolution(Dictionary<int, bool> correct, Action solutionCallback)
        {
            _solutionCallback = solutionCallback;

            for (int i = 0; i < _titleEntries.Count; i++)
            {
                InhibitionMemoryTitleEntry entry = _titleEntries[i];

                if (correct.ContainsKey(i))
                {
                    entry.ShowSolution(correct[i]);
                }

                entry.gameObject.SetActive(true);
            }

            continueButtonRoot.SetActive(true);
        }

        public void OnContinueButtonPressed() => _solutionCallback?.Invoke();

        public void Hide()
        {
            continueButtonRoot.SetActive(false);

            _callback = null;
            _solutionCallback = null;

            foreach (var entry in _titleEntries)
            {
                Destroy(entry.gameObject);
            }

            _titleEntries.Clear();

            gameObject.SetActive(false);
        }
    }
}
