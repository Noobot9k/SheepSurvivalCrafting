using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_Log : Unit {

    public GameObject DestroyEffect;
    public List<Item> DroppedItems = new List<Item>();

    protected override IEnumerator Die() {
        yield return StartCoroutine(base.Die());

        Collider collider = GetComponent<Collider>();
        if(collider) collider.enabled = false;

        DestroyEffect.SetActive(true);
        DestroyEffect.transform.parent = null;
        yield return new WaitForSeconds(.25f);

        foreach(Item PrefabToDrop in DroppedItems) {
            Item DroppedItem = Instantiate<Item>(PrefabToDrop);
            DroppedItem.transform.position = transform.position + Vector3.up * 1;
            DroppedItem.transform.rotation = Quaternion.Euler(0, Random.Range(-180, 180), 0);
            StartCoroutine(DroppedItem.SpawnFling());
        }

        GameObject.Destroy(gameObject);
    }
    protected override void Start() {
        base.Start();
    }
    protected override void Update() {
        base.Update();
    }
}
