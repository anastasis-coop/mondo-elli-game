using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace End {
    public class End_SetExitMessage : MonoBehaviour {

        public LocalizedString localizedNormalText;
        public LocalizedString localizedWebText;

        void Start() {
            Text textComponent = GetComponent<Text>();

            if (Application.platform == RuntimePlatform.WebGLPlayer)
                textComponent.text = localizedWebText.GetLocalizedString();
            else
                textComponent.text = localizedNormalText.GetLocalizedString();
        }
    }
}
