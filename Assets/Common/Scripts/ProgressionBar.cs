using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressionBar : MonoBehaviour
{
    [SerializeField, Range(0, 1)] private float fillAmount;

    public float FillAmount
    {
        get => fillAmount;
        set
        {
            fillAmount = value;
            ApplyFillAmount();
        }
    }

    [SerializeField] private Image[] fills;

    [SerializeField] private Transform indicator;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // if (!Application.isPlaying)
        // {
        //     ApplyFillAmount();
        //     foreach (Image fill in fills)
        //     {
        //         UnityEditor.EditorUtility.SetDirty(fill);
        //     }
        // }
    }
#endif

    private void ApplyFillAmount()
    {
        // Calculate local x of the fill

        Rect rect = ((RectTransform)transform).rect;
        float fillX = rect.xMin + rect.width * fillAmount;

        float lastFilledWorldX = 0;

        foreach (Image fill in fills)
        {
            // Fill all Image fills based on their relative bounds

            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(transform, fill.transform);
            float localFill = Mathf.InverseLerp(bounds.min.x, bounds.max.x, fillX);
            fill.fillAmount = Mathf.Clamp01(localFill);

            // Keep track of the last filled fill
            if (localFill > 0)
            {
                lastFilledWorldX = fill.transform.position.x;
            }
        }

        if (indicator != null)
        {
            Vector3 pos = indicator.position;
            pos.x = lastFilledWorldX;
            indicator.position = pos;
        }
    }
}