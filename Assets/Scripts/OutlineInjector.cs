using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class OutlineInjector : MonoBehaviour {

    MeshRenderer MR;

    public bool TrueByNormalFalseByScale = true;
    [Range(0, .5f)]
    public float OutlineWidth = .1f;

    void Start() {
        MR = GetComponent<MeshRenderer>();
        foreach(Material mat in MR.materials) {
            mat.SetInt("TrueByNormalFalseByScale", Convert.ToInt32(TrueByNormalFalseByScale));
            mat.SetFloat("OutlineWidth", OutlineWidth);

        }
    }
    void Update() {

    }
}
