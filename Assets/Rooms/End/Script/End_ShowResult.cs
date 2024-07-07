using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace End {
    public class End_ShowResult : MonoBehaviour {

        public LocalizedString scoreString;
        public LocalizedString recordString;

        void Start() {
            GameState.Instance.levelBackend.EndSession(res => {
                Text textComponent = GetComponent<Text>();

                string scoreMessage = string.Format(scoreString.GetLocalizedString(), res.score);
                if (res.scoreRecord)
                    scoreMessage += recordString.GetLocalizedString();

                textComponent.text = scoreMessage;
            },
            err => {
                Debug.Log(err.Message);
            });
        }
    }
}
