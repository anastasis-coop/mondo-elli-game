using System;
using UnityEngine;
using UnityEngine.UI;

namespace Room3
{
    public class TilesController : MonoBehaviour
    {
        public enum TileType { None = -1, Up, Down, Left, Right, Ello, House, Grass, Street }

        [SerializeField]
        private float tileSize;

        [SerializeField]
        private GameObject elloCellPrefab;

        [SerializeField]
        private GameObject cellPrefab;

        [SerializeField]
        private Transform backgroundRoot;

        [SerializeField]
        private Transform foregroundRoot;

        public Sprite[] sprites;
        public float moveDuration = 1f;

        private Transform moveRef;
        private Action callback;
        private Vector3 startPos;
        private Vector3 targetPos;
        private float elapsed;
        private bool moving;

        void Start()
        {
            moving = false;
            callback = null;
        }

        void Update()
        {
            if (moving)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / moveDuration);
                moveRef.localPosition = Vector3.Lerp(startPos, targetPos, t);
                if (t == 1f)
                {
                    moving = false;
                    callback();
                }
            }
        }

        public void Clear()
        {
            foreach (Transform child in backgroundRoot)
                Destroy(child.gameObject);
            foreach (Transform child in foregroundRoot)
                Destroy(child.gameObject);
        }

        public GameObject AddElloCell(int x, int y)
        {
            return AddCell(elloCellPrefab, x, y, false);
        }

        public GameObject AddCell(int x, int y, TileType tileType, bool background = false)
        {
            GameObject cell = AddCell(cellPrefab, x, y, background);

            Image cellImage = cell.GetComponent<Image>();

            Sprite sprite = sprites[(int)tileType];
            cellImage.sprite = sprite;
            cellImage.SetNativeSize();

            // HACK remove sprite swap logic and use prefabs so we don't have to do this
            RectTransform rect = cell.GetComponent<RectTransform>();
            rect.pivot = sprite.pivot / rect.rect.size;

            return cell;
        }

        private GameObject AddCell(GameObject prefab, int x, int y, bool background)
        {
            Vector2 gridPos = new Vector2(x, y);
            GameObject cell = Instantiate(prefab, background ? backgroundRoot : foregroundRoot);
            cell.transform.localPosition = gridPos * tileSize;

            return cell;
        }

        public void moveToDirection(GameObject obj, Direction dir, Action onMoved)
        {
            elapsed = 0;
            callback = onMoved;
            moveRef = obj.transform;
            startPos = moveRef.localPosition;
            targetPos = startPos;
            switch (dir)
            {
                case Direction.Up: targetPos.y += tileSize; break;
                case Direction.Down: targetPos.y -= tileSize; break;
                case Direction.Left: targetPos.x -= tileSize; break;
                case Direction.Right: targetPos.x += tileSize; break;
            }
            moving = true;
        }

        public void moveToCell(GameObject obj, int x, int y)
        {
            obj.transform.localPosition = new Vector3(x * tileSize, y * tileSize, 0);
        }

        public Vector2 getCellPosition(GameObject obj)
        {
            return obj.transform.localPosition / tileSize;
        }

    }
}
