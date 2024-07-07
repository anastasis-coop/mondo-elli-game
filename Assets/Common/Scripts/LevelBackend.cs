using UnityEngine;
using Proyecto26;
using System;
using System.Collections.Generic;

public class EndExplorationParams {
    public long inizio;
    public string quartiere;
    public string compito;
    public string esecuzione;
    public bool corretto;
    public int tempoImpiegato;
    public bool ombra;
    public string prospettiva;
    public bool finale;
}

public enum EndExercizeSaveResponse { NONE, PASSATO, IN_CORSO, FALLITO };

public class LevelBackend : ScriptableObject {

    public bool isActive = true;

#if UNITY_WEBGL && !UNITY_EDITOR
    private readonly string basePath = "/mondoelli-server"; // URL relativo al server
#else
    private readonly string basePath = "http://localhost:8080";
#endif

    private readonly string getParamsPath = "/parametri"; //GET
    private readonly string saveParamsPath = "/parametri"; //POST
    private readonly string initSessionPath = "/sessioni/init"; //POST
    private readonly string endSessionPath = "/sessioni/{0}/end"; //POST
    private readonly string saveExplorationPath = "/sessioni/{0}/esplorazioni"; //POST
    private readonly string startExplorationPath = "/esplorazioni/init"; //GET
    private readonly string enterRoomPath = "/sessioni/{0}/stanze/init"; //POST
    private readonly string exitRoomPath = "/stanze/{0}"; //POST
    private readonly string quizPath = "/quiz/{0}"; //GET
    private readonly string saveQuizPath = "/sessioni/{0}/quiz"; //POST
    private readonly string mustShowVideoPath = "/stanze/{0}/video"; //GET
    private readonly string saveAreaCompletedPath = "/quartieri/{0}/completed"; //POST
    private readonly string mediaLiteracyPath = "/medialiteracy/init"; // GET
    private readonly string endMediaLiteracyPath = "/sessioni/{0}/medialiteracy"; //POST

    //private readonly string resetAreaCompletedPath = "/sessione/sessione/resetQuartieri";

    // State of the city at loading
    public bool areaCompleted = false;
    
    // The game hasn't started yet (maybe today is before the given first week?)
    public bool notStartedYet;

    // All game is completed
    public bool gameCompleted = false;

    // Rooms menu: current area type
    public AreaType areaType = AreaType.QUARTIERE;

    // Rooms menu: current position type
    public PositionType positionType = PositionType.INIZIO;

    // State of the city at loading
    public Island island = Island.INTRODUZIONE;

    // Completed islands
    public List<Island> completedIslands = new();

    // State of the channel between city and rooms
    public RoomChannel roomChannel = RoomChannel.VISIVO;

    //private RoomChannel roomChannel = RoomChannel.VISIVO;
    public RoomLevel roomLevel = RoomLevel.LEVEL_01;

    // Livello di difficoltà Coding 0-3
    public int CodingLevel = 0;

    // Livello di complessità delle tessere coding 0-5 TODO enum
    public int CodingTileSetLevel = 0;

    // Enabled Media Literacy features
    public bool MediaLiteracyEnabled { get; set; }

    // Enabled Demo Mode
    public bool Demo { get; private set; }

    public EndExercizeSaveResponse lastExercizeResult = EndExercizeSaveResponse.NONE;

    // Internal cache
    private int sessionId = 0;
    private int exerciseId = 0;
    private long startTime = 0;
    private int maxExplorationTime = 5;

    [Serializable]
    public class ParametriDto {
        public string nomeEllo;
        public int accessorio1;
        public int accessorio2;
        public int accessorio3;
        public bool demo;
        public string quartiereCorrente;
        public List<string> quartieriCompletati;
    }

    [Serializable]
    public class SalvataggioParametriDto {
        public string nomeEllo;
        public int accessorio1;
        public int accessorio2;
        public int accessorio3;
    }

