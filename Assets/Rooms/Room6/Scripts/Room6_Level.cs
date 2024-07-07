using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine;

namespace Room6
{
    public class Room6_Level : MonoBehaviour
    {
        private Room6_GameController _controller;

        public int LEVEL_TIME; // Tempo per il livello
        public int MISS_TIME;  // Tempo prima che venga segnalato il miss

        public bool UsesBlackboard;

        public Transform BlackboardCameraPosition;
        public Transform NoBlackboardCameraPosition;

        // Numero di oggetti corretti da creare, numero di tentativi falliti e riusciti
        protected int numRandomObjToSpawn = 0;
        protected int numFailTrial = 0;
        protected int numGoodTrial = 0;

        // Campo che indica se è stata riascoltata la sequenza di immagini
        protected bool helpReceived = false;

        // Campo che indica se c'è stato un errore nella selezione degli oggetti da parte del giocatore
        private bool goodSelection = true;

        // Indice dell'oggetto corrente da selezionare nella lista degli oggetti corretti in caso di selezione ordinata
        protected int currentSelectionIndex = 0;

        // Liste contenenti riferimenti agli oggetti da selezionare ed eventuali di disturbo
        [HideInInspector]
        public List<VisualObject> correctObjList = new List<VisualObject>();
        [HideInInspector]
        public List<VisualObject> wrongObjList = new List<VisualObject>();
        [HideInInspector]
        public List<VisualObject> completeObjList = new List<VisualObject>();
        [HideInInspector]
        public List<GameObject> selectedObjList = new List<GameObject>();

        private List<VisualObject> unorderedSelectObjList = null;

        private bool _timerEndedOnce;

        // Numero minimo di oggetti da creare per ciascun livello
        protected int numMinRandomToSpawn = 0;

        // Indice del messaggio da visualizzare per il livello corrente
        protected int currentMessageIndex = 0;

        public GameObject nextBehaviour;

        protected BigElloSaysConfig currentMessage;

        [ColorUsage(false)]
        public Color SelectedColor;

        public ParticleSystem _feedbackParticles;

        protected Room6_GameController controller => _controller ??= Room6_CommonRefs.controller;

        protected virtual void Start()
        {
            controller.timer.SetTime(LEVEL_TIME);
            controller.Bomb.timeoutSeconds = MISS_TIME;
            controller.Bomb.onExplode.AddListener(AnswerTimerEnded);

            controller.relistenButton.onClick.RemoveAllListeners();
            controller.relistenButton.onClick.AddListener(Relisten);

            SelectedColor = new Color(SelectedColor.r, SelectedColor.g, SelectedColor.b, 1);
        }

        public void InitializeView()
        {
            controller.interactionHandler.SetVisiblePanel(UsesBlackboard);

            var cameraTransform = Camera.main.transform;
            cameraTransform.position = UsesBlackboard ? BlackboardCameraPosition.position : NoBlackboardCameraPosition.position;
            cameraTransform.rotation = UsesBlackboard ? BlackboardCameraPosition.rotation : NoBlackboardCameraPosition.rotation;
        }

        /* Metodo che mostra gli elementi di gioco e fa partire il timer */
        protected void Init()
        {
            controller.energyBar.SetActive(true);

            controller.sceneObj.SetActive(true);

            controller.timer.SetActivation(true);
        }

        /* Handler per la gestione del tasto Help, va legata a quest'ultimo */
        private void Relisten()
        {
            StartCoroutine(RelistenHandler());
        }

        protected virtual IEnumerator RelistenHandler()
        {
            yield break;
        }

        /* Metodo che seleziona e mostra gli oggetti, viene chiamata in caso di miss, doppio errore o 
         * scelta corretta fino a che non scade il tempo */
        public virtual IEnumerator ShowItems(bool refresh)
        {
            yield break;
        }

        /* Metodo che estrae un certo numero di oggetti casuali corretti e distraenti non ancora estratti
         * scegliendo tra oggetti generici, divisi per categorie o alfanumerici */
        public virtual void SelectCasualObjects(int numCorrectObjects, int numWrongObjects)
        {
        }

