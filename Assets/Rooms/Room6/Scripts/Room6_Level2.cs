using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using UnityEngine;

namespace Room6
{
    public class Room6_Level2 : Room6_Level
    {
        // Used in level 2 2, enables the "select all the objects in order" first phase
        public bool dualPhase;

        public int MAX_OBJS_PER_STAGE = 4; // Numero di elementi distraenti massimi oltre il minimo definito per stage

        public BigElloSaysConfig dualCategoryConfig;
        public MessageSystem messageSystem;

        // Increased when spawned "wrong" items are MAX_OBJS_PER_STAGE
        // 0 = 1 correct item, 1 category
        // 1 = 1 correct item, 2 categories
        // Note: the original behaved differently with a 2 item 1 category intermediate stage
        private int numStage = 0;

        // Indici delle categorie selezionate nell'esercizio 2
        private List<int> selectedCategoryIndexList = new List<int>();

        private int totalObjNum;

        // Campo che indica se ci si trova nella prima fase di una risposta (in caso di doppia fase come es. 2.2)
        private bool firstPhase = true;

        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();

            // Controllo che in ogni categoria ci siano almeno 2 elementi
            foreach (Category cat in controller.categoryObjList)
                if (cat.objectList.Length < 2)
                    Debug.LogError("Troppi pochi elementi nelle categorie");

            // Verifico che sia presente un numero sufficiente di elementi
            totalObjNum = 0;
            foreach (Category cat in controller.categoryObjList)
                totalObjNum += cat.objectList.Length;

            // Changed from the original (had 2 max correct items)
            const int MAX_CORRECT_ITEMS = 1;

            int numObj = MAX_CORRECT_ITEMS + numMinRandomToSpawn + MAX_OBJS_PER_STAGE;

            if (numObj > totalObjNum)
                Debug.LogError("Troppi pochi elementi tra tutte le categorie");

            if (dualPhase)
                numMinRandomToSpawn = 3;
            else
                numMinRandomToSpawn = 4;

            numRandomObjToSpawn = numMinRandomToSpawn;

            Init();
            StartGame();
        }

        protected override IEnumerator RelistenHandler()
        {
            controller.SetHelpButtonState(false);
            controller.Bomb.gameObject.SetActive(false);
            controller.Bomb.timeoutSeconds = MISS_TIME;
            controller.Bomb.SetVisibleWhenReaches(5);
            yield return StartCoroutine(controller.objHandler.ListenSelectedCategories(selectedCategoryIndexList, false));
            controller.Bomb.gameObject.SetActive(true);
            controller.Bomb.StartCountdown();
        }

        protected override void AnswerTimerEnded()
        {
            firstPhase = true;
            base.AnswerTimerEnded();
        }

        /* Metodo che calcola se è stato raggiunto il limite di oggetti utilizzabili per lo stage attuale */
        protected override bool ObjectsLimitReached()
        {
            // Changed from the original, now we need to always find 1 object
            const int CORRECT_ITEMS_COUNT = 1;
            return numRandomObjToSpawn + CORRECT_ITEMS_COUNT >= totalObjNum;
        }

