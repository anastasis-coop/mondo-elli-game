using System;
using TMPro;
using UnityEngine;

namespace Room9
{
    public class ProductionEditorEntry : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI snippetLabel;

        [SerializeField]
        private ReadableLabel snippetReadable;

        [SerializeField]
        private GameObject addCardRoot;

        [SerializeField]
        private ProductionEditorCard card;

        public event Action<ProductionEditorEntry> ReplacePressed;
        public event Action<ProductionEditorEntry> RemovePressed;
        public event Action<ProductionEditorEntry, bool> MovePressed;
        public event Action<ProductionEditorEntry> SetCardPressed;
        public event Action<ProductionEditorEntry> ClearCardPressed;

        public int TextIndex { get; private set; }
        public int SnippetIndex { get; private set; }

        public bool HasCard => card.CardConfig != null;
        public ProductionEditor.CardConfig CardConfig => card.CardConfig;

        public void Init(int textIndex, int snippetIndex, ReadableString text)
        {
            TextIndex = textIndex;
            SnippetIndex = snippetIndex;

            snippetLabel.text = text?.String;
            snippetReadable.AudioClip = text?.AudioClip;

            card.Init(OnReplaceCardPressed);
        }

        public void OnReplacePressed()
        {
            ReplacePressed?.Invoke(this);
        }

        public void OnRemovePressed()
        {
            RemovePressed?.Invoke(this);
        }

        public void OnMovePressed(bool up)
        {
            MovePressed?.Invoke(this, up);
        }

        public void SetCard(ProductionEditor.CardConfig cfg)
        {
            card.gameObject.SetActive(true);
            addCardRoot.SetActive(false);

            card.CardConfig = cfg;
        }

        public void ClearCard()
        {
            card.gameObject.SetActive(false);
            addCardRoot.SetActive(true);

            card.CardConfig = null;
        }

        public void OnSetCardPressed()
        {
            SetCardPressed?.Invoke(this);
        }

        public void OnReplaceCardPressed(ProductionEditorCard _)
        {
            SetCardPressed?.Invoke(this);
        }

        public void OnClearCardPressed()
        {
            ClearCardPressed?.Invoke(this);
        }
    }
}