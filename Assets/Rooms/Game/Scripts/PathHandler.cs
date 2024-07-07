using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Game.ArrowState;

namespace Game
{
    public class PathHandler : MonoBehaviour
    {
        private List<GroundHighlight> streetBlockList;
        private List<Color> streetColorList;

        private bool shadowEnabled = false;

        private int currentArrowIndex = 0; // Definisce l'indice della freccia corrente nella definizione del percorso

        private List<ArrowState> pathState = new List<ArrowState>();

        private GameController controller;
        private MovementHandler movementHandler;

        [SerializeField]
        private CodingUI codingUI;

        private Vector3 currentFlagPosition;

        void Start()
        {
            controller = GetComponent<GameController>();
            movementHandler = GetComponent<MovementHandler>();

            streetBlockList = new List<GroundHighlight>();
            streetColorList = new List<Color>();
        }

        /***** Funzioni per la gestione del percorso *****/

        //Moved here from GameController

        [SerializeField]
        private Color leftColor;
        [SerializeField]
        private Color rightColor;
        [SerializeField]
        private Color upColor;
        [SerializeField]
        private Color defaultColorH;
        public Color DefaultColorH => defaultColorH;

        private GroundHighlight CurrentGround
        {
            get => movementHandler.CurrentGround?.GetComponent<GroundHighlight>();
            set => movementHandler.CurrentGround = value?.gameObject;
        }

        public void ResetPath(bool neutral = true)
        {
            CodingSubPath currentSubPath = controller.CurrentSubPath;

            controller.player.transform.position = currentSubPath.Start.Value;
            controller.player.transform.forward = (Vector3)currentSubPath.StartDirection * CodingUtility.GRID_SIZE;

            CurrentGround = streetBlockList[0];

            controller.currentUnlightedColor = streetColorList[0];
            CurrentGround.SetColorH(defaultColorH);
            CurrentGround.HighlightOn();

            for (int i = 1; i < streetBlockList.Count; i++)
            {
                streetBlockList[i].SetColorH(neutral ? defaultColorH : streetColorList[i]);
                streetBlockList[i].HighlightOn();
            }

            controller.flag.SetActive(true);
        }

        public void ClearPath()
        {
            // Resetto l'appartenenza ad un percorso, il colore di default e l'highlight dei blocchi del percorso precedente
            for (int i = 0; i < streetBlockList.Count; i++)
            {
                streetBlockList[i].SetPathState(false);
                streetBlockList[i].HighlightOff();
                streetBlockList[i].SetColorH(defaultColorH);
            }

            // Resetto la lista di blocchi del percorso
            streetBlockList.Clear();
            streetColorList.Clear();
        }

        /*** Funzione che rimuove il percorso programmato precedente e mostra il successivo ***/
        public void ShowNextPath(bool neutral = true)
        {
            ClearPath();

            if (controller.currentSubPathIndex == controller.currentSubPaths.Length)
                return;

            CodingSubPath currentSubPath = controller.CurrentSubPath;

            // Sposto il giocatore nella direzione di partenza
            controller.player.transform.position = currentSubPath.Start.Value;
            controller.player.transform.forward = (Vector3)currentSubPath.StartDirection * CodingUtility.GRID_SIZE;

            // Sposto la bandiera sull'ultimo blocco del percorso
            currentFlagPosition = new Vector3(currentSubPath.End.Value.x, controller.flag.transform.position.y, currentSubPath.End.Value.z);
            controller.flag.transform.position = currentFlagPosition;
            controller.flag.SetActive(true);

            Vector3 dir = currentSubPath.StartDirection;
            int stepCount = currentSubPath.SuggestedPath.Length;

            for (int stepIndex = 0; stepIndex < stepCount; stepIndex++)
            {
                Vector3 pos = currentSubPath.SuggestedPath[stepIndex];
                Ray ray = new(pos + (Vector3.down * 20), Vector3.up);
                _ = Physics.Raycast(ray, out RaycastHit hit);
                GroundHighlight highlight = hit.collider.gameObject.GetComponent<GroundHighlight>();

                Color color;

                if (stepIndex == stepCount - 1)
                {
                    color = upColor;
                }
                else
                {
                    Vector3 next = currentSubPath.SuggestedPath[stepIndex + 1];
                    Vector3 nextDir = (next - pos).normalized;

                    if (dir == nextDir)
                    {
                        color = upColor;
                    }
                    else
                    {
                        float sign = Mathf.Sign(Vector3.SignedAngle(dir, nextDir, Vector3.up));

                        color = sign > 0 ? rightColor : leftColor;

                        dir = nextDir;
                    }
                }

                highlight.SetColorH(neutral ? defaultColorH : color);
                highlight.HighlightOn();
                highlight.SetPathState(true);

                streetBlockList.Add(highlight);
                streetColorList.Add(color);
            }
        }

