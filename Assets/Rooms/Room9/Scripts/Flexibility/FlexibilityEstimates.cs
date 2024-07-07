using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace Room9
{
    public class FlexibilityEstimates : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI propertyLabel;

        [SerializeField]
        private FlexibilityEntry entry;

        [SerializeField]
        private TextReview textReview;

        private ReadableString[] _snippets;
        private int _depth;
        private bool mergeSnippets;
        private Vector2Int _range;
        private ReadableString[] _layers;
        private Action<int[]> _callback;

        private List<int> _estimates = new();

        public void Show(string property, Vector2Int range, ReadableString[] layers, int depth, ReadableString[] snippets, bool mergeSnippets, Action<int[]> callback)
        {
            _snippets = snippets;
            _range = range;
            _layers = layers;
            _callback = callback;

            propertyLabel.text = property;

            _ = TryShowLayer(depth);

            gameObject.SetActive(true);
        }

        private bool TryShowLayer(int depth)
        {
            if (depth >= _layers.Length) return false;

            ReadableString title = _layers[0];
            ReadableString[] texts = new ReadableString[depth];
            
            for (int i = 0; i < depth; i++)
            {
                texts[i] = _layers[i + 1];
            }

            textReview.Show(title, null, texts, OnContinueButtonPressed);
            entry.Init(_range, null);

            _depth = depth;

            return true;
        }

        public void OnTimerExpired() => _callback.Invoke(_estimates.ToArray());

        public void OnContinueButtonPressed()
        {
            textReview.Hide();

            _estimates.Add(entry.Value);

            if (!TryShowLayer(_depth + 1))
            {
                ReadableString title = _layers[0];

                // HACK this is mostly 0 except when pages info is a layer
                // TODO too messy: remove layer logic and implement explicit per exercise parameters
                // All _layers minus title and preview
                int introLength = _layers.Length - 2;

                List<ReadableString> texts = new List<ReadableString>();

                for (int i = 0; i < introLength; i++)
                    texts.Add(_layers[i + 1]); // +1 for no title

                if (!mergeSnippets)
                    texts.AddRange(_snippets);

                textReview.Show(title, null, texts.ToArray(), OnTextReviewDone, highlight: !mergeSnippets);
                entry.Init(_range, null);

                if (mergeSnippets)
                {
                    StringBuilder builder = new();
                    List<AudioClip> clips = new();

                    foreach (var snippet in _snippets)
                    {
                        builder.Append(snippet);
                        clips.Add(snippet.AudioClip);
                    }

                    textReview.AppendMultiClipLabel(builder.ToString(), clips);
                }
            }
        }

        private void OnTextReviewDone()
        {
            _estimates.Add(entry.Value);

            textReview.Hide();
            _callback?.Invoke(_estimates.ToArray());
        }

        public void Hide()
        {
            _estimates.Clear();

            _layers = null;
            _callback = null;

            gameObject.SetActive(false);
        }
    }
}
