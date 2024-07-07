using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Room2
{
    [System.Serializable]
    public class SelectedPhoneCallBack : UnityEvent<Transform>
    {
    }

    public class TableController : MonoBehaviour
    {
        private class PhoneElement
        {
            private PhoneScreenController _screen;
            private float _timer;
            
            public readonly GameObject Phone;
            public readonly float OnTime;
            public readonly float OffTime;

            public PhoneElement(GameObject phone, float onTime, float offTime)
            {
                Phone = phone;
                OnTime = onTime;
                OffTime = offTime;

                _screen = phone.GetComponent<PhoneScreenController>();

                _screen.IsOn = false;
                _timer = 0;
            }

            public void Update(float time)
            {
                if (OnTime == 0) return;
                
                _timer += time;

                if (_screen.IsOn)
                {
                    if (_timer < OnTime) return;

                    _timer -= OnTime;
                    _screen.IsOn = false;
                }
                else
                {
                    if (_timer < OffTime) return;

                    _timer -= OffTime;
                    _screen.IsOn = true;
                }
            }
        }
        
        public enum AllowedNumbers
        {
            None,
            Phones_6,
            Phones_8,
            PhoneTabletsAndLaptops_15
        }

        public AllowedNumbers NumberOfObjects;
        public List<GameObject> PhonesAndTablets;
        [SerializeField] private GameObject phonesContainer1;

        [SerializeField]
        private Vector2 _phoneScreenOnTimeRange;

        [SerializeField]
        private Vector2 _phoneScreenOffTimeRange;
        
        [SerializeField] private List<Transform> phonesSpawnPoints;
        public SelectedPhoneCallBack OnSelectedObject;

        private int objectsCount;
        private Transform currentRoot;
        private readonly List<GameObject> prefabsToUse = new();
        private readonly List<PhoneElement> currentSpawnedPhonesGO = new();

        public bool AllowDistractions { get; set; }
        
        void Start()
        {
            ShowNextGroup();
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
                {
                    string selectedName = hit.transform.name;
                    if (selectedName.StartsWith("Phone") || selectedName.StartsWith("Tablet") || selectedName.StartsWith("Laptop"))
                    {
                        OnSelectedObject.Invoke(hit.transform);
                    }
                }
            }
            
            foreach (var element in currentSpawnedPhonesGO) element.Update(Time.deltaTime);
        }

        public void ShowNextGroup()
        {
            DestroyOldPhones();
            CheckParams();

            var spawnTransforms = phonesSpawnPoints.Select(x => x.transform).ToList();

            for (var i = 0; i < objectsCount; i++)
            {
                var spawnIndex = Random.Range(0, spawnTransforms.Count);
                var randomPrefab = prefabsToUse[Random.Range(0, prefabsToUse.Count)];
                prefabsToUse.Remove(randomPrefab);
                
                var newObj = Instantiate(randomPrefab, spawnTransforms[spawnIndex].position, spawnTransforms[spawnIndex].rotation);
                newObj.transform.SetParent(phonesContainer1.transform);
                newObj.name = randomPrefab.name;

                var onTime = Mathf.Max(0, Random.Range(_phoneScreenOnTimeRange.x, _phoneScreenOnTimeRange.y));
                var offTime = Mathf.Max(0, Random.Range(_phoneScreenOffTimeRange.x, _phoneScreenOffTimeRange.y));
                
                currentSpawnedPhonesGO.Add(new PhoneElement(newObj, AllowDistractions ? onTime : 0, offTime));

                spawnTransforms.RemoveAt(spawnIndex);
            }
        }

        private void CheckParams()
        {
            if (NumberOfObjects != AllowedNumbers.None)
            {
                if (NumberOfObjects == AllowedNumbers.PhoneTabletsAndLaptops_15)
                    objectsCount = 11;
                else
                {
                    objectsCount = (NumberOfObjects == AllowedNumbers.Phones_6) ? 6 : 8;
                }

                prefabsToUse.Clear();

                for (int i = 0; i < objectsCount && i < PhonesAndTablets.Count; i++)
                {
                    prefabsToUse.Add(PhonesAndTablets[i]);
                }
                
                //extraPhonesContainer.SetActive(prefabsToUse.Count > 8);
            }
        }

        private void DestroyOldPhones()
        {
            foreach (var phone in currentSpawnedPhonesGO)
            {
                Destroy(phone.Phone);
            }

            currentSpawnedPhonesGO.Clear();
        }
    }
}