    [Serializable]
    public class DatiSessioneDto
    {
        public int id;
        public long inizio;
        public string quartiere;
        public int tempoMassimoEsplorazione;
        public bool demo;
        public bool mediaLiteracy;
        public bool quartiereCompletato;
        public bool percorsoCompletato;
        public bool percorsoNonIniziato;
        public int livello; // livello difficoltà percorso (0-1-2-3)
        public int tessere; // tipologia tessere disponibili (0-1-2-3-4-5)
    }

    [Serializable]
    public class RisultatiSessioneDto
    {
        public int score;
        public bool scoreRecord;
    }

    [Serializable]
    public class InizioEsplorazioneDto
    {
        public long inizio;
    }

    [Serializable]
    public class FineEsplorazioneDto
    {
        public long inizio;
        public string quartiere;
        public string compito;
        public string esecuzione;
        public bool corretto;
        public int tempoImpiegato;
        public bool ombra;
        public string prospettiva;
        public bool finale;
    }

    [Serializable]
    public class InitEsercizioDto
    {
        public string funzioneEsecutiva;
        public string canale;
    }

    [Serializable]
    public class EsercizioStanzaDto
    {
        public string livello;
        public long inizio;
        public int idEsercizio;
        public string funzioneEsecutiva;
        public string canale;
        public bool finito;
    }

    [Serializable]
    public class RisultatoEsercizioDto
    {
        public long inizio;
        public int durata;
        public int numeroStimoliCorretti;
        public int numeroStimoliErrati;
        public int numeroStimoliSaltati;
        public int numeroCombo;
        public int tempoReazioneMedio; // in ms
        public int tempoEsposizioneOggetti; // in ms
        public int feedbackAttenzione;
    }

    [Serializable]
    public class AvanzamentoEsercizioDto
    {
        public string esito;
    }

    [Serializable]
    public class QuizDto {
        public int id;
        public List<string> situazioni;
        public string corretta;
        public string funzioneEsecutiva;
    }

    [Serializable]
    public class RisultatoQuizDto {
        public int idQuiz; // id del QuizDto precedente
        public int tempoImpiegato; // in secs
        public string risposta;
    }

    [Serializable]
    public class PunteggioQuizDto {
        public int score;
    }

    [Serializable]
    public class BooleanDto
    {
        public bool value;
    }

    [Serializable]
    public class EmptyDto {
    }

    [Serializable]
    public class InizioMediaLiteracyDto
    {
        public long inizio;
    }

    [Serializable]
    public class RisultatiMediaLiteracyDto 
    {
        public long inizio;
        public int knowledgeEstimate;             // Quanto conosci l'argomento da 1 a 10
        public bool[] knowledgeQuizzesAnswers;    // Risultato quiz conoscenze pregresse
        public bool outlineNeededHelp;            // Scaletta ordinata al primo colpo
        public int[] relevancePoints;             // Valutazione rilevanza, risultato per ogni testo
        public int[] relevanceQuizzesAnswers;     // Indice risposte ai quiz dove bisogna motivare l'esclusione di un testo 
        public int snippetMinutes;                // Stima di tempo (minuti) sulla fase selezione snippet
        public int productionMinutes;             // Stima di tempo (minuti) sulla fase produzione testo
        public int revisionMinutes;               // Stima di tempo (minuti) sulla fase revisione testo
        public bool timeEstimatesMet;             // L'esercizio è stato completato in un intervallo simile alla stima iniziale
        public int[] snippetEstimates;            // Allocazione numero snippet per ogni testo
        public bool snippetEstimatesMet;          // Il numero di snippet estratti dai testi è simile a quello stimato
        public List<int>[] productionPicks;       // Per ogni testo, lista degli indici degli snippet estratti
        public int productionPoints;              // Somma dei punti totali degli snippet estratti
        public string productionResult;           // Testo prodotto (snippet + card) senza titolo e tag
        public int tagsCorrectAnswers;            // Numero di tag corretti selezionati
        public bool titleCorrectAnswer;           // Il titolo scelto è quello corretto
        public int[] submissionQuizzesAnswers;    // Indici risposte sondaggio finale
        public int[] submissionTipsAnswers;       // Indici consigli per i compagni selezionati
    }


//[Serializable]
//public class ResetAreaCompletataDto {
//    public string nomeUtente;
//}

public int getMaxExplorationMinutes()
    {
        return maxExplorationTime;
    }

