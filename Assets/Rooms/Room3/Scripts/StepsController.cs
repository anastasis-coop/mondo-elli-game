using System.Collections.Generic;
using UnityEngine;

namespace Room3
{
    public class StepsController : MonoBehaviour
    {
        public Sprite arrow;
        public int current;

        [SerializeField]
        private GameObject arrowPrefab;

        [SerializeField]
        private Transform arrowRoot;

        private int _current;

        private List<Direction> _steps = new List<Direction>();

        public List<Direction> steps
        {
            get => _steps;
            set
            {
                SetSteps(value);
            }
        }

        void Start()
        {
            current = 0;
            _current = 0;
        }

        void Update()
        {
            if (current != _current)
            {
                _current = current;
            }
        }

        public void Clear()
        {
            _steps = new List<Direction>();
            DestroyChildren();
        }

        private void DestroyChildren()
        {
            foreach (Transform child in arrowRoot)
            {
                Destroy(child.gameObject);
            }
        }

        private void SetSteps(List<Direction> newSteps)
        {
            Clear();
            int i = 1;

            foreach (Direction step in newSteps)
            {
                _steps.Add(step);

                if (step != Direction.None)
                {
                    createArrow(step);
                }
                i++;
            }
        }

        private void createArrow(Direction dir)
        {
            GameObject arrowObj = Instantiate(arrowPrefab, arrowRoot);
            arrowObj.name = "arrow";

            switch (dir)
            {
                case Direction.Up:
                    arrowObj.transform.Rotate(0, 0, 90);
                    break;
                case Direction.Down:
                    arrowObj.transform.Rotate(0, 0, -90);
                    break;
                case Direction.Right:
                    arrowObj.transform.Rotate(0, 0, 0);
                    break;
                case Direction.Left:
                    arrowObj.transform.Rotate(0, 180, 0);
                    break;
            }
        }

    }
}
