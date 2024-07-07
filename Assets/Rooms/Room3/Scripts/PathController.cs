using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static Room3.TilesController;
using Random = UnityEngine.Random;

namespace Room3
{
    public enum Direction { None, Up, Down, Left, Right };

    public class PathController : MonoBehaviour
    {
        public int numSteps = 1;
        public int cellsX = 9;
        public int cellsY = 6;
        public FeedbackController feedback;
        public StepsController steps;
        public TilesController tiles;
        public Timer timer;
        public Score score;
        public UnityEvent onGameOver = new UnityEvent();
        public Bomb bomb;
        public float maxTimeToAnswer = 60;
        public float timerVisibleSeconds = 10;

        private List<PathStep> pathSteps = new();
        private int startX;
        private int startY;
        private int targetX;
        private int targetY;
        private bool showArrows;

        private enum StateType { Stop, Edit, Play };
        private StateType state;
        private GameObject ello;
        private Animator elloAnimator;

        private bool _cachedExplode;

        private const string elloMoveBool = "Moving";

        void Start()
        {
            state = StateType.Stop;

            bomb.onExplode.AddListener(OnBombExploded);
        }

        private void OnBombExploded()
        {
            if (state is not StateType.Edit || timer.itIsEnd)
            {
                _cachedExplode = true;
                return;
            }

            feedback.wrongAnswerFeedback();
            score.MissedCounter++;

            _cachedExplode = false;

            resetPath();
            createNewPath();
            addPathTiles();

            timer.activation = true;
            bomb.timeoutSeconds = maxTimeToAnswer;
            bomb.gameObject.SetActive(true);
            bomb.SetVisibleWhenReaches(10);
            bomb.StartCountdown();
        }

        void Update()
        {
            if (timer.itIsEnd && state == StateType.Edit)
            {
                tiles.enabled = false;
                CancelInvoke(nameof(playNextStep));
                state = StateType.Stop;
                onGameOver.Invoke();
            }
        }

        public void prepareGame(bool showArrows)
        {
            this.showArrows = showArrows;

            initSteps();
            createNewPath();
            addPathTiles();
        }

        public void startGame()
        {
            state = StateType.Edit;

            bomb.timeoutSeconds = maxTimeToAnswer;
            bomb.gameObject.SetActive(true);
            bomb.SetVisibleWhenReaches(timerVisibleSeconds);
            bomb.StartCountdown();
        }

        private void initSteps()
        {
            List<Direction> stepsList = new();
            for (int i = 0; i < numSteps; i++)
            {
                stepsList.Add(Direction.None);
            }
            steps.steps = stepsList;
        }

        private void addStep(Direction dir)
        {
            if (state == StateType.Edit)
            {
                List<Direction> stepsList = steps.steps;
                int index = stepsList.FindIndex(s => s == Direction.None);
                if (index >= 0)
                {
                    stepsList[index] = dir;
                }
                steps.steps = stepsList;
            }
        }

        public void arrowUpPressed()
        {
            addStep(Direction.Down);
        }

        public void arrowDownPressed()
        {
            addStep(Direction.Up);
        }

        public void arrowLeftPressed()
        {
            addStep(Direction.Right);
        }

        public void arrowRightPressed()
        {
            addStep(Direction.Left);
        }

        public void playPressed()
        {
            if (state == StateType.Edit)
            {
                timer.activation = false;
                bomb.StopCountdown();
                bomb.gameObject.SetActive(false);

                state = StateType.Play;
                steps.current = 0;
                playNextStep();
            }
        }

        private void playNextStep()
        {
            steps.current++;
            if (checkIfValid())
            {
                if ((steps.current > steps.steps.Count) || (steps.steps[steps.current - 1] == Direction.None))
                {
                    elloAnimator.SetBool(elloMoveBool, false);
                    endPath();
                }
                else
                {
                    elloAnimator.SetBool(elloMoveBool, true);
                    tiles.moveToDirection(ello, steps.steps[steps.current - 1], playNextStepDelayed);
                }
            }
            else
            {
                wrongCell();
            }
        }

        private void playNextStepDelayed()
        {
            Invoke(nameof(playNextStep), 0.25f);
        }

        private bool checkIfValid()
        {
            Vector2 cell = tiles.getCellPosition(ello);
            int x = (int)cell.x;
            int y = (int)cell.y;
            return isTarget(x, y) || isPathStep(x, y);
        }

        private bool checkIfTarget()
        {
            Vector2 cell = tiles.getCellPosition(ello);
            int x = (int)cell.x;
            int y = (int)cell.y;
            return isTarget(x, y);
        }

        private void endPath()
        {
            if (checkIfTarget())
            {
                feedback.rightAnswerFeedback();
                score.RightCounter++;
                resetPath();
                createNewPath();
                addPathTiles();
            }
            else
            {
                wrongCell();
            }

            if (_cachedExplode)
            {
                OnBombExploded();
                return;
            }

            if (!timer.itIsEnd)
            {
                timer.activation = true;
                bomb.timeoutSeconds = maxTimeToAnswer;
                bomb.gameObject.SetActive(true);
                bomb.StartCountdown();
            }
        }

