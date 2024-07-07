using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Room9
{
    public class NotebookEntry : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI label;

        [SerializeField]
        private Image image;

        [SerializeField]
        private Sprite defaultSprite;

        [SerializeField]
        private Sprite selectedSprite;

        [SerializeField]
        private Button button;

        [SerializeField]
        private ReadableLabel readable;

        public int TextIndex { get; private set; }
        public int SnippetIndex { get; private set; }
        public ReadableString Text { get; private set; }
        
        private Action<NotebookEntry> _pressed;

        public bool Selected
        {
            set => image.sprite = value ? selectedSprite : defaultSprite;
        }

        public bool Interactable
        {
            set => button.interactable = value;
        }

        public void Init(int textIndex, int snippetIndex, ReadableString text, Action<NotebookEntry> onPressed)
        {
            TextIndex = textIndex;
            SnippetIndex = snippetIndex;
            Text = text;
            label.text = text?.String;
            readable.AudioClip = text?.AudioClip;
            _pressed = onPressed;
        }

        public void OnButtonPressed()
        {
            _pressed?.Invoke(this);
        }
    }
}