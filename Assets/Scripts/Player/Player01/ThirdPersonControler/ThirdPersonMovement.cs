using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{
    public CharacterController controller;

    //Camera prise en compte dans le calcul de l'angle du personnage
    public Transform characterCamera;

    //Speed and jump Variable
    private float speed = 8f;

    [Header("SPEED_&_JUMP_VARIABLES")]
    public float onGroundWalkSpeed = 2f;
    public float onGroundRunSpeed = 8f;
    public float jumpSpeedAddition = 5f;
    public float jumpforce = 1f;


    [Header("ROLL_VARIABLES")]
    public float rollTime = 1f;
    public float rollSpeed = 10f;
    public float rollOnFallTime = 0.5f;
    public float rollLerpSpeed = 5f;

    [Header("CLIMBING_VARIABLES")]
    public bool isClimbing;
    public float speedClimb;

    [Header("TURN_VARIABLES")]
    public float rollTurnSmoothTime = 0.7f;
    private float turnSmoothVelocity;
    private float turnSmoothTime = 0.1f;

    [Header("GRAVITY")]
    public float gravity01 = -40f;

    [Header("GROUND_DETECTION_PARAMETERS")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("ENEMY_DETECTION_PARAMTERS")]
    public Collider[] inSwordArea;
    public LayerMask detectionLayer;
    public Transform swordCenter;
    public Vector3 boxSize;

    [Header("CANVAS")]
    public Animator canvasAnimator;
    
    //Liste des différents états du joueur qui permettent de déclencher des evenements et ou Fx particulier dans d'autres script ensuite (Attention à ne pas renommer)
    [HideInInspector] public bool isInMovement;
    [HideInInspector] public bool getHurt;
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool isFalling;
    [HideInInspector] public bool isRolling;
    [HideInInspector] public bool playerDown;
    [HideInInspector] public bool isAttacking;
    [HideInInspector] public bool isFallingImpact;

    private Animator characterAnimator;
    private float horizontal;
    private float vertical;
    private Coroutine RollCoroutine;
    private Vector3 moveDir;
    Vector3 velocity;
    Vector3 direction;

    private void Start()
    {
        //Récupère le character animator pour set les animations du perso
        characterAnimator = GetComponentInChildren<Animator>();

        getHurt = false;

    }
    void Update()
    {
        ////MOVEMENT////
        Movement();

        ////GRAVITY AND VELOCITY.Y////
        Gravity();

        ////CLIMBING////
        if (isClimbing == true)
        {
            Climb();
        }

        Roll();

        ////JUMP////
        //Input qui délchenche la fonction "Jump" et fait référence au bouton A de la manette de Xbox One (input manager). Déclenche un Jump différent si le joueur marche ou si le joueur est statique
        if (Input.GetButtonDown("JumpA") && playerDown == false)
        {
            if (direction.magnitude >= 0.1f)
            {
                Jump();
            }
            else
            {
                JumpOnWalk();
            }


        }

        /////FALLING////
        Falling();

        /////ATTACK////
        //Detecte les ennemies qui sont à porté d'attaque
        inSwordArea = Physics.OverlapBox(swordCenter.position, boxSize, Quaternion.identity, detectionLayer);
        //Detecte si le joueur a appuie sur X et lance la fonction Attack
        if (Input.GetButtonDown("Attack"))
        {
            Attack();
        }
        if (characterAnimator.GetCurrentAnimatorStateInfo(1).IsName("Attack"))
        {
            isAttacking = true;
        }
        else
        {
            isAttacking = false;
        }

        /////GRASS SHADER INTERACTIVITY////
        Shader.SetGlobalVector("_PositionMoving", transform.position);

    }

    private void Attack()
    {
        if (playerDown == false && isFalling == false)
        {
            characterAnimator.SetTrigger("Attack");
        }
    }

    private void Movement()
    {
        ////MOVING PARAMETERS////
        horizontal = Input.GetAxis("MoveHorizontal");
        vertical = Input.GetAxis("MoveVertical");
        direction = new Vector3(horizontal, 0f, vertical);

        ////WALK RUN AND IDLE////
        //Détermine sur le joueur est en train de faire une roulade, si c'est le cas et que le joystick est penché, 
        //cela récupère la vitesse actuelle du joueur et fait un lerp sur la direction pour empecher de tourner de façon brut
        if (isRolling)
        {
            if (direction.magnitude >= 0.1f)
            {
                float targetAngle = RotatePlayer(rollTurnSmoothTime);
                Vector3 targetMoveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                moveDir = Vector3.Lerp(moveDir, targetMoveDir, Time.deltaTime * rollLerpSpeed);
            }

            controller.Move(moveDir.normalized * rollSpeed * Time.deltaTime);
        }
        ////WALK RUN AND IDLE////
        //Si le joueur appuie sur un input de déplacement, cela déplace le personage mais également l'oriente en fonction de l'angle de camera
        else if (direction.magnitude >= 0.5f && playerDown == false && isClimbing == false)
        {
            isInMovement = true;
            float targetAngle = RotatePlayer(turnSmoothTime);

            moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
         
            //Cela déclenche également l'animation de course du personnage
            characterAnimator.SetFloat("Speed", 1f, 0.1f, Time.deltaTime);

        }
        //Si le joueur appuie légèrement sur son stick, cela déclenche la fonction et l'animation de marche
        else if (direction.magnitude >= 0.1f && direction.magnitude <= 0.5f && playerDown == false && isClimbing == false)
        {
            Walk();
            isInMovement = true;
        }
        //Si le joueur n'appuie pas sur un input de déplacement, cela déclenche l'animation d'idle du personnage
        else if (direction.magnitude <= 0.1f)
        {
            characterAnimator.SetFloat("Speed", 0, 0.1f, Time.deltaTime);
            isInMovement = false;
        }
    }

    private void Gravity()
    {
        //Gère la detection du sol avec une sphère de detection qui se place sous le personnage, si le joueur est au sol sa vélocité est à sur l'axe y est à 0
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = 0f;
            speed = onGroundRunSpeed;
            characterAnimator.SetBool("IsGrounded", true);
        }
        else if (isGrounded == false)
        {
            characterAnimator.SetBool("IsGrounded", false);
        }
        //Formule qui applique la gravité a la vélocité en y du personnage 
        velocity.y += gravity01 * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // Cette fontion permet de récuperer la variable d'orientation du joueur (targetAngle) elle prends en paramètre un smoothtime qui dépend de si le joueur marche ou est en roulade et applique donc un smooth différent
    private float RotatePlayer(float smoothTime)
    {
        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + characterCamera.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, smoothTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);
        return targetAngle;
    }

    // Cette fonction permet de déclencher la roulade, si le joueur est déjà en train de rouler elle ne s'exécute pas. 
    //Si le joueur est au sol et qu'il indique une direction cela lance la coroutine de roulade si il est en chute cela lance la coroutine de chute
    private void Roll()
    {
        if (isRolling) return;

        if (isGrounded == true && (direction.magnitude >= 0.1f))
        {
            if (characterAnimator.GetCurrentAnimatorStateInfo(0).IsName("RollOnFall"))
            {
                RollCoroutine = StartCoroutine(RollOnFallRoutine(rollOnFallTime));
            }
            else if (Input.GetButton("RollB"))
            {
                RollCoroutine = StartCoroutine(RollRoutine(rollTime));
            }

        }
        
    }

    //Coroutine de roulade, applique le statue "is rolling" et lance l'animation de roulade pendant un temps défini
    private IEnumerator RollRoutine(float time)
    {
        isRolling = true;
        characterAnimator.SetBool("Roll", true);

        yield return new WaitForSeconds(time);
        isRolling = false;
        characterAnimator.SetBool("Roll", false);
    }

    //Coroutine de roulade en tombamt, applique le statue "is rolling" et applique un temps défini !!ce temps doit être légèrement supérieur au temps de l'animation sinon la routine se relance!!
    private IEnumerator RollOnFallRoutine(float time)
    {
        isRolling = true;

        yield return new WaitForSeconds(time);
        isRolling = false;
    }

    //Cette fonction permet au joueur de grimper, elle applique une valeur au blend tree d'animation lorsque le joueur grimpe vers le haut ou le bas
    private void Climb()
    {
        ////CLIMBING////
        Vector3 climb = new Vector3(0f, vertical, 0f);
        velocity.y = 0f;
        gravity01 = -40f;
        transform.Translate(climb * speedClimb * Time.deltaTime);
        if (vertical >= 0.1)
        {
            characterAnimator.SetFloat("SpeedClimb", 0, 0.05f, Time.deltaTime);
        }
        if (vertical <= 0.1f && vertical >= -0.1f)
        {
            gravity01 = 0f;
            characterAnimator.SetFloat("SpeedClimb", 0.5f, 0.05f, Time.deltaTime);
        }
        if (vertical <= -0.1f)
        {
            characterAnimator.SetFloat("SpeedClimb", 1f, 0.05f, Time.deltaTime);
        }
    }

    private void Walk()
    {
        //Determine les controls et l'animation lorsque le joueur marche
        float horizontal = Input.GetAxis("MoveHorizontal");
        float vertical = Input.GetAxis("MoveVertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical);

        speed = onGroundWalkSpeed;
        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + characterCamera.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);

        Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        controller.Move(moveDir.normalized * speed * Time.deltaTime);

        characterAnimator.SetFloat("Speed", 0.5f, 0.1f, Time.deltaTime);
    }

    private void Jump()
    {
        if (isGrounded == true && isRolling == false)
        {
            characterAnimator.SetTrigger("Jump");
            //Fonction qui permet au personnage de sauter en prenant en compte une valeur de gravité pour la chute
            velocity.y = Mathf.Sqrt(jumpforce * -3 * gravity01);
            //Augmente la speed du perso en l'air
            speed += jumpSpeedAddition;
        }
    }

    private void JumpOnWalk()
    {
        if (isGrounded == true)
        {
            characterAnimator.SetTrigger("JumpOnWalk");
            velocity.y = Mathf.Sqrt(jumpforce * -3 * gravity01);
        }
    }

    private void Falling()
    {
        ////FALLING////
        //Detecte si le joueur a déclenché l'animation de chute. Celle ci se déclenche automatiquement lorsque l'animation de saut est terminée
        if (velocity.y <= 8f && velocity.y >= -8f)
        {
            characterAnimator.SetFloat("Velocity", 0f);
        }
        else if (velocity.y <= -25f && velocity.y >= -37f)
        {
            characterAnimator.SetFloat("Velocity", 0.3f);
        }
        else if (velocity.y <= -37f && velocity.y >= -50f)
        {
            characterAnimator.SetFloat("Velocity", 0.8f);
        }
        else if (velocity.y <= -50f)
        {
            characterAnimator.SetFloat("Velocity", 1f);
        }

        if (characterAnimator.GetCurrentAnimatorStateInfo(0).IsName("WillFallingImpact"))
        {
            isFalling = true;
        }
        //Detecte si le joueur a toucher le sol alors qu'il était en animation de chute et déclenche l'animation d'impact au sol
        if (isGrounded == true && isFalling == true)
        {
            characterAnimator.SetBool("WillFallingImpact", true);
            isFalling = false;
            isFallingImpact = true;
        }
        //Detecte si le joueur est au sol ou en train de se relever et bloque les controls du joueur et indique que le joueur c'est blessé si c'est le cas. Sinon indique que le joueur n'est pas au sol
        if (characterAnimator.GetCurrentAnimatorStateInfo(0).IsName("FallingImpact") || characterAnimator.GetCurrentAnimatorStateInfo(0).IsName("GetUp"))
        {
            playerDown = true;
            if (characterAnimator.GetCurrentAnimatorStateInfo(0).IsName("FallingImpact"))
            {
                getHurt = true;
                canvasAnimator.SetBool("GetHurt", true);
                characterAnimator.SetBool("WillFallingImpact", false);
            }
            if (characterAnimator.GetCurrentAnimatorStateInfo(0).IsName("GetUp"))
            {
                getHurt = false;
                canvasAnimator.SetBool("GetHurt", false);
            }
        }
        else
        {
            playerDown = false;
        }

    }

    private void OnTriggerEnter(Collider Col)
    {
        if (Col.gameObject.tag == "Foliage")
        {
            isClimbing = true;
            characterAnimator.SetTrigger("Climb");
        }
    }

    private void OnTriggerExit(Collider Col)
    {
        if (Col.gameObject.tag == "Foliage")
        {
            isClimbing = false;
            characterAnimator.SetTrigger("NotClimb");
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(swordCenter.position, boxSize);
    }
}
