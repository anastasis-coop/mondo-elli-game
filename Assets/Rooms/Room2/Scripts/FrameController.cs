using UnityEngine;

namespace Room2
{
    public class FrameController : MonoBehaviour
    {

        public enum FrameType
        {
            Left, Right
        }

        public GameObject LeftFrame;
        public GameObject RightFrame;

        public void SelectFrame(FrameType frame)
        {
            LeftFrame.SetActive(frame == FrameType.Left);
            RightFrame.SetActive(frame == FrameType.Right);
        }

    }
}
