using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Room9
{
    public class SnippetsAssign : MonoBehaviour
    {
        [SerializeField]
        private SnippetsAssignEntry entryPrefab;

        [SerializeField]
        private Transform entriesRoot;
       
        [SerializeField]
        private TextMeshProUGUI snippetsLabel;

        [SerializeField]
        private Button continueButton;

        [SerializeField]
        private int snippetsToAssign = 20;

        private MediaLiteracyExercise.Text[] _texts;
        private List<SnippetsAssignEntry> _entries = new();
        private Action<int[]> _callback;

        public void Show(MediaLiteracyExercise.Text[] texts, Action<int[]> callback)
        {
            _texts = texts;
            _callback = callback;

            continueButton.interactable = false;

            for (int i = 0; i < texts.Length; i++)
            {
                MediaLiteracyExercise.Text text = texts[i];

                if (text.Relevance == 0) continue;

                var entry = Instantiate(entryPrefab, entriesRoot);

                Vector2Int range = new(0, text.Snippets.Length);

                entry.Init(i, text.TitleAudio, text.PreviewAudio, text.AuthorAudio, range, OnEntryValueChanged);

                _entries.Add(entry);
            }

            snippetsLabel.text = snippetsToAssign.ToString();

            gameObject.SetActive(true);
        }

        private void OnEntryValueChanged()
        {
            int snippetsLeft = snippetsToAssign;

            foreach (var entry in _entries)
            {
                snippetsLeft -= entry.Value;
            }

            snippetsLabel.text = snippetsLeft.ToString();

            continueButton.interactable = snippetsLeft == 0;
        }

        public void OnContinuePressed()
        {
            int[] result = new int[_texts.Length];

            foreach (SnippetsAssignEntry entry in _entries)
            {
                result[entry.TextIndex] = entry.Value;
            }

            _callback?.Invoke(result);
        }

        public void Hide()
        {
            foreach (SnippetsAssignEntry entry in _entries)
            {
                Destroy(entry.gameObject);
            }

            _entries.Clear();

            _callback = null;

            continueButton.interactable = false;

            gameObject.SetActive(false);
        }
    }
}