        /* Metodo che gestisce la selezione di un oggetto di scena */
        protected virtual void TargetSelectedHandler(GameObject selectedObject)
        {
        }

        /* Callback per gestire il miss di una risposta perché finito il tempo */
        protected virtual void AnswerTimerEnded()
        {
            controller.objHandler.selectionAllowed = false;

            controller.timer.SetActivation(false);
            controller.Bomb.gameObject.SetActive(false);

            if (_timerEndedOnce)
            {
                controller.score.MissedCounter++;
                controller.PlayWrong();
                StartCoroutine(ShowItems(true));
                return;
            }

            _timerEndedOnce = true;

            helpReceived = false;
            controller.SetHelpButtonState(false);

            selectedObjList.ForEach(obj => obj.GetComponent<ColorHighlighter>().DeEmphasize());

            ResetSelection();

            currentSelectionIndex = 0;

            StartCoroutine(ShowItems(false));
        }

        public void StartGame()
        {
            controller.timer.SetActivation(true);

            controller.Bomb.gameObject.SetActive(true);
            controller.Bomb.timeoutSeconds = MISS_TIME;
            controller.Bomb.SetVisibleWhenReaches(5);
            controller.Bomb.StartCountdown();

            controller.objHandler.selectionAllowed = true;

            StartCoroutine(ShowItems(true));
        }

        /* Metodo che integra le operazioni iniziali da svolgere per mostrare gli oggetti (ShowItems) */
        protected void InitShowItems()
        {
            controller.Bomb.gameObject.SetActive(false);
            controller.Bomb.timeoutSeconds = MISS_TIME;
            controller.Bomb.SetVisibleWhenReaches(5);

            correctObjList.Clear();
            wrongObjList.Clear();
            completeObjList.Clear();

            _timerEndedOnce = false;

            ResetSelection();

            controller.objHandler.ClearScreenObjects();
        }

        /* Metodo che integra le operazioni finali da svolgere per mostrare gli oggetti (ShowItems) */
        protected virtual void EndShowItems()
        {

            controller.objHandler.SpawnObjects(completeObjList);

            controller.PlayAnswerTimeChime();

            controller.Bomb.gameObject.SetActive(true);
            controller.Bomb.StartCountdown();
            controller.timer.SetActivation(true);
        }

        /* Metodo per la gestione del fine gioco */
        public void LevelEnd()
        {
            controller.Bomb.gameObject.SetActive(false);

            controller.sceneObj.SetActive(false);
            controller.energyBar.SetActive(false);

            controller.objHandler.ClearScreenObjects();

            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            nextBehaviour.SetActive(true);
        }

