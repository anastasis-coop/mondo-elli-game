using System;
using UnityEngine;

namespace Room7
{
    public class ParkingSpot : MonoBehaviour
    {
        private Tile tile;
        public Tile Tile
        {
            get => tile;
            set => Setup(value);
        }

        [SerializeField]
        private ParkingCar carPrefab;

        [SerializeField]
        private GameObject wall;

        [SerializeField]
        private GameObject redBorder;

        public ParkingCar Car { get; set; }
        public bool Free => Car == null && !isWall;
        public bool Vertical => tile.vertical;

        public event Action<ParkingSpot> SpotBeginDrag;
        public event Action<ParkingSpot> SpotDrag;
        public event Action<ParkingSpot> SpotEndDrag;
        public event Action<ParkingSpot> SpotClick;

        private bool dragging;
        private bool invalidClick;
        private bool isWall;

        private void Setup(Tile tile)
        {
            this.tile = tile;

            bool occupied = tile.number > 0;

            if (occupied)
            {
                if (Car == null)
                {
                    Car = Instantiate(carPrefab, transform);
                }

                Car.Setup(tile.color, tile.number, tile.symbol);

            }
            else if (Car != null)
            {
                Destroy(Car.gameObject);
            }

            //HACK remove color check to discriminate wall from empty
            isWall = !occupied && tile.background == Color.white;
            wall.SetActive(isWall);
            redBorder.SetActive(tile.border);
        }


        private void OnMouseEnter()
        {
            invalidClick = dragging;
        }

        private void OnMouseUpAsButton()
        {
            if (invalidClick)
            {
                invalidClick = false;
                return;
            }

            SpotClick?.Invoke(this);
        }

        private void OnMouseBeginDrag()
        {
            SpotBeginDrag?.Invoke(this);
        }

        private void OnMouseDrag()
        {
            if (!dragging)
            {
                OnMouseBeginDrag();
                dragging = true;
            }

            SpotDrag?.Invoke(this);
        }

        private void OnMouseUp()
        {
            if (dragging)
            {
                OnMouseEndDrag();
                dragging = false;
            }
        }

        private void OnMouseEndDrag()
        {
            SpotEndDrag?.Invoke(this);
        }
    }
}
