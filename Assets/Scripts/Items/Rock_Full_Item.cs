using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Rock_Full_Item : Item {

    public Item DroppedItem1;
    public Item DroppedItem2;

    public float ThrowRange = 4;
    public VisualEffect BreakEffect;

    bool _Interupted = false;
    Unit _LastHoldingUnit = null;

    public override IEnumerator UseItem(ItemUseData UseData = null) {
        yield return StartCoroutine(base.UseItem(UseData));
        if(UseData != null && UseData.TargetPosition.magnitude > 0) {
            bool Interupted = false;
            if ( (HoldingUnit.transform.position - UseData.TargetPosition).magnitude > ThrowRange) {
                HoldingUnit.SetDestination(UseData.TargetPosition);
                //HoldingUnit.SetAction(Unit.ActionType.MoveTo, UseData.TargetPosition);
                while((HoldingUnit.transform.position - UseData.TargetPosition).magnitude > ThrowRange) {
                    yield return new WaitForEndOfFrame();
                    if(CheckForActionInterupt()) { Interupted = true; break; }
                }
            }

            if(Interupted) { print("Rock Interupted"); yield break; }

            HoldingUnit.StopMoving();

            Vector3 startPos = transform.position;
            HoldingUnit.DropItem(this);
            transform.position = startPos;

            Vector3 throwVector = (UseData.TargetPosition - startPos).normalized;
            yield return StartCoroutine(Fling(throwVector * 10, Vector3.down * 1));
            transform.position += Vector3.up * .1f;

            BreakEffect.enabled = true;

            Item Instance1 = Instantiate<Item>(DroppedItem1);
            Item Instance2 = Instantiate<Item>(DroppedItem2);
            Instance1.transform.position = transform.position + Vector3.left * .15f;
            Instance2.transform.position = transform.position + Vector3.right * .15f;

            StartCoroutine(Instance1.SpawnFling());
            StartCoroutine(Instance2.SpawnFling());
            //StartCoroutine(Instance1.Fling(RandomVector3() * 3, Vector3.down * Physics.gravity.magnitude));
            //StartCoroutine(Instance2.Fling(RandomVector3() * 3, Vector3.down * Physics.gravity.magnitude));

            Collider collider = GetComponent<Collider>();
            if (collider) collider.enabled = false;
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if(renderer) renderer.enabled = false;

            yield return new WaitForSeconds(3);
            GameObject.Destroy(gameObject);
        }
    }

    //Vector3 RandomVector3() {
    //    Random.InitState(Mathf.RoundToInt(Time.time * 100) + Mathf.RoundToInt(Random.value * 100));
    //    Vector3 Randomized = Random.onUnitSphere;
    //    Randomized = new Vector3(Randomized.x, Mathf.Abs(Randomized.y / 2) + .5f, Randomized.z).normalized;
    //    return Randomized;
    //}
    void ActionInterupt() {
        _Interupted = true;
    }
    bool CheckForActionInterupt() {
        bool returnvalue = _Interupted;
        _Interupted = false;
        return returnvalue;
    }

    
    public override void Start() {
        base.Start();
    }
    public override void Update() {
        base.Update();
        if (HoldingUnit != _LastHoldingUnit) {
            if(_LastHoldingUnit)
                _LastHoldingUnit.ActionSet.RemoveListener(ActionInterupt);
            _LastHoldingUnit = HoldingUnit;
            if(HoldingUnit)
                HoldingUnit.ActionSet.AddListener(ActionInterupt);
        }
    }
    private void OnEnable() {
        if(HoldingUnit)
            HoldingUnit.ActionSet.AddListener(ActionInterupt);
    }
    private void OnDisable() {
        if(HoldingUnit)
            HoldingUnit.ActionSet.RemoveListener(ActionInterupt);
    }
}
