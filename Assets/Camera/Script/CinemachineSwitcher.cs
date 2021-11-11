using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinemachineSwitcher : MonoBehaviour
{

    [Header ("Player Parameters")]
    public LayerMask DetectionLayer;
    public Transform PlayerTransform;

    [Header("Enemy Detection Parameters")]
    public Collider[] NearPlayer;
    public float DetectionDistance = 5f;

    [Header("Animators")]
    public Animator CameraAnimator;
    public Animator CanvasAnimator;

    private bool FightCamera = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        bool PreviousFightCamera = FightCamera;

        //Crée une sphère qui détecte le layer "enemy" et active la camera de fight si le collider de detection du joueur détecte au moins un ennemie
        NearPlayer = Physics.OverlapSphere(PlayerTransform.position, DetectionDistance, DetectionLayer);
        if (NearPlayer.Length == 0)
            FightCamera = false;
        else
            FightCamera = true;

        if (!FightCamera && FightCamera != PreviousFightCamera)
        {
            CameraAnimator.SetTrigger("TopDown");
            CanvasAnimator.SetTrigger("OutFight");
        }
        else if (FightCamera && FightCamera != PreviousFightCamera)
        {
            CameraAnimator.SetTrigger("TopDownFight");
            CanvasAnimator.SetTrigger("InFight");
        }
    }

    //Desinne des guizmo pour afficher la portée de detection et change de couleur lorsque le joueur detecte un ennemie
    private void OnDrawGizmosSelected()
    {
        if (!PlayerTransform) return;
        Gizmos.color = NearPlayer.Length == 0 ? Color.green : Color.red;
        Gizmos.DrawWireSphere(PlayerTransform.position, DetectionDistance);
    }
}
