using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Room1
{
    public class CanvasAspectRatio : MonoBehaviour
    {

        public AspectRatio aspectRatio;

        private float getAspectValue(float currentValue)
        {
            switch (aspectRatio.aspect)
            {
                case AspectRatio.AspectRatioType.W5H4: return 5f / 4f;
                case AspectRatio.AspectRatioType.W4H3: return 4f / 3f;
                case AspectRatio.AspectRatioType.W3H2: return 3f / 2f;
                case AspectRatio.AspectRatioType.W16H9: return 16f / 9f;
                case AspectRatio.AspectRatioType.W16H10: return 16f / 10f;
                default: return currentValue;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            Canvas canvas = GetComponent<Canvas>().rootCanvas;
            RectTransform rect = GetComponent<RectTransform>();
            float w = canvas.pixelRect.width;
            float h = canvas.pixelRect.height;
            Debug.Log("Canvas DIM = " + w + " x " + h);
            float aspect = getAspectValue(w / h);
            float aw = h * aspect;
            float ah = w / aspect;
            Vector2 middle = new Vector2(0.5f, 0.5f);
            rect.anchorMin = middle;
            rect.anchorMax = middle;
            rect.pivot = middle;
            if (aw < w)
            {
                Debug.Log("Set size " + aw + " x " + h);
                rect.sizeDelta = new Vector2(aw, h);
            }
            else
            {
                Debug.Log("Set size " + w + " x " + ah);
                rect.sizeDelta = new Vector2(w, ah);
            }
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}
