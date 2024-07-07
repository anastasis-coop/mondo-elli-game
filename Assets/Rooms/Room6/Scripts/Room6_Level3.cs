using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Room6
{
    public class Room6_Level3 : Room6_Level {
        
        public int ITERATIONS_PER_QUESTION = 2; // Numero di oggetti aggiuntivi da raggiungere per passare allo stage successivo

        public int NUM_MIN_CORRECT_OBJS = 3; // Numero di oggetti corretti di partenza

        public int NUM_WRONG_OBJ = 2; // Numero di oggetti distraenti

        private int numStage = 0; 
        
        public BigElloSaysConfig[] MessageList;

        protected override void Start() {
            
            base.Start();
            
            controller.messagePanel.oneShotRead.RemoveAllListeners();
            controller.messagePanel.oneShotRead.AddListener(MessageReadHandler);

            numMinRandomToSpawn = NUM_MIN_CORRECT_OBJS;
            numRandomObjToSpawn = numMinRandomToSpawn;

            ShowMessage();
        }
        
        public void ShowMessage() {
            controller.objHandler.selectionAllowed = false;

            controller.timer.SetActivation(false);
            controller.Bomb.gameObject.SetActive(false);

            if (currentMessageIndex >= MessageList.Length)
                Debug.LogError("Numero di messaggi non sufficienti per questo livello");

            currentMessage = MessageList[currentMessageIndex];
            controller.messagePanel.ShowMessage(currentMessage);
        }

        protected override bool ObjectsLimitReached() {
            int numObjs = numRandomObjToSpawn + NUM_WRONG_OBJ;

            if (numObjs <= controller.alfanumObjList.Length)
                return false;

            return true;
        }

        protected override IEnumerator RelistenHandler() {
            controller.SetHelpButtonState(false);
            controller.Bomb.gameObject.SetActive(false);
            controller.Bomb.timeoutSeconds = MISS_TIME;
            controller.Bomb.SetVisibleWhenReaches(5);
            yield return StartCoroutine(controller.objHandler.ListenObjects(completeObjList, false));
            controller.Bomb.gameObject.SetActive(true);
            controller.Bomb.StartCountdown();
        }

        public override IEnumerator ShowItems(bool refresh) {
            if (controller.timer.itIsEnd) {
                LevelEnd();
                yield break;
            }

            if (refresh)
            {
                InitShowItems();

                if (numStage >= MessageList.Length)
                    Debug.LogError("Numero di stage non previsto");

                switch (numStage) {
                    case 0:
                        // Solo pari
                        SelectAlfanumObjects(numRandomObjToSpawn, correctObjList, true, false, false, false);
                        SelectAlfanumObjects(NUM_WRONG_OBJ, wrongObjList, false, true, true, true);
                        break;
                    case 1:
                        // Solo dispari
                        SelectAlfanumObjects(numRandomObjToSpawn, correctObjList, false, true, false, false);
                        SelectAlfanumObjects(NUM_WRONG_OBJ, wrongObjList, true, false, true, true);
                        break;
                    case 2:
                        // Solo vocali
                        SelectAlfanumObjects(numRandomObjToSpawn, correctObjList, false, false, true, false);
                        SelectAlfanumObjects(NUM_WRONG_OBJ, wrongObjList, true, true, false, true);
                        break;
                    case 3:
                        // Solo consonanti
                        SelectAlfanumObjects(numRandomObjToSpawn, correctObjList, false, false, false, true);
                        SelectAlfanumObjects(NUM_WRONG_OBJ, wrongObjList, true, true, true, false);
                        break;
                    case 4:
                        // Vocali e pari
                        SelectAlfanumObjects(numRandomObjToSpawn, correctObjList, true, false, true, false);
                        SelectAlfanumObjects(NUM_WRONG_OBJ, wrongObjList, false, true, false, true);
                        break;
                    case 5:
                        // Consonanti e dispari
                        SelectAlfanumObjects(numRandomObjToSpawn, correctObjList, false, true, false, true);
                        SelectAlfanumObjects(NUM_WRONG_OBJ, wrongObjList, true, false, true, false);
                        break;
                    case 6:
                        // Consonanti e pari
                        SelectAlfanumObjects(numRandomObjToSpawn, correctObjList, true, false, false, true);
                        SelectAlfanumObjects(NUM_WRONG_OBJ, wrongObjList, false, true, true, false);
                        break;
                }

                completeObjList.AddRange(correctObjList);
                completeObjList.AddRange(wrongObjList);
                Shuffle(completeObjList);
            } else controller.objHandler.SetCurrentObjectsVisible(false);

            // Riproduco gli audio
            yield return StartCoroutine(controller.objHandler.ListenObjects(completeObjList, false));

            if (refresh) EndShowItems();
            else controller.objHandler.SetCurrentObjectsVisible(true);
        }

        public override void SelectCasualObjects(int numCorrectObjects, int numWrongObjects) {
            int objIndex;

            if (correctObjList.Count + wrongObjList.Count + numCorrectObjects + numWrongObjects > controller.alfanumObjList.Length) {
                Debug.LogError("Troppi oggetti da visualizzare richiesti");
                return;
            }

            for (int i = 0; i < numCorrectObjects; i++) {
                bool found = false;
                do {
                    objIndex = UnityEngine.Random.Range(0, controller.alfanumObjList.Length);

                    if (!wrongObjList.Contains(controller.alfanumObjList[objIndex]) && !correctObjList.Contains(controller.alfanumObjList[objIndex])) {
                        found = true;
                        correctObjList.Add(controller.alfanumObjList[objIndex]);
                    }

                } while (!found);
            }

            for (int i = 0; i < numWrongObjects; i++) {
                bool found = false;
                do {
                    objIndex = UnityEngine.Random.Range(0, controller.alfanumObjList.Length);

                    if (!wrongObjList.Contains(controller.alfanumObjList[objIndex]) && !correctObjList.Contains(controller.alfanumObjList[objIndex])) {
                        found = true;
                        wrongObjList.Add(controller.alfanumObjList[objIndex]);
                    }

                } while (!found);
            }
        }

        protected override void TargetSelectedHandler(GameObject selectedObject) {
            int complete = TargetSelectedLogic(selectedObject, correctObjList);

            if (complete == -1) {
                StartCoroutine(ShowItems(true));

            } else if (complete == 1) {
                if (numRandomObjToSpawn - numMinRandomToSpawn == ITERATIONS_PER_QUESTION && numStage < MessageList.Length) {
                    numStage++;
                    numRandomObjToSpawn = numMinRandomToSpawn;
                    ShowMessage();
                
                } else {
                    // Se ho raggiunto il massimo degli oggetti e lo stage finale rimango nelle stesse condizioni
                    if (numRandomObjToSpawn - numMinRandomToSpawn == ITERATIONS_PER_QUESTION && numStage == MessageList.Length)
                        numRandomObjToSpawn--;
                    StartCoroutine(ShowItems(true));
                }
            }
        }

        protected int TargetSelectedLogic(GameObject selectedObject, List<VisualObject> targetList) {
            int result = SearchUnOrdered(selectedObject, targetList);

            ClickFeedback(selectedObject);
            
            if (result == 1) 
            {
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

            return 0;
        }

        /* Metodo per gestire l'avvenuta lettura di un messaggio */
        public void MessageReadHandler() {

            if (currentMessageIndex == 0)
                Init();

            if (controller.timer.itIsEnd) {
                LevelEnd();
                return;
            }

            controller.timer.SetActivation(true);
            controller.Bomb.gameObject.SetActive(true);
            controller.Bomb.timeoutSeconds = MISS_TIME;
            controller.Bomb.SetVisibleWhenReaches(5);
            controller.Bomb.StartCountdown();

            // Passo al messaggio successivo solo se ho superato un messaggio di livello e non il miss
            if (currentMessageIndex < MessageList.Length && currentMessage == MessageList[currentMessageIndex])
                currentMessageIndex++;

            controller.objHandler.selectionAllowed = true;

            StartCoroutine(ShowItems(true));
        }

        /* Funzione che estrae un numero di oggetti alfanumerici non ancora estratti e li inserisce in una lista passata 
         * come parametro specificando se gli oggetti possano essere pari, dispari, vocali o consonanti */
        public void SelectAlfanumObjects(int numObjects, List<VisualObject> objList, bool even, bool odd, bool vocal, bool consonant) {
            int maxElems = 0;

            if (even)
                maxElems += 5;
            if (odd)
                maxElems += 5;
            if (vocal)
                maxElems += 5;
            if (consonant)
                maxElems += controller.alfanumObjList.Length - 15;

            if (objList.Count > 0 || numObjects > maxElems)
                Debug.LogError("Troppi elementi richiesti o lista di destinazione non vuota");

            for (int i = 0; i < numObjects; i++) {
                bool found = false;
                do {
                    int objIndex = UnityEngine.Random.Range(0, controller.alfanumObjList.Length);

                    string objName = controller.alfanumObjList[objIndex].name;

                    bool isVocal = false, isEven = false;
                    bool isNumber = int.TryParse(objName, out int num);

                    if (isNumber) {
                        if (num % 2 == 0)
                            isEven = true;
                    
                    } else {
                        objName = objName.Trim().ToLower();
                        if (objName.Equals("a") || objName.Equals("e") || objName.Equals("i") || objName.Equals("o") || objName.Equals("u"))
                            isVocal = true;
                    }

                    if (!isNumber && (vocal && isVocal || consonant && !isVocal) || isNumber && (even && isEven || odd && !isEven)) {
                        if (!correctObjList.Contains(controller.alfanumObjList[objIndex]) && !wrongObjList.Contains(controller.alfanumObjList[objIndex])) {
                            found = true;
                            objList.Add(controller.alfanumObjList[objIndex]);
                        }
                    }
                } while (!found);
            }
        }
    }
}
