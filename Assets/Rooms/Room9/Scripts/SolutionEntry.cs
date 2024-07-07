using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Room9
{
    public class SolutionEntry : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI titleLabel;

        [SerializeField]
        private TextMeshProUGUI answerLabel;

        [SerializeField]
        private TextMeshProUGUI solutionLabel;

        [SerializeField]
        private Image answerBackground;

        [SerializeField]
        private Image solutionBackground;

        [SerializeField]
        private Sprite[] answerSprites;

        [SerializeField]
        private Image emoji;

        [SerializeField]
        private Sprite correctEmojiSprite;

        [SerializeField]
        private Sprite almostEmojiSprite;

        [SerializeField]
        private Sprite wrongEmojiSprite;

        [SerializeField]
        private ReadableLabel readable;

        public void Init(ReadableString title, int answer, int solution)
        {
            titleLabel.text = title?.String;
            answerLabel.text = answer.ToString();
            solutionLabel.text = solution.ToString();

            readable.AudioClip = title?.AudioClip;

            // HACK
            //answerBackground.sprite = answerSprites[answer];
            //solutionBackground.sprite = answerSprites[solution];

            emoji.sprite = answer == solution ? correctEmojiSprite :
                (solution == 0 || answer == 0) ? wrongEmojiSprite :
                almostEmojiSprite;
        }
    }
}
