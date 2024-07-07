using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game {
    public class GroundHighlight : MonoBehaviour {

        public GameObject highlightObj;
        public float animSpeed;
        public bool traversable;

        private bool isPathBlock;

        private Color colorH;
        private Color colorB;

        private float time;
        private bool fadeOn;
        private bool fadeOff;
        private Renderer r;
        private bool isHighlighted;

        void Awake() {
            isPathBlock = false;
            colorB = new Color(0, 0, 0, 0);
            colorH = Color.clear;

            r = highlightObj.GetComponent<Renderer>();

            fadeOn = false;
            fadeOff = false;
            time = 0;
            
            r.material.color = colorB;
            isHighlighted = false;

            highlightObj.SetActive(true);
        }

        public bool GetPathState() {
            return isPathBlock;
        }

        public void LightUp() {
            if (colorH.Equals(Color.clear))
                colorH = GameController.Instance.pathHandler.DefaultColorH;
            r.material.color = colorH;
            isHighlighted = true;
        }

        public void SetPathState(bool s) {
            isPathBlock = s;
        }

        public void SetColorH(Color h) {
            colorH = h;
        }

        public Color GetColorH() {
            return colorH;
        }

        public void HighlightOn() {
            if (colorH.Equals(Color.clear))
                colorH = GameController.Instance.pathHandler.DefaultColorH;

            if (isHighlighted && !fadeOff && !fadeOn) {
                r.material.color = colorH;
                return;
            }
            fadeOn = true;
            fadeOff = false;
        }

        public void HighlightOff() {
            if (!isHighlighted && !fadeOff && !fadeOn)
                return;
            fadeOff = true;
            fadeOn = false;
        }

        public bool isHightlighted() {
            return isHighlighted;
        }

        void Update() {
            if (fadeOn) {
                time += Time.deltaTime * animSpeed;
                Color col = Color.Lerp(colorB, colorH, time);
                if (time > 1) {
                    fadeOn = false;
                    col = colorH;
                    time = 0;
                    isHighlighted = true;
                }
                r.material.color = col;
            }

            if (fadeOff) {
                time += Time.deltaTime * animSpeed;
                Color col = Color.Lerp(colorH, colorB, time);
                if (time > 1) {
                    fadeOff = false;
                    col = colorB;
                    time = 0;
                    isHighlighted = false;
                }
                r.material.color = col;
            }
        }
    }
}