        public void HighlightPath()
        {
            for (int i = 0; i < streetBlockList.Count; i++)
            {
                streetBlockList[i].SetColorH(streetColorList[i]);
                streetBlockList[i].HighlightOn();
            }
        }

        public string GetExpectedPath()
        {
            string result = "";

            // Alias definito per comodità, array contenente la sequenza di direzioni del percorso
            List<ArrowState> directions = CodingUtility.CodingSubPathToArrowState(controller.CurrentSubPath);

            foreach (ArrowState dir in directions)
            {
                if (dir == LEFT)
                    result += "l";
                else if (dir == RIGHT)
                    result += "r";
                else if (dir == UP)
                    result += "u";
                else
                    result += "n";
            }

            return result;
        }

        public string GetUserPath()
        {
            string result = "";

            for (int i = 0; i < currentArrowIndex; i++)
            {
                if (pathState[i] == LEFT)
                    result += "l";
                else if (pathState[i] == RIGHT)
                    result += "r";
                else if (pathState[i] == UP)
                    result += "u";
                else
                    result += "n";
            }

            return result;
        }

        public void CodingListChanged()
        {
            pathState = codingUI.GetArrowList();
            currentArrowIndex = pathState.Count - 1;

            if (shadowEnabled) MoveShadow();
        }

        /* Funzione che esegue la sequenza di mosse preprogrammate */
        public void PlaySequence()
        {
            DisableShadow();
            StartCoroutine(MoveSequenceCoroutine());
        }

        public void ResetArrows()
        {
            pathState.Clear();
            codingUI.ClearListSlots();
        }

        public void ResetArrowHighlights()
        {
            codingUI.HighlightListSlots(true);
        }

        /* Coroutine che contiene il codice per l'esecuzione delle mosse preprogrammate */
        IEnumerator MoveSequenceCoroutine()
        {
            // Se ho completato tutti i percorsi interrompo
            if (controller.currentSubPathIndex == controller.currentSubPaths.Length)
                yield break;

            // Se non sono specificate frecce notifico il percorso sbagliato e interrompo
            if (pathState.Count == 0)
            {
                GameEvent ev;
                ev.eventType = GameEventType.WRONG_PATH;
                ev.triggerObject = null;
                controller.HandleEvent(ev);
                yield break;
            }

            ArrowState lastArrow = NONE;

            // Eseguo le azioni specificate colorando le frecce corrispondenti
            for (int i = 0; i < pathState.Count; i++)
            {
                yield return new WaitUntil(movementHandler.IsNotMoving);

                if (i > 0) codingUI.HighlightListSlot(i - 1, false);

                codingUI.HighlightListSlot(i, true);

                ArrowState arrow = pathState[i];

                switch (arrow)
                {
                    case X2:
                        yield return MovePlayerByMultiplierArrow(lastArrow, 2);
                        break;
                    case X3:
                        yield return MovePlayerByMultiplierArrow(lastArrow, 3);
                        break;
                    case X4:
                        yield return MovePlayerByMultiplierArrow(lastArrow, 4);
                        break;
                    default:
                        yield return MovePlayerByArrow(arrow);
                        break;
                }

                lastArrow = arrow;
            }

            Vector3 end = controller.CurrentSubPath.End.Value;

            // Controllo che il percorso specificato sia corretto
            const int TILE_SIZE = 20; // TODO centralize constants
            bool ok = Vector3.Distance(controller.player.transform.position, end) < (TILE_SIZE * 0.5f);

            if (!ok)
            {
                controller.SaveExplorationResult(false, false);

                shadowEnabled = false;

                ResetArrowHighlights();

                GameEvent ev;
                ev.eventType = GameEventType.WRONG_PATH;
                ev.triggerObject = null;
                controller.HandleEvent(ev);
            }
            else
            {
                // Se è l'ultimo percorso faccio in modo di non richiedere un nuovo id al server
                controller.SaveExplorationResult(true, controller.currentSubPathIndex == controller.currentSubPaths.Length - 1);

                ResetArrows();

                shadowEnabled = false;

                GameEvent ev;
                ev.eventType = GameEventType.CORRECT_PATH;
                ev.triggerObject = null;
                controller.HandleEvent(ev);
            }
        }

