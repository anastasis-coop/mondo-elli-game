using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Room3 {
    public class FindOppositeController : MonoBehaviour {
        public FeedbackController feedback;

        public Sprite[] primaryIcons;
        public Sprite[] oppositeIcons;
        public Sprite[] primaryImages;
        public Sprite[] oppositeImages;

        public RectTransform layout;
        public Image backgroundImage;
        public Image leftButtonSpriteImage;
        public Image rightButtonSpriteImage;
        [SerializeField] private Collider _leftButtonCollider;
        [SerializeField] private Collider _rightButtonCollider;

        public Timer timer;
        public Score score;
        public Bomb bomb;
        public UnityEvent onGameOver = new UnityEvent();

        private Button _buttonLeft;
        private Button _buttonRight;

        private bool currentIsPrimary;
        private int currentIndex;
        private int count;
        private bool selected;
        private float elapsed = 0;
        private bool waiting = false;

        void Start() {
            count = primaryIcons.Length;
            if ((oppositeIcons.Length != count) || (primaryImages.Length != count) || (oppositeImages.Length != count)) {
                Debug.Log("ERROR IN \"FIND OPPOSITE\" DATA");
            }
        }

        public void prepareGame(float timeout, bool alwaysVisibleBomb)
        {
            bomb.timeoutSeconds = timeout;
            setBombMode(alwaysVisibleBomb);
        }
        public void startGame() {
            extractRandomImage();
        }

        private void setBombMode(bool isAlwaysVisible) {
            layout.anchoredPosition = new Vector2(-220f, 0);
            bomb.gameObject.SetActive(true);
            bomb.onExplode.AddListener(onExplode);

            if (isAlwaysVisible)
                bomb.SetAlwaysVisible();
            else
                bomb.SetVisibleWhenReaches(3);
        }

        public void extractRandomImage() {
            selected = false;
            _leftButtonCollider.enabled = true;
            _rightButtonCollider.enabled = true;
            bool isPrimary;
            int index;
            do {
                isPrimary = (Random.Range(0, 2) == 0);
                index = Random.Range(0, count);
            } while ((isPrimary == currentIsPrimary) && (index == currentIndex));
            currentIsPrimary = isPrimary;
            currentIndex = index;
            //layout.gameObject.SetActive(true);
            backgroundImage.sprite = currentIsPrimary ? primaryImages[currentIndex] : oppositeImages[currentIndex];
            leftButtonSpriteImage.sprite = primaryIcons[currentIndex];
            rightButtonSpriteImage.sprite = oppositeIcons[currentIndex];
            bomb.StartCountdown();
        }

        public void buttonLeftPressed() {
            if (!selected) {
                selected = true;
                _leftButtonCollider.enabled = false;
                _rightButtonCollider.enabled = false;
                if (currentIsPrimary) {
                    wrongAnswer();
                } else {
                    rightAnswer();
                }
                next();
            }
        }

        public void buttonRightPressed() {
            if (!selected) {
                selected = true;
                _leftButtonCollider.enabled = false;
                _rightButtonCollider.enabled = false;
                if (currentIsPrimary) {
                    rightAnswer();
                } else {
                    wrongAnswer();
                }
                next();
            }
        }

        private void rightAnswer() {
            feedback.rightAnswerFeedback();
            score.RightCounter++;
        }

        private void wrongAnswer() {
            feedback.wrongAnswerFeedback();
            score.WrongCounter++;
        }

        private void missedAnswer() {
            feedback.wrongAnswerFeedback();
            score.MissedCounter++;
        }

        private void onExplode() {
            missedAnswer();
            layout.gameObject.SetActive(false);
            Invoke(nameof(next), bomb.explosionSeconds);
            (_buttonLeft ??= _leftButtonCollider.GetComponent<Button>()).interactable = false;
            (_buttonRight ??= _rightButtonCollider.GetComponent<Button>()).interactable = false;
        }

        private void next() {
            bomb.StopCountdown();
            if (timer.itIsEnd) {
                onGameOver.Invoke();
            } else {
                (_buttonLeft ??= _leftButtonCollider.GetComponent<Button>()).interactable = true;
                (_buttonRight ??= _rightButtonCollider.GetComponent<Button>()).interactable = true;
                Invoke(nameof(extractRandomImage), 0.5f);
            }
        }

    }
}
