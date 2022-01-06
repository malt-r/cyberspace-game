using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator animator;
    [SerializeField]
    private string currentState;

    // Update is called once per frame
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void ChangeAnimationState(string newState)
    {
        if (currentState == newState) { return; }
        animator.Play(newState);
        currentState = newState;
        Debug.Log(newState);
    }
    
    public void Trigger(string newState)
    {
        if (currentState == newState) { return; }
        animator.SetTrigger(newState);
        Debug.Log(newState);
    }
}
