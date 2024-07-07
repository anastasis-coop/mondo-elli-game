using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetAnimationTrigger : MonoBehaviour
{
    [SerializeField]
    private string trigger;

    [SerializeField]
    private Animator animator;

    private void OnEnable()
    {
        animator.SetTrigger(trigger);
    }
}
