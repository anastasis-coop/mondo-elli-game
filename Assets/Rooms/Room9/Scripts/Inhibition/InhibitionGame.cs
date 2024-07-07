using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using static RoomLevel;
using System.Collections;

namespace Room9
{
    public class InhibitionGame : MonoBehaviour
    {
        [Header("Steps")]

        [SerializeField]
        private InhibitionTitleRelevanceStep titleRelevanceStep;

        [SerializeField]
        private InhibitionSnippetRelevanceStep negativeSnippetRelevanceStep;

        [SerializeField]
        private InhibitionSnippetRelevanceStep positiveSnippetRelevanceStep;

        [Header("Widgets")]

        [SerializeField]
        private GameObject themeHeaderRoot;

        [SerializeField]
        private TextMeshProUGUI themeHeaderLabel;

        [SerializeField]
        private ReadableLabel themeHeaderReadable;

        [Header("Data")]

        [SerializeField]
        private LocalizedExerciseList localizedExerciseList;

        private RoomLevel _level;
        private ExerciseList _exercises;
        private InhibitionExercise _exercise;
        private Action _callback;

        public void Show(RoomLevel level, Action callback)
        {
            _level = level;
            _callback = callback;

            StopAllCoroutines();
            StartCoroutine(LoadExerciseList());
        }

        private IEnumerator LoadExerciseList()
        {
            var handle = localizedExerciseList.LoadAssetAsync();

            yield return handle;

            _exercises = handle.Result;

            OnExerciseListLoaded();
        }

        private void OnExerciseListLoaded()
        {
            _exercise = (InhibitionExercise)_exercises[Random.Range(0, _exercises.Count)];

            var theme = _exercise.ThemeAudio;

            themeHeaderLabel.text = theme?.String;
            themeHeaderReadable.AudioClip = theme?.AudioClip;
            themeHeaderRoot.SetActive(true);

            if (_level is LEVEL_01 or LEVEL_11 or LEVEL_22)
                titleRelevanceStep.Show(theme, _exercise.Titles, OnGameCompleted);
            else if (_level is LEVEL_02 or LEVEL_12 or LEVEL_31)
                negativeSnippetRelevanceStep.Show(theme, _exercise.NegativeRelevanceTexts, OnGameCompleted);
            else // level is LEVEL_21 or LEVEL_32
                positiveSnippetRelevanceStep.Show(theme, _exercise.PositiveRelevanceTexts, OnGameCompleted);
        }

        private void OnGameCompleted() => _callback?.Invoke();
    }
}
