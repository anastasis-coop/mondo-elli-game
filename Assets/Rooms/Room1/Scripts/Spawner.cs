using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Common;

namespace Room1
{
    [System.Serializable]
    public class Spawner : MonoBehaviour
    {    
        public ConveyorRoom conveyorRoom;

        public Timer timer;

        public string firstTarget;
        public string secondTarget;

        public string nextFirstTarget;
        public string nextSecondTarget;

        public Transform spawnPoint;
        
        public GameObject root;

        public float delay;
        public bool activation = false;

        public int rightPercent;
        
        public bool addTargetsAfterHalf;
        
        [Range(0, .5f)]
        public float _randomScaleOffsetMargins;
        [Range(0, .1f)]
        public float _randomPosZOffsetMargins;
        
        [Range(0, 180)]
        public float _randomOrientOffsetMargins;
        
        public bool freezeRotation = false;

        private ItemsSet prefabs;

        private float baseTime;

		// Called from ConveyorRoom prepare
        public void Prepare()
        {
            Debug.Log("Spawner prepare");
            prefabs = conveyorRoom.ItemsSet;

            // First extraction at start (anyway one target only)
            firstTarget = extractRandomTarget();
            nextFirstTarget = ExtractRandomTargetExcluding(new string[] { firstTarget });
            secondTarget = "";
            nextSecondTarget = "";

            conveyorRoom.prepareFirstTarget(firstTarget);

            // From  here we have the list of objects to extract
            baseTime = Time.time;
        }

        // Update is called once per frame
        void Update()
        {
            if (Time.time - baseTime > delay)
            {
                CreateNewObject();
                baseTime = Time.time;
            }
        }

        void CreateNewObject()
        {
            if (!activation) return;
            
            string newObjectName = extractRandomObject();
            Item element = prefabs.SingleOrDefault(elem => elem.Key == newObjectName).Value;
            
            if (element == null) return;
            
            GameObject newObject = Instantiate(element.Prefab, root.transform, true);
            var rb = newObject.AddComponent<Rigidbody>();
                    
            rb.constraints = 
                RigidbodyConstraints.FreezePositionZ | 
                RigidbodyConstraints.FreezeRotationX | 
                RigidbodyConstraints.FreezeRotationY;
            if (freezeRotation)
            {
                rb.constraints |= RigidbodyConstraints.FreezeRotationZ;
            }
                    
            newObject.AddComponent<ObjectProperties>().objectName = newObjectName;
            newObject.transform.position = spawnPoint == null ? transform.position : spawnPoint.position;
                    
            newObject.name = element.name;
        }

        private string extractRandomObject()
        {
            if (Random.Range(0, 100) < rightPercent)
            {
                if (secondTarget != "")
                {
                    // Can be first or second target
                    return (Random.Range(0, 2) == 0) ? firstTarget : secondTarget;
                }
                
                return firstTarget;
            }
            
            return ExtractRandomTargetExcluding(new [] { firstTarget, nextFirstTarget, secondTarget, nextSecondTarget });
        }

        private string extractRandomTarget()
        {
            return ExtractRandomStringFrom(prefabs.Keys);
        }

        private string ExtractRandomTargetExcluding(string[] itemsToExclude)
        {
            return ExtractRandomStringFrom(prefabs.Keys.Where(v => !itemsToExclude.Contains(v)));
        }

        private string ExtractRandomStringFrom(IEnumerable<string> options)
        {
            var list = options.ToList();
            if (list.Count > 0)
            {
                return list[Random.Range(0, list.Count)];
            }
            
            Debug.LogError("Cannot extract random prefab: there are few prefabs!");
            return "";
        }

        public void nextTargets()
        {
            firstTarget = nextFirstTarget;
            if (addTargetsAfterHalf && timer.getTimeFactor() > 0.5)
            {
                secondTarget = nextSecondTarget;
            } else
            {
                secondTarget = "";
            }
            nextFirstTarget = ExtractRandomTargetExcluding(new string[] { firstTarget, secondTarget });
            nextSecondTarget = ExtractRandomTargetExcluding(new string[] { firstTarget, secondTarget, nextFirstTarget });
        }

    }
}
