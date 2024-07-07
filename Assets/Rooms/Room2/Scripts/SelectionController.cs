using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Room2
{
    [System.Serializable]
    public class SelectedObjectCallBack : UnityEvent<int, string>
    {
    }

    public class SelectionController : MonoBehaviour
    {

        public enum SelectionMode
        {
            Objects22, Objects31, Objects32
        }

        public ResourcesManager resourcesManager;

        public SelectionMode mode;
        public GameObject SelectionPanel;
        [SerializeField] private List<Collider> containersColliders;
        public List<GameObject> objectsParents;
        public List<GameObject> targetsPrefabs;
        public List<GameObject> distractorsPrefabs;
        public SelectedObjectCallBack OnSelectedObject = new SelectedObjectCallBack();

        private List<GameObject> objects;

        public void LoadTargetsAndDistractors(string path)
        {
            targetsPrefabs = resourcesManager.GetPrefabsByFolderName(path + "/Targets");
            distractorsPrefabs = resourcesManager.GetPrefabsByFolderName(path + "/Distractors");
        }

        void Start()
        {
            objects = new List<GameObject>();
        }

        public void ShowNextGroup(string target)
        {
            foreach (GameObject obj in objects)
            {
                DestroyImmediate(obj);
            }
            
            //SelectionPanel.SetActive(true);
            EnableContainersColliders(true);
            
            List<GameObject> choosenPrefabs = new List<GameObject>();
            List<GameObject> availableDistractors = new List<GameObject>(distractorsPrefabs);
            for (int i = 0; i < 3; i++)
            {
                GameObject choosenPrefab = availableDistractors[Random.Range(0, availableDistractors.Count)];
                availableDistractors.Remove(choosenPrefab);
                choosenPrefabs.Add(choosenPrefab);
            }
            GameObject targetPrefab = targetsPrefabs.Find(obj=>obj.name == target);
            choosenPrefabs.Insert(Random.Range(0, 4), targetPrefab);
            objects = new List<GameObject>();
            for (int i = 0; i < 4; i++)
            {
                GameObject prefab = choosenPrefabs[i];
                GameObject newObj = Instantiate(prefab);
                newObj.transform.parent = objectsParents[i].transform;
                newObj.name = prefab.name;
                newObj.transform.localPosition = Vector3.zero;
                newObj.transform.localEulerAngles = Vector3.zero;
                objects.Add(newObj);
            }
        }

        private void EnableContainersColliders(bool enable)
        {
            foreach (Collider collider in containersColliders)
            {
                collider.enabled = enable;
            }
        }

        public void ObjectSelected(int index)
        {
            if (objects.Count > index)
            {
                OnSelectedObject.Invoke(index, objects[index].name);
            }
            
            EnableContainersColliders(false);
        }

        public void HidePanel()
        {
            SelectionPanel.SetActive(false);
        }

    }
}