    private void SetHeader() {
        // Tutte le richieste al server devono avere lo stesso header
        RestClient.DefaultRequestHeaders["Content-Type"] = "application/json";
    }
    
    public void GetElloParams(Action<ParametriDto> onSuccess, Action<Exception> onFailure) {
        if (isActive) {
            Debug.Log("LEVEL BACKEND: Game customization request");

            SetHeader();
            // Richiesta parametri utente
            RestClient.Get<ParametriDto>(basePath + getParamsPath)
            .Then(res => {
                Debug.Log("LEVEL BACKEND: Game customization request success: " + JsonUtility.ToJson(res, true));

                completedIslands.Clear();

                foreach (string name in res.quartieriCompletati)
                {
                    if (Enum.TryParse(name, out Island island))
                        completedIslands.Add(island);
                }

                Enum.TryParse(res.quartiereCorrente, out island);

                onSuccess(res);
            })
            .Catch(err => onFailure(err));
        }
        else {
            ParametriDto res = new ParametriDto();
            res.nomeEllo = "";
            res.accessorio1 = 0;
            res.accessorio2 = 0;
            res.accessorio3 = 0;
            res.demo = false;
            res.quartiereCorrente = "";
            res.quartieriCompletati = new();
            onSuccess(res);
        }
    }

    public void SaveElloParams(string nome, int accessorio1, int accessorio2, int accessorio3, Action onSuccess, Action<Exception> onFailure) {
        if (isActive) {
            Debug.Log($"LEVEL BACKEND: Game customization save request, nomeEllo: {nome}, accessorio1: {accessorio1}, accessorio2: {accessorio2}, accessorio3: {accessorio3}");

            SetHeader();
            RestClient.Post(basePath + saveParamsPath, new SalvataggioParametriDto {
                nomeEllo = nome,
                accessorio1 = accessorio1,
                accessorio2 = accessorio2,
                accessorio3 = accessorio3
            })
            .Then(res => {
                Debug.Log("LEVEL BACKEND: Game customization save request success: " + JsonUtility.ToJson(res, true));
                onSuccess();
            })
            .Catch(err => onFailure(err));
        }
        else {
            onSuccess();
        }
    }

    public void StartSession(Action<DatiSessioneDto> onSuccess, Action<Exception> onFailure)
    {
        if (isActive)
        {
            Debug.Log("LEVEL BACKEND: Start session request");

            SetHeader();
            // Richiesta inizio sessione
            RestClient.Post<DatiSessioneDto>(basePath + initSessionPath, new EmptyDto())
            .Then(res => {
                Debug.Log("LEVEL BACKEND: Start session request success: " + JsonUtility.ToJson(res, true));
                sessionId = res.id;
                startTime = res.inizio;
                if (res.quartiere != null)
                {
                    island = (Island)Enum.Parse(typeof(Island), res.quartiere, false);
                }
                maxExplorationTime = res.tempoMassimoEsplorazione;
                areaCompleted = res.quartiereCompletato;
                gameCompleted = res.percorsoCompletato;
                notStartedYet = res.percorsoNonIniziato;
                MediaLiteracyEnabled = res.mediaLiteracy;
                Demo = res.demo;
                CodingLevel = res.livello;
                CodingTileSetLevel = res.tessere;
                onSuccess(res);
            })
            .Catch(err => onFailure(err));
        }
        else
        {
            DatiSessioneDto res = new DatiSessioneDto();
            res.id = 0;
            res.inizio = 0;
            res.tempoMassimoEsplorazione = 5;
            res.quartiereCompletato = false;
            sessionId = res.id;
            startTime = res.inizio;
            maxExplorationTime = res.tempoMassimoEsplorazione;
            areaCompleted = res.quartiereCompletato;
            onSuccess(res);
        }
    }

