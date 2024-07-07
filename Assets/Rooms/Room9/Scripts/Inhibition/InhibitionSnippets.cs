using System;
using System.Collections.Generic;
using UnityEngine;

namespace Room9
{
    public class InhibitionSnippets : MonoBehaviour
    {
        [SerializeField]
        private NotebookEntry notebookEntryPrefab;

        [SerializeField]
        private Transform notebookEntriesRoot;

        private Action<HashSet<int>> _callback;

        private bool _pickMultiple;

        private List<NotebookEntry> _notebookEntries = new();
        private HashSet<int> _selectedSnippets = new();

        public void Show(ReadableString[] snippets, bool pickMultiple, Action<HashSet<int>> callback)
        {
            _pickMultiple = pickMultiple;

            _callback = callback;

            ClearNotebook();

            for (int s = 0; s < snippets.Length; s++)
            {
                var entry = Instantiate(notebookEntryPrefab, notebookEntriesRoot);
                entry.Init(0, s, snippets[s], OnNotebookEntryPressed);

                _notebookEntries.Add(entry);
            }

            gameObject.SetActive(true);
        }

        private void OnNotebookEntryPressed(NotebookEntry entry)
        {
            bool selected = _selectedSnippets.Contains(entry.SnippetIndex);

            if (selected)
            {
                entry.Selected = false;
                _selectedSnippets.Remove(entry.SnippetIndex);
            }
            else
            {
                if (!_pickMultiple)
                {
                    foreach (var selectedEntry in _notebookEntries)
                    {
                        selectedEntry.Selected = false;
                    }

                    _selectedSnippets.Clear();
                }

                entry.Selected = true;
                _selectedSnippets.Add(entry.SnippetIndex);
            }
        }

        public void OnContinueButtonPressed()
        {
            _callback?.Invoke(_selectedSnippets);
        }

        private void ClearNotebook()
        {
            foreach (var entry in _notebookEntries)
                Destroy(entry.gameObject);

            _notebookEntries.Clear();
            _selectedSnippets.Clear();
        }

        public void Hide()
        {
            ClearNotebook();

            gameObject.SetActive(false);
        }
    }
}