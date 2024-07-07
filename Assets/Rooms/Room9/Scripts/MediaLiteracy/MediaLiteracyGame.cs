using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Room9
{
    public class MediaLiteracyGame : MonoBehaviour
    {
        [SerializeField]
        private Score score;

        [Header("Widgets")]

        [SerializeField]
        private GameObject themeHeaderRoot;

        [SerializeField]
        private TextMeshProUGUI themeHeaderLabel;

        [SerializeField]
        private ReadableLabel themeHeaderReadable;

        [SerializeField]
        private GameObject timerLabelRoot;

        [Header("Steps")]

        [SerializeField]
        private KnowledgeStep knowledgeStep;

        [SerializeField]
        private OutlineStep outlineStep;

        [SerializeField]
        private RelevanceStep relevanceStep;

        [SerializeField]
        private TimeStep timeStep;

        [SerializeField]
        private SnippetsStep snippetsStep;

        [SerializeField]
        private ProductionStep productionStep;

        [SerializeField]
        private ConclusionStep conclusionStep;

        [SerializeField]
        private SubmissionStep submissionStep;

        [Header("Exercise Data")]

        [SerializeField]
        private LocalizedExerciseList localizedExerciseList;

        [Header("General Data")]
        [SerializeField]
        private ReadableString[] tipsAudio;

        [SerializeField]
        private MediaLiteracyExercise.Quiz[] submissionQuizzes;

        private ExerciseList _exercises;
        private MediaLiteracyExercise _exercise;
        private MediaLiteracyResult _result;
        private Action _callback;

        private long _cachedInizio;

        public void Show(Action callback)
        {
            SendInitToBackend();

            _result = new MediaLiteracyResult();
            _callback = callback;

            StopAllCoroutines();
            StartCoroutine(LoadExerciseList());
        }

        private void SendInitToBackend()
        {
            return;
            if (GameState.Instance.testMode) return;

            GameState.Instance.levelBackend.MediaLiteracy(result => _cachedInizio = result.inizio, (_) => { });
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
            //_exercise = (MediaLiteracyExercise)_exercises[Random.Range(0, _exercises.Count)];
            _exercise = (MediaLiteracyExercise)_exercises[selectMediaLiteracyExercise()];
            
            themeHeaderLabel.text = _exercise.ThemeAudio?.String;
            themeHeaderReadable.AudioClip = _exercise.ThemeAudio?.AudioClip;
            themeHeaderRoot.SetActive(true);
            timerLabelRoot.SetActive(false);

            if (_exercise.Tips != null && _exercise.Tips.Length > 0)
                tipsAudio = _exercise.Tips;
            knowledgeStep.Show(_exercise, _result, tipsAudio, OnKnowledgeStepDone);
        }

        private int selectMediaLiteracyExercise()
        {
            var e = ElliPrefs.GetMediaLiteracyExercisesStarted(GameState.Instance.comprendoBackend.id.ToString());

            List<int> eserciziIniziati = new List<int>();
            List<int> eserciziMancanti = new List<int>();
            if (!string.IsNullOrEmpty(e))
            {
                eserciziIniziati = e.Split(',').Select(int.Parse).ToList();
                eserciziMancanti = Enumerable.Range(0, _exercises.Count).Except(eserciziIniziati).ToList();
            }
            
            
            int esercizioSelezionato = -1;
            if (eserciziMancanti.Count == 0)
            {
                esercizioSelezionato = Random.Range(0, _exercises.Count);
                
                eserciziIniziati.Clear();
            }
            else
            {
                System.Random random = new System.Random();
                esercizioSelezionato = eserciziMancanti[random.Next(eserciziMancanti.Count)];
            }
            
            eserciziIniziati.Add(esercizioSelezionato);
            e = string.Join(",", eserciziIniziati);
            ElliPrefs.SetMediaLiteracyExercisesStarted(GameState.Instance.comprendoBackend.id.ToString(), e);

            return esercizioSelezionato;
        }

        private void OnKnowledgeStepDone()
        {
            outlineStep.Show(_result, OnOutlineStepDone);
        }

        private void OnOutlineStepDone()
        {
            relevanceStep.Show(_exercise, _result, OnRelevanceStepDone);
        }

        private void OnRelevanceStepDone()
        {
            timerLabelRoot.SetActive(true);

            timeStep.Show(_result, OnTimeStepDone);
        }

        private void OnTimeStepDone()
        {
            snippetsStep.Show(_exercise, _result, OnSnippetsStepDone);
        }

        private void OnSnippetsStepDone()
        {
            productionStep.Show(_exercise, _result, OnProductionStepDone);
        }

        private void OnProductionStepDone()
        {
            conclusionStep.Show(_exercise, _result, OnConclusionStepDone);
        }

        private void OnConclusionStepDone()
        {
            timerLabelRoot.SetActive(false);
            submissionStep.Show(_exercise, _result, OnSubmissionStepDone);
        }

        private void OnSubmissionStepDone()
        {
            SendResultToBackend();

            int stars = _result.GetTotalStars();
            int maxStars = _result.GetMaxStars();

            score.RightCounter += stars;
            score.WrongCounter += maxStars - stars;

            GameState.Instance.RoomStars += stars;

            _callback.Invoke();
        }

        private void SendResultToBackend()
        {
            return;
            if (GameState.Instance.testMode) return;

            GameState.Instance.levelBackend.EndMediaLiteracy(_cachedInizio, _result, () => { }, (_) => { });
        }

#if UNITY_EDITOR
        [ContextMenu("Cheat")]
        public void Cheat()
        {
            _result.SnippetEstimates = new int[_exercise.Texts.Length];
            productionStep.Show(_exercise, _result, OnProductionStepDone);
        }
#endif
    }
}
