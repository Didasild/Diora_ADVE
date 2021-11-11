using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFx_Manager : MonoBehaviour
{


    public Transform fxTransform;
    private Vector3 fxPosition;
    [Space(20)]
    public GameObject walkFx;
    public float walkFxTime;
    [Space(20)]
    public GameObject jumpFx;
    public GameObject fallingImpactFx01;
    public GameObject fallingImpactFx02;
    public float fallingImpactTime;
    [Space(20)]
    public GameObject[] attackFx;

    //R�f�rence le script du character controler pour r�cuperer les informations n�cessaire pour d�clencher les Fx
    private ThirdPersonMovement movementScript;

    //Float qui permet de bloquer l'apparition du fx de chute d'impact
    private float i = 0;
    private float j = 0;
    private Coroutine fallingImpactCoroutine;
    private Coroutine walkFxCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        //r�cup�re le script du controler qui servira ensuite a check les �tats du joueur pour lui appliquer les bons Fx associ�s
        movementScript = GetComponent<ThirdPersonMovement>();

        //Parcours la liste et passe d�sactive tout les objets

        foreach (var GameObject in attackFx)
            GameObject.SetActive(false);        
    }

    // Update is called once per frame
    void Update()
    {
        MovementFx();

        AttackFx();

        JumpFx();

        FallingImpactFx();

        //R�cup�re le transform pour appliquer uniquement la position
        fxPosition = fxTransform.position;
    }

    //Int�ragit avec le bool "isInMovement" du script du character controler
    private void MovementFx()
    {
        //Active les Fx li�s au mouvements
        if (movementScript.isInMovement == true && j < 1 && movementScript.isGrounded == true)
        {
            Instantiate(walkFx, fxPosition, Quaternion.identity);
            walkFxCoroutine = StartCoroutine(walkFxRoutine(walkFxTime));
        }
    }

    private IEnumerator walkFxRoutine(float time)
    {
        j = 2;
        yield return new WaitForSeconds(time);
        j = 0;
    }

    //Int�ragit avec le bool "isAttacking" du script du character controler
    private void AttackFx()
    {
        if (movementScript.isAttacking == true)
        {
            foreach (var GameObject in attackFx)
                GameObject.SetActive(true);
        }
        else
        {
            foreach (var GameObject in attackFx)
                GameObject.SetActive(false);
        }
    }

    //Int�ragit avec le bool "isGrounded" du script du character controler et l'input Jump du joueur
    private void JumpFx()
    {
        if (Input.GetButtonDown("JumpA") && movementScript.isRolling == false && movementScript.isGrounded == true)
        {
            //Cr�e une instance du Fx de Jump
            Instantiate (jumpFx, fxPosition, Quaternion.identity);
        }
    }

//Pour ce Fx d�s que la condition est d�tect� cela lance une coroutine qui donne viens annuler la condition de fa�on � joueur le FX qu'une seule fois. La condition est reset apr�s un certain temps de fa�on a pouvoir la red�clencher si n�cessaire
    private void FallingImpactFx()
    {
                            
        if (movementScript.isFallingImpact == true && i < 1)
        {
            Instantiate(fallingImpactFx01, fxPosition, Quaternion.identity);
            Instantiate(fallingImpactFx02, fxPosition, Quaternion.identity);
            fallingImpactCoroutine = StartCoroutine(fallingImpactRoutine(fallingImpactTime));
        }
    }

    private IEnumerator fallingImpactRoutine(float time)
    {
        i = 2;
        yield return new WaitForSeconds(time);
        movementScript.isFallingImpact = false;
        i = 0;
    }
         
}
