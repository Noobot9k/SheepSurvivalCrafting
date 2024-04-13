using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.SceneManagement;

[EditorTool("Prefab Tool")]
public class Editor_PrefabPlacement : EditorTool {

    Transform PrefabContainer;

    private void OnEnable() {
        CheckPrefabContainer();
    }
    void CheckPrefabContainer() {
        foreach(GameObject sceneItem in EditorSceneManager.GetActiveScene().GetRootGameObjects()) {
            if(sceneItem.name == "Prefabs")
                PrefabContainer = sceneItem.transform;
        }
        if (PrefabContainer == null) {
            PrefabContainer = new GameObject("Prefabs").transform;
        }
    }

    float roundTo(float numberToRound, float Interval) {
        if(Interval == 0) return numberToRound;
        return Mathf.Round(numberToRound / Interval) * Interval;
    }

    public override void OnToolGUI(EditorWindow window) {
        //base.OnToolGUI(window);
        if(PrefabPlacementWindow.SaveObject == null) return;

        Event Current = Event.current;

        //Check the event type and make sure it's left click.
        if((Current.type == EventType.MouseDown) && Current.button == 0) { //Current.type == EventType.MouseDrag
            Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hitInfo;

            if(Physics.Raycast(worldRay, out hitInfo)) {
                //Undo.RegisterUndo(target, "Add Path Node");
                //Undo.RegisterCompleteObjectUndo(target, "Add Path Node");

                // do stuff
                PlacableItem selected = PrefabPlacementWindow.SaveObject.SelectedForPlacement;
                if (selected != null && selected.Prefab != null) {

                    Vector3 newPos = hitInfo.point;
                    newPos = new Vector3(roundTo(newPos.x, selected.AlignToGrid), roundTo(newPos.y, selected.AlignToGrid), roundTo(newPos.z, selected.AlignToGrid));

                    float x = 0; float y = 0; float z = 0;
                    if(selected.RotateRandomY || selected.RotateRandomXYZ) y = Random.Range(-1800, 1800)/10;
                    if (selected.RotateRandomXYZ) { x = Random.Range(-1800, 1800) / 10; z = Random.Range(-1800, 1800) / 10; }
                    x = roundTo(x, selected.AlignRotationToGrid);
                    y = roundTo(y, selected.AlignRotationToGrid);
                    z = roundTo(z, selected.AlignRotationToGrid);
                    Vector3 upVector = Vector3.Lerp(Vector3.up, hitInfo.normal, selected.LeanInheritance);
                    Quaternion newRot = Quaternion.LookRotation(upVector) * Quaternion.Euler(x, z, y) * Quaternion.Euler(selected.RotationOffset);
                    float randomHeight = Random.Range(selected.RandomHeight.x * 100, selected.RandomHeight.y * 100) / 100;
                    float RandomWidth = Random.Range(selected.RandomWidth.x * 100, selected.RandomWidth.y * 100) / 100;
                    if(selected.LockWidthToHeight) RandomWidth = randomHeight;
                    Vector3 newSize = new Vector3(RandomWidth, randomHeight, RandomWidth);

                    Debug.Log(selected.Prefab);
                    //Object newInst = Instantiate<Object>(selected.Prefab as Object, newPos, newRot, PrefabContainer);
                    Object newInst = PrefabUtility.InstantiatePrefab(selected.Prefab as Object, PrefabContainer);
                    Transform newTransform;
                    if(newInst as Transform) {
                        Undo.RegisterCreatedObjectUndo((newInst as Transform).gameObject, "Add prefab");
                        newTransform = newInst as Transform;
                    } else {
                        Undo.RegisterCreatedObjectUndo(newInst, "Add prefab");
                        newTransform = (newInst as GameObject).transform;
                    }
                    newTransform.localScale = newSize;
                    newTransform.position = newPos;
                    newTransform.rotation = newRot;
                }

            }

            Current.Use();  //Eat the event so it doesn't propagate through the editor.
        } else if(Current.type == EventType.Layout)
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));
    }

}

[System.Serializable]
public class PrefabPlacementWindow : EditorWindow {
    //values
    //[HideInInspector]
    //public static List<PlacableItem> Placables;
    //[HideInInspector]
    //public static PlacableItem SelectedForPlacement = null;

