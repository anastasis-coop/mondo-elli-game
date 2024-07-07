using UnityEngine;

public class AspectRatio: MonoBehaviour
{

    public enum AspectRatioType
    {
        FreeAspect, W5H4, W4H3, W3H2, W16H9, W16H10
    };

    public AspectRatioType aspect;

    void Start()
    {
        CreateFillOutCamera();
        Camera camera = GetComponent<Camera>();
        float nativeAspect = camera.aspect;
        float wantedAspect = getAspectValue(nativeAspect);
        camera.aspect = wantedAspect;
        float w = 1f;
        float h = 1f;
        if (nativeAspect < wantedAspect)
        {
            h = nativeAspect / wantedAspect;
        }
        else
        {
            w = wantedAspect / nativeAspect;
        }
        float x = (1f - w) / 2f;
        float y = (1f - h) / 2f;
        camera.rect = new Rect(x, y, w, h);
    }

    private float getAspectValue(float currentValue)
    {
        switch (aspect)
        {
            case AspectRatioType.W5H4: return 5f / 4f;
            case AspectRatioType.W4H3: return 4f / 3f;
            case AspectRatioType.W3H2: return 3f / 2f;
            case AspectRatioType.W16H9: return 16f / 9f;
            case AspectRatioType.W16H10: return 16f / 10f;
            default: return currentValue;
        }
    }

    private void CreateFillOutCamera()
    {
        if (GameObject.Find("FillOutCamera") == null) {
            GameObject fillOutCamera = new GameObject();
            fillOutCamera.name = "FillOutCamera";
            fillOutCamera.GetComponent<Transform>().position = new Vector3(0, 0, 2000);
            Camera camera = fillOutCamera.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.depth = -1000;
        }
    }

}
