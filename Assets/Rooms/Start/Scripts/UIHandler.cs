using UnityEngine;

namespace Start {
    public class UIHandler : MonoBehaviour {

        private GameController controller;

        private void Awake() {
            controller = GameObject.Find("GameController").GetComponent<GameController>();
        }

        public void LoginButtonHandler() {
            controller.LoginTest();
        }

        public void StartButtonHandler() {
            controller.ConfirmSkin();
        }

        public void LoginDemoButtonHandler(){
            controller.LoginTestDemo();
        }
    }
}