        protected void Shuffle(List<VisualObject> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = UnityEngine.Random.Range(0, n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        protected virtual bool ObjectsLimitReached()
        {
            return false;
        }

        protected void ResetSelection()
        {
            selectedObjList.Clear();
            goodSelection = true;
            unorderedSelectObjList = null;
        }

        /* Metodo che gestisce la selezione errata di una sequenza di elementi senza possibilità di aiuto */
        protected void WrongSelection()
        {
            controller.PlayWrong();

            numFailTrial++;
            if (numFailTrial + controller.NUM_TRIAL_TO_WIN > controller.NUM_TRIAL_PER_SPAN)
            {
                if (numRandomObjToSpawn > numMinRandomToSpawn)
                    numRandomObjToSpawn--;
                numGoodTrial = 0;
                numFailTrial = 0;
            }

            helpReceived = false;
            controller.SetHelpButtonState(false);

            controller.score.WrongCounter++;
        }

        protected void ClickFeedback(GameObject selectedObject)
        {
            controller.PlaySelection();

            selectedObject.GetComponent<ColorHighlighter>().Emphasize();

            _feedbackParticles.transform.position = selectedObject.transform.position;
            _feedbackParticles.Play();
        }

        /* Metodo che gestisce la selezione errata di una sequenza di elementi con aiuto ancora disponibile*/
        protected void GetHelp()
        {
            ResetSelection();
            controller.PlayWrong();
            helpReceived = true;
            controller.SetHelpButtonState(true);

            controller.objHandler.ClearScreenObjects();
            controller.objHandler.SpawnObjects(completeObjList);
        }

        /* Metodo che gestisce la selezione corretta di una sequenza di elementi */
        protected void GoodSelection()
        {
            helpReceived = false;
            controller.SetHelpButtonState(false);

            numGoodTrial++;
            if (numGoodTrial == controller.NUM_TRIAL_TO_WIN)
            {
                numGoodTrial = 0;
                numFailTrial = 0;
                if (!ObjectsLimitReached())
                    numRandomObjToSpawn++;
            }

            controller.score.RightCounter++;
        }

        /* Metodo che ricerca un elemento selezionato in una lista in modo ordinato. Restituisce:
        *      1: se è stato trovato un elemento corrispondente e non ci sono elementi seguenti da esaminare
        *      0: se è stato trovato un elemento e ne rimangono altri
        *     -1: se non è stato trovato l'elemento
        */
        protected int SearchOrdered(GameObject selectedObject, List<VisualObject> targetList, bool reverse)
        {
            selectedObjList.Add(selectedObject);

            if (OutOfRange(targetList, currentSelectionIndex) || selectedObject.name != targetList[currentSelectionIndex].prefab.name)
                goodSelection = false;

            if (reverse)
                currentSelectionIndex--;
            else
                currentSelectionIndex++;

            if (selectedObjList.Count == targetList.Count)
            {
                if (goodSelection)
                    return 1;
                return -1;
            }
            return 0;
        }

        private static bool OutOfRange<T>(List<T> array, int index)
            => index < 0 || index >= array.Count;

        /* Metodo che ricerca un elemento selezionato in una lista. Restituisce:
         *      1: se è stato trovato un elemento corrispondente e non ci sono più elementi nella lista da cercare
         *      0: se è stato trovato un elemento e ne rimangono altri
         *     -1: se non è stato trovato l'elemento
         */
        protected int SearchUnOrdered(GameObject selectedObject, List<VisualObject> targetList)
        {
            if (unorderedSelectObjList == null)
                unorderedSelectObjList = new List<VisualObject>(targetList);

            bool found = false;
            int position = -1;
            for (int i = 0; i < unorderedSelectObjList.Count; i++)
            {
                if (selectedObject.name == unorderedSelectObjList[i].prefab.name)
                {
                    found = true;
                    position = i;
                    break;
                }
            }

            if (!found)
            {
                goodSelection = false;
                // Rimuovo comunque l'oggetto per tenere il conto di quanto manca alla fine della selezione
                unorderedSelectObjList.RemoveAt(0);
            }
            else
            {
                unorderedSelectObjList.RemoveAt(position);
            }

            if (unorderedSelectObjList.Count == 0)
            {
                if (goodSelection)
                    return 1;
                return -1;
            }
            return 0;
        }

        void Update()
        {
            if (controller.timer.itIsEnd)
            {
                LevelEnd();
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mPos = Input.mousePosition;
                // Nel caso il cursore non sia confinato nella view controllo che il click 
                // sia avvenuto all'interno della schermata di gioco
                if (mPos.x > 0 && mPos.y > 0 && mPos.x < Screen.width && mPos.y < Screen.height && controller.objHandler.selectionAllowed)
                {
                    Ray ray = Camera.main.ScreenPointToRay(mPos);
                    if (Physics.Raycast(ray, out var hitInfo))
                    {
                        if (hitInfo.transform.gameObject.CompareTag("Target"))
                        {
                            if (!selectedObjList.Contains(hitInfo.transform.gameObject))
                                TargetSelectedHandler(hitInfo.transform.gameObject);
                        }
                    }
                }
            }
        }
    }
}
