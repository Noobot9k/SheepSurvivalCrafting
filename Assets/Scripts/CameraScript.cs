using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {

    Camera cam;

    public float CameraDistance = 20;

    public float MarginNudgePixelThickness = 30;
    public float MarginNudgeSpeed = 4;

    public bool PlayerHasControl = true;
    public Transform TargetToTrack;

    [HideInInspector, Tooltip("Read only")]
    public Vector3 FocalPosition;

    void Start() {
        cam = GetComponent<Camera>();
    }
    void Update() {
        float pixelsFromRight = Screen.width - Input.mousePosition.x;
        float pixelsFromLeft = Input.mousePosition.x;
        float pixelsFromTop = Screen.height - Input.mousePosition.y;
        float pixelsFromBottom = Input.mousePosition.y;

        Vector3 moveVector = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        if(PlayerHasControl) {
            if(Mathf.Abs(pixelsFromRight) < MarginNudgePixelThickness)
                moveVector += Vector3.right;
            else if(Mathf.Abs(pixelsFromLeft) < MarginNudgePixelThickness)
                moveVector += Vector3.left;
            if(Mathf.Abs(pixelsFromTop) < MarginNudgePixelThickness)
                moveVector += Vector3.forward;
            else if(Mathf.Abs(pixelsFromBottom) < MarginNudgePixelThickness)
                moveVector += Vector3.back;

            moveVector = Vector3.ClampMagnitude(moveVector, 1);
            transform.position += moveVector * MarginNudgeSpeed * Time.deltaTime;
        } else {
            if(TargetToTrack) {
                transform.position = TargetToTrack.position + (-transform.forward * CameraDistance);
            }
        }

        FocalPosition = transform.position + transform.forward * CameraDistance;

    }

    public IEnumerator TweenTo(Vector3 TargetPos, float Speed) {
        bool hadControlBefore = PlayerHasControl;
        TargetToTrack = null;
        PlayerHasControl = false;
        Vector3 camTargetPos = TargetPos + (-transform.forward * CameraDistance);
        while( (transform.position - camTargetPos).magnitude > .1f) {
            transform.position = Vector3.Lerp(transform.position, camTargetPos, Speed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        PlayerHasControl = hadControlBefore;
    }
}