    public void EndSession(Action<RisultatiSessioneDto> onSuccess, Action<Exception> onFailure)
    {
        if (isActive)
        {
            Debug.Log("LEVEL BACKEND: End session request, sessione: " + sessionId);

            SetHeader();
            // Richiesta parametri utente
            RestClient.Post<RisultatiSessioneDto>(basePath + string.Format(endSessionPath, sessionId), new EmptyDto { })
            .Then(res => {
                Debug.Log("LEVEL BACKEND: End session request success: " + JsonUtility.ToJson(res, true));
                onSuccess(res);
            })
            .Catch(err => onFailure(err));
        }
        else
        {
            RisultatiSessioneDto res = new RisultatiSessioneDto();
            res.score = 50;
            res.scoreRecord = false;
            onSuccess(res);
        }
    }

    public void Exploration(Action<InizioEsplorazioneDto> onSuccess, Action<Exception> onFailure) {
        if (isActive) {
            Debug.Log("LEVEL BACKEND: Exploration request");

            SetHeader();
            RestClient.Get<InizioEsplorazioneDto>(basePath + startExplorationPath)
            .Then(res => {
                Debug.Log("LEVEL BACKEND: Exploration request success: " + JsonUtility.ToJson(res, true));

                onSuccess(res);
            })
            .Catch(err => onFailure(err));
        }
        else {
            InizioEsplorazioneDto res = new InizioEsplorazioneDto();
            res.inizio = 0;
            onSuccess(res);
        }
    }

    public void EndExploration(EndExplorationParams param, Action onSuccess, Action<Exception> onFailure)
    {
        if (isActive)
        {
            Debug.Log("LEVEL BACKEND: End exploration request, sessione: " + sessionId + " params: "+ JsonUtility.ToJson(param));

            SetHeader();
            RestClient.Post(basePath + string.Format(saveExplorationPath, sessionId), new FineEsplorazioneDto
            {
                inizio = param.inizio,
                quartiere = param.quartiere,
                compito = param.compito,
                esecuzione = param.esecuzione,
                corretto = param.corretto,
                tempoImpiegato = param.tempoImpiegato,
                ombra = param.ombra,
                prospettiva = param.prospettiva,
                finale = param.finale
            })
            .Then(res => {
                Debug.Log("LEVEL BACKEND: End exploration request success: " + JsonUtility.ToJson(res, true));
                onSuccess();
            })
            .Catch(err => onFailure(err));
        }
        else
        {
            onSuccess();
        }
    }

    public void EnterRoom(Action<EsercizioStanzaDto> onSuccess, Action<Exception> onFailure)
    {
        if (isActive)
        {
            Debug.Log("LEVEL BACKEND: Enter room request, sessione: " + sessionId + ", funzione: " + island.ToString() + ", canale: " + roomChannel.ToString());

            SetHeader();
            RestClient.Post<EsercizioStanzaDto>(basePath + string.Format(enterRoomPath, sessionId), new InitEsercizioDto
            {
                funzioneEsecutiva = island.ToString(),
                canale = roomChannel.ToString()
            })
            .Then(res => {
                Debug.Log("LEVEL BACKEND: Enter room request success: " + JsonUtility.ToJson(res, true));
                roomLevel = (RoomLevel)Enum.Parse(typeof(RoomLevel), res.livello, false);
                exerciseId = res.idEsercizio;
                startTime = res.inizio;
                onSuccess(res);
            })
            .Catch(err => onFailure(err));
        }
        else
        {
            EsercizioStanzaDto res = new EsercizioStanzaDto();
            res.funzioneEsecutiva = island.ToString();
            res.canale = roomChannel.ToString();
            res.livello = RoomLevel.LEVEL_11.ToString();
            res.finito = false;
            res.idEsercizio = 0;
            res.inizio = 0;
            roomLevel = (RoomLevel)Enum.Parse(typeof(RoomLevel), res.livello, false);
            exerciseId = res.idEsercizio;
            startTime = res.inizio;
            onSuccess(res);
        }
    }

