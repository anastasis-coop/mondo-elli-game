using UnityEngine;

namespace Game {
    public class UIHandler : MonoBehaviour {

        private GameController controller;

        void Start() {
            controller = GameController.Instance;
        }

        public void PlaySequence() {
            GameEvent ev;
            ev.eventType = GameEventType.PLAY;
            ev.triggerObject = gameObject;
            controller.HandleEvent(ev);
        }

        public void ShadowHandler() {
            GameEvent ev;
            ev.eventType = GameEventType.GHOST;
            ev.triggerObject = gameObject;
            controller.HandleEvent(ev);
        }

        public void PowerHandler() {
            GameEvent ev;
            ev.eventType = GameEventType.POWER;
            ev.triggerObject = gameObject;
            controller.HandleEvent(ev);
        }

        public void FirstPowerButtonHandler() {
            controller.TutorialEndPopupClose();
        }

        public void MessagePanelButtonHandler() {
            controller.StopAudio();
            GameEvent ev;
            ev.eventType = GameEventType.MESSAGE_READ;
            ev.triggerObject = gameObject;
            controller.HandleEvent(ev);
        }

        public void HelpButtonHandler() {
            GameEvent ev;
            ev.eventType = GameEventType.HELP;
            ev.triggerObject = null;
            controller.HandleEvent(ev);
        }

        public void CodingListChangedHandler()
        {
            GameEvent ev;
            ev.eventType = GameEventType.CODING_LIST_CHANGED;
            ev.triggerObject = gameObject;
            controller.HandleEvent(ev);
        }
    }
}