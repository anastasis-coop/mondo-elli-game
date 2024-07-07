using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using static RoomLevel;
using System.Collections;

namespace Room9
{
    public class MemoryGame : MonoBehaviour
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
        private MemoryTitleRelevanceStep titleRelevanceStep;

        [SerializeField]
        private MemoryOutlineStep outlineStep;

        [SerializeField]
        private MemoryPreviewStep previewStep;

        [Header("Data")]

        [SerializeField]
        private LocalizedExerciseList localizedExerciseList;

        private ExerciseList _exercises;
        private MemoryExercise _exercise;
        private RoomLevel _level;
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
            _exercise = (MemoryExercise)_exercises[Random.Range(0, _exercises.Count)];

            var theme = _exercise.ThemeAudio;

            themeHeaderLabel.text = theme?.String;
            themeHeaderReadable.AudioClip = theme?.AudioClip;
            themeHeaderRoot.SetActive(true);

            if (_level is LEVEL_01 or LEVEL_11 or LEVEL_22)
                titleRelevanceStep.Show(theme, _exercise.Titles, OnGameCompleted);
            else if (_level is LEVEL_02 or LEVEL_12 or LEVEL_31)
                outlineStep.Show(theme, _exercise.OutlineParagraphs, OnGameCompleted);
            else // _level is LEVEL_21 or LEVEL_32
                previewStep.Show(theme, _exercise.PreviewQuizzes, OnGameCompleted);
        }

        private void OnGameCompleted() => _callback?.Invoke();
    }
}