    public void ExitRoom(int exerciseTime, Score score, Action onSuccess, Action<Exception> onFailure)
    {
        if (isActive)
        {
            Debug.Log("LEVEL BACKEND: Exit room request, idEsercizio: " + exerciseId + ", inizio: " + startTime + ", durata: " + exerciseTime + ", livello: " + roomLevel.ToString() +
                      ", funzione: " + island.ToString() + ", canale: " + roomChannel.ToString() + ", stimoliCorretti: " + score.RightCounter +
                      ", stimoliErrati: " + score.WrongCounter + ", stimoliSaltati: " + score.MissedCounter);

            SetHeader();
            RestClient.Post<AvanzamentoEsercizioDto>(basePath + string.Format(exitRoomPath, exerciseId), new RisultatoEsercizioDto
            {
                inizio = startTime,
                durata = exerciseTime,
                numeroStimoliCorretti = score.RightCounter,
                numeroStimoliErrati = score.WrongCounter,
                numeroStimoliSaltati = score.MissedCounter,
                numeroCombo = score.ComboCounter
            })
            .Then(res => {
                Debug.Log("LEVEL BACKEND: Exit room request success: " + JsonUtility.ToJson(res, true));
                lastExercizeResult = (EndExercizeSaveResponse)Enum.Parse(typeof(EndExercizeSaveResponse), res.esito, false);
                onSuccess();
            })
            .Catch(err => onFailure(err));
        }
        else
        {
            lastExercizeResult = EndExercizeSaveResponse.IN_CORSO;
            onSuccess();
        }
    }


    public void GetQuiz(Action<QuizDto> onSuccess, Action<Exception> onFailure) {
        if (isActive) {
            Debug.Log("LEVEL BACKEND: Get quiz request, funzione: " + island.ToString());

            SetHeader();
            RestClient.Get<QuizDto>(basePath + string.Format(quizPath, island.ToString()))
            .Then(res => {
                Debug.Log("LEVEL BACKEND: Get quiz request success: " + JsonUtility.ToJson(res, true));
                onSuccess(res); 
            })
            .Catch(err => onFailure(err));
        }
        else {
            QuizDto res = new QuizDto();
            res.id = 0;
            res.funzioneEsecutiva = island.ToString();
            res.situazioni = new List<string>();
            res.situazioni.Add("Risposta errata 1");
            res.situazioni.Add("Risposta errata 2");
            res.corretta = "Risposta corretta";
            onSuccess(res);
        }
    }

    public void SaveQuiz(int id, int timeOccured, string answer, Action<PunteggioQuizDto> onSuccess, Action<Exception> onFailure) {
        if (isActive) {
            Debug.Log("LEVEL BACKEND: Save quiz request, sessione: " + sessionId + ", inizio: " + id + ", tempo: " + timeOccured + ", risposta: " + answer);

            SetHeader();
            RestClient.Post<PunteggioQuizDto>(basePath + string.Format(saveQuizPath, sessionId), new RisultatoQuizDto {
                idQuiz = id,
                tempoImpiegato = timeOccured,
                risposta = answer
            })
            .Then(res => {
                Debug.Log("LEVEL BACKEND: Save quiz request success: " + JsonUtility.ToJson(res, true));
                onSuccess(res);
            })
            .Catch(err => onFailure(err));
        }
        else {
            PunteggioQuizDto res = new PunteggioQuizDto();
            res.score = 10;
            onSuccess(res);
        }
    }

    public void MustShowVideo(Action<bool> onSuccess, Action<Exception> onFailure) {
        if (isActive) {
            Debug.Log("LEVEL BACKEND: Must show video request, sessione: " + sessionId + ", funzione: " + island.ToString());

            SetHeader();
            RestClient.Get<BooleanDto>(basePath + string.Format(mustShowVideoPath, island.ToString()))
            .Then(res => {
                Debug.Log("LEVEL BACKEND: Must show video request success: " + JsonUtility.ToJson(res, true));
                bool result = false;
                if (res != null) {
                    result = res.value;
                }
                onSuccess(result);
            })
            .Catch(err => onFailure(err));
        }
        else {
            onSuccess(false);
        }
    }

