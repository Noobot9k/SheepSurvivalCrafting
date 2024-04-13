using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour {

    public enum UIFacingMode { Align, Face }
    public UIFacingMode FacingMode = UIFacingMode.Align;

    void Start() {

    }
    void LateUpdate() {
        if (FacingMode == UIFacingMode.Align) {
            transform.rotation = Camera.main.transform.rotation;
        } else {
            transform.LookAt(Camera.main.transform);
            transform.rotation *= Quaternion.Euler(0, 180, 0);
        }
    }
}
