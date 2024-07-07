using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Room1
{
    public class FlickeringLight : MonoBehaviour
    {
        public GameObject reel;

        private float timer;
        private bool activation = false;
        private Color color;

        private void Start()
        {
        }

        public void startFlickering(Color c)
        {
            activation = true;
            color = c;
            StartCoroutine(FlickerLight());
        }

        public void stopFlickering()
        {
            activation = false;
        }

        IEnumerator FlickerLight()
        {
            reel.GetComponent<MeshRenderer>().material.color = Color.gray;
            timer = UnityEngine.Random.Range(0.1f, 1);
            yield return new WaitForSeconds(timer);

            if (activation)
            {
                reel.GetComponent<MeshRenderer>().material.color = color;
                timer = UnityEngine.Random.Range(0.1f, 1);
                yield return new WaitForSeconds(timer);
                if (activation)
                {
                    StartCoroutine(FlickerLight());
                } else
                {
                    reel.GetComponent<MeshRenderer>().material.color = Color.gray;
                }
            }
        }
    }
}