        public override IEnumerator ShowItems(bool refresh)
        {
            if (controller.timer.itIsEnd)
            {
                LevelEnd();
                yield break;
            }

            if (refresh)
            {
                InitShowItems();

                selectedCategoryIndexList.Clear();

                // Changed from the original, now we need to always find 1 object
                int numObj = 1;

                // Changed from the original, double category is on stage 1 just in level 2 1
                if (!dualPhase && numStage > 0) SelectCategory(numObj);

                SelectCategory(numObj);

                // Seleziono oggetti per distrarre
                SelectCasualObjects(0, numRandomObjToSpawn);

                completeObjList.AddRange(wrongObjList);
                Shuffle(completeObjList);

                foreach (var obj in correctObjList)
                {
                    var objCategory = controller.categoryObjList.FirstOrDefault(c => c.objectList.Contains(obj));

                    if (objCategory == default)
                    {
                        Debug.LogError($"Object {obj.name} has no category");
                        continue;
                    }

                    int highestIndexOfSameCategory;
                    for (highestIndexOfSameCategory = completeObjList.Count - 1;
                         highestIndexOfSameCategory >= 0;
                         highestIndexOfSameCategory--)
                    {
                        var otherObjCategory = controller.categoryObjList.FirstOrDefault(c =>
                            c.objectList.Contains(completeObjList[highestIndexOfSameCategory]));

                        if (otherObjCategory == objCategory) break;
                    }

                    completeObjList.Insert(Random.Range(highestIndexOfSameCategory + 1, completeObjList.Count), obj);
                }
            }
            else controller.objHandler.SetCurrentObjectsVisible(false);

            yield return StartCoroutine(controller.objHandler.ListenSelectedCategories(selectedCategoryIndexList, false));
            yield return StartCoroutine(controller.objHandler.ListenObjects(completeObjList, false));

            currentSelectionIndex = 0;

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

        public override void SelectCasualObjects(int numCorrectObjects, int numWrongObjects)
        {
            int catIndex, objIndex;
            VisualObject[] objArray;

            if (correctObjList.Count + wrongObjList.Count + numCorrectObjects + numWrongObjects > totalObjNum)
            {
                Debug.LogError("Troppi oggetti da visualizzare richiesti");
                return;
            }

            for (int i = 0; i < numCorrectObjects; i++)
            {
                bool found = false;
                do
                {
                    catIndex = UnityEngine.Random.Range(0, controller.categoryObjList.Length);

                    objArray = controller.categoryObjList[catIndex].objectList;

                    objIndex = UnityEngine.Random.Range(0, objArray.Length);

                    if (!wrongObjList.Contains(objArray[objIndex]) && !correctObjList.Contains(objArray[objIndex]))
                    {
                        found = true;
                        correctObjList.Add(objArray[objIndex]);
                    }

                } while (!found);
            }

            for (int i = 0; i < numWrongObjects; i++)
            {
                bool found = false;
                do
                {
                    catIndex = UnityEngine.Random.Range(0, controller.categoryObjList.Length);

                    objArray = controller.categoryObjList[catIndex].objectList;

                    objIndex = UnityEngine.Random.Range(0, objArray.Length);

                    if (!wrongObjList.Contains(objArray[objIndex]) && !correctObjList.Contains(objArray[objIndex]))
                    {
                        found = true;
                        wrongObjList.Add(objArray[objIndex]);
                    }

                } while (!found);
            }
        }

        protected override void TargetSelectedHandler(GameObject selectedObject)
        {
            int complete = TargetSelectedLogic(selectedObject);

            if (complete == -1)
            {
                StartCoroutine(ShowItems(true));

            }
            else if (complete == 1)
            {
                // Changed from the original, now we have 2 stages (1 and 2 categories)
                if (numRandomObjToSpawn - numMinRandomToSpawn == MAX_OBJS_PER_STAGE && numStage < 1)
                {
                    numStage++;
                    numRandomObjToSpawn = numMinRandomToSpawn;

                    controller.Bomb.StopCountdown();
                    controller.Bomb.gameObject.SetActive(false);
                    controller.timer.activation = false;

                    messageSystem.oneShotRead.AddListener(OnDualCategoryWarningRead);
                    messageSystem.ShowMessage(dualCategoryConfig);

                }
                else
                {
                    if (numRandomObjToSpawn - numMinRandomToSpawn == MAX_OBJS_PER_STAGE && numStage == 1)
                        numRandomObjToSpawn--;
                    StartCoroutine(ShowItems(true));
                }
            }
        }

        private void OnDualCategoryWarningRead()
        {
            messageSystem.oneShotRead.RemoveListener(OnDualCategoryWarningRead);

            StartGame();
        }

        /// <returns>1 for correct answer, 0 for incomplete, -1 for wrong.</returns>
        protected int TargetSelectedLogic(GameObject selectedObject)
        {
            int result;

            if (dualPhase)
            {
                result = firstPhase ? SearchOrdered(selectedObject, completeObjList, false) :
                        SearchUnOrdered(selectedObject, correctObjList);
            }
            else result = SearchUnOrdered(selectedObject, correctObjList);

            ClickFeedback(selectedObject);

            if (result == 1)
            {
                controller.PlayGood();

                if (dualPhase && firstPhase)
                {
                    firstPhase = false;
                    selectedObjList.ForEach(go => go.GetComponent<Highlighter>().DeEmphasize());
                    selectedObjList.Clear();
                    return 0;
                }

                GoodSelection();

                firstPhase = true;

                return 1;
            }

            if (result != -1) return 0;

            if (helpReceived)
            {
                WrongSelection();
                return -1;
            }

            GetHelp();
            currentSelectionIndex = 0;

            return 0;
        }

        public void SelectCategory(int numObjs)
        {
            int catIndex;
            do
            {
                catIndex = UnityEngine.Random.Range(0, controller.categoryObjList.Length);
            } while (selectedCategoryIndexList.Contains(catIndex));

            if (numObjs <= controller.categoryObjList[catIndex].objectList.Length)
            {
                for (int i = controller.categoryObjList[catIndex].objectList.Length - 1; i > controller.categoryObjList[catIndex].objectList.Length - 1 - numObjs; i--)
                    correctObjList.Add(controller.categoryObjList[catIndex].objectList[i]);
                selectedCategoryIndexList.Add(catIndex);
            }
            else
            {
                Debug.LogError("Troppi oggetti richiesti");
            }
        }
    }
}
