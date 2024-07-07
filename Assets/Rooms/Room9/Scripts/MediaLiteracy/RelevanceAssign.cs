using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Room9
{
    public class RelevanceAssign : MonoBehaviour
    {
        [SerializeField]
        private RelevanceEntry entryPrefab;

        [SerializeField]
        private Transform entriesRoot;

        [SerializeField]
        private DragDrop dragDrop;

        [SerializeField]
        private Button continueButton;

        private List<RelevanceEntry> _entries = new();
        private Action<int[]> _callback;

        public void Show(MediaLiteracyExercise.Text[] texts, Action<int[]> callback)
        {
            dragDrop.Changed += OnDragDropChanged;

            _callback = callback;

            continueButton.interactable = false;

            var shuffled = new int[texts.Length];

            for (int i = 0; i < texts.Length; i++) shuffled[i] = i;

            Shuffle(shuffled);

            foreach (int textIndex in shuffled)
            {
                var text = texts[textIndex];
                var entry = Instantiate(entryPrefab, entriesRoot);
                entry.Init(textIndex, text.TitleAudio, text.PreviewAudio, text.AuthorAudio);
                
                _entries.Add(entry);

                dragDrop.AddSlot(entry.Slot);
            }

            gameObject.SetActive(true);
        }

        private static void Shuffle<T>(IList<T> list)
        {
            var rng = new System.Random();

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[n], list[k]) = (list[k], list[n]);
            }
        }

        private void OnDragDropChanged()
        {
            bool allAssigned = true;

            foreach (RelevanceEntry entry in _entries)
            {
                if (entry.Assigned) continue;

                allAssigned = false;
                break;
            }

            continueButton.interactable = allAssigned;
        }

        public void OnContinueButtonPressed()
        {
            int[] answer = new int[_entries.Count];

            foreach (RelevanceEntry entry in _entries)
            {
                answer[entry.TextIndex] = entry.Relevance;
            }

            _callback?.Invoke(answer);
        }

        public void Hide()
        {
            foreach (RelevanceEntry entry in _entries)
            {
                Destroy(entry.gameObject);
            }

            _entries.Clear();

            _callback = null;

            dragDrop.Changed -= OnDragDropChanged;

            continueButton.interactable = false;

            gameObject.SetActive(false);
        }
    }
}