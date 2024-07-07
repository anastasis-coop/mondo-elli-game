using System;
using UnityEngine;

namespace Room1
{
    [ExecuteInEditMode]
    public class ChildrenCentralizer : MonoBehaviour
    {
        [SerializeField]
        private Vector3 _distance;

        [SerializeField]
        private bool _recalculateOnUpdate;

#if UNITY_EDITOR
        private void OnEnable()
        {
            for (var i = 0; i < transform.childCount; i++)
                transform.GetChild(i).hideFlags = HideFlags.NotEditable;
        }

        private void OnDisable()
        {
            for (var i = 0; i < transform.childCount; i++)
                transform.GetChild(i).hideFlags = HideFlags.None;
        }
#endif

        private void Update()
        {
            if (!_recalculateOnUpdate) return;
            RecalculateChildren();
        }

        public void RecalculateChildren()
        {
            var distance = _distance;
            distance.x = Mathf.Max(0, distance.x);
            distance.y = Mathf.Max(0, distance.y);
            distance.z = Mathf.Max(0, distance.z);

            var activeCount = 0;
            for (var i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject.activeSelf)
                    activeCount++;
            }
            
            var start = - distance * (activeCount / 2);
            if (activeCount % 2 == 0) start += distance / 2;

            for (int i = 0, count = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (!child.gameObject.activeSelf) continue;

                child.localPosition = start + count * distance;
                count++;
            }
        }
    }
}