        public void EnableShadow()
        {
            // Resetto la posizione dell'ombra
            controller.shadow.transform.position = new Vector3(0, 0, 0);
            controller.shadow.transform.rotation = Quaternion.identity;
            // Rendo visibile l'ombra
            controller.shadow.SetActive(true);
            // Cambio lo stato per informare le altre componenti che l'ombra è attiva
            shadowEnabled = true;
        }

        public void DisableShadow()
        {
            // Nascondo l'ombra
            controller.shadow.SetActive(false);
        }

        public bool IsShadowEnabled()
        {
            return shadowEnabled;
        }

        public void MoveShadow()
        {
            controller.shadow.transform.position = controller.player.transform.position;
            controller.shadow.transform.rotation = controller.player.transform.rotation;

            ArrowState lastArrow = NONE;

            foreach (ArrowState arrow in codingUI.GetArrowList())
            {
                switch (arrow)
                {
                    case X2:
                        MoveShadowByMultiplierArrow(lastArrow, 2);
                        break;
                    case X3:
                        MoveShadowByMultiplierArrow(lastArrow, 3);
                        break;
                    case X4:
                        MoveShadowByMultiplierArrow(lastArrow, 4);
                        break;
                    default:
                        MoveShadowByArrow(arrow);
                        break;
                }

                lastArrow = arrow;
            }
        }

        private IEnumerator MovePlayerByMultiplierArrow(ArrowState arrow, int multiplier)
        {
            if (arrow == NONE || arrow == X2 || arrow == X3 || arrow == X4)
                yield break;

            for (int i = 0; i < multiplier - 1; i++)
            {
                yield return MovePlayerByArrow(arrow);
            }
        }

        private IEnumerator MovePlayerByArrow(ArrowState arrow)
        {
            switch (arrow)
            {
                case UP:
                    movementHandler.MoveUp();
                    break;
                case LEFT:
                    movementHandler.MoveLeft();
                    break;
                case RIGHT:
                    movementHandler.MoveRight();
                    break;
                case DOUBLE_UP:
                    movementHandler.MoveUp(amount: 2);
                    break;
                case TRIPLE_UP:
                    movementHandler.MoveUp(amount: 3);
                    break;
            }

            yield return new WaitUntil(movementHandler.IsNotMoving);
        }

        private void MoveShadowByMultiplierArrow(ArrowState arrow, int multiplier)
        {
            if (arrow == NONE || arrow == X2 || arrow == X3 || arrow == X4)
                return;

            for (int i = 0; i < multiplier - 1; i++)
            {
                MoveShadowByArrow(arrow);
            }
        }

        private void MoveShadowByArrow(ArrowState arrow)
        {
            Transform shadow = controller.shadow.transform;
            switch (arrow)
            {
                case UP:
                    shadow.position += CodingUtility.GRID_SIZE * shadow.forward;
                    break;
                case LEFT:
                    shadow.Rotate(Vector3.up, -90, Space.Self);
                    break;
                case RIGHT:
                    shadow.Rotate(Vector3.up, 90, Space.Self);
                    break;
                case DOUBLE_UP:
                    shadow.position += 2 * CodingUtility.GRID_SIZE * shadow.forward;
                    break;
                case TRIPLE_UP:
                    shadow.position += 3 * CodingUtility.GRID_SIZE * shadow.forward;
                    break;
            }
        }
    }
}
