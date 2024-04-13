using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Prefab Placement Save Object")]
public class PrefabPlacementObject : ScriptableObject {

    [SerializeField]
    public List<PlacableItem> Placables = new List<PlacableItem>();
    [HideInInspector]
    public PlacableItem SelectedForPlacement = null;

    PrefabPlacementObject() {

    }

}



[System.Serializable]
public class PlacableItem {
    public enum Axis {X, Y, Z, nX, nY, nZ };

    public string name = "";
    public bool VisibleInWindow = true;
    public Object Prefab;
    public Vector2 RandomHeight = new Vector2(1, 1);
    public bool LockWidthToHeight = true;
    public Vector2 RandomWidth = new Vector2(1, 1);
    public bool RotateRandomY = true;
    public bool RotateRandomXYZ = false;
    [Tooltip("lock the random rotation of this item to <AlignRotationToGrid> degree intervals")]
    public float AlignRotationToGrid = 0;
    public float LeanInheritance = 1;
    public Vector3 RotationOffset = new Vector3();
    [Tooltip("0 for no grid, 1 for 1 stud grid alignment, etc.")]
    public float AlignToGrid = 0;

    public PlacableItem(Object pref) {
        Prefab = pref;
    }
    public PlacableItem(Object pref, string n) {
        Prefab = pref;
        name = n;
    }
    public PlacableItem(string n) {
        name = n;
    }
}