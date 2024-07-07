using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Room1
{
    public class ObjectProperties1232 : MonoBehaviour
    {
        public bool isTarget = false;
        public SpawnerforLevel1232 spawner;
        private float _acceleration = 2;
        private float currAccy;
        
        public bool Inhibited { get; set; }
        
        void Start()
        {
            currAccy = transform.position.y;
        }
        // Update is called once per frame
        void Update()
        {
            if (Inhibited) return;
            currAccy -= Time.deltaTime * _acceleration;
            transform.position = new Vector3(transform.position.x, currAccy, transform.position.z);
        }

        public void Despawn() => spawner.RemoveObject(this);
    }
}
