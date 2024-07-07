using UnityEngine;
using UnityEngine.Events;

using UnityEngine.EventSystems;

namespace Room2
{
    [System.Serializable]
    public class ClickCallBack : UnityEvent<int>
    {
    }

    public class ClickAction : MonoBehaviour, IPointerClickHandler
    {

        public int index;
        public ClickCallBack OnClick = new ClickCallBack();

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick.Invoke(index);
        }

    }
}