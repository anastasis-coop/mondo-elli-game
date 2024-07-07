using UnityEngine;

namespace Room8
{
    public class Room8_LevelEnd : MonoBehaviour
    {
        [SerializeField]
        private MessageSystem messageSystem;
        
        [SerializeField]
        private BigElloSaysConfig _config;

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

            Room8_CommonRefs.controller.levelEnd();
        }
    }
}
