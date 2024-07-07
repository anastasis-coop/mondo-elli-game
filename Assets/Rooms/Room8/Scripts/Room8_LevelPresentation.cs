using UnityEngine;

namespace Room8
{
    public class Room8_LevelPresentation : MonoBehaviour
    {
        [SerializeField]
        private MessageSystem messageSystem;
        
        [SerializeField]
        private BigElloSaysConfig[] _configs;

        [SerializeField]
        private GameObject nextBehaviour;

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
            Room8_CommonRefs.controller.helpRequested = false;

            if (!nextBehaviour.activeSelf)
            {
                nextBehaviour.SetActive(true);
            }
        }

        private void OnRelistenButtonPressed()
        {
            messageSystem.ShowSpecificRangeOfMessages(relistenRange.x, relistenRange.y);
        }

        private void OnReviewButtonPressed()
        {
            messageSystem.ShowSpecificRangeOfMessages(reviewRange.x, reviewRange.y);
        }

        private void OnDisable()
        {
            if (messageSystem == null) return;

            if (messageSystem.messageRead != null)
            {
                messageSystem.messageRead.RemoveListener(OnMessageRead);
            }

            if (messageSystem.greenButtonClicked != null)
            {
                messageSystem.greenButtonClicked.RemoveListener(OnStartButtonPressed);
            }

            if (messageSystem.yellowButtonClicked != null)
            {
                messageSystem.yellowButtonClicked.RemoveListener(OnRelistenButtonPressed);
            }

            if (messageSystem.redButtonClicked != null)
            {
                messageSystem.redButtonClicked.RemoveListener(OnReviewButtonPressed);
            }
        }

    }
}