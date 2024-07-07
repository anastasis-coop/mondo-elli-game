using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Room8
{

    public enum Category { ANIMAL, RED, BROWN, WOOD, PINK, BLUE, GREEN, YELLOW, WHITE, PURPLE, GREY };

    [Serializable]
    public class CategorySound
    {
        public Category[] category;
        public bool matchAll;
        public LocalizedAudioClip localizedClip;
        public LocalizedString hint;
    }

    [Serializable]
    public class ScreenObject
    {
        public Category[] category;
        public GameObject screenElem;
    }

    public class Room8_Level1 : MonoBehaviour
    {
        public int LEVEL_TIME;

        public float SPAWN_INTERVAL;

        public int MOVEMENT_SPEED;

        public int MAX_SOUND_REP;

        public int BASE_CATEGORY_DURATION;

        public int MAX_CATEGORY_STEP;

        [SerializeField]
        private float categoryChangeGraceSeconds = 2;

        public GameObject[] spawnPoints;

        public ScreenObject[] screenObjList;

        public CategorySound[] categories;

        private CategorySound currentCategory;

        private List<GameObject> activeObjectsList;

        public GameObject nextBehaviour;

        private Room8_GameController1 controller;

        private int[] categoryIndexSequence;
        private int extractedCategories = 0;

        public bool visualHelp;

        public GameObject helpPanel;
        public TMP_Text helpText;

        private Plane[] cameraPlanes;

        private bool _categoryChangeGracePeriod;

        void Start()
        {
            controller = (Room8_GameController1)Room8_CommonRefs.controller;

            controller.levelTimer.SetTime(LEVEL_TIME);

            activeObjectsList = new List<GameObject>();
            categoryIndexSequence = new int[MAX_SOUND_REP];

            Init();

            if (visualHelp)
            {
                helpPanel.SetActive(true);
            }

            if (categories.Length < 2)
                Debug.LogError("Troppo poche categorie");

            cameraPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

            StartCoroutine(BeginSelection());
            StartCoroutine(SpawnScreenObjects());
        }

        private void Init()
        {
            controller.energyBar.SetActive(true);

            // controller.helpButton.SetActive(true);

            controller.levelTimer.SetActivation(true);
        }

        private void InsertElemLast(int[] array, int elem)
        {
            for (int i = 0; i < array.Length - 1; i++)
            {
                array[i] = array[i + 1];
            }

            array[array.Length - 1] = elem;
        }

        private bool RepeatedSequence(int[] array, int elem)
        {
            bool result = true;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] != elem)
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        public IEnumerator BeginSelection()
        {

            while (true)
            {
                yield return new WaitUntil(() => !controller.helpRequested);

                yield return new WaitUntil(() => !controller.voiceSource.isPlaying);

                int categoryIndex = UnityEngine.Random.Range(0, categories.Length);

                if (extractedCategories < MAX_SOUND_REP)
                {
                    categoryIndexSequence[extractedCategories] = categoryIndex;
                    extractedCategories++;

                }
                else
                {
                    while (RepeatedSequence(categoryIndexSequence, categoryIndex))
                        categoryIndex = UnityEngine.Random.Range(0, categories.Length);

                    InsertElemLast(categoryIndexSequence, categoryIndex);
                }

                Debug.Log("category " + categoryIndex);

                string s = "categoryList";
                for (int i = 0; i < extractedCategories; i++)
                {
                    s = s + " " + categoryIndexSequence[i];
                }
                Debug.Log(s);

                yield return new WaitUntil(() => !controller.voiceSource.isPlaying);

                var op = categories[categoryIndex].localizedClip.LoadAssetAsync();

                yield return op;

                currentCategory = categories[categoryIndex];

                if (visualHelp)
                {
                    if (categories[categoryIndex].hint != null)
                    {
                        helpText.text = categories[categoryIndex].hint.GetLocalizedString();
                    }
                    else
                    {
                        Debug.LogError("Hint per la categoria non impostato");
                    }
                }

                controller.voiceSource.clip = op.Result;
                controller.voiceSource.Play();

                yield return new WaitUntil(() => !controller.voiceSource.isPlaying);

                _categoryChangeGracePeriod = true;

                yield return new WaitForSeconds(categoryChangeGraceSeconds);

                _categoryChangeGracePeriod = false;

                int step = UnityEngine.Random.Range(1, MAX_CATEGORY_STEP + 1);

                yield return new WaitForSeconds(Mathf.Max((BASE_CATEGORY_DURATION * step) - categoryChangeGraceSeconds, 0));
            }
        }

        public IEnumerator SpawnScreenObjects()
        {
            while (true)
            {
                yield return new WaitUntil(() => !controller.helpRequested);

                int spawnPointIndex = UnityEngine.Random.Range(0, spawnPoints.Length);
                int objIndex = UnityEngine.Random.Range(0, screenObjList.Length);

                GameObject newObj = Instantiate(screenObjList[objIndex].screenElem, spawnPoints[spawnPointIndex].transform);
                newObj.transform.eulerAngles = new Vector3(0, UnityEngine.Random.Range(-25, 25));
                newObj.GetComponent<Room8_ScreenObject>().category = (Category[])screenObjList[objIndex].category.Clone();

                activeObjectsList.Add(newObj);

                yield return new WaitForSeconds(SPAWN_INTERVAL);
            }
        }

        private void Update()
        {
            if (controller.levelTimer.itIsEnd)
            {
                LevelEnd();
                return;
            }

            if (!controller.helpRequested)
            {
                List<GameObject> destroyList = new List<GameObject>();
                foreach (GameObject go in activeObjectsList)
                {
                    bool visible = GeometryUtility.TestPlanesAABB(cameraPlanes, go.GetComponent<BoxCollider>().bounds);
                    Room8_ScreenObject screenObject = go.GetComponent<Room8_ScreenObject>();

                    if (!visible && screenObject.enteredScreen)
                    {
                        bool selected = false;

                        if (currentCategory.matchAll)
                            selected = currentCategory.category.All(c => screenObject.category.Contains(c));
                        else
                            selected = currentCategory.category.Any(c => screenObject.category.Contains(c));

                        if (selected && !_categoryChangeGracePeriod)
                            controller.MissedAnswer();

                        destroyList.Add(go);

                    }
                    else if (visible && !screenObject.enteredScreen)
                    {
                        screenObject.enteredScreen = true;
                    }
                }

                activeObjectsList.RemoveAll(x => destroyList.Contains(x));

                for (int i = destroyList.Count - 1; i >= 0; i--)
                    Destroy(destroyList[i]);

                foreach (GameObject obj in activeObjectsList)
                {
                    obj.transform.position = obj.transform.position + new Vector3(MOVEMENT_SPEED * Time.deltaTime, 0, 0);
                }

                if (Input.GetMouseButtonDown(0))
                {
                    Vector3 mPos = Input.mousePosition;
                    // Nel caso il cursore non sia confinato nella view controllo che il click 
                    // sia avvenuto all'interno della schermata di gioco
                    if (mPos.x > 0 && mPos.y > 0 && mPos.x < Screen.width && mPos.y < Screen.height)
                    {
                        RaycastHit hitInfo = new RaycastHit();
                        Ray ray = Camera.main.ScreenPointToRay(mPos);

                        if (Physics.Raycast(ray, out hitInfo))
                        {
                            if (hitInfo.transform.gameObject.CompareTag("Target"))
                            {
                                TargetSelectedHandler(hitInfo.transform.gameObject);
                            }
                        }
                    }
                }
            }
        }

        public void TargetSelectedHandler(GameObject go)
        {
            activeObjectsList.Remove(go);

            var screenObject = go.GetComponent<Room8_ScreenObject>();

            if (screenObject == null) return;

            bool selected = false;

            if (currentCategory.matchAll)
                selected = currentCategory.category.All(c => screenObject.category.Contains(c));
            else
                selected = currentCategory.category.Any(c => screenObject.category.Contains(c));

            if (selected) controller.RightAnswer();
            else if (!_categoryChangeGracePeriod) controller.WrongAnswer();
            controller.ShowParticles(go.transform.position, selected);
            controller.bomb.gameObject.SetActive(false);
            Destroy(go);
        }

        public void LevelEnd()
        {
            StopAllCoroutines();

            if (helpPanel.activeSelf)
                helpPanel.SetActive(false);

            controller.energyBar.SetActive(false);
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            nextBehaviour.SetActive(true);
        }
    }
}
