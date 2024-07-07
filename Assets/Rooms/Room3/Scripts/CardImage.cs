using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Room3 {
  public class CardImage : MonoBehaviour
  {

    public Image layer1;
    public Image layer2;
    public Image layer34;
    public Sprite[] sprites_layer1;
    public Sprite[] sprites_layer2;
    public Sprite[] sprites_layer34;

    private List<int> layerIndexes;

    // Start is called before the first frame update
    void Start()
    {
      layerIndexes = new List<int>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void extractRandomLayers()
    {
      layerIndexes = new List<int>();
      for (int i = 0; i < 4; i++)
      {
        layerIndexes.Add(Random.Range(0, 3));
      }
      layer1.sprite = sprites_layer1[layerIndexes[0]];
      layer2.sprite = sprites_layer2[layerIndexes[1]];
      layer34.sprite = sprites_layer34[layerIndexes[2] * 3 + layerIndexes[3]];
    }

    public List<int> getLayers()
    {
      return layerIndexes;
    }

  }
}