    public void SaveAreaCompleted(string user, Action onSuccess, Action<Exception> onFailure)
    {
        if (isActive)
        {
            Debug.Log("LEVEL BACKEND: Save area completed request, user: " + user + ", funzione: " + island.ToString());

            SetHeader();
            RestClient.Post(basePath + string.Format(saveAreaCompletedPath, island.ToString()), new EmptyDto())
            .Then(res => {
                Debug.Log("LEVEL BACKEND: Save area completed request success: " + JsonUtility.ToJson(res, true));
                onSuccess();
            })
            .Catch(err => onFailure(err));
        }
        else
        {
            onSuccess();
        }
    }

    //public void ResetAreaCompleted(string user, Action onSuccess, Action<Exception> onFailure) {
    //    if (isActive) {
    //        Debug.Log("LEVEL BACKEND: Reset area completed request, user: " + user);

    //        SetHeader();
    //        RestClient.Post(basePath + resetAreaCompletedPath, new ResetAreaCompletataDto {
    //            nomeUtente = user,
    //        })
    //        .Then(res => {
    //            Debug.Log("LEVEL BACKEND: Reset area completed request success: " + JsonUtility.ToJson(res, true));
    //            onSuccess();
    //        })
    //        .Catch(err => onFailure(err));
    //    }
    //    else {
    //        onSuccess();
    //    }
    //}

    public void MediaLiteracy(Action<InizioMediaLiteracyDto> onSuccess, Action<Exception> onFailure)
    {
        if (isActive)
        {
            Debug.Log("LEVEL BACKEND: MediaLiteracy request");

            SetHeader();
            RestClient.Get<InizioMediaLiteracyDto>(basePath + mediaLiteracyPath)
            .Then(res => {
                Debug.Log("LEVEL BACKEND: MediaLiteracy request success: " + JsonUtility.ToJson(res, true));

                onSuccess(res);
            })
            .Catch(err => onFailure(err));
        }
        else
        {
            InizioMediaLiteracyDto res = new InizioMediaLiteracyDto();
            res.inizio = 0;
            onSuccess(res);
        }
    }

    public void EndMediaLiteracy(long inizio, Room9.MediaLiteracyResult param, Action onSuccess, Action<Exception> onFailure)
    {
        if (isActive)
        {
            Debug.Log("LEVEL BACKEND: End Media Literacy request, sessione: " + sessionId + " params: " + JsonUtility.ToJson(param));

            SetHeader();
            RestClient.Post(basePath + string.Format(endMediaLiteracyPath, sessionId), new RisultatiMediaLiteracyDto
            {
                inizio = inizio,
                knowledgeEstimate = param.KnowledgeEstimate,
                knowledgeQuizzesAnswers = param.KnowledgeQuizzesAnswers,
                outlineNeededHelp = param.OutlineNeededHelp,
                relevancePoints = param.RelevancePoints,
                relevanceQuizzesAnswers = param.RelevanceQuizzesAnswers,
                snippetMinutes = param.SnippetMinutes,
                productionMinutes = param.ProductionMinutes,
                revisionMinutes = param.RevisionMinutes,
                timeEstimatesMet = param.TimeEstimatesMet,
                snippetEstimates = param.SnippetEstimates,
                snippetEstimatesMet = param.SnippetEstimatesMet,
                productionPicks = param.ProductionPicks,
                productionPoints = param.ProductionPoints,
                productionResult = param.ProductionResult,
                tagsCorrectAnswers = param.TagsCorrectAnswers,
                titleCorrectAnswer = param.TitleCorrectAnswer,
                submissionQuizzesAnswers = param.SubmissionQuizzesAnswers,
                submissionTipsAnswers = param.SubmissionTipsAnswers
            })
            .Then(res => {
                Debug.Log("LEVEL BACKEND: End Media Literacy request success: " + JsonUtility.ToJson(res, true));
                onSuccess();
            })
            .Catch(err => onFailure(err));
        }
        else
        {
            onSuccess();
        }
    }
}