        private void wrongCell()
        {
            resetPath();
            feedback.wrongAnswerFeedback();
            score.WrongCounter++;
            tiles.moveToCell(ello, startX, startY);
            elloAnimator.SetBool(elloMoveBool, false);
        }

        private void resetPath()
        {
            initSteps();
            steps.current = 0;
            state = StateType.Edit;

            bomb.gameObject.SetActive(true);
            bomb.StartCountdown();
        }

        public void trashPressed()
        {
            if (state == StateType.Edit)
            {
                initSteps();
            }
        }

        private class PathStep
        {
            public int x;
            public int y;
            public Direction dir;

            public PathStep(int x, int y, Direction dir)
            {
                this.x = x;
                this.y = y;
                this.dir = dir;
            }
        }

        private void createNewPath()
        {
            pathSteps.Clear();

            int x = 0;
            int y = 0;
            Direction dir = Direction.None;

            for (int i = 0; i < numSteps; i++)
            {
                dir = GetRandomFreeDirection(x, y, pathSteps);

                pathSteps.Add(new(x, y, dir));

                if (dir == Direction.None) break;
                else if (dir == Direction.Up) y++;
                else if (dir == Direction.Down) y--;
                else if (dir == Direction.Left) x--;
                else if (dir == Direction.Right) x++;
            }

            // Debug.Log("x="+x+" y="+y+" Path generato!!!");
            // calcola l'estensione occupata dal percorso (target compreso)
            int minX = x;
            int minY = y;
            int maxX = x;
            int maxY = y;
            foreach (PathStep pathStep in pathSteps)
            {
                minX = Mathf.Min(minX, pathStep.x);
                minY = Mathf.Min(minY, pathStep.y);
                maxX = Mathf.Max(maxX, pathStep.x);
                maxY = Mathf.Max(maxY, pathStep.y);
            }
            // Debug.Log("minX="+minX+" maxX="+maxX);
            // Debug.Log("minY="+minY+" maxY="+maxY);
            // centra il percorso nelle caselle
            int extX = maxX - minX + 1;
            int extY = maxY - minY + 1;
            // Debug.Log("extX="+extX+" extY="+extY);
            int newMinX = (cellsX - extX) / 2;
            int newMinY = (cellsY - extY) / 2;
            // Debug.Log("newMinX="+newMinX+" newMinY="+newMinY);
            int diffX = newMinX - minX;
            int diffY = newMinY - minY;
            // Debug.Log("diffX="+diffX+" diffY="+diffY);
            foreach (PathStep pathStep in pathSteps)
            {
                pathStep.x += diffX;
                pathStep.y += diffY;
                // Debug.Log("x="+pathStep.x+" y="+pathStep.y+" dir="+pathStep.dir);
            }
            x += diffX;
            y += diffY;
            // Debug.Log("x="+x+" y="+y+" Path generato!!!");
            startX = pathSteps[0].x;
            startY = pathSteps[0].y;
            targetX = x;
            targetY = y;
        }

        private Direction GetRandomFreeDirection(int x, int y, List<PathStep> steps)
        {
            List<Direction> available = new() { Direction.Up, Direction.Down, Direction.Left, Direction.Right };

            foreach (PathStep step in steps)
            {
                if (step.x == x)
                {
                    if (step.y == y + 1) available.Remove(Direction.Up);
                    else if (step.y == y - 1) available.Remove(Direction.Down);
                }
                else if (step.y == y)
                {
                    if (step.x == x - 1) available.Remove(Direction.Left);
                    else if (step.x == x + 1) available.Remove(Direction.Right);
                }
            }

            return available.Count > 0 ? available[Random.Range(0, available.Count)] : Direction.None;
        }

        void addPathTiles()
        {
            tiles.Clear();
            foreach (PathStep pathStep in pathSteps)
            {
                tiles.AddCell(pathStep.x, pathStep.y, TileType.Street, true);

                if (showArrows)
                {
                    var tileType = pathStep.dir switch
                    {
                        Direction.Up => TileType.Up,
                        Direction.Down => TileType.Down,
                        Direction.Left => TileType.Left,
                        Direction.Right => TileType.Right,
                        _ => TileType.None,
                    };

                    tiles.AddCell(pathStep.x, pathStep.y, tileType);
                }
            }
            tiles.AddCell(targetX, targetY, TileType.House);
            ello = tiles.AddElloCell(startX, startY);
            elloAnimator = ello.GetComponent<Animator>();
        }

        bool isTarget(int x, int y)
        {
            return ((x == targetX) && (y == targetY));
        }

        bool isPathStep(int x, int y)
        {
            return (pathSteps.Find(c => (c.x == x) && (c.y == y)) != null);
        }

    }
}
