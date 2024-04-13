using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Unit_Tree : Unit {

    public GameObject DestroyEffect;
    public List<Item> DroppedItems = new List<Item>();

    protected override IEnumerator Die() {
        yield return StartCoroutine(base.Die());

        //IEnumerator dieInternal() {
            Collider collider = GetComponent<Collider>();
            if(collider) collider.enabled = false;

            yield return new WaitForSeconds(.75f);
            DestroyEffect.SetActive(true);
            yield return new WaitForSeconds(.25f);
            Visual.gameObject.SetActive(false);
            foreach(Item PrefabToDrop in DroppedItems) {
                Item DroppedItem = Instantiate<Item>(PrefabToDrop);
                DroppedItem.transform.position = transform.position + Vector3.up * 1;
                DroppedItem.transform.rotation = Quaternion.Euler(0,Random.Range(-180, 180),0);
                StartCoroutine(DroppedItem.SpawnFling());
            }
            yield return new WaitForSeconds(2);
            GameObject.Destroy(gameObject);
        //}
        //StartCoroutine(dieInternal());
    }

    //new void Start() {
    //    base.Start();
    //}
    //void Update() {

    //}
}
