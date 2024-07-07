using UnityEngine;

namespace Room7
{

  public class TileInfo : MonoBehaviour
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

    public void SetTile(Tile tile) {
      row = tile.row;
      col = tile.col;
      idx = tile.idx;
      vertical = tile.vertical;
      border = tile.border;
      isTarget = tile.isTarget;
      number = tile.number;
      symbol = tile.symbol;
      color = tile.color;
      background = tile.background;
    }

  }

}