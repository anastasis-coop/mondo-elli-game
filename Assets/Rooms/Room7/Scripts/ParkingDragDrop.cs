using System;
using System.Collections.Generic;
using UnityEngine;

namespace Room7
{
    public class ParkingDragDrop : MonoBehaviour
    {
        [SerializeField]
        private Camera mainCamera;

        [SerializeField]
        private ParkingSpot poolSpot;
        public ParkingSpot PoolSpot
        {
            get => poolSpot;
            set => poolSpot = value;
        }

        [SerializeField]
        private List<ParkingSpot> gridSpots;
        public List<ParkingSpot> GridSpots
        {
            get => gridSpots;
            set => gridSpots = value;
        }

        [SerializeField]
        private Transform dragRoot;

        private Tile poolTile;
        public Tile PoolTile
        {
            get => poolTile;
            set
            {
                poolTile = value;
                ResetPoolSpot();
            }
        }

        private Plane floor;

        private const int MAX_RAYCAST_HITS = 16;

        private RaycastHit[] hits = new RaycastHit[MAX_RAYCAST_HITS];

        private Vector3 MouseOnXZPlane
        {
            get
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

                if (!floor.Raycast(ray, out float distance)) return default;

                return ray.GetPoint(distance);
            }
        }

        public event Action<ParkingSpot> SpotValidDrop;

        private void OnEnable()
        {
            floor = new Plane(Vector3.up, 0);

            poolSpot.SpotBeginDrag += OnSpotBeginDrag;
            poolSpot.SpotDrag += OnSpotDrag;
            poolSpot.SpotEndDrag += OnSpotEndDrag;

            foreach (ParkingSpot spot in gridSpots)
            {
                spot.SpotClick += OnGridSpotClick;
            }
        }

        private void OnSpotBeginDrag(ParkingSpot spot)
        {
            ParkingCar car = spot.Car;

            if (car == null) return;

            car.Spot = null;

            car.transform.SetParent(dragRoot);
            car.transform.position = MouseOnXZPlane;
            car.OnBeginDrag();
        }


        private void OnSpotDrag(ParkingSpot spot)
        {
            ParkingCar car = spot.Car;

            if (car == null) return;

            car.transform.position = MouseOnXZPlane;
            car.OnDrag();
        }

        private void OnSpotDrop(ParkingSpot from, ParkingSpot to)
        {
            if (from.Free || !to.Free || from.Vertical != to.Vertical) return;

            from.Car.OnDrop();

            MoveCar(from, to);

            SpotValidDrop?.Invoke(to);
        }

        private void OnSpotEndDrag(ParkingSpot spot)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            int hitCount = Physics.RaycastNonAlloc(ray, hits);

#if UNITY_EDITOR
            if (hitCount == MAX_RAYCAST_HITS)
                Debug.LogWarning("RaycastNonAlloc max hits reached.");
#endif

            if (hitCount > 0)
            {
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider == null) continue;

                    ParkingSpot dropSpot = hit.collider.GetComponent<ParkingSpot>();

                    if (dropSpot == null || dropSpot == spot) continue;

                    OnSpotDrop(spot, dropSpot);
                    break;
                }
            }

            ParkingCar car = spot.Car;

            if (car == null) return;

            if (car.Spot == null)
            {
                Destroy(car.gameObject);

                if (spot == poolSpot)
                {
                    spot.Car = null; // Car will be destroyed, poolspot needs to spawn a new one
                    ResetPoolSpot();
                }
                else
                {
                    spot.Car = null;
                }
            }
        }

        private void OnGridSpotClick(ParkingSpot spot)
        {
            if (!spot.Free || poolSpot.Free || poolSpot.Vertical != spot.Vertical) return;

            MoveCar(poolSpot, spot);

            SpotValidDrop?.Invoke(spot);
        }

        private void MoveCar(ParkingSpot from, ParkingSpot to)
        {
            ParkingCar car = from.Car;

            if (car == null) return;

            if (from == poolSpot)
            {
                from.Car = null;
                // Don't reset pool spot, it's a valid drop
                //ResetPoolSpot();
            }
            else if (from != to)
            {
                from.Car = null;
            }

            to.Car = car;
            car.Spot = to;

            car.transform.SetParent(to.transform);
            car.transform.localPosition = Vector3.zero;
        }

        private void ResetPoolSpot()
        {
            poolSpot.Tile = PoolTile;
        }

        private void OnDisable()
        {
            if (poolSpot != null)
            {
                poolSpot.SpotBeginDrag -= OnSpotBeginDrag;
                poolSpot.SpotDrag -= OnSpotDrag;
                poolSpot.SpotEndDrag -= OnSpotEndDrag;
                poolSpot.SpotClick -= OnGridSpotClick;
            }

            foreach (ParkingSpot spot in gridSpots)
            {
                if (spot == null) continue;

                spot.SpotClick -= OnGridSpotClick;
            }
        }
    }
}