    public static PrefabPlacementObject SaveObject;
    static public Vector2 currentScroll;

    private void OnEnable() {
        SaveObject = (PrefabPlacementObject)AssetDatabase.LoadAssetAtPath("Assets/Editor/PrefabPlacementSaveObject.Asset", typeof(PrefabPlacementObject));
        Debug.Log(SaveObject);
        if (SaveObject == null) {
            Debug.Log("can't find. making new...");
            PrefabPlacementObject newitem = PrefabPlacementObject.CreateInstance<PrefabPlacementObject>();
            AssetDatabase.CreateAsset(newitem, "Assets/PrefabPlacementSaveObject.Asset");
            SaveObject = (PrefabPlacementObject)AssetDatabase.LoadAssetAtPath("Assets/Editor/PrefabPlacementSaveObject.Asset", typeof(PrefabPlacementObject));

        }
        Debug.Log(SaveObject);
}

    // setup
    [MenuItem("Tools/Prefab Placer")]
    public static void ShowWindow() {
        EditorWindow.GetWindow(typeof(PrefabPlacementWindow));
    }
    private void OnGUI() {
        if(SaveObject == null) {
            GUILayout.Label("waiting for SaveObject...", EditorStyles.boldLabel);
            return;
        }
            
        //if(SaveObject.Placables == null) {
        //    SaveObject.Placables = new List<PlacableItem>() { new PlacableItem(null, "test"), new PlacableItem(null, "test2") };
        //}//

        
        GUILayout.Label("Placables:", EditorStyles.boldLabel);
        if(GUILayout.Button("New")){
            SaveObject.Placables.Add(new PlacableItem("New Item"));
        }
        //EditorGUILayout.BeginScrollView();
        currentScroll = EditorGUILayout.BeginScrollView(currentScroll);

        foreach(PlacableItem item in SaveObject.Placables) {
            GUIContent gay = new GUIContent(); gay.text = item.name;
            item.VisibleInWindow = EditorGUILayout.BeginFoldoutHeaderGroup(item.VisibleInWindow, gay);
            bool selected = EditorGUILayout.Toggle("Selected", SaveObject.SelectedForPlacement == item);
            if(selected) SaveObject.SelectedForPlacement = item;
            if(item.VisibleInWindow) {
                item.name = EditorGUILayout.TextField("    Name", item.name);
                item.Prefab = EditorGUILayout.ObjectField("    Prefab", item.Prefab, typeof(Transform), false);
                item.RandomHeight = EditorGUILayout.Vector2Field("    Height", item.RandomHeight);
                EditorGUILayout.MinMaxSlider(ref item.RandomHeight.x, ref item.RandomHeight.y, 0.001f, 25);
                item.LockWidthToHeight = EditorGUILayout.Toggle("    LockWidthToHeight", item.LockWidthToHeight);
                if(!item.LockWidthToHeight) {
                    item.RandomWidth = EditorGUILayout.Vector2Field("    Width", item.RandomWidth);
                    EditorGUILayout.MinMaxSlider(ref item.RandomWidth.x, ref item.RandomWidth.y, 0.001f, 25);
                }
                item.RotateRandomY = EditorGUILayout.Toggle("    RotateRandomY", item.RotateRandomY);
                item.RotateRandomXYZ = EditorGUILayout.Toggle("    RotateRandomXYZ", item.RotateRandomXYZ);
                item.AlignRotationToGrid = EditorGUILayout.FloatField("    AlignRotationToGrid", item.AlignRotationToGrid);
                item.LeanInheritance = EditorGUILayout.FloatField("    LeanInheritance", item.LeanInheritance);
                item.RotationOffset = EditorGUILayout.Vector3Field("    RotationOffset", item.RotationOffset);
                item.AlignToGrid = EditorGUILayout.FloatField("    AlignToGrid", item.AlignToGrid);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();

        // save?
        if(GUI.changed) {
            //EditorUtility.SetDirty(castedTarget);
            //EditorSceneManager.MarkSceneDirty(castedTarget.gameObject.scene);
            Undo.RegisterCompleteObjectUndo(SaveObject, "changed in window");//
            //EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorUtility.SetDirty(SaveObject);
        }
    }

}