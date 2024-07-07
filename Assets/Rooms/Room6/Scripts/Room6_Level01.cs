using System.Collections;
using UnityEngine;

namespace Room6 {
    public class Room6_Level01 : Room6_Level
    {
        public bool insertWrongObjs;
        public bool reverseSelection;
        public bool showImages;
        public VisualObjListType objType;

        public int NUM_WRONG_OBJ = 2; // Numero di oggetti distraenti

        private VisualObject[] objList;

        // Start is called before the first frame update
        protected override void Start() {

            base.Start();

            if (objType == VisualObjListType.ALPHANUM) {
                numMinRandomToSpawn = 3;
                objList = controller.alfanumObjList;
            }
            else if (objType == VisualObjListType.GENERIC) {
                numMinRandomToSpawn = 2;
                objList = controller.genericObjList;
            }

            numRandomObjToSpawn = numMinRandomToSpawn;

            Init();
            StartCoroutine(ShowItems(true));
        }

        protected override IEnumerator RelistenHandler() {
            controller.SetHelpButtonState(false);
            controller.Bomb.gameObject.SetActive(false);
            controller.Bomb.timeoutSeconds = MISS_TIME;
            controller.Bomb.SetVisibleWhenReaches(5);
            yield return StartCoroutine(controller.objHandler.ListenObjects(correctObjList, showImages));
            controller.Bomb.gameObject.SetActive(true);
            controller.Bomb.StartCountdown();
        }

        protected override bool ObjectsLimitReached() {
            int numObjs = numRandomObjToSpawn;
            if (insertWrongObjs)
                numObjs += NUM_WRONG_OBJ;

            if ((objType == VisualObjListType.ALPHANUM && numObjs < controller.alfanumObjList.Length) || (objType == VisualObjListType.GENERIC && numObjs < controller.genericObjList.Length))
                return false;

            return true;
        }

        public override IEnumerator ShowItems(bool refresh) {
            if (controller.timer.itIsEnd) {
                LevelEnd();
                yield break;
            }

            if (refresh)
            {
                InitShowItems();

                // Seleziono oggetti casuali e eventuali oggetti per distrarre
                if (insertWrongObjs)
                    SelectCasualObjects(numRandomObjToSpawn, NUM_WRONG_OBJ);
                else
                    SelectCasualObjects(numRandomObjToSpawn, 0);

                completeObjList.AddRange(correctObjList);
                completeObjList.AddRange(wrongObjList);

                if (reverseSelection && correctObjList.Count > 0)
                    currentSelectionIndex = correctObjList.Count - 1;
                else
                    currentSelectionIndex = 0;
            }
            else controller.objHandler.SetCurrentObjectsVisible(false);

            // Mostro le immagini e riproduco gli audio
            yield return StartCoroutine(controller.objHandler.ListenObjects(correctObjList, showImages));

            
            if (refresh) EndShowItems();
            else
            {
                controller.objHandler.SetCurrentObjectsVisible(true);
                controller.Bomb.gameObject.SetActive(true);
                controller.Bomb.timeoutSeconds = MISS_TIME;
                controller.Bomb.SetVisibleWhenReaches(5);
                controller.Bomb.StartCountdown();
                controller.timer.SetActivation(true);
            }
        }

        public override void SelectCasualObjects(int numCorrectObjects, int numWrongObjects) {
            int objIndex;

            if (numCorrectObjects + numWrongObjects + correctObjList.Count + wrongObjList.Count > objList.Length) {
                Debug.LogError("Troppi oggetti da visualizzare richiesti");
                return;
            }

            for (int i = 0; i < numCorrectObjects; i++) {
                bool found = false;
                do {
                    objIndex = UnityEngine.Random.Range(0, objList.Length);

                    if (!wrongObjList.Contains(objList[objIndex]) && !correctObjList.Contains(objList[objIndex])) {
                        found = true;
                        correctObjList.Add(objList[objIndex]);
                    }

                } while (!found);
            }

            for (int i = 0; i < numWrongObjects; i++) {
                bool found = false;
                do {
                    objIndex = UnityEngine.Random.Range(0, objList.Length);

                    if (!wrongObjList.Contains(objList[objIndex]) && !correctObjList.Contains(objList[objIndex])) {
                        found = true;
                        wrongObjList.Add(objList[objIndex]);
                    }

                } while (!found);
            }
        }

        protected override void TargetSelectedHandler(GameObject selectedObject) {
            int complete = TargetSelectedLogic(selectedObject);
            if (reverseSelection && complete == -1 && CheckWrongSelectionDir()) {
                ShowWrongDirectionMessage();
                return;
            }

            if (complete == -1 || complete == 1) 
            {
                controller.Bomb.StopCountdown();
                StartCoroutine(ShowItems(true));
            }
        }

        private void ShowWrongDirectionMessage() {
            controller.objHandler.selectionAllowed = false;

            controller.timer.SetActivation(false);
            controller.Bomb.gameObject.SetActive(false);

            currentMessage = controller.WrongSelectionDirMessage;

            // Mostro il messaggio di attenzione
            controller.messagePanel.ShowMessage(currentMessage);
        }

        /* Funzione che verifica se la selezione è stata fatta in ordine inverso rispetto a quella richiesta */
        private bool CheckWrongSelectionDir() {
            if (selectedObjList.Count != correctObjList.Count)
                return false;

            bool equal = true;
            for (int i = correctObjList.Count - 1; i > 0; i--) {
                if (correctObjList[i].prefab.transform.GetChild(0).name != selectedObjList[i].name) {
                    equal = false;
                    break;
                }
            }

            return equal;
        }

        protected int TargetSelectedLogic(GameObject selectedObject) {
            int result = SearchOrdered(selectedObject, correctObjList, reverseSelection);
            
            ClickFeedback(selectedObject);
            
            if (result == 1) {
                controller.PlayGood();
                GoodSelection();
                return 1;

            }

            if (result != -1) return 0;
            
            if (helpReceived) 
            {
                WrongSelection();
                return -1;
            }
                
            GetHelp();

            if (reverseSelection && correctObjList.Count > 0)
                currentSelectionIndex = correctObjList.Count - 1;
            else
                currentSelectionIndex = 0;

            return 0;
        }
    }
}

