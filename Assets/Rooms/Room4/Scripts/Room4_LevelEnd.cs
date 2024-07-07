using UnityEngine;

namespace Room4
{
    public class Room4_LevelEnd : MonoBehaviour
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

            GameController.Instance.levelEnd();
        }
    }
}
