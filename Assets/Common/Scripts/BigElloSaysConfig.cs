using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BigElloSaysConfig
{
    [field: SerializeField]
    public BigElloMessage Message { get; set; }
    [field: SerializeField]
    public Vector3 BigElloPosition { get; set; }
    [field: SerializeField]
    public Vector3 MessagePosition { get; set; }
    [field: SerializeField]
    public Transform HighlightObject { get; set; }
    [field: SerializeField]
    public bool IgnoreHighlightEnbledState { get; set; }
    [field: SerializeField]
    public List<GameObject> GameObjectsToActivate { get; set; }
    [field: SerializeField]
    public List<GameObject> GameobjectsToDeactivate { get; set; }
    [field: SerializeField]
    public bool ShowRedButton { get; set; }
    [field: SerializeField]
    public bool ShowYellowButton { get; set; }
    [field: SerializeField]
    public bool ShowGreenButton { get; set; }

    public BigElloSaysConfig Clone()
    {
        return new BigElloSaysConfig
        {
            Message = Message,
            BigElloPosition = BigElloPosition,
            MessagePosition = MessagePosition,
            HighlightObject = HighlightObject,
            GameObjectsToActivate = GameObjectsToActivate,
            GameobjectsToDeactivate = GameobjectsToDeactivate,
            ShowRedButton = ShowRedButton,
            ShowYellowButton = ShowYellowButton,
            ShowGreenButton = ShowGreenButton
        };
    }
}