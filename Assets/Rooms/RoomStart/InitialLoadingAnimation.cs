using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InitialLoadingAnimation : MonoBehaviour
{
    public Image elloImage;
    public List<Sprite> elloImages;
    private int currentIndex = 0;
    private float timer = 0.0f;
    public float changeInterval = 0.2f;
    void Start()
    {
        
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= changeInterval)
        {
            currentIndex = (currentIndex + 1) % elloImages.Count;
            elloImage.sprite = elloImages[currentIndex];

            timer = 0.0f;
        }
    }
}
