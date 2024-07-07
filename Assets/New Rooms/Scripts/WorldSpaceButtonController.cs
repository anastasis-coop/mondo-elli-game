using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(Collider))]
public class WorldSpaceButtonController : MonoBehaviour
{
    public event Action Clicked;
    
    [SerializeField] private Animator _animator;
    [SerializeField] private UnityEvent _onClick;
    [SerializeField] private string clickAnimationName = "Click";

    private void OnMouseDown()
    {
        if (_animator != null) _animator.SetTrigger(clickAnimationName);

        _onClick.Invoke();
        Clicked?.Invoke();
    }
}