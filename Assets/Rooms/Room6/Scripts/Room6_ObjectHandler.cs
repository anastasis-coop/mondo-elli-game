using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Room6 {

    public enum VisualObjListType { GENERIC, ALPHANUM };

    public enum ListenCategoryRequest { CATEGORY, SELECTED_CATEGORIES, ALL_CATEGORIES };

    public class Room6_ObjectHandler : MonoBehaviour {
        public bool selectionAllowed = true;

        private Room6_GameController controller;
        private Room6_InteractionHandler interactionHandler;

        // Ripiani dove posizionare gli oggetti
        public GameObject plane1;
        public GameObject plane2;

        // Distanza dal piano alla quale vengono instanziati gli oggetti
        public float distance = 5;

        // Punti di minimo dei piani e altezze minime a cui creare gli oggetti
        private float planeXMin1;
        private float spawnHeight1;
        private float planeXMin2;
        private float spawnHeight2;

        // Lista con tutti i riferimenti ai GameObject creati
        private List<GameObject> spawnedObjList = new();

        void Awake() {
            // Inizializzo le variabili di supporto per il calcolo della posizione degli oggetti
            planeXMin1 = plane1.GetComponent<Renderer>().bounds.min.x;
            spawnHeight1 = plane1.GetComponent<Renderer>().bounds.max.y + distance;
            planeXMin2 = plane2.GetComponent<Renderer>().bounds.min.x;
            spawnHeight2 = plane2.GetComponent<Renderer>().bounds.max.y + distance;
        }

        void Start() {
            controller = gameObject.GetComponent<Room6_GameController>();
            interactionHandler = gameObject.GetComponent<Room6_InteractionHandler>();
        }

        public void ClearScreenObjects() {
            // Rimuovo eventuali vecchi oggetti a schermo
            for (int i = spawnedObjList.Count - 1; i >= 0; i--)
                Destroy(spawnedObjList[i]);

            spawnedObjList.Clear();
        }

        /* Funzione che presenta tutte le categorie */
        public IEnumerator ListenAllCategories() {

            selectionAllowed = false;

            // Mostro le immagini associate agli oggetti riproducendone il nome
            for (int i = 0; i < controller.categoryObjList.Length; i++) {

                interactionHandler.SetImagePanel(controller.categoryObjList[i].image);

                if (!interactionHandler.IsPanelDown)
                {
                    interactionHandler.PanelDown();
                    yield return new WaitUntil(interactionHandler.IsPanelNotMoving);
                }

                var op = controller.categoryObjList[i].localizedAudioDescription.LoadAssetAsync();

                yield return op;

                controller.mainSource.clip = op.Result;
                controller.mainSource.Play();
                
                yield return new WaitUntil(() => !controller.mainSource.isPlaying);
                yield return new WaitForSeconds(1f);
            }

            // Alzo il pannello
            interactionHandler.PanelUp();

            // Aspetto finché il pannello non sia in alto
            yield return new WaitUntil(interactionHandler.IsPanelNotMoving);

            selectionAllowed = true;
        }

        /* Funzione che presenta solo alcune categorie selezionate usando una lista di indici che
         * fanno riferimento alla lista di categorie presente nel controller */
        public IEnumerator ListenSelectedCategories(List<int> categoryIndexList, bool rememberLastTwo) {

            selectionAllowed = false;

            // Mostro le immagini associate agli oggetti riproducendone il nome
            for (int i = 0; i < categoryIndexList.Count; i++) {

                int catIndex = categoryIndexList[i];

                if (i < 0 || i >= controller.categoryObjList.Length) {
                    Debug.LogError("Indice " + i + " della categoria non valido");
                    yield break;
                }

                var op = (rememberLastTwo ? controller.categoryObjList[catIndex].localizedDoubleSelectionClip
                    : controller.categoryObjList[catIndex].localizedSelectionClip).LoadAssetAsync();

                yield return op;

                AudioClip remclip = op.Result;

                interactionHandler.SetImagePanel(controller.categoryObjList[catIndex].image);

                if (!interactionHandler.IsPanelDown)
                {
                    interactionHandler.PanelDown();
                    yield return new WaitUntil(interactionHandler.IsPanelNotMoving);
                }
                
                controller.mainSource.clip = remclip;
                controller.mainSource.Play();

                yield return new WaitUntil(() => !controller.mainSource.isPlaying);

                // Attendo tra una categoria e la successiva
                if (i != categoryIndexList.Count - 1)
                    yield return new WaitForSeconds(1f);
            }

            // Alzo il pannello
            interactionHandler.PanelUp();

            // Aspetto finché il pannello non sia in alto
            yield return new WaitUntil(interactionHandler.IsPanelNotMoving);

            selectionAllowed = true;
        }

        /* Funzione che presenta una lista di oggetti */
        public IEnumerator ListenObjects(List<VisualObject> objList, bool showPanel) {

            selectionAllowed = false;

            // Mostro le immagini associate agli oggetti riproducendone il nome
            for (int i = 0; i < objList.Count; i++) {

                if (showPanel)
                {
                    interactionHandler.SetImagePanel(objList[i].image);

                    if (!interactionHandler.IsPanelDown)
                    {
                        interactionHandler.PanelDown();
                        yield return new WaitUntil(interactionHandler.IsPanelNotMoving);
                    }
                }

                var op = objList[i].localizedAudioDescription.LoadAssetAsync();

                yield return op;

                controller.mainSource.clip = op.Result;
                controller.mainSource.Play();


                yield return new WaitUntil(() => !controller.mainSource.isPlaying);
                controller.mainSource.Stop();
                yield return new WaitForSeconds(0.5f);
            }

            if (showPanel) {
                // Alzo il pannello
                interactionHandler.PanelUp();

                // Aspetto finché il pannello non sia in alto
                yield return new WaitUntil(interactionHandler.IsPanelNotMoving);
            }

            selectionAllowed = true;
        }

        /* Funzione che restituisce i bounds di un oggetto, se l'oggetto non ha già 
         * disponibili queste info (per esempio perché è l'oggetto padre e le componenti
         * disegnate sono nei figli) considera i bounds dei figli per costruire quelli complessivi */
        private Bounds getObjBounds(Transform t) {
            Bounds objBounds;
            bool init = false;
            Renderer render = t.GetComponent<Renderer>();
            if (!render) {
                objBounds = new Bounds();
                Renderer[] childRenders = t.GetComponentsInChildren<Renderer>();
                foreach (Renderer childRend in childRenders) {
                    if (!init) {
                        objBounds = childRend.bounds;
                        init = true;
                    } else {
                        objBounds.Encapsulate(childRend.bounds);
                    }
                }
            }
            else {
                objBounds = render.bounds;
            }

            return objBounds;
        }

        /* Funzione che crea oggetti sui ripiani da selezionare a partire da una lista passata come parametro */
        public void SpawnObjects(List<VisualObject> objList) {
            // La lista verrà modificata, per evitare problemi ne faccio una copia
            List<VisualObject> clonedCompleteObjList = new List<VisualObject>(objList);

            // Seleziono il numero di elementi per piano
            int numObjects = clonedCompleteObjList.Count;
            int planeCount1, planeCount2;

            // Scelgo quanti oggetti posizionare sui due piani
            if (numObjects % 2 == 0) {
                planeCount1 = numObjects / 2;
                planeCount2 = numObjects / 2;
            }
            else {
                int coin = UnityEngine.Random.Range(0, 2);
                if (coin == 0) {
                    planeCount1 = numObjects / 2 + 1;
                    planeCount2 = numObjects / 2;
                }
                else {
                    planeCount1 = numObjects / 2;
                    planeCount2 = numObjects / 2 + 1;
                }
            }

            // Calcolo il numero di intervalli per posizionare gli oggetti su ciascun piano
            float step1 = plane1.GetComponent<Renderer>().bounds.size.x / (planeCount1 + 1);
            float step2 = plane2.GetComponent<Renderer>().bounds.size.x / (planeCount2 + 1);

            for (int i = 0; i < planeCount1; i++) {
                int selected = UnityEngine.Random.Range(0, clonedCompleteObjList.Count);

                Bounds b = getObjBounds(clonedCompleteObjList[selected].prefab.transform.GetChild(0));
                float height = spawnHeight1 + b.extents.y;
                GameObject obj = Instantiate(clonedCompleteObjList[selected].prefab, new Vector3(planeXMin1 + step1 * (i + 1), height, plane1.transform.position.z), clonedCompleteObjList[selected].prefab.transform.rotation);
                obj.name = clonedCompleteObjList[selected].prefab.name;

                spawnedObjList.Add(obj);

                clonedCompleteObjList.RemoveAt(selected);
            }

            for (int i = 0; i < planeCount2; i++) {
                int selected = UnityEngine.Random.Range(0, clonedCompleteObjList.Count);

                Bounds b = getObjBounds(clonedCompleteObjList[selected].prefab.transform.GetChild(0));
                float height = spawnHeight2 + b.extents.y;
                GameObject obj = Instantiate(clonedCompleteObjList[selected].prefab, new Vector3(planeXMin2 + step2 * (i + 1), height, plane2.transform.position.z), clonedCompleteObjList[selected].prefab.transform.rotation);
                obj.name = clonedCompleteObjList[selected].prefab.name;

                spawnedObjList.Add(obj);

                clonedCompleteObjList.RemoveAt(selected);
            }
        }

        public void SetCurrentObjectsVisible(bool visible)
        {
            foreach (var o in spawnedObjList)
                o.SetActive(visible);
        }
    }
}
