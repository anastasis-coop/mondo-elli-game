using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using static RoomLevel;
using System.Collections;

namespace Room9
{
    public class FlexibilityGame : MonoBehaviour
    {
        [Header("Widgets")]

        [SerializeField]
        private GameObject themeHeaderRoot;

        [SerializeField]
        private TextMeshProUGUI themeHeaderLabel;

        [SerializeField]
        private ReadableLabel themeHeaderReadable;

        [Header("Steps")]

        [SerializeField]
        private FlexibilityEstimatesStep relevanceStep;

        [SerializeField]
        private FlexibilityEstimatesStep snippetsStep;

        [SerializeField]
        private FlexibilityEstimatesStep timeStep;

        [Header("Data")]

        [SerializeField]
        private LocalizedExerciseList localizedExerciseList;

        private RoomLevel _level;
        private ExerciseList _exercises;
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
            var exercise = (FlexibilityExercise)_exercises[Random.Range(0, _exercises.Count)];

            var theme = exercise.ThemeAudio;

            themeHeaderLabel.text = theme?.String;
            themeHeaderReadable.AudioClip = theme?.AudioClip;
            themeHeaderRoot.SetActive(true);

            if (_level is LEVEL_01 or LEVEL_11 or LEVEL_22)
                relevanceStep.Show(theme, exercise.RelevanceTexts, 0, true, OnGameCompleted);
            else if (_level is LEVEL_02 or LEVEL_12 or LEVEL_31)
                snippetsStep.Show(theme, exercise.SnippetsTexts, 1, false, OnGameCompleted);
            else // _level is LEVEL_21 or LEVEL_32
                timeStep.Show(theme, exercise.TimeTexts, 2, true, OnGameCompleted);
        }

        private void OnGameCompleted() => _callback?.Invoke();
    }
}