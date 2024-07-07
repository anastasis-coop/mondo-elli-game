using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Room9
{
    public class ProductionPick : MonoBehaviour
    {
        [SerializeField]
        private TabButton tabPrefab;

        [SerializeField]
        private Transform tabsRoot;

        [SerializeField]
        private string tabFormat;

        [SerializeField]
        private TextMeshProUGUI titleLabel;

        [SerializeField]
        private ReadableLabel titleReadable;

        [SerializeField]
        private TextMeshProUGUI relevanceLabel;

        [SerializeField]
        private TextMeshProUGUI authorLabel;

        [SerializeField]
        private ReadableLabel authorReadable;

        [SerializeField]
        private Image relevanceImage;

        [SerializeField]
        private Sprite[] relevanceSprites;

        [SerializeField]
        private NotebookEntry entryPrefab;

        [SerializeField]
        private Transform entriesRoot;

        [SerializeField]
        private ScrollRect entriesScroll;

        [SerializeField]
        private Transform notebookRoot;

        [SerializeField]
        private ScrollRect notebookScroll;

        [SerializeField]
        private Button continueButton;

        [SerializeField]
        private TextMeshProUGUI totalSnippetsLabel;

        [SerializeField]
        private string totalSnippetsFormat;

        [SerializeField]
        private int totalSnippetsMin;

        [SerializeField]
        private Color greatEstimateColor = Color.green;

        [SerializeField]
        private Color goodEstimateColor = Color.yellow;

        [SerializeField]
        private Color badEstimateColor = Color.red;

        [SerializeField]
        private int goodEstimateDelta = 1;

        [SerializeField]
        private GameObject yesNoRoot;

        private MediaLiteracyExercise.Text[] _texts;
        private int[] _estimates;
        private Action<List<int>[]> _callback;

        private List<int>[] _picks;
        private int _totalSnippetsEstimate;
        private int _picksCount;

        private int _textIndex;

        private List<TabButton> _tabs = new();
        private List<NotebookEntry> _entries = new();
        private List<NotebookEntry> _notebook = new(); //better as dictionary?
        
        public void Show(MediaLiteracyExercise.Text[] texts, int[] estimates, Action<List<int>[]> callback)
        {
            _texts = texts;
            _estimates = estimates;
            _callback = callback;

            _picks = new List<int>[texts.Length];
            _picksCount = 0;
            _totalSnippetsEstimate = 0;

            for (int i = 0; i < texts.Length; i++)
            {
                _picks[i] = new List<int>();

                if (texts[i].Relevance < 1) continue;

                int snippetsCount = estimates[i];

                TabButton tab = Instantiate(tabPrefab, tabsRoot);
                tab.Init(i, OnTabPressed);

                _tabs.Add(tab);
                UpdateTabTitle(i);

                // HACK to set first relevant text as selected
                if (_tabs.Count == 1) _textIndex = i;

                _totalSnippetsEstimate += snippetsCount;
            }

            gameObject.SetActive(true);

            _tabs[_textIndex].Selected = true;

            UpdateTotalSnippetsLabel();

            SpawnText();

            continueButton.interactable = false;
        }

        private void OnTabPressed(int index)
        {
            if (_textIndex == index) return;

            _textIndex = index;

            for (int i = 0; i < _tabs.Count; i++)
            {
                _tabs[i].Selected = i == _textIndex;
            }

            SpawnText();
        }

        private void SpawnText()
        {
            DestroyAndClearList(_entries);

            MediaLiteracyExercise.Text text = _texts[_textIndex];
            titleLabel.text = text.TitleAudio?.String;
            authorLabel.text = text.AuthorWithBioAudio?.String;
            titleReadable.AudioClip = text.TitleAudio?.AudioClip;
            authorReadable.AudioClip = text.AuthorWithBioAudio?.AudioClip;
            relevanceLabel.text = text.Relevance.ToString();
            relevanceImage.sprite = relevanceSprites[text.Relevance];

            for (int i = 0; i < text.Snippets.Length; i++)
            {
                MediaLiteracyExercise.Text.Snippet snippet = text.Snippets[i];
                var entry = Instantiate(entryPrefab, entriesRoot);
                entry.Init(_textIndex, i, snippet.TextAudio, OnEntryPressed);

                entry.Selected = _picks[_textIndex].Contains(i);

                _entries.Add(entry);
            }

            DoNextFrame(() => notebookScroll.verticalNormalizedPosition = 1);
        }

        private void OnEntryPressed(NotebookEntry entry)
        {
            bool selected = _picks[_textIndex].Contains(entry.SnippetIndex);

            entry.Selected = !selected;

            if (selected)
                RemoveFromNotebook(entry);
            else
                AddToNotebook(entry);

            UpdateTabTitle(_textIndex);
            UpdateTotalSnippetsLabel();

            continueButton.interactable = _picksCount >= totalSnippetsMin;
        }

        private void OnNotebookEntryPressed(NotebookEntry entry)
        {
            if (entry.TextIndex == _textIndex)
            {
                var listEntry = _entries.Find(e => e.SnippetIndex == entry.SnippetIndex);

                listEntry.Selected = false;
            }

            RemoveFromNotebook(entry);

            UpdateTabTitle(entry.TextIndex);
            UpdateTotalSnippetsLabel();
        }

        private void AddToNotebook(NotebookEntry entry)
        {
            _picks[entry.TextIndex].Add(entry.SnippetIndex);
            _picksCount++;

            var copy = Instantiate(entryPrefab, notebookRoot);
            copy.Init(entry.TextIndex, entry.SnippetIndex, entry.Text, OnNotebookEntryPressed);

            _notebook.Add(copy);

            DoNextFrame(() => notebookScroll.verticalNormalizedPosition = 0);
        }

        private void RemoveFromNotebook(NotebookEntry entry)
        {
            _picks[entry.TextIndex].Remove(entry.SnippetIndex);
            _picksCount--;

            var notebookEntry = _notebook.Find(e => e.SnippetIndex == entry.SnippetIndex
                && e.TextIndex == entry.TextIndex);

            _notebook.Remove(notebookEntry);

            Destroy(notebookEntry.gameObject);
        }

        private void UpdateTabTitle(int textIndex)
        {
            MediaLiteracyExercise.Text text = _texts[textIndex];
            TabButton tab = _tabs.Find(t => t.Index == textIndex);
            int picksCount = _picks[textIndex].Count;
            int estimate = _estimates[textIndex];

            tab.Text = string.Format(tabFormat, textIndex + 1, picksCount, estimate);

            Color color = picksCount == estimate ? greatEstimateColor :
                MathF.Abs(picksCount - estimate) <= goodEstimateDelta ? goodEstimateColor :
                badEstimateColor;

            tab.TextColor = color;
        }

        private void UpdateTotalSnippetsLabel()
        {
            totalSnippetsLabel.text = string.Format(totalSnippetsFormat, _picksCount, _totalSnippetsEstimate);

            Color color = _picksCount == _totalSnippetsEstimate ? greatEstimateColor :
                MathF.Abs(_picksCount - _totalSnippetsEstimate) <= goodEstimateDelta ? goodEstimateColor :
                badEstimateColor;

            totalSnippetsLabel.color = color;
        }

        public void OnContinueButtonPressed()
        {
            yesNoRoot.SetActive(true);
        }

        public void OnConfirmButtonPressed()
        {
            yesNoRoot.SetActive(false);
            _callback?.Invoke(_picks);
        }

        public void Hide()
        {
            DestroyAndClearList(_entries);
            DestroyAndClearList(_notebook);
            DestroyAndClearList(_tabs);

            _callback = null;
            gameObject.SetActive(false);
        }

        private void DestroyAndClearList<T>(List<T> list) where T : MonoBehaviour
        {
            foreach (T entry in list)
            {
                Destroy(entry.gameObject);
            }

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
