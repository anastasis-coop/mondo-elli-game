using UnityEngine;
using UnityEngine.Events;

namespace Room1
{
    public class MoveBox : MonoBehaviour
    {

        public UnityEvent onBoxMoved = new UnityEvent();

        public void startMoving()
        {
            GetComponent<Animation>().Play();
        }

        public void animationEnded()
        {
            onBoxMoved.Invoke();
        }

    }
}
