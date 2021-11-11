using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSet : MonoBehaviour
{
    public GameObject objectFalse;
    public GameObject objectTrue;
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider colliderCollision)
    {
        if (colliderCollision.tag == "Player")
        {
            objectFalse.SetActive(false);
            objectTrue.SetActive(true);
        }
        
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
