using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Room9
{
    public class InhibitionMemoryTitleEntry : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI titleLabel;

        [SerializeField]
        private Image backgroundImage;

        [SerializeField]
        private Sprite correctSprite;

        [SerializeField]
        private Sprite wrongSprite;

        [SerializeField]
        private ReadableLabel readable;

        [SerializeField]
        private Button button;

        private Action<InhibitionMemoryTitleEntry> _selectCallback;

        public int TitleIndex { get; private set; }

        public void Init(int textIndex, ReadableString title, Action<InhibitionMemoryTitleEntry> selectCallback)
        {
            titleLabel.text = title?.String;
            readable.AudioClip = title?.AudioClip;
            _selectCallback = selectCallback;

            TitleIndex = textIndex;
            button.interactable = true;
        }

        public void ShowSolution(bool correct)
        {
            backgroundImage.sprite = correct ? correctSprite : wrongSprite;
            button.interactable = false;
        }

        public void OnSelectButtonPress() => _selectCallback?.Invoke(this);
    }
}