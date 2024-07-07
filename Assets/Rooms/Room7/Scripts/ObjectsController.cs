using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace Room7
{

    internal class Slot
    {

        internal Insieme type;
        internal List<ObjectInfo> slotObjects;

        private List<ObjectInfo> availableObjects;

        internal Slot(Insieme type, List<ObjectInfo> objects)
        {
            this.type = type;
            availableObjects = new List<ObjectInfo>(objects);
            slotObjects = new List<ObjectInfo>();
        }

        internal void FilterAvailablesForNumber(int number)
        {
            if (type == Insieme.Numero)
            {
                availableObjects = availableObjects.FindAll(obj => obj.numberOfObjects == number);
            }
            else
            {
                availableObjects = availableObjects.FindAll(obj => obj.numberOfObjects != number);
            }
        }

        internal void FilterAvailablesForShape(Forma shape)
        {
            availableObjects = availableObjects.FindAll(obj => {
                return !obj.avoidShapes.Contains(shape);
            });
            if (type == Insieme.Forma)
            {
                availableObjects = availableObjects.FindAll(obj => obj.shape == shape);
            }
            else
            {
                availableObjects = availableObjects.FindAll(obj => obj.shape != shape);
            }
        }

        internal void FilterAvailablesForColor(Colore color)
        {
            availableObjects = availableObjects.FindAll(obj => !obj.avoidColors.Contains(color));
            if (type == Insieme.Colore)
            {
                availableObjects = availableObjects.FindAll(obj => obj.color == color);
            }
            else
            {
                availableObjects = availableObjects.FindAll(obj => obj.color != color);
            }
        }

        internal void FilterAvailablesForFunction(Funzione function)
        {
            availableObjects = availableObjects.FindAll(obj => !obj.avoidFunctions.Contains(function));
            if (type == Insieme.Funzione)
            {
                availableObjects = availableObjects.FindAll(obj => obj.function == function);
            }
            else
            {
                availableObjects = availableObjects.FindAll(obj => obj.function != function);
            }
        }

        internal ObjectInfo GetRandomObject()
        {
            if (availableObjects.Count == 0)
            {
                return null;
            }
            else
            {
                return availableObjects[UnityEngine.Random.Range(0, availableObjects.Count)];
            }
        }

        internal void RemoveAvailableObject(ObjectInfo objToRemove)
        {
            availableObjects = availableObjects.FindAll(obj => obj != objToRemove);
        }

        internal void AddObject(ObjectInfo objToAdd)
        {
            slotObjects.Add(objToAdd);
        }

        internal List<string> GetObjectNames()
        {
            return slotObjects.ConvertAll(new Converter<ObjectInfo, string>(obj => obj.name));
        }

    }

    public class ObjectsController : MonoBehaviour
    {

        const int TOTALSTEPS = 6;

        public float timeout = 40f;
        public FeedbackController feedback;
        public ResourcesManager resourcesManager;

        public Transform slotsRoot;
        public Transform objectsRoot;
        public GameObject slotPrefab;
        public Button confirmButton;
        public Timer timer;
        public Score score;
        public Bomb bomb;
        public UnityEvent onGameOver = new UnityEvent();

        [SerializeField]
        private GameObject[] slotCountTo3DRoot; 

        private bool mustConfirm;
        private bool extraObjects;
        private bool useBomb;
        private bool bombIsAlwaysVisible;
        private bool useLabels;

        private ObjectsDatabase objectsDatabase;
        List<Slot> slots = new List<Slot>();
        List<DropRectangle> containers = new List<DropRectangle>();
        private int objectsNum = 0;
        private int currentStep = 0;
        private bool autoCheckEnabled = false;

        private int GetRandomNumber()
        {
            return UnityEngine.Random.Range(2, 5);
        }

        private Colore GetRandomColor()
        {
            return (Colore)UnityEngine.Random.Range(1, Enum.GetValues(typeof(Colore)).Length);
        }

        private Forma GetRandomShape()
        {
            return (Forma)UnityEngine.Random.Range(1, Enum.GetValues(typeof(Forma)).Length);
        }

        private Funzione GetRandomFunction()
        {
            return (Funzione)UnityEngine.Random.Range(1, Enum.GetValues(typeof(Funzione)).Length);
        }

        private int GetNumberOfObjectsForStep(int step, int numberOfSlots)
        {
            return (extraObjects ? step + 4 : numberOfSlots * 2);
        }
        private List<Insieme> GetSlotTypesForStep(int step)
        {
            List<Insieme> slotTypes = new List<Insieme>();
            if (step != 2)
            {
                slotTypes.Add(Insieme.Colore);
            }
            if (step % 2 == 1 || step == 6)
            {
                slotTypes.Add(Insieme.Forma);
            }
            if (step > 1)
            {
                slotTypes.Add(Insieme.Funzione);
            }
            if (step == 5)
            {
                slotTypes.Add(Insieme.Numero);
            }
            if (step % 2 == 0)
            {
                slotTypes.Add(Insieme.Dimensione);
            }
            return slotTypes;
        }

        private List<Slot> PrepareDataForStep(int step, List<ObjectInfo> objects)
        {
            List<Insieme> slotTypes = GetSlotTypesForStep(step);
            int numberOfObjects = GetNumberOfObjectsForStep(step, slotTypes.Count);
            List<Slot> slots;
            int loops = 0;
            bool found = false;
            do
            {
                loops++;
                int number;
                Forma shape;
                Colore color;
                Funzione function;
                slots = slotTypes.ConvertAll(new Converter<Insieme, Slot>(slotType => { return new Slot(slotType, objects); }));
                if (slotTypes.Contains(Insieme.Numero))
                {
                    number = GetRandomNumber();
                    slots.ForEach(slot => slot.FilterAvailablesForNumber(number));
                }
                if (slotTypes.Contains(Insieme.Forma))
                {
                    shape = GetRandomShape();
                    slots.ForEach(slot => slot.FilterAvailablesForShape(shape));
                }
                if (slotTypes.Contains(Insieme.Colore))
                {
                    color = GetRandomColor();
                    slots.ForEach(slot => slot.FilterAvailablesForColor(color));
                }
                if (slotTypes.Contains(Insieme.Funzione))
                {
                    function = GetRandomFunction();
                    slots.ForEach(slot => slot.FilterAvailablesForFunction(function));
                }
                int count = 0;
                while (count < numberOfObjects)
                {
                    foreach (Slot slot in slots)
                    {
                        ObjectInfo obj = slot.GetRandomObject();
                        if (obj == null)
                        {
                            count = numberOfObjects;
                            break;
                        }
                        slot.AddObject(obj);
                        slots.ForEach(s => s.RemoveAvailableObject(obj));
                        count++;
                        if (count == numberOfObjects)
                        {
                            found = true;
                            break;
                        }
                    }
                }
            } while (!found && loops < 100);
            return slots;
        }

        public void PrepareGame(bool minorIs2, bool majorIs0, bool bombAlwaysVisible = false)
        {
            //mustConfirm = minorIs2;
            mustConfirm = true; // Design change
            extraObjects = minorIs2;
            useBomb = minorIs2;
            bombIsAlwaysVisible = bombAlwaysVisible;
            useLabels = majorIs0;
            objectsDatabase = new ObjectsDatabase();
            // Debug.Log(objectsDatabase.objects.Count + " oggetti disponibili");
            if (useBomb)
            {
                bomb.timeoutSeconds = timeout;
                bomb.onExplode.AddListener(onExplode);
            }
            PrepareStep(1);
        }

        private List<Vector2> GetSlotsPositions(int count)
        {
            List<Vector2> result = new List<Vector2>();
            switch (count)
            {
                case 2:
                    result.Add(new Vector2(-460, -140));
                    result.Add(new Vector2(360, -80));
                    break;
                case 3:
                    result.Add(new Vector2(-640, -130));
                    result.Add(new Vector2(440, -150));
                    result.Add(new Vector2(-70, -70));
                    break;
                default:
                    result.Add(new Vector2(-700, 100));
                    result.Add(new Vector2(550, 82));
                    result.Add(new Vector2(-300, -160));
                    result.Add(new Vector2(180, -200));
                    break;
            }
            return result;
        }

        private void PrepareStep(int step)
        {
            RemoveChildren(slotsRoot);
            RemoveChildren(objectsRoot);
            currentStep = step;
            slots = PrepareDataForStep(step, objectsDatabase.objects);
            int objectsForEachSlot = 0;
            slots.ForEach(slot => {
                if (slot.slotObjects.Count > objectsForEachSlot)
                {
                    objectsForEachSlot = slot.slotObjects.Count;
                }
            });
            List<Vector2> positions = GetSlotsPositions(slots.Count);
            List<string> objectNames = new List<string>();
            List<string> smallObjects = new List<string>();
            containers.Clear();
            for (int i = 0; i < slots.Count; i++)
            {
                Slot slot = slots[i];
                GameObject slotObj = CreateSlot(slot.type.ToString(), positions[i]);
                List<string> slotObjNames = slots[i].GetObjectNames();
                objectNames.AddRange(slotObjNames);
                if (slot.type.Equals(Insieme.Dimensione))
                {
                    smallObjects.AddRange(slotObjNames);
                }
                DropRectangle container = slotObj.GetComponentInChildren<DropRectangle>();
                
                //slot disponibili = uno per ogni oggetto
                container.numberOfSlots = objectsForEachSlot*slots.Count;

                //questo era il comportamento di prima con gli spazi limitati
                //container.numberOfSlots = objectsForEachSlot + 1; //workaround per bugfix caso limite: spazio aggiuntivo per permettere di fare swap tra oggetti nel caso tutti fossero stati gia' messi nelle aree 
                
                container.onAcceptObject.AddListener(ContainerReceivedObject);
                containers.Add(container);
            }

            for (int i = 0; i < slotCountTo3DRoot.Length; i++)
            {
                GameObject root = slotCountTo3DRoot[i];

                if (root == null) continue;

                root.SetActive(i == slots.Count);
            }

            objectsNum = objectNames.Count;
            CreateObjects(ShuffleList(objectNames), smallObjects, containers);
            if (mustConfirm)
            {
                confirmButton.gameObject.SetActive(true);
                confirmButton.interactable = false;
            }
            else
            {
                autoCheckEnabled = true;
            }
            bomb.gameObject.SetActive(useBomb);
        }

        public void StartGame()
        {
            if (useBomb)
            {
                if (!bombIsAlwaysVisible) bomb.SetVisibleWhenReaches(5);
                bomb.StartCountdown();
            }
        }

        private void rightAnswer()
        {
            feedback.rightAnswerFeedback();
            score.RightCounter++;
        }

        private void wrongAnswer()
        {
            feedback.wrongAnswerFeedback();
            score.WrongCounter++;
        }

        private void missedAnswer()
        {
            feedback.wrongAnswerFeedback();
            score.MissedCounter++;
        }

        private void onExplode()
        {
            missedAnswer();
            Invoke(nameof(NextStep), bomb.explosionSeconds);
        }

        private void RemoveChildren(Transform parent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }

        private List<string> ShuffleList(List<string> list)
        {
            List<string> result = new List<string>();
            list.ForEach(item => result.Insert(UnityEngine.Random.Range(0, result.Count + 1), item));
            return result;
        }

        private void CreateObjects(List<string> objectNames, List<string> smallObjects, List<DropRectangle> containers)
        {
            int ObjDistance = (11 - objectNames.Count) * 10;
            const int ObjWidth = 100;
            Vector2 position = new Vector2(-((objectNames.Count - 1) * (ObjDistance + ObjWidth)) / 2, 360);
            objectNames.ForEach(objName => {
                Vector3 size = (smallObjects.Contains(objName) ? 0.70f : 1.30f) * Vector3.one;
                CreateObject(objName, position, size, containers);
                position.x += ObjWidth + ObjDistance;
            });
        }

        private GameObject CreateObject(string objName, Vector2 position, Vector3 size, List<DropRectangle> containers)
        {
            GameObject newObj = new GameObject();
            newObj.name = objName;
            newObj.transform.SetParent(objectsRoot);
            newObj.transform.localScale = size;
            newObj.AddComponent<RectTransform>().anchoredPosition = position;
            newObj.AddComponent<Image>().sprite = resourcesManager.GetSpriteByName("ObjectSprites/" + objName);
            Draggable draggable = newObj.AddComponent<Draggable>();
            draggable.canReposition = true;
            draggable.containers = new List<DropRectangle>(containers);
            return newObj;
        }

        private GameObject CreateSlot(string name, Vector2 position)
        {
            GameObject slotObj = Instantiate(slotPrefab);
            slotObj.name = name;
            slotObj.transform.SetParent(slotsRoot);
            slotObj.transform.localScale = Vector3.one;
            slotObj.GetComponent<RectTransform>().anchoredPosition = position;

            var label = slotObj.GetComponentInChildren<TextMeshProUGUI>();
            label.transform.parent.gameObject.SetActive(useLabels);

            if (useLabels)
            {
                label.text = name.ToUpper();
            }
            return slotObj;
        }

        private bool AllObjectsPlaced()
        {
            int total = 0;
            containers.ForEach(container => { total += container.getObjects().Count; });
            return (total == objectsNum);
        }

        void Update()
        {
            if (timer.itIsEnd)
            {
                EndExercise();
            }
        }

        void ContainerReceivedObject()
        {
            if (AllObjectsPlaced())
            {
                if (mustConfirm)
                {
                    confirmButton.interactable = true;
                }
                if (autoCheckEnabled)
                {
                    autoCheckEnabled = false;
                    Confirm();
                }
            }
        }

        public void Confirm()
        {
            confirmButton.gameObject.SetActive(false);
            if (isRightAnswer(useLabels))
            {
                rightAnswer();
            }
            else
            {
                wrongAnswer();
            }
            
            bomb.StopCountdown();
            
            Invoke(nameof(NextStep), 1f);
        }

        private bool isRightAnswer(bool sameOrder)
        {
            List<string> rightAnswer = new List<string>();
            slots.ForEach(slot => {
                List<string> objNames = slot.GetObjectNames();
                objNames.Sort();
                rightAnswer.Add(String.Join(",", objNames));
            });
            List<string> answerGiven = new List<string>();
            containers.ForEach(container => {
                List<string> objNames = container.getObjects();
                objNames.Sort();
                answerGiven.Add(String.Join(",", objNames));
            });
            if (!sameOrder)
            {
                rightAnswer.Sort();
                answerGiven.Sort();
            }
            string rightAnswerStr = String.Join("/", rightAnswer);
            string answerGivenStr = String.Join("/", answerGiven);
            return (answerGivenStr.Equals(rightAnswerStr));
        }

        public void NextStep()
        {
            if (useBomb)
            {
                bomb.StopCountdown();
            }
            if (currentStep < TOTALSTEPS)
            {
                PrepareStep(currentStep + 1);
                StartGame();
            }
            else
            {
                EndExercise();
            }
        }

        private void EndExercise()
        {
            foreach (Draggable draggable in objectsRoot.GetComponentsInChildren<Draggable>())
            {
                draggable.enabled = false;
            }
            onGameOver.Invoke();
        }

    }

}