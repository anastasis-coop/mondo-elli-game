using UnityEngine;

namespace Room6 {
    public class Room6_InteractionHandler : MonoBehaviour 
    {
        // Pannello per le immagini e relativo campo per la visualizzazione
        public GameObject Blackboard;
        public SpriteRenderer BlackboardRenderer;

        // Stato dell'animazione del pannello immagini (0-inizio, 1-fine)
        private float tMove;
        private int _endAlpha;
        private bool blackboardAnimating;
        
        public bool IsPanelDown { get; private set; }

        public void SetVisiblePanel(bool isVisible) => Blackboard.SetActive(isVisible);
        
        public void SetImagePanel(Sprite sp) {
            BlackboardRenderer.sprite = sp;
        }

        public bool IsPanelNotMoving() {
            return !blackboardAnimating;
        }

        /* Funzione per comandare il movimento del pannello immagini verso il basso */
        public void PanelDown() {
            if (!Blackboard.activeSelf || blackboardAnimating && _endAlpha == 1)
                return;
            
            var color = BlackboardRenderer.color;
            color.a = 0;
            BlackboardRenderer.color = color;
            
            tMove = 0;
            _endAlpha = 1;
            blackboardAnimating = true;
        }

        /* Funzione per comandare il movimento del pannello immagini verso l'alto */
        public void PanelUp() {
            if (!Blackboard.activeSelf || blackboardAnimating && _endAlpha == 0)
                return;
            
            var color = BlackboardRenderer.color;
            color.a = 1;
            BlackboardRenderer.color = color;
            
            tMove = 0;
            _endAlpha = 0;
            blackboardAnimating = true;
        }

        private void Update() {
            if (blackboardAnimating) {
                tMove += Time.deltaTime;

                var color = BlackboardRenderer.color;
                color.a = Mathf.Lerp(1 - _endAlpha, _endAlpha, tMove);
                BlackboardRenderer.color = color;

                if (tMove > 1) {
                    tMove = 0;
                    blackboardAnimating = false;
                    IsPanelDown = _endAlpha == 1;
                }
            }
        }
    }
}