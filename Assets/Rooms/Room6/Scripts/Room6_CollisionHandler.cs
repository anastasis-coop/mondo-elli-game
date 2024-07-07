using UnityEngine;

namespace Room6 {
    public class Room6_CollisionHandler : MonoBehaviour {

        /* Funzione che ferma l'oggetto appena tocca il piano */
        private void OnCollisionEnter(Collision collision) {
            GetComponent<Rigidbody>().isKinematic = true;
        }
    }
}
