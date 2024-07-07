using System;
using UnityEngine;

namespace Room9
{
    public class MemoryPreviews : MonoBehaviour
    {
        private Action<bool> _callback;

        public void Show(Action<bool> callback)
        {
            _callback = callback;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            _callback = null;
            gameObject.SetActive(false);
        }
    }
}
