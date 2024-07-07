using UnityEngine;

namespace Room3 {
    public class Lever : MonoBehaviour {

        public float duration = 1f;
        public AnimationCurve pullCurve;

        private bool pulling;
        private float elapsed;

        // Start is called before the first frame update
        void Start() {
            pulling = false;
            elapsed = 0;
        }

        // Update is called once per frame
        void Update() {
            if (pulling) {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                Vector3 rot = transform.localEulerAngles;
                rot.x = Mathf.Lerp(0, -75, pullCurve.Evaluate(t));
                transform.localEulerAngles = rot;
                if (t==1) {
                    pulling = false;
                }
            }
        }

        public void Pull() {
            if (!pulling) {
                pulling = true;
                elapsed = 0;
            }
        }

    }
}
