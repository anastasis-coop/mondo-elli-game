using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

namespace Room1
{
    public class BouncingBall : MonoBehaviour
    {

        public bool activation = false;

        public float height= 10f;
        public float speed = 1f;

        private double angle = 0f;

        void Start()
        {
            speed = UnityEngine.Random.Range(2f, 5f);
        }

        void Update()
        {
            if (activation || angle > 0)
            {
                angle += speed * Time.deltaTime;
                if (angle>Math.PI)
                {
                    if (activation)
                    {
                        angle -= Math.PI;
                    } else
                    {
                        angle = 0;
                    }
                }
                Vector3 pos = transform.localPosition;
                pos.y = (float)Math.Abs(Math.Sin(angle)) * height;
                transform.localPosition = pos;
            }
            if (activation && angle==0)
            {
                angle = 0.000001;
            }
        }
    }
}
