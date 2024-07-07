using UnityEngine;

namespace Room6
{
    public class Room6_LevelEnd : MonoBehaviour
    {
        [SerializeField]
        private MessageSystem messageSystem;

        [SerializeField]
        private BigElloSaysConfig _config;


        [SerializeField]
        private RoomEndPopup endPopup;

        private void OnEnable()
        {
            messageSystem.ShowMessage(_config);
            messageSystem.oneShotRead.AddListener(OnMessageRead);
        }

        private void OnMessageRead()
        {
            enabled = false;
        }

        private void OnDisable()
        {
            if (messageSystem?.oneShotRead != null)
            {
                messageSystem.oneShotRead.RemoveListener(OnMessageRead);
            }

            endPopup.Show(Room6_CommonRefs.controller.SaveResultsAndExit);
        }
    }
}
