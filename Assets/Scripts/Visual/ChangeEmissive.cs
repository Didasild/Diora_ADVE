using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Ce script récupère le material de l'objet et modifie l'emissive pour définir la couleur 

public class ChangeEmissive : MonoBehaviour
{
    private Material m_material;
    public float emissiveIntensity;
    public Color MyColor;


    void Start()
    {
        m_material = GetComponent<Renderer>().material;
        m_material.EnableKeyword("_Emission");
        
    }
    void Update()
    {
        m_material.SetColor("_EmissionColor", new Vector4(MyColor.r, MyColor.g, MyColor.b, 0) * emissiveIntensity);
    }
}
