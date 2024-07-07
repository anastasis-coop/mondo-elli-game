using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Room9
{
    public class OutlineEntry : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI indexLabel;

        [SerializeField]
        private TextMeshProUGUI textLabel;

        [SerializeField]
        private ReadableLabel readable;
        
        [SerializeField] private GameObject solutionIndexGO;
        [SerializeField] private TextMeshProUGUI solutionIndexLabel;

        public int Index { set => indexLabel.text = value.ToString(); }
        public int SolutionIndex { set => solutionIndexLabel.text = value.ToString(); }

        public bool localizeText = false;
        public ReadableString Text
        {
            set
            {
                textLabel.text = localizeText? LocalizationSettings.StringDatabase.GetLocalizedString(value?.String): value?.String;
                readable.AudioClip = value?.AudioClip;
            }
        }
        
        public void ShowSolutionIndex(bool _show)
        {
            solutionIndexGO.SetActive(_show);
        }
    }
}