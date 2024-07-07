using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
namespace Room9
{

    public class QuizChoice : MonoBehaviour
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
        private Color defaultColor = Color.white;

        [SerializeField]
        private Color selectedColor = Color.blue;

        [SerializeField]
        private Color correctColor = Color.green;

        [SerializeField]
        private Color wrongColor = Color.red;

        private Action<int> _callback;
        private int _correct;
        private int _choice;
        private int[] _shuffled;

        public void Show(LocalizedReadableStringAsset title, LocalizedReadableStringAsset[] choices, Action<int> callback, int correct = -1, bool shuffle = false)
        {
            titleLabel.SetText(string.Empty);
            titleReadable.AudioClip = null;

            for (int i = 0; i < choiceLabels.Length; i++)
            {
                choiceBackgrounds[i].gameObject.SetActive(false);
            }

            gameObject.SetActive(true);
            StopAllCoroutines();

            StartCoroutine(ShowLocalizedRoutine(title, choices, callback, correct, shuffle));
        }

        private IEnumerator ShowLocalizedRoutine(LocalizedReadableStringAsset localizedTitle, LocalizedReadableStringAsset[] localizedChoices,
            Action<int> callback, int correct = -1, bool shuffle = false)
        {
            var titleHandle = localizedTitle.LoadAssetAsync();

            yield return titleHandle;

            var title = titleHandle.Result.Value;

            var choices = new List<ReadableString>();

            foreach (var choice in localizedChoices)
            {
                var handle = choice.LoadAssetAsync();
                yield return handle;
                choices.Add(handle.Result.Value);
            }

            Show(title, choices.ToArray(), callback, correct, shuffle);
        }

        public void Show(ReadableString title, ReadableString[] choices, Action<int> callback, int correct = -1, bool shuffle = false)
        {
            titleLabel.text = title?.String;
            titleReadable.AudioClip = title?.AudioClip;

            _shuffled = new int[choices.Length];

            for (int i = 0; i < choices.Length; i++) _shuffled[i] = i;

            if (shuffle) Shuffle(_shuffled);

            for (int i = 0; i < choiceLabels.Length; i++)
            {
                choiceBackgrounds[i].gameObject.SetActive(i < choices.Length);

                if (i >= choices.Length) continue;

                choiceLabels[i].text = choices[_shuffled[i]]?.String;
                choiceReadables[i].AudioClip = choices[_shuffled[i]]?.AudioClip;
                choiceBackgrounds[i].color = defaultColor;
            }

            _correct = correct;
            _callback = callback;

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
            _choice = choice;

            for (int i = 0; i < choiceBackgrounds.Length; i++)
            {
                choiceBackgrounds[i].color = i == choice ? selectedColor : defaultColor;
            }

            continueButton.interactable = true;
        }

        public void OnContinueButtonPressed()
        {
            canvasGroup.interactable = false;
            continueButton.interactable = false;

            if (_correct != -1)
            {
                if (_shuffled[_choice] != _correct)
                {
                    choiceBackgrounds[_choice].color = wrongColor;
                }

                int correctIndex = Array.IndexOf(_shuffled, _correct);

                choiceBackgrounds[correctIndex].color = correctColor;
            }

            Invoke(nameof(Continue), 1);
        }

        private void Continue()
        {
            _callback?.Invoke(_shuffled[_choice]);
        }

        public void Hide()
        {
            StopAllCoroutines();

            _callback = null;

            CancelInvoke(nameof(Continue));

            gameObject.SetActive(false);
        }
    }
}