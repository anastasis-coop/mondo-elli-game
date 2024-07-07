using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Room9
{
    public class QuizMultipleChoice : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI titleLabel;

        [SerializeField]
        private ReadableLabel titleReadable;

        [SerializeField]
        private TextMeshProUGUI[] choiceLabels;

        [SerializeField]
        private ReadableLabel[] choiceReadables;

        [SerializeField]
        private Image[] choiceBackgrounds;

        [SerializeField]
        private Button continueButton;

        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private Sprite defaultBG;

        [SerializeField]
        private Sprite selectedBG;

        [SerializeField]
        private Sprite correctBG;

        [SerializeField]
        private Sprite wrongBG;

        private Action<HashSet<int>> _callback;
        private int[] _correct;
        private HashSet<int> _choices = new();
        private int[] _shuffled;
        private int _minimumAnswers = 1;

        public void Show(LocalizedReadableStringAsset title, LocalizedReadableStringAsset[] choices,
            Action<HashSet<int>> callback, int[] correct = null, bool shuffle = false, int minimumAnswers = 1)
        {
            titleLabel.SetText(string.Empty);
            titleReadable.AudioClip = null;

            gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(ShowLocalizedRoutine(title, choices, callback, correct, shuffle, minimumAnswers));
        }

        public IEnumerator ShowLocalizedRoutine(LocalizedReadableStringAsset localizedTitle, LocalizedReadableStringAsset[] localizedChoices,
            Action<HashSet<int>> callback, int[] correct = null, bool shuffle = false, int minimumAnswers = 1)
        {
            var titleHandle = localizedTitle.LoadAssetAsync();

            yield return titleHandle;

            ReadableString title = titleHandle.Result.Value;

            List<ReadableString> choices = new();

            foreach (var localizedChoice in localizedChoices)
            {
                var choiceHandle = localizedChoice.LoadAssetAsync();

                yield return choiceHandle;

                choices.Add(choiceHandle.Result.Value);
            }

            Show(title, choices.ToArray(), callback, correct, shuffle, minimumAnswers);
        }

        public void Show(ReadableString title, ReadableString[] choices, Action<HashSet<int>> callback, int[] correct = null, bool shuffle = false, int minimumAnswers = 1)
        {
            titleLabel.text = title?.String;
            titleReadable.AudioClip = title?.AudioClip;

            _shuffled = new int[choices.Length];

            for (int i = 0; i < choices.Length; i++) _shuffled[i] = i;

            if (shuffle) Shuffle(_shuffled);

            for (int i = 0; i < choiceLabels.Length; i++)
            {
                var choice = choices[_shuffled[i]];
                choiceLabels[i].text = choice?.String;
                choiceReadables[i].AudioClip = choice?.AudioClip;
                choiceBackgrounds[i].sprite = defaultBG;
            }

            _correct = correct;
            _callback = callback;
            _minimumAnswers = minimumAnswers;

            canvasGroup.interactable = true;
            continueButton.interactable = false;

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

        public void OnChoiceSelected(int choice)
        {
            if (_choices.Contains(choice))
                _choices.Remove(choice);
            else
                _choices.Add(choice);

            for (int i = 0; i < choiceBackgrounds.Length; i++)
            {
                choiceBackgrounds[i].sprite = _choices.Contains(i) ? selectedBG : defaultBG;
            }

            continueButton.interactable = _choices.Count >= _minimumAnswers;
        }

        public void OnContinueButtonPressed()
        {
            canvasGroup.interactable = false;
            continueButton.interactable = false;

            if (_correct != null)
            {
                for (int i = 0; i < choiceBackgrounds.Length; i++)
                {
                    bool isChoice = _choices.Contains(i);
                    bool isCorrect = Array.IndexOf(_correct, _shuffled[i]) != -1;

                    choiceBackgrounds[i].sprite = isCorrect ? correctBG :
                        isChoice ? wrongBG : defaultBG;
                }
            }

            Invoke(nameof(Continue), 1);
        }

        private void Continue()
        {
            HashSet<int> answers = new HashSet<int>(_choices.Count);

            foreach (int choice in _choices) answers.Add(_shuffled[choice]);

            _callback?.Invoke(answers);
        }

        public void Hide()
        {
            _callback = null;

            CancelInvoke(nameof(Continue));

            gameObject.SetActive(false);
        }
    }
}
