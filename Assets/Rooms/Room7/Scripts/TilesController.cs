using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Room7
{

    public enum TileSymbol { circle, cross, star, triangle };
    public enum TileColor { blue, green, red };

    public class Tile
    {

        public int row;
        public int col;
        public int idx;
        public bool vertical;
        public bool border;
        public bool isTarget;
        public int number;
        public TileSymbol symbol;
        public TileColor color;
        public Color background;

        internal Tile(int row, int col, int idx, bool vertical)
        {
            this.row = row;
            this.col = col;
            this.idx = idx;
            this.vertical = vertical;
        }

        internal void FillRandomly()
        {
            isTarget = false;
            number = Random.Range(2, 5);
            symbol = (TileSymbol)Random.Range(0, Enum.GetValues(typeof(TileSymbol)).Length);
            color = (TileColor)Random.Range(0, Enum.GetValues(typeof(TileColor)).Length);
            background = Color.black;
            border = false;
        }

        internal string getTileName()
        {
            if (number > 0)
            {
                return number + " " + color + " " + symbol;
            }
            else
            {
                if (background == Color.white)
                {
                    return "White";
                }
                else
                {
                    return "Gray";
                }
            }
        }

    }

    class Grid
    {

        internal List<List<List<Tile>>> tiles;

        internal Grid()
        {
            tiles = new List<List<List<Tile>>>();
        }

        internal void FillWithRandomTiles(int gridRows, int gridCols)
        {
            tiles.Clear();
            for (int row = 0; row < gridRows; row++)
            {
                List<List<Tile>> rowList = new List<List<Tile>>();
                for (int col = 0; col < gridCols; col++)
                {
                    bool vertical = (Random.value < 0.5f);
                    List<Tile> block = new List<Tile>();
                    Tile tile0 = new Tile(row, col, 0, vertical);
                    Tile tile1 = new Tile(row, col, 1, vertical);
                    tile0.FillRandomly();
                    tile1.FillRandomly();
                    block.Add(tile0);
                    block.Add(tile1);
                    rowList.Add(block);
                }
                tiles.Add(rowList);
            }
        }

        internal void ColorSomeTiles(int howMany, Color color, bool makeTarget)
        {
            int changed = 0;
            int loops = 0;
            while (changed < howMany && loops < 50)
            {
                int row = Random.Range(0, tiles.Count);
                int col = Random.Range(0, tiles[row].Count);
                int idx = Random.Range(0, tiles[row][col].Count);
                Tile tile = tiles[row][col][idx];
                if (tile.number > 0 || !tile.background.Equals(color))
                {
                    List<Tile> adjacentTiles = GetAdjacentTiles(tile);
                    if (adjacentTiles.Find(item => item.number == 0 && item.background.Equals(color)) == null)
                    {
                        tile.number = 0;
                        tile.background = color;
                        tile.isTarget = makeTarget;
                        changed++;
                    }
                }
                loops++;
            }
        }

        private List<Tile> GetTilesUp(Tile tile)
        {
            List<Tile> result = new List<Tile>();
            if (tile.idx == 1 && !tile.vertical)
            {
                result.Add(tiles[tile.row][tile.col][0]);
            }
            else if (tile.row > 0)
            {
                List<Tile> next = tiles[tile.row - 1][tile.col];
                if (tile.idx == 0 && next[0].vertical)
                {
                    result.Add(next[0]);
                }
                if (tile.idx == 1 || !tile.vertical || !next[0].vertical)
                {
                    result.Add(next[1]);
                }
            }
            return result;
        }

        private List<Tile> GetTilesDown(Tile tile)
        {
            List<Tile> result = new List<Tile>();
            if (tile.idx == 0 && !tile.vertical)
            {
                result.Add(tiles[tile.row][tile.col][1]);
            }
            else if (tile.row < tiles.Count - 1)
            {
                List<Tile> next = tiles[tile.row + 1][tile.col];
                if (tile.idx == 1 && next[0].vertical)
                {
                    result.Add(next[1]);
                }
                if (tile.idx == 0 || !tile.vertical || !next[0].vertical)
                {
                    result.Add(next[0]);
                }
            }
            return result;
        }

        private List<Tile> GetTilesLeft(Tile tile)
        {
            List<Tile> result = new List<Tile>();
            if (tile.idx == 1 && tile.vertical)
            {
                result.Add(tiles[tile.row][tile.col][0]);
            }
            else if (tile.col > 0)
            {
                List<Tile> next = tiles[tile.row][tile.col - 1];
                if (tile.idx == 0 && !next[0].vertical)
                {
                    result.Add(next[0]);
                }
                if (tile.idx == 1 || tile.vertical || next[0].vertical)
                {
                    result.Add(next[1]);
                }
            }
            return result;
        }

        private List<Tile> GetTilesRight(Tile tile)
        {
            List<Tile> result = new List<Tile>();
            if (tile.idx == 0 && tile.vertical)
            {
                result.Add(tiles[tile.row][tile.col][1]);
            }
            else if (tile.col < tiles[tile.row].Count - 1)
            {
                List<Tile> next = tiles[tile.row][tile.col + 1];
                if (tile.idx == 1 && !next[0].vertical)
                {
                    result.Add(next[1]);
                }
                if (tile.idx == 0 || tile.vertical || next[0].vertical)
                {
                    result.Add(next[0]);
                }
            }
            return result;
        }

        internal Tile GetTile(int row, int col, int idx)
        {
            return tiles[row][col][idx];
        }

        internal List<Tile> GetTargets()
        {
            List<Tile> result = new List<Tile>();
            tiles.ForEach(row => {
                row.ForEach(block => {
                    block.ForEach(item => {
                        if (item.isTarget)
                        {
                            result.Add(item);
                        }
                    });
                });
            });
            return result;
        }

        internal List<Tile> GetAdjacentTiles(Tile tile)
        {
            List<Tile> result = new List<Tile>();
            result.AddRange(GetTilesUp(tile));
            result.AddRange(GetTilesDown(tile));
            result.AddRange(GetTilesLeft(tile));
            result.AddRange(GetTilesRight(tile));
            return result;
        }

        internal bool AllTargetsHaveSameOrientation()
        {
            bool someHorizontal = false;
            bool someVertical = false;
            GetTargets().ForEach(target => {
                if (target.vertical)
                {
                    someVertical = true;
                }
                else
                {
                    someHorizontal = true;
                }
            });
            return !(someHorizontal && someVertical);
        }

    }

    public class TilesController : MonoBehaviour
    {
        const int TileSize = 12; // Switching to 3D size

        public int GridRows = 4;
        public int GridCols = 5;

        public float timeout = 7f;
        public FeedbackController feedback;

        public Transform GridRoot;
        public int trial = 0;

        public Vector3 draggablePos;
        public Vector3 draggablePosWithBomb;

        public Timer timer;
        public Score score;
        public Bomb bomb;
        public UnityEvent onGameOver = new UnityEvent();

        [SerializeField]
        private ParkingSpot spotPrefab;

        [SerializeField]
        private ParkingDragDrop parkingDragDrop;

        [SerializeField]
        private Transform sourceBorder;

        private bool useBomb = false;
        private bool canHaveRedBorder = false;

        private Tile source;
        private Grid grid;

        public void PrepareGame(bool minorIs2, bool majorIs3)
        {
            useBomb = minorIs2;
            canHaveRedBorder = majorIs3;

            if (useBomb)
            {
                bomb.timeoutSeconds = timeout;
                bomb.SetVisibleWhenReaches(5);
                bomb.onExplode.AddListener(onExplode);
            }
            trial = 0;

            parkingDragDrop.enabled = false;

            RemoveChildren(GridRoot);
            PrepareGrid();
        }

        public void PrepareTutorial(bool vertical, bool red = false)
        {
            const int VERTICAL_SEED = 69420;
            const int HORIZONTAL_SEED = 42069;

            Random.InitState(vertical ? VERTICAL_SEED : HORIZONTAL_SEED);

            parkingDragDrop.enabled = false;
            canHaveRedBorder = red;

            RemoveChildren(GridRoot);
            PrepareGrid(red);
        }

        private void PrepareGrid(bool forceRed = false)
        {
            trial++;
            // Create grid
            int loops = 0;
            do
            {
                grid = new Grid();
                grid.FillWithRandomTiles(GridRows, GridCols);
                grid.ColorSomeTiles(Random.Range(5, 11), Color.white, false);
                grid.ColorSomeTiles(Random.Range(5, 11), Color.gray, true);
                loops++;
            } while (grid.AllTargetsHaveSameOrientation() && loops < 10f);
            // Create source
            source = new Tile(-1, -1, -1, Random.value < 0.5f);
            source.FillRandomly();
            if (canHaveRedBorder)
            {
                source.border = forceRed || Random.value < 0.3333f;
            }
            // Check targets
            List<Tile> targets = grid.GetTargets();
            if (NoRightAnswers(source, targets))
            {
                MakeOneTargetRight(source, targets);
            }
            // Count right answers (debug only)
            Debug.Log(CountRightAnswers(source, targets) + " right answers");
            // Create unity objects
            CreateGridObjects(grid);

            GameObject obj = CreateTile(useBomb ? draggablePosWithBomb : draggablePos, source);

            sourceBorder.localEulerAngles = Vector3.up * (source.vertical ? 0 : 90);

            parkingDragDrop.PoolSpot = obj.GetComponentInChildren<ParkingSpot>();
            parkingDragDrop.PoolTile = source;

            parkingDragDrop.SpotValidDrop += TargetReceivedTile;
        }

        public void StartGame()
        {
            parkingDragDrop.enabled = true;

            if (useBomb)
            {
                bomb.gameObject.SetActive(true);
                bomb.StartCountdown();
            }
        }

        private void CreateGridObjects(Grid grid)
        {
            Vector3 topleft = new Vector3(-TileSize * GridCols / 2 + TileSize / 2, 0, (TileSize - 1) * GridRows / 2 - TileSize / 2);
            Vector3 pos = Vector3.zero;

            grid.tiles.ForEach(row => {
                pos.x = topleft.x;
                row.ForEach(block => {

                    CreateTwoTilesBlock(pos, block);
                    pos.x += TileSize;
                });
                pos.z -= TileSize; // 3D is on the XZ plane
            });
        }

        private void CreateTwoTilesBlock(Vector3 pos, List<Tile> block)
        {
            if (block[0].vertical)
            {
                CreateTile(new Vector3(pos.x - TileSize / 4, pos.y, pos.z), block[0]);
                CreateTile(new Vector3(pos.x + TileSize / 4, pos.y, pos.z), block[1]);
            }
            else
            {
                CreateTile(new Vector3(pos.x, pos.y, pos.z + TileSize / 4), block[0]);
                CreateTile(new Vector3(pos.x, pos.y, pos.z - TileSize / 4), block[1]);
            }
        }

        private GameObject CreateTile(Vector3 pos, Tile tile)
        {
            GameObject tileObj = new GameObject();
            tileObj.name = tile.getTileName();
            tileObj.transform.SetParent(GridRoot);
            tileObj.transform.localScale = Vector3.one;
            tileObj.transform.localPosition = pos; // No more RectTransform

            AddSpot(tileObj, tile);

            TileInfo info = tileObj.AddComponent<TileInfo>();
            info.SetTile(tile);
            return tileObj;
        }

        private bool isRightAnswer(Tile source, Tile target)
        {
            if (!target.isTarget || target.vertical != source.vertical) return false;
            
            List<Tile> adjacentTiles = grid.GetAdjacentTiles(target);
                
            if (source.border)
                // Check the number of items
                return (adjacentTiles.Find(item => item.number > 0 && !item.number.Equals(source.number)) == null);

            if (source.vertical)
                // Check the symbol
                return (adjacentTiles.Find(item => item.number > 0 && !item.symbol.Equals(source.symbol)) == null);

            // Check the color
            return (adjacentTiles.Find(item => item.number > 0 && !item.color.Equals(source.color)) == null);

        }

        private bool NoRightAnswers(Tile source, List<Tile> targets)
        {
            return (targets.Find(target => isRightAnswer(source, target)) == null);
        }

        private int CountRightAnswers(Tile source, List<Tile> targets)
        {
            int count = 0;
            targets.ForEach(target => {
                if (isRightAnswer(source, target))
                    count++;
            });
            return count;
        }

        private void MakeTargetRight(Tile source, Tile target)
        {
            List<Tile> adjacentTiles = grid.GetAdjacentTiles(target);
            List<Tile> tilesWithSymbols = adjacentTiles.FindAll(item => item.number > 0);
            if (tilesWithSymbols.Count == 0)
            {
                tilesWithSymbols.Add(adjacentTiles[UnityEngine.Random.Range(0, adjacentTiles.Count)]);
            }
            tilesWithSymbols.ForEach(tile => {
                tile.FillRandomly();
                if (source.border)
                {
                    // Forcing same number of items
                    tile.number = source.number;
                }
                else
                {
                    if (source.vertical)
                    {
                        // Forcing same symbol
                        tile.symbol = source.symbol;
                    }
                    else
                    {
                        // Forcing same color
                        tile.color = source.color;
                    }
                }
            });
        }

        private void MakeOneTargetRight(Tile source, List<Tile> targets)
        {
            List<Tile> targetsSameOrientation = targets.FindAll(target => target.vertical == source.vertical);
            MakeTargetRight(source, targetsSameOrientation[Random.Range(0, targetsSameOrientation.Count)]);
        }

        private void AddSpot(GameObject obj, Tile tile)
        {
            ParkingSpot spot = Instantiate(spotPrefab); // Using prefabs
            spot.transform.SetParent(obj.transform);
            spot.transform.localPosition = Vector3.zero;
            spot.transform.localScale = Vector3.one;

            if (tile.vertical)
            {
                spot.transform.localEulerAngles = new Vector3(0, 90, 0); // 3D is on XZ plane
            }

            spot.Tile = tile; // Custom component to handle visuals

            parkingDragDrop.GridSpots.Add(spot);
        }

        private void rightAnswer()
        {
            feedback.rightAnswerFeedback();
            score.RightCounter++;
        }

        private void wrongAnswer()
        {
            feedback.wrongAnswerFeedback();
            score.WrongCounter++;
        }

        private void missedAnswer()
        {
            feedback.wrongAnswerFeedback();
            score.MissedCounter++;
        }

        private void onExplode()
        {
            missedAnswer();

            CancelInvoke(nameof(CleanStep));
            bomb.gameObject.SetActive(false);
            Invoke(nameof(CleanStep), bomb.explosionSeconds);
        }

        void TargetReceivedTile(ParkingSpot spot)
        {
            Tile target = spot.Tile;

            if (isRightAnswer(source, target))
            {
                rightAnswer();
            }
            else
            {
                wrongAnswer();
            }

            if (useBomb)
            {
                bomb.StopCountdown();
            }

            CancelInvoke(nameof(CleanStep));
            Invoke(nameof(CleanStep), 1f);
        }

        public void CleanStep()
        {
            if (useBomb)
            {
                bomb.StopCountdown();
                bomb.gameObject.SetActive(false);
            }

            parkingDragDrop.enabled = false;
            parkingDragDrop.SpotValidDrop -= TargetReceivedTile;
            RemoveChildren(GridRoot);
            Invoke(nameof(NextStep), 0.1f);
        }
        public void NextStep()
        {
            PrepareGrid();
            StartGame();
        }

        private void RemoveChildren(Transform parent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Destroy(parent.GetChild(i).gameObject);
            }

            parkingDragDrop.GridSpots.Clear();
        }

        public void EndExercise()
        {
            parkingDragDrop.enabled = false;
            if (useBomb)
            {
                bomb.StopCountdown();
            }
            onGameOver.Invoke();
        }

        public ParkingSpot FindOneRightAnswerSpot()
        {
            foreach (ParkingSpot spot in parkingDragDrop.GridSpots)
            {
                if (isRightAnswer(source, spot.Tile))
                    return spot;
            }

            return null;
        }
    }

}