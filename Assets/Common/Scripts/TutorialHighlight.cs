using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TutorialHighlight : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private RectTransform maskRect;
    [SerializeField] private Image blackImage;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Camera camera;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private Color blackImageColor = Color.black;

    private Vector2 maskAnchorMin;
    private Vector2 maskAnchorMax;
    private Vector2 maskAnchorPos;

    private void Awake()
    {
        blackImage.color = Color.clear;

        maskAnchorMin = maskRect.anchorMin;
        maskAnchorMax = maskRect.anchorMax;
        maskAnchorPos = maskRect.anchoredPosition;
    }

    public void MaskTransform(Transform transform, bool ignoreEnabledStates, float duration = 0.5f)
    {
        if (transform is RectTransform rect)
        {
            ExecuteNextFrame(() => MaskRectTransform(rect, ignoreEnabledStates, duration));
        }
        else
        {
            ExecuteNextFrame(() => MaskRenderers(transform, ignoreEnabledStates, duration));
        }
    }

    private void MaskRectTransform(RectTransform rect, bool ignoreEnabledStates, float duration = 0.5f)
    {
        Canvas canvas = rect.GetComponentInParent<Canvas>();

        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            MaskWorldSpaceRectTransform(rect, ignoreEnabledStates, duration);
        }
        else
        {
            MaskScreenSpaceRectTransform(rect, ignoreEnabledStates, duration);
        }
    }

    private void MaskScreenSpaceRectTransform(RectTransform rect, bool ignoreEnabledStates, float duration)
    {
        Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvas.transform, rect);

        root.SetActive(true);

        maskRect.DOKill();
        blackImage.DOKill();

        blackImage.DOColor(blackImageColor, duration);

        maskRect.DOAnchorMax(maskAnchorMax, duration);
        maskRect.DOAnchorMin(maskAnchorMin, duration);
        maskRect.DOAnchorPos(bounds.center, duration);
        maskRect.DOSizeDelta(bounds.size, duration);
    }

    private void MaskWorldSpaceRectTransform(RectTransform rect, bool ignoreEnabledStates, float duration)
    {
        Bounds bounds = GetRectTransformBounds(rect, ignoreEnabledStates);
        Rect viewportRect = GetViewportRect(camera, bounds);

        root.SetActive(true);

        maskRect.DOKill();
        blackImage.DOKill();

        blackImage.DOColor(blackImageColor, duration);

        maskRect.DOAnchorMax(viewportRect.max, duration);
        maskRect.DOAnchorMin(viewportRect.min, duration);
        maskRect.DOAnchorPos(Vector2.zero, duration);
        maskRect.offsetMin = Vector2.zero;
        maskRect.offsetMax = Vector2.zero;
    }

    public static Bounds GetRectTransformBounds(RectTransform transform, bool ignoreEnabledStates)
    {
        var corners = new Vector3[4];

        transform.GetWorldCorners(corners);
        Bounds bounds = new Bounds(corners[0], Vector3.zero);
        for (int i = 1; i < 4; ++i)
        {
            bounds.Encapsulate(corners[i]);
        }
        return bounds;
    }

    private void MaskRenderers(Transform obj, bool ignoreEnabledStates, float duration = 0.5f)
    {
        Rect viewportRect = GetViewportRect(camera, GetBounds(obj, ignoreEnabledStates));

        root.SetActive(true);

        maskRect.DOKill();
        blackImage.DOKill();

        blackImage.DOColor(blackImageColor, duration);

        maskRect.DOAnchorMax(viewportRect.max, duration);
        maskRect.DOAnchorMin(viewportRect.min, duration);
        maskRect.DOAnchorPos(Vector2.zero, duration);
        maskRect.offsetMin = Vector2.zero;
        maskRect.offsetMax = Vector2.zero;
    }

    public void ClearMask(float duration = 0.2f)
    {
        StopAllCoroutines();

        if (!root.activeSelf) return;

        maskRect.DOKill();
        blackImage.DOKill();

        var tween = blackImage.DOColor(Color.clear, duration);
        maskRect.DOAnchorMax(maskAnchorMax, duration);
        maskRect.DOAnchorMin(maskAnchorMin, duration);
        maskRect.DOAnchorPos(maskAnchorPos, duration);

        tween.onComplete = () => root.SetActive(false);
    }
    
    public void ObscureEverything()
    {
        root.SetActive(true);
        maskRect.DOSizeDelta(Vector2.zero, duration);
        blackImage.DOKill();
        blackImage.DOColor(blackImageColor, duration);
    }

    private void ExecuteNextFrame(Action callback)
    {
        StopAllCoroutines();
        StartCoroutine(ExecuteNextFrameRoutine(callback));
    }

    private IEnumerator ExecuteNextFrameRoutine(Action callback)
    {
        yield return null;

        callback?.Invoke();
    }

#if UNITY_EDITOR

    [ContextMenu(nameof(TestMask))]
    private void TestMask()
    {
        MaskTransform(UnityEditor.Selection.activeTransform, false, duration);
    }

    [ContextMenu(nameof(TestClear))]
    private void TestClear()
    {
        ClearMask();
    }

#endif

    static Vector3[] s_Corners = new Vector3[4];

    private Bounds GetBounds(Transform obj, bool ignoreEnabledStates)
    {
        var renderers = obj.GetComponentsInChildren<Renderer>(ignoreEnabledStates);

        var bounds = new Bounds();
        if (renderers.Length == 0) return bounds;

        //Encapsulate for all renderers
        foreach (var renderer in renderers)
        {
            if (!renderer.enabled && !ignoreEnabledStates) continue;
            
            
            if (bounds.size == Vector3.zero) bounds = renderer.bounds;
            else bounds.Encapsulate(renderer.bounds);
        }

        return bounds;
    }

    private Rect GetViewportRect(Camera cam, Bounds bounds)
    {
        Vector3 cen = bounds.center;
        Vector3 ext = bounds.extents;
        Vector2[] extentPoints = new Vector2[8]
        {
            cam.WorldToViewportPoint(new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z - ext.z)),
            cam.WorldToViewportPoint(new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z - ext.z)),
            cam.WorldToViewportPoint(new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z + ext.z)),
            cam.WorldToViewportPoint(new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z + ext.z)),
            cam.WorldToViewportPoint(new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z - ext.z)),
            cam.WorldToViewportPoint(new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z - ext.z)),
            cam.WorldToViewportPoint(new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z + ext.z)),
            cam.WorldToViewportPoint(new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z + ext.z))
        };
        Vector2 min = extentPoints[0];
        Vector2 max = extentPoints[0];
        foreach (Vector2 v in extentPoints)
        {
            min = Vector2.Min(min, v);
            max = Vector2.Max(max, v);
        }

        return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
    }
}