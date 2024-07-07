using System;
using System.Collections.Generic;
using UnityEngine;

namespace Room9
{
    public class Solution : MonoBehaviour
    {
        [SerializeField]
        private SolutionEntry entryPrefab;

        [SerializeField]
        private Transform entriesRoot;

        private Action<int[]> _callback;
        private List<SolutionEntry> _entries = new();
        private int[] _points;

        public void Show(ReadableString[] texts, int[] answers, int[] solutions, Action<int[]> callback)
        {
            _callback = callback;

            _points = new int[texts.Length];

            for (int i = 0; i < texts.Length; i++)
            {
                var text = texts[i];
                int answer = answers[i];
                int solution = solutions[i];

                var entry = Instantiate(entryPrefab, entriesRoot);
                entry.Init(text, answer, solution);

                _entries.Add(entry);

                _points[i] = answer == solution ? 2 : (solution == 0 || answer == 0) ? 1 : 0;
            }

            gameObject.SetActive(true);
        }

        public void OnContinueButtonPressed()
        {
            _callback?.Invoke(_points);
        }

        public void Hide()
        {
            foreach (SolutionEntry entry in _entries)
            {
                Destroy(entry.gameObject);
            }

            _entries.Clear();

            gameObject.SetActive(false);
        }
    }
}