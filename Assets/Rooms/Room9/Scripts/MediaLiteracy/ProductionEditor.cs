using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Room9
{
    public class ProductionEditor : MonoBehaviour
    {
        [SerializeField]
        private MessageSystem bigEllo;

        [SerializeField]
        private ProductionEditorEntry entryPrefab;

        [SerializeField]
        private Transform addSnippetButton;

        [SerializeField]
        private Transform editorRoot;

        [SerializeField]
        private ScrollRect editorScrollRect;

        [SerializeField]
        private GameObject notebookPanelRoot;

        [SerializeField]
        private Transform notebookEntriesRoot;

        [SerializeField]
        private NotebookEntry notebookEntryPrefab;

        [SerializeField]
        private GameObject cardSelectPanel;

        [SerializeField]
        private Transform cardEntriesRoot;

        [SerializeField]
        private ProductionEditorCard cardEntryPrefab;

        [Serializable]
        public class CardConfig
        {
            public int Id;
            public ReadableString TextAudio;
            public Color Color;
        }

        [SerializeField]
        private CardConfig[] cardConfigs;

        [SerializeField]
        private GameObject reviewPanel;

        [SerializeField]
        private TextMeshProUGUI reviewLabel;

        [SerializeField]
        private ReadableLabel reviewReadable;

        [SerializeField]
        private BigElloSaysConfig interferenceTip;

        [SerializeField]
        private BigElloSaysConfig memoryTip;

        [SerializeField]
        private BigElloSaysConfig flexibilityTip;

        [SerializeField]
        private GameObject yesNoRoot;

        [SerializeField]
        private TextMeshProUGUI snippetsLabel;

        private MediaLiteracyExercise.Text[] _texts;
        private Action<string, List<AudioClip>> _callback;

        private List<ProductionEditorEntry> _editorEntries = new();
        private List<NotebookEntry> _notebookEntries = new();
        private List<ProductionEditorCard> _cardSelectEntries = new();

        private ProductionEditorEntry _selectedEntry = null;

        public void Show(MediaLiteracyExercise.Text[] texts, List<int>[] choices, Action<string, List<AudioClip>> callback)
        {
            _texts = texts;
            _callback = callback;

            for (int textIndex = 0; textIndex < choices.Length; textIndex++)
            {
                foreach (int snippetIndex in choices[textIndex])
                {
                    var snippet = texts[textIndex].Snippets[snippetIndex];
                    var entry = Instantiate(notebookEntryPrefab, notebookEntriesRoot);
                    entry.Init(textIndex, snippetIndex, snippet.TextAudio, OnNotebookEntryPressed);
                    _notebookEntries.Add(entry);
                }
            }

            foreach (CardConfig config in cardConfigs)
            {
                var entry = Instantiate(cardEntryPrefab, cardEntriesRoot);
                entry.Init(OnCardSelectButtonPressed);
                entry.CardConfig = config;

                _cardSelectEntries.Add(entry);
            }

            snippetsLabel.text = "0";
            gameObject.SetActive(true);
        }

        public void OnAddSnippetButtonPressed()
        {
            _selectedEntry = null;

            notebookPanelRoot.SetActive(true);
        }

        public void OnEntryReplacePressed(ProductionEditorEntry entry)
        {
            _selectedEntry = entry;

            notebookPanelRoot.SetActive(true);
        }

        public void OnNotebookEntryPressed(NotebookEntry notebookEntry)
        {
            var snippet = _texts[notebookEntry.TextIndex].Snippets[notebookEntry.SnippetIndex];

            ProductionEditorEntry editorEntry;

            if (_selectedEntry != null)
            {
                editorEntry = _selectedEntry;

                // HACK use a better data structure
                var oldNotebookEntry = _notebookEntries.Find(entry => entry.TextIndex == editorEntry.TextIndex
                    && entry.SnippetIndex == editorEntry.SnippetIndex);

                oldNotebookEntry.Selected = false;
                oldNotebookEntry.Interactable = true;

                _selectedEntry = null;
            }
            else
            {
                editorEntry = Instantiate(entryPrefab, editorRoot);
                _editorEntries.Add(editorEntry);
                snippetsLabel.text = _editorEntries.Count.ToString();

                DoNextFrame(() => editorScrollRect.verticalNormalizedPosition = 0);
            }

            editorEntry.Init(notebookEntry.TextIndex, notebookEntry.SnippetIndex, snippet.TextAudio);
            editorEntry.ReplacePressed += OnEntryReplacePressed;
            editorEntry.RemovePressed += OnEntryRemovePressed;
            editorEntry.MovePressed += OnEntryMovePressed;
            editorEntry.SetCardPressed += OnEntrySetCardPressed;
            editorEntry.ClearCardPressed += OnEntryClearCardPressed;

            addSnippetButton.SetAsLastSibling();

            notebookEntry.Selected = true;
            notebookEntry.Interactable = false;

            notebookPanelRoot.SetActive(false);
        }

        public void OnEntryRemovePressed(ProductionEditorEntry entry)
        {
            // HACK use a better data structure
            var notebookEntry = _notebookEntries.Find(e => entry.TextIndex == e.TextIndex
                && entry.SnippetIndex == e.SnippetIndex);

            notebookEntry.Selected = false;
            notebookEntry.Interactable = true;

            _editorEntries.Remove(entry);
            snippetsLabel.text = _editorEntries.Count.ToString();

            entry.ReplacePressed -= OnEntryReplacePressed;
            entry.RemovePressed -= OnEntryRemovePressed;
            entry.MovePressed -= OnEntryMovePressed;
            entry.SetCardPressed -= OnEntrySetCardPressed;
            entry.ClearCardPressed -= OnEntryClearCardPressed;

            Destroy(entry.gameObject);
        }

        public void OnEntryMovePressed(ProductionEditorEntry entry, bool up)
        {
            int index = _editorEntries.IndexOf(entry);

            index += up ? -1 : 1;

            if (index < 0 || index > _editorEntries.Count - 1) return;

            _editorEntries.Remove(entry);
            _editorEntries.Insert(index, entry);

            entry.transform.SetSiblingIndex(index);
        }

        public void OnEntrySetCardPressed(ProductionEditorEntry entry)
        {
            _selectedEntry = entry;
            cardSelectPanel.SetActive(true);
        }

        public void OnCardSelectButtonPressed(ProductionEditorCard card)
        {
            _selectedEntry.SetCard(card.CardConfig);
            cardSelectPanel.SetActive(false);
        }

        public void OnEntryClearCardPressed(ProductionEditorEntry entry)
        {
            entry.ClearCard();
        }

        public void OnReviewButtonPressed()
        {
            (string text, List<AudioClip> clips) = GetEssay();

            reviewLabel.text = text;
            reviewReadable.AudioClips = clips;
            reviewPanel.SetActive(true);
        }

        public void OnInterferenceButtonPressed()
        {
            bigEllo.ShowMessage(interferenceTip);
        }

        public void OnMemoryButtonPressed()
        {
            bigEllo.ShowMessage(memoryTip);
        }

        public void OnFlexibilityButtonPressed()
        {
            bigEllo.ShowMessage(flexibilityTip);
        }

        public void OnSubmitButtonPressed()
        {
            yesNoRoot.SetActive(true);
        }

        public void OnConfirmSubmissionPressed()
        {
            yesNoRoot.SetActive(false);

            (string text, List<AudioClip> audios) = GetEssay();

            _callback?.Invoke(text, audios);
        }

        private (string, List<AudioClip>) GetEssay()
        {
            StringBuilder builder = new();

            List<AudioClip> audios = new();

            foreach (ProductionEditorEntry entry in _editorEntries)
            {
                var snippet = _texts[entry.TextIndex].Snippets[entry.SnippetIndex];
                string snippetText = snippet.TextAudio.String;

                //HACK snippets shouldn't start with spaces
                snippetText = snippetText.TrimStart().TrimEnd();

                if (entry.HasCard)
                {
                    builder.Append("<b>" + entry.CardConfig.TextAudio?.String + "</b> ");
                    audios.Add(entry.CardConfig.TextAudio?.AudioClip);

                    snippetText = char.ToLower(snippetText[0]) + snippetText[1..];
                }
                else
                {
                    builder.AppendLine();
                    builder.AppendLine();
                }

                builder.Append(snippetText + " ");
                audios.Add(snippet.TextAudio?.AudioClip);
            }

            return (builder.ToString(), audios);
        }

        public void Hide()
        {
            DestroyAndClearList(_editorEntries);
            DestroyAndClearList(_notebookEntries);
            DestroyAndClearList(_cardSelectEntries);

            _callback = null;
            gameObject.SetActive(false);
        }

        private void DestroyAndClearList<T>(List<T> list) where T : MonoBehaviour
        {
            foreach (T entry in list) Destroy(entry.gameObject);

            list.Clear();
        }

        private void DoNextFrame(Action action)
        {
            StartCoroutine(DoNextFrameRoutine(action));
        }

        private IEnumerator DoNextFrameRoutine(Action action)
        {
            yield return null;

            action?.Invoke();
        }
    }
}