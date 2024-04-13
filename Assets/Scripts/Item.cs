using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {

    public string DisplayID = "";
    public bool IsTool = false;
    public bool IsTwoHanded = false;
    public float Range = 2;
    public float ActionCooldown = 1;
    public float Damage = 0;
    public float AxePower = 0;
    [HideInInspector]
    public Unit HoldingUnit;

    public virtual IEnumerator UseItem(ItemUseData UseData = null) {
        yield return null;
    }

    // Flinging
    /// <summary>
    /// Flings the item in a direction. Usefull for when it's just been crafted or the Unit is throwing it.
    /// </summary>
    /// <param name="Direction"> Direction and strength of the fling </param>
    /// <param name="DeltaVelocity"> DeltaVelocity is how much velocity is applied to the flung object per second. Useful for gravit. </param>
    public IEnumerator Fling(Vector3 Direction, Vector3 DeltaVelocity) {
        Vector3 velocity = Direction;
        while(true) {
            if(HoldingUnit) break;
            Vector3 Delta = velocity * Time.deltaTime;
            string[] mask = new string[] { "Unit", "Item", "Ignore Raycast" };
            bool hit = Physics.Raycast(transform.position, Delta, out RaycastHit hitInfo, Delta.magnitude, ~LayerMask.GetMask(mask));

            if(hit) {
                transform.position = hitInfo.point + Vector3.up * .1f;
                break;
            } else {
                transform.position += Delta;
                velocity += DeltaVelocity * Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }
    }
    /// <summary>
    /// Same as Item.Fling but had paramaters set to defaults good for the default spawn of an item.
    /// </summary>
    /// <returns></returns>
    public IEnumerator SpawnFling() {
        Vector3 RandomVector3() {
            Random.InitState(Mathf.RoundToInt(Time.time * 100) + Mathf.RoundToInt(Random.Range(-100, 100) * 100));
            Vector3 Randomized = Random.onUnitSphere;
            Randomized = new Vector3(Randomized.x, Mathf.Abs(Randomized.y / 2) + .5f, Randomized.z).normalized;
            return Randomized;
        }
        yield return StartCoroutine(Fling(RandomVector3() * 3, Vector3.down * Physics.gravity.magnitude));
    }

    // Main
    public virtual void Start() {

    }
    public virtual void Update() {

    }
}

[System.Serializable]
public class ItemUseData {
    public Vector3 TargetPosition;
    public GameObject TargetObject;
    public ItemUseData() { }
    public ItemUseData(Vector3 targetPos) {
        TargetPosition = targetPos;
    }
    public ItemUseData(Vector3 targetPos, GameObject targetObj) {
        TargetPosition = targetPos;
        TargetObject = targetObj;
    }
}
