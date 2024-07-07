using UnityEngine;
using UnityEngine.UI;

public class CodingTile : MonoBehaviour
{
    public CodingSlot Slot { get; set; }

    [SerializeField]
    private Game.ArrowState arrow;
    public Game.ArrowState Arrow => arrow;

    public bool IsMultiplierArrow => arrow == Game.ArrowState.X2 ||
        arrow == Game.ArrowState.X3 || arrow == Game.ArrowState.X4;

    [SerializeField]
    private Image image;
    public Image Image => image;

    private RectTransform cachedRectTransform;
    public RectTransform RectTransform
    {
        get
        {
            if (cachedRectTransform == null)
            {
                cachedRectTransform = (RectTransform)transform;
            }

            return cachedRectTransform;
        }
    }
}
