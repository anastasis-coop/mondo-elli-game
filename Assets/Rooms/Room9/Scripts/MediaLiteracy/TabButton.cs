using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Room9
{
    public class TabButton : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI label;

        [SerializeField]
        private Image image;

        [SerializeField]
        private Color selectedColor = Color.white;

        [SerializeField]
        private Color defaultColor = Color.gray;

        [SerializeField]
        private Button button;

        public int Index { get; private set; }
        public string Text { set => label.text = value; }
        public Color TextColor { set => label.color = value; }
        private Action<int> _pressed;

        public bool Selected
        {
            set
            {
                image.color = value ? selectedColor : defaultColor;
                button.interactable = !value;
            }
        }

        public void Init(int index, Action<int> onPressed)
        {
            Index = index;
            _pressed = onPressed;

            Selected = false;
        }

        public void OnButtonPressed()
        {
            _pressed?.Invoke(Index);
        }
    }
}