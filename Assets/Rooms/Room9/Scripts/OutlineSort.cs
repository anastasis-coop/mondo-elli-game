using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Room9
{
    public class OutlineSort : MonoBehaviour
    {
        [SerializeField]
        private Transform outlineRoot;

        [SerializeField]
        private GameObject solutionSlider;

        [SerializeField]
        private Image solutionSliderFill;

        [SerializeField]
        private OutlineEntry outlineEntryPrefab;

        [SerializeField]
        private OutlineDragDrop outlineDragDrop;

        [SerializeField]
        private Button continueButton;

        private bool _help;
        private Action<int[]> _callback;

        private List<(OutlineEntry, int)> entries = new();

        public void Show(bool help, ReadableString[] texts, bool localizeReadableStrings,  Action<int[]> callback)
        {
            int count = texts.Length;

            List<int> solution = new(count);

            for (int i = 0; i < count; i++)
                solution.Add(i);

            Shuffle(solution);

            for (int i = 0; i < count; i++)
            {
                var entry = Instantiate(outlineEntryPrefab, outlineRoot);
                entry.localizeText = localizeReadableStrings;
                entry.Index = i + 1;
                entry.Text = texts[solution[i]];

                entry.SolutionIndex = solution[i]+1;
                entry.ShowSolutionIndex(false);
                
                entries.Add((entry, solution[i]));
            }

            _help = help;

            solutionSlider.SetActive(true);

            solutionSliderFill.fillAmount = 0;

            continueButton.interactable = !_help;

            outlineDragDrop.OrderChanged += OnOutlineOrderChanged;

            _callback = callback;

            gameObject.SetActive(true);
            
            if (help)
            {
                DOVirtual.DelayedCall(180f, delegate
                {
                    foreach ((OutlineEntry entry, int solution) in entries)
                    {
                        entry.ShowSolutionIndex(true);
                    }
                });
            }
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

        private void OnOutlineOrderChanged()
        {
            int correct = 0;

            foreach ((OutlineEntry entry, int solution) in entries)
            {
                int siblingIndex = entry.transform.GetSiblingIndex();

                if (siblingIndex == solution) correct++;

                entry.Index = siblingIndex + 1;
            }

            continueButton.interactable = !_help || correct == entries.Count;
            
            solutionSliderFill.DOKill();
            solutionSliderFill.DOFillAmount(correct / (float)entries.Count, 0.5f);
        }

        public void OnContinueButtonPressed()
        {
            // -> an array of the correct order indexes in user answer order
            int[] answers = new int[entries.Count];

            foreach ((OutlineEntry entry, int solution) in entries)
            {
                answers[entry.transform.GetSiblingIndex()] = solution;
            }

            _callback?.Invoke(answers);
        }

        public void OnTimerExpired() => OnContinueButtonPressed();

        public void Hide()
        {
            outlineDragDrop.OrderChanged -= OnOutlineOrderChanged;

            solutionSliderFill.DOKill();

            foreach ((OutlineEntry entry, _) in entries)
            {
                Destroy(entry.gameObject);
            }

            entries.Clear();

            _callback = null;

            gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        [ContextMenu("Cheat")]
        public void Cheat()
        {
            if (isActiveAndEnabled)
            {
                int[] solution = new int[entries.Count];

                for (int i = 0; i < entries.Count; i++) solution[i] = i;

                _callback?.Invoke(solution);
            }
        }
#endif
    }
}