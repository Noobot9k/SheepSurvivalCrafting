using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroySelfWithTimer : MonoBehaviour {

    public float Delay = 1;

    IEnumerator Start() {
        yield return new WaitForSeconds(Delay);
        GameObject.Destroy(gameObject);
    }
}
