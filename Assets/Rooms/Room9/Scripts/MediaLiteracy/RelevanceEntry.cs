using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Room9
{
    public class RelevanceEntry : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI titleLabel;

        [SerializeField]
        private ReadableLabel titleReadable;

        [SerializeField, FormerlySerializedAs("previewLabel")]
        private TextMeshProUGUI textLabel;

        [SerializeField]
        private ReadableLabel textReadable;

        [SerializeField, FormerlySerializedAs("authorLabel")]
        private TextMeshProUGUI bottomLabel;

        [SerializeField]
        private ReadableLabel bottomReadable;

        [SerializeField]
        private DragDropSlot slot;
        public DragDropSlot Slot => slot;

        [SerializeField]
        private string[] relevanceTags;

        public int TextIndex { get; private set; }
        public bool Assigned => slot.Object != null;

        public int Relevance
        {
            get
            {
                if (!Assigned) return -1;

                for (int i = 0; i < relevanceTags.Length; i++)
                {
                    if (slot.Object.CompareTag(relevanceTags[i]))
                        return i;
                }

                return -1;
            }
        }

        public void Init(int textIndex, ReadableString title, ReadableString text, ReadableString bottom)
        {
            TextIndex = textIndex;

            titleLabel.text = title?.String;
            textLabel.text = text?.String;
            bottomLabel.text = bottom?.String;

            titleReadable.AudioClip = title?.AudioClip;
            textReadable.AudioClip = text?.AudioClip;
            bottomReadable.AudioClip = bottom?.AudioClip;
        }
    }
}