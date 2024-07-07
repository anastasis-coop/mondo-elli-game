using UnityEngine;

namespace Room6
{
    public class Room6_LevelPresentation : MonoBehaviour
    {
        [SerializeField]
        public BigElloSaysConfig[] Configs;
        
        [SerializeField]
        private MessageSystem messageSystem;

        public Room6_Level nextBehaviour;

        [SerializeField]
        private Vector2Int relistenRange;

        [SerializeField]
        private Vector2Int reviewRange;

        private void OnEnable()
        {
            nextBehaviour.InitializeView();
            messageSystem.SetCurrentMessagesBatch(Configs);
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
            gameObject.SetActive(false);

            if (!nextBehaviour.gameObject.activeSelf)
            {
                nextBehaviour.gameObject.SetActive(true);
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

            messageSystem.messageRead?.RemoveListener(OnMessageRead);
            messageSystem.greenButtonClicked?.RemoveListener(OnStartButtonPressed);
            messageSystem.yellowButtonClicked?.RemoveListener(OnRelistenButtonPressed);
            messageSystem.redButtonClicked?.RemoveListener(OnReviewButtonPressed);
        }
    }
}