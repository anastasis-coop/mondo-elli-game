using UnityEngine;

namespace Room4
{
    public class Room4_LevelPresentation : MonoBehaviour
    {
        [SerializeField]
        public BigElloSaysConfig[] _configs;
        
        [SerializeField]
        private MessageSystem messageSystem;

        public GameObject nextBehaviour;

        [SerializeField]
        private Vector2Int relistenRange;

        [SerializeField]
        private Vector2Int reviewRange;

        private void OnEnable()
        {
            messageSystem.SetCurrentMessagesBatch(_configs);
            messageSystem.messageRead.AddListener(OnMessageRead);
            messageSystem.greenButtonClicked.AddListener(OnStartButtonPressed);
            messageSystem.yellowButtonClicked.AddListener(OnRelistenButtonPressed);
            messageSystem.redButtonClicked.AddListener(OnReviewButtonPressed);
            messageSystem.ShowNextMessage();
        }

        private void OnMessageRead(int index)
        {
            if (index >= messageSystem.currentMessagesBatch.Count - 1)
                return;

            messageSystem.TryShowNextMessage();
        }

        private void OnStartButtonPressed()
        {
            enabled = false;

            if (!nextBehaviour.activeSelf)
            {
                nextBehaviour.SetActive(true);
            }
        }

        private void OnRelistenButtonPressed()
        {
            messageSystem.SetCurrentMessagesBatch(_configs);
            messageSystem.ShowSpecificRangeOfMessages(relistenRange.x, relistenRange.y);
        }

        private void OnReviewButtonPressed()
        {
            messageSystem.SetCurrentMessagesBatch(_configs);
            messageSystem.ShowSpecificRangeOfMessages(reviewRange.x, reviewRange.y);
        }

        private void OnDisable()
        {
            if (messageSystem == null) return;

            messageSystem.messageRead?.RemoveListener(OnMessageRead);
            messageSystem.greenButtonClicked?.RemoveListener(OnStartButtonPressed);
            messageSystem.yellowButtonClicked?.RemoveListener(OnRelistenButtonPressed);
            messageSystem.redButtonClicked?.RemoveListener(OnReviewButtonPressed);
        }
    }
}