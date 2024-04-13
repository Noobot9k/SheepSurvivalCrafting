using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThoughtBubbleScript : MonoBehaviour {

    CameraScript cam;

    public Transform Bubble1;
    public Transform Bubble2;
    public TextMesh TextObject;
    public TextMesh ProgressTextObject;

    Transform Connected;
    Vector3 DefaultScale = Vector3.zero;

    public float TransitionAnimationSpeed = 2;
    public float DesiredDistanceOffGround = 3;
    public float MaxDistanceFromFocal = 5;
    public float MinDistanceToParent = 3;
    public float MoveAwayFromParentSpeed = 4;
    public float BubbleZOffset = 1;

    void Start() {
        cam = Camera.main.GetComponent<CameraScript>();
    }
    private void OnEnable() {
        if (!Connected) Connected = transform.parent;
        transform.parent = null;
        if(DefaultScale.magnitude == 0) DefaultScale = transform.localScale;
    }
    void Update() {

        //if ((Vector3.ProjectOnPlane(transform.position, cam.transform.forward) - Vector3.ProjectOnPlane(Connected.position, cam.transform.forward)).magnitude < MinDistanceToParent) {
        Vector3 awayFromParentVector = Vector3.ProjectOnPlane(transform.position - Connected.position, cam.transform.forward).normalized * MinDistanceToParent;
            transform.position = Vector3.Lerp(transform.position, Connected.position + awayFromParentVector, MoveAwayFromParentSpeed * Time.deltaTime);
        //}

        Vector3 focalPos = cam.transform.position + (cam.transform.forward * (cam.CameraDistance - DesiredDistanceOffGround));
        Debug.DrawLine(cam.transform.position, focalPos, Color.blue);

        //if (Vector3.ProjectOnPlane(transform.position - focalPos, cam.transform.forward).magnitude > MaxDistanceFromFocal) {
            Vector3 vectorToThoughtBubble = Vector3.ProjectOnPlane(transform.position - focalPos, cam.transform.forward);
            transform.position = focalPos + Vector3.ClampMagnitude(vectorToThoughtBubble, MaxDistanceFromFocal);
        //}

        Bubble1.position = transform.position + (Connected.position - transform.position) * .5f + cam.transform.forward * -BubbleZOffset;
        Bubble2.position = transform.position + (Connected.position - transform.position) * .75f + cam.transform.forward * -BubbleZOffset;
        
    }

    static public IEnumerator Say(ThoughtBubbleScript Speaker, string Message, bool YieldForInput = false, float MessageDisplayTime = 5) {
        Speaker.gameObject.SetActive(true);

        yield return Speaker.StartCoroutine(Speaker.InternalSay(Message, YieldForInput, MessageDisplayTime));
    }
    public IEnumerator InternalSay(string Message, bool YieldForInput = false, float MessageDisplayTime = 5) {
        //gameObject.SetActive(true); doesn't work.
        transform.position = Connected.position + (-Connected.right * .1f);
        TextObject.text = "";
        //ProgressTextObject.text = //lol CANT DO THAT!

        for(float alpha = 0; alpha < 1;) {
            alpha += Mathf.Clamp01(TransitionAnimationSpeed * Time.deltaTime);
            transform.localScale = DefaultScale * alpha;
            yield return new WaitForEndOfFrame();
        }

        for(int i = 0; i < Message.Length; i++) {
            TextObject.text = Message.Substring(0, i);
            yield return new WaitForSeconds(.05f);
            if(Input.GetButton("ProgressDialog")) break;
        }
        TextObject.text = Message;

        if(YieldForInput) {
            yield return new WaitUntil(()=> Input.GetButtonDown("ProgressDialog"));
        } else if (MessageDisplayTime > 0) {
            yield return new WaitForSeconds(MessageDisplayTime);
        }

        for(float alpha = 1; alpha > 0;) {
            alpha -= Mathf.Clamp01(TransitionAnimationSpeed * Time.deltaTime);
            transform.localScale = DefaultScale * alpha;
            yield return new WaitForEndOfFrame();
        }
        gameObject.SetActive(false);
    }
}
