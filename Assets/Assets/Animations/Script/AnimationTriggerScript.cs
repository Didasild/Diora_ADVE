using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTriggerScript : MonoBehaviour
{
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player");
        {
            animator.SetTrigger("Retract");
            animator.SetTrigger("Extend_Pilar");
            animator.SetTrigger("Bridge_Open");
            animator.SetTrigger("Bridge_Open01");
            animator.SetTrigger("Wall_Trigger");
        }

    }
    //private void OnTriggerExit(Collider collider)
    //{
    //    if (collider.tag == "Player");
    //    {
    //        animator.SetTrigger("Retract");
    //    }
    //}
}
