using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace Room1
{
    [System.Serializable]
    public class SpawnerforLevel1232 : MonoBehaviour
    {
        [SerializeField]
        private MessageSystem messageSystem;

        [SerializeField]
        private BigElloSaysConfig halftimeConfig;

        [SerializeField]
        private Transform[] _spawnPoints;
        
        [SerializeField]
        private TargetScreen[] _screens;

        [SerializeField]
        private int _screensIn12 = 6;

        [SerializeField]
        private int _screensIn2232 = 3;

        public ConveyorRoomforLevel1232 conveyorRoom;

        public Timer timer;

        public GameObject root;

        public float delay = 1f;

        public bool activation = false;

        public int rightPercent = 50;
        
        public bool addTargetAfterHalf;

        private ItemsSet prefabs;

        private float baseTime;

        private bool _extraTargetAdded;

        private readonly List<string> targets = new();
        private readonly List<string> distractors = new();
        private readonly List<ObjectProperties1232> objects = new();

        public bool levelIs12;
        int targetRequested;

        private readonly List<Item> selectedTarget = new();

        // Called from ConveyorRoom prepare
        public void Prepare()
        {
            targetRequested = levelIs12 ? _screensIn12 : _screensIn2232;
            if (_extraTargetAdded) targetRequested++;

            prefabs = conveyorRoom.ItemsSet;
            
            for (var i = 0; i < _screens.Length; i++)
            {
                _screens[i].gameObject.SetActive(i < targetRequested);
                _screens[i].Prepare(prefabs);
            }
            
            targets.Clear();
            distractors.Clear();
            
            for (var i = selectedTarget.Count; i < targetRequested; i++)
            {
                var select = prefabs.PickRandom(selectedTarget);
                selectedTarget.Add(select);
                _screens[i].ChangeTarget(select.Name);
            }

            foreach (var prefab in prefabs)
            {
                foreach (var ele in selectedTarget)
                {
                    if (ele.Name == prefab.Key)
                    {
                        //Creating the list of objects can be a target.
                        targets.Add(prefab.Key);
                    }
                    else
                    {
                        //Creating the list of objects never going to be a target.
                        distractors.Add(prefab.Key);
                    }
                }
            }
        }

        void Update()
        {
            if (Mathf.Abs(timer.timeLeft - baseTime) > delay)
            {
                if (activation)
                {
                    CreateNewObject();
                }

                baseTime = timer.timeLeft;
            }

            if (!_extraTargetAdded && addTargetAfterHalf && timer.getTimeFactor() > .5f)
            {
                _extraTargetAdded = true;
                activation = false;
                timer.activation = false;
                messageSystem.oneShotRead.AddListener(OnHalftimeMessageRead);
                messageSystem.ShowMessage(halftimeConfig);
                
                foreach (var obj in objects) obj.Inhibited = true;
                
                Prepare();
            }
        }

        private void OnHalftimeMessageRead()
        {
            messageSystem.oneShotRead.RemoveListener(OnHalftimeMessageRead);
            timer.activation = true;
            activation = true;

            foreach (var obj in objects) obj.Inhibited = false;
        }

        private bool isTarget(string name)
        {
            return (targets.Find(s => s == name) != null);
        }

        void CreateNewObject()
        {
            var newObjectName = extractRandomObject();
            var element = prefabs.SingleOrDefault(prefab => prefab.Key == newObjectName).Value;
            
            if (element == null) return;
            
            var newObject = Instantiate(element.Prefab, root.transform, true);
            var props = newObject.AddComponent<ObjectProperties1232>();
            props.isTarget = isTarget(newObjectName);
            props.spawner = this;

            objects.Add(props);
            
            newObject.name = element.name;
            newObject.transform.position = _spawnPoints.Length == 0 ? 
                    Vector3.zero : 
                    _spawnPoints[Random.Range(0, _spawnPoints.Length)].position;
            newObject.transform.rotation = Quaternion.identity;
            
            newObject.AddComponent<Rigidbody>();
            newObject.GetComponent<Rigidbody>().isKinematic = true;
        }

        private string extractRandomObject()
        {
            if (Random.Range(0, 100) < rightPercent)
            {
                return extractRandomStringFromList(targets);
            }
            else
            {
                return extractRandomStringFromList(distractors);
            }
        }

        private string extractRandomStringFromList(List<string> list)
        {
            if (list.Count > 0)
            {
                return list[Random.Range(0, list.Count)];
            }

            Debug.LogError("Cannot extract random prefab: there are few prefabs!");
            return "";
        }

        public void RemoveObject(ObjectProperties1232 obj)
        {
            objects.Remove(obj);
            Destroy(obj.gameObject);
        }
    }

}
