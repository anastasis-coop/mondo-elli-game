using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Room7
{
    public class ParkingCar : MonoBehaviour
    {
        private const float TWEEN_DURATION = 0.3f;

        public ParkingSpot Spot { get; set; }

        [SerializeField]
        private MeshRenderer carRenderer;

        [SerializeField]
        private Transform carTranslate;

        [SerializeField]
        private Transform carTilt;

        [SerializeField]
        private Color blue = UnityEngine.Color.blue;

        [SerializeField]
        private Color green = UnityEngine.Color.green;

        [SerializeField]
        private Color red = UnityEngine.Color.red;

        [SerializeField]
        private SpriteRenderer symbolRenderer;

        [System.Serializable]
        private class SymbolSprites
        {
            public TileSymbol Symbol;
            public Sprite[] NumberToSprite;
        }

        [SerializeField]
        private List<SymbolSprites> symbolSprites;

        private Vector3 lastPosition;
        private Vector3 delta;
        private float elapsedTime;

        public void Setup(TileColor color, int number, TileSymbol symbol)
        {
            const int colorMaterialIndex = 0;
            const string colorPropertyName = "_AlbedoColor";

            Material carMaterial = carRenderer.materials[colorMaterialIndex];
            carMaterial.SetColor(colorPropertyName, GetColor(color));

            symbolRenderer.sprite = symbolSprites.Find(entry => entry.Symbol == symbol).NumberToSprite[number];
        }

        // TODO change original code
        private Color GetColor(TileColor tileColor)
        {
            switch (tileColor)
            {
                case TileColor.blue:
                    return blue;
                case TileColor.green:
                    return green;
                case TileColor.red:
                    return red;
            }

            return UnityEngine.Color.magenta;
        }

        public void OnBeginDrag()
        {
            carTranslate
                .DOMoveY(transform.parent.position.y, TWEEN_DURATION)
                .SetEase(Ease.OutBack);

            lastPosition = transform.position;
        }

        private void Update()
        {
            elapsedTime += Time.deltaTime;

            const float SAMPLE_TIME = 0.05f;

            if (elapsedTime < SAMPLE_TIME) return;

            elapsedTime = 0;

            delta = (transform.position - lastPosition);

            lastPosition = transform.position;
        }

        public void OnDrag()
        {
            const float MAX_TILT_EULER = 30;
            const float DELTA_FOR_MAX_TILT = 4;
            const float LERP_SPEED = 20;

            float tx = Mathf.Clamp01(Mathf.Abs(delta.x) / DELTA_FOR_MAX_TILT);
            float tz = Mathf.Clamp01(Mathf.Abs(delta.z) / DELTA_FOR_MAX_TILT);

            float tiltX = Mathf.Sign(delta.x) * MAX_TILT_EULER * tx;
            float tiltZ = Mathf.Sign(delta.z) * MAX_TILT_EULER * tz;

            Quaternion tilt = Quaternion.Euler(tiltX, carTilt.eulerAngles.y, tiltZ);
            tilt = Quaternion.Lerp(carTilt.rotation, tilt, Time.deltaTime * LERP_SPEED);

            float eulerY = carTilt.localEulerAngles.y;

            carTilt.rotation = tilt;

            // Locking Y axis rotation because it seems to drift even if we don't change it directly, maybe float errors
            carTilt.localEulerAngles = new Vector3(carTilt.localEulerAngles.x, eulerY, carTilt.localEulerAngles.z);
        }

        public void OnDrop()
        {
            carTilt.rotation = Quaternion.Euler(0, carTilt.eulerAngles.y, 0);

            carTranslate
                .DOMoveY(transform.position.y, TWEEN_DURATION)
                .SetEase(Ease.OutBounce);
        }

        private void OnDestroy()
        {
            if (carTranslate != null) carTranslate.DOKill();
        }
    }
}