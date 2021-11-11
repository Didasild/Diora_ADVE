using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationScript : MonoBehaviour
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
        if (Input.GetKeyDown(KeyCode.E))
            animator.SetTrigger("Extend");
        if (Input.GetKeyDown(KeyCode.R))
            animator.SetTrigger("Retract");
        if (Input.GetKeyDown(KeyCode.Space))
            animator.SetTrigger("Intro");
        if (Input.GetKeyDown(KeyCode.Space))
            animator.SetTrigger("Intro_01");
       
    }
}
