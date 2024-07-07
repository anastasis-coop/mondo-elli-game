using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using static UnityStandardAssets.Utility.TimedObjectActivator;

namespace Room9
{
    [CreateAssetMenu]
    public class MediaLiteracyExercise : Exercise
    {
#if UNITY_EDITOR
        [ContextMenu("PrintJSON")]
        public void PrintJson()
        {
            Debug.LogError(JsonUtility.ToJson(this));
        }
#endif

        [Serializable]
        public class Quiz
        {
            [FormerlySerializedAs("QuestionAudio")]
            public ReadableString QuestionAudio;
            [FormerlySerializedAs("ChoicesAudio")]
            public ReadableString[] ChoicesAudio;
            public int[] Correct;

            public ReadableString[] CorrectChoices => Array.ConvertAll(Correct, i => ChoicesAudio[i]);
        }

        public Quiz[] KnowledgeQuizzes;

        [Serializable]
        public class Text
        {
            [FormerlySerializedAs("TitleAudio")]
            public ReadableString TitleAudio;
            [FormerlySerializedAs("PreviewAudio")]
            public ReadableString PreviewAudio;
            [FormerlySerializedAs("AuthorAudio")]
            public ReadableString AuthorAudio;
            [FormerlySerializedAs("AuthorWithBioAudio")]
            public ReadableString AuthorWithBioAudio;
            public int Relevance;

            [Serializable]
            public class Snippet
            {
                [FormerlySerializedAs("TextAudio")]
                public ReadableString TextAudio;
                public int Relevance;
            }

            public Snippet[] Snippets;
        }

        public Text[] Texts;

        public Quiz TagsQuiz;
        public Quiz TitleQuiz;

        public Quiz[] SubmissionQuizzes;
        public ReadableString TipsQuizTitle;
        public ReadableString[] Tips;
    }

    [Serializable]
    public class MediaLiteracyResult
    {
        public int KnowledgeEstimate; //slider iniziale 1-10
        public bool[] KnowledgeQuizzesAnswers; //risposte corrette o meno ai quiz

        public bool OutlineNeededHelp; //se ha fallito la prima iterazione della scaletta

        public int[] RelevancePoints; //rilevanza assegnata in base agli snippet
        public int[] RelevanceQuizzesAnswers; //valutazione testi non rilevanti

        public int SnippetMinutes; //assegnazione minuti snippet
        public int ProductionMinutes; //assegnazione minuti produzione
        public int RevisionMinutes; //assegnazione minuti revisione

        public bool TimeEstimatesMet; //ha rispettato o meno i tempi

        public int[] SnippetEstimates; //numero stimati di snippet

        public bool SnippetEstimatesMet; //ha rispettato o meno il numero di snippet allocati

        public List<int>[] ProductionPicks; //indici degli snippet per ogni testo selezionati

        public int ProductionPoints; //punteggio di rilevanza sommato degli snippet selezionati

        // TODO maybe don't put the string here
        public string ProductionResult; //testo prodotto
        public List<AudioClip> ProductionAudio; //audio del testo prodotto in sequenza

        public int TagsCorrectAnswers; //quanti tag corretti sono stati selezionati
        public bool TitleCorrectAnswer; //selezionato corretto titolo o no

        public int[] SubmissionQuizzesAnswers; //indice delle risposte ai quiz
        public int[] SubmissionTipsAnswers; //indice dei consigli

        // Stars calculation
        public int GetKnowledgeStars()
        {
            bool[] answers = KnowledgeQuizzesAnswers;
            int correctCount = answers.Count(correct => correct);
            float t = (float)correctCount / answers.Length;

            return t < 0.5f ? 0 : 1;
        }

        public int GetOutlineStars() => OutlineNeededHelp ? 1 : 2;

        public int GetRelevanceStars()
        {
            int count = RelevancePoints.Length;
            int max = 2 * count;
            int score = RelevancePoints.Sum();
            float percentage = (float)score / max;

            return percentage <= 0.4f ? 0 :
                percentage <= 0.6f ? 1 :
                percentage < 0.8f ? 2 : 3;
        }

        public int GetTimeStars() => TimeEstimatesMet ? 1 : 0;
        public int GetSnippetsStars() => SnippetEstimatesMet ? 1 : 0;
        public int GetProductionStars() => 3;

        public int GetConclusionStars()
            => TitleCorrectAnswer ? TagsCorrectAnswers == 3 ? 2 : 1 : 0;

        public int GetTotalStars() =>
            GetKnowledgeStars() +
            GetOutlineStars() +
            GetRelevanceStars() +
            GetTimeStars() +
            GetSnippetsStars() +
            GetProductionStars() +
            GetConclusionStars();

        public int GetMaxStars() => 14;
    }
}

