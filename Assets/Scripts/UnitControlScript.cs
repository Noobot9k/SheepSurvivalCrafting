using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UnitControlScript : MonoBehaviour {

    [Header("Status")]
    List<Unit> Selection = new List<Unit>();
    Unit.ActionType ActionMode = Unit.ActionType.None;
    Unit.ActionType _oldActionMode = Unit.ActionType.None;
    GameObject _lastHoveredEntity = null;
    Transform _currentTooltip;

    public Color SelectionColor = Color.blue;
    public Color SelectionHoverColor = Color.white;

    [Header("References")]
    Camera cam;
    public EventSystem eventSystem;
    public GraphicRaycaster raycaster;
    public Transform CraftVFX;
    public Transform TooltipPrefab;

    [Header("Mouse Icons")]
    public Texture2D DefaultIcon;
    public Texture2D ActionIcon;
    public Texture2D ThrowItemIcon;
    public Texture2D PickupItemIcon;
    public Texture2D UseItemIcon;
    public Texture2D AttackIcon;
    Texture2D _currentIcon;


    // Gameplay
    void Start() {
        cam = GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.Confined;
    }
    void Update() {
        if(Input.GetButtonDown("ThrowItem"))
            ThrowItem();
        if(Input.GetButtonDown("UseItem"))
            UseItem();
        if(Input.GetButtonDown("CraftItem"))
            CraftItem();
        if(Input.GetButtonDown("AttackUnit"))
            AttackUnit();

        if(_oldActionMode != ActionMode) {
            _oldActionMode = ActionMode;
            if(ActionMode == Unit.ActionType.None)
                SetCursor(DefaultIcon, new Vector2(128, 0));
            else {
                SetCursor(ActionIcon, new Vector2(128, 128));
                SetHoverTarget();
            }
        }

        // CODE BEYOND THIS POINT RELIES ON THE RETURN FUNCTION! NEW CODE MAY NOT RUN!
        if(isMouseOverUI()) return;
        Ray MouseRay = cam.ScreenPointToRay(Input.mousePosition);
        string[] MouseIgnoreList = { }; // TODO add enemyunits and friendlyunits;
        bool MouseHit = Physics.Raycast(MouseRay, out RaycastHit MouseHitInfo, 1000, ~LayerMask.GetMask(MouseIgnoreList));

        if(Input.GetMouseButtonDown(1)) { // Reset ActionMode when right-mouse is pressed (don't put other code here)
            if(ActionMode != Unit.ActionType.None) {
                ActionMode = Unit.ActionType.None;
                return;
            }
        }
        if(ActionMode == Unit.ActionType.None) {
            if(MouseHit) {
                Unit unitComp = MouseHitInfo.transform.GetComponent<Unit>();
                if(!unitComp || !Selection.Contains(unitComp)) // don't hover if the entity is a unit and already selected (only hover if it's not a unit or if it's not selected)
                    SetHoverTarget(MouseHitInfo.transform.gameObject);
                else
                    SetHoverTarget();
            } else
                SetHoverTarget();

            if(Input.GetMouseButtonDown(0))
                if(Input.GetButton("SelectionAdd") == false)
                    ClearSelection();

            if(MouseHit && MouseHitInfo.transform.CompareTag("ControllableUnit")) {
                Unit unitComp = MouseHitInfo.transform.GetComponent<Unit>();
                if(unitComp) {
                    SetCursor(ActionIcon, new Vector2(128, 128));
                    if(Input.GetMouseButtonDown(0)) {
                        Selection.Add(unitComp);
                        unitComp.OutlineColor = SelectionColor;
                        ClearLastHoveredStatus(unitComp.gameObject);
                    } else {
                        // SetHoverTarget(unitComp);
                    }
                }
            } else {
                SetCursor(DefaultIcon, new Vector2(128, 0), ActionIcon);
            }

            if(MouseHit) {
                Item item = MouseHitInfo.transform.GetComponent<Item>();
                Unit unit = MouseHitInfo.transform.GetComponent<Unit>();
                if(item) {
                    SetCursor(PickupItemIcon, new Vector2(128,128));
                    if(Input.GetMouseButtonDown(1)) {
                        foreach(Unit Selected in Selection) {
                            Selected.SetAction(Unit.ActionType.PickupItem, item);
                        }
                    }
                } else if(unit && unit.CompareTag("ControllableUnit") == false) {
                    SetCursor(AttackIcon, new Vector2(128,128));
                    if(Input.GetMouseButtonDown(1)) {
                        foreach(Unit Selected in Selection) {
                            Selected.SetAction(Unit.ActionType.AttackUnit, unit);
                        }
                    }
                } else {
                    SetCursor(DefaultIcon, new Vector2(128, 0), PickupItemIcon);
                    SetCursor(DefaultIcon, new Vector2(128, 0), AttackIcon);
                    Vector3 moveToTarget = MouseHitInfo.point;
                    if(unit) {
                        moveToTarget = MouseHitInfo.transform.position;
                    }
                    if(Input.GetMouseButtonDown(1)) {
                        foreach(Unit Selected in Selection) {
                            Selected.SetAction(Unit.ActionType.MoveTo, moveToTarget);
                        }
                    }
                }
            }

        } else if(MouseHit && ActionMode == Unit.ActionType.ThrowItem) {
            if(Input.GetMouseButtonDown(0)) {
                foreach(Unit Selected in Selection) {
                    Selected.SetAction(Unit.ActionType.ThrowItem, MouseHitInfo.point);
                }
            }
        } else if (ActionMode == Unit.ActionType.UseItem) {
            SetCursor(UseItemIcon, new Vector2(128, 128));
            if(Input.GetMouseButtonDown(0)) {
                foreach(Unit Selected in Selection) {
                    Selected.SetAction(Unit.ActionType.UseItem, new ItemUseData(MouseHitInfo.point, MouseHitInfo.transform.gameObject));
                }
            }
        } else if (ActionMode == Unit.ActionType.AttackUnit) {
            SetCursor(AttackIcon, new Vector2(128, 128));
            if(Input.GetMouseButtonDown(0) && MouseHit && MouseHitInfo.transform.CompareTag("ControllableUnit") == false) {
                Unit unit = MouseHitInfo.transform.GetComponent<Unit>();

                foreach(Unit Selected in Selection) {
                    Selected.SetAction(Unit.ActionType.AttackUnit, unit);
                }
            }
        }
        if (Input.GetMouseButtonDown(0)) // Reset ActionMode after performing action (don't put other code here)
            ActionMode = Unit.ActionType.None;

    }

    void ClearSelection() {
        foreach(Unit Selected in Selection) {
            Selected.OutlineColor = Color.black;
        }
        Selection = new List<Unit>();
    }

    // UI
    bool isMouseOverUI() {
        PointerEventData pointerData = new PointerEventData(eventSystem);
        pointerData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerData, results);
        //foreach(RaycastResult result in results) {
        //    print(result.gameObject.name);
        //}

        return results.Count > 0;
    }
    /// <summary>
    /// Sets the mouse cursor to the given value. Repeatedly setting the cursor each frame can cause flickering. Use this function to avoid that.
    /// </summary>
    /// <param name="newCursor"></param>
    /// <param name="hotSpot"></param>
    /// <param name="ExclusivePreviousCursor"> Icon will only change if the current cursor == this value. Null = Cursor will always change. </param>
    void SetCursor(Texture2D newCursor, Vector2 hotSpot, Texture2D ExclusivePreviousCursor = null) {
        if(ExclusivePreviousCursor != null && _currentIcon != ExclusivePreviousCursor) return;
        if (_currentIcon != newCursor) {
            _currentIcon = newCursor;
            Cursor.SetCursor(newCursor, hotSpot, CursorMode.Auto);
        }
    }
    void SetHoverStatus(GameObject Entity, bool Hovering) {
        ClearTooltip();
        Item asItem = Entity.GetComponent<Item>();
        if(asItem) {
            Transform outline = asItem.transform.Find("Outline");
            if(outline) {
                outline.gameObject.SetActive(Hovering);
            }

            if(Hovering) {
                _currentTooltip = Instantiate<Transform>(TooltipPrefab);
                _currentTooltip.parent = asItem.transform;
                _currentTooltip.localPosition = Vector3.zero;
                TextMesh text = _currentTooltip.GetComponentInChildren<TextMesh>();
                if(text) text.text = asItem.DisplayID;
                Transform isTool = _currentTooltip.Find("ToolIndicator");
                if(isTool) isTool.gameObject.SetActive(asItem.IsTool);
            }
        }
        Unit asUnit = Entity.GetComponent<Unit>();
        if(asUnit) {

            if(Hovering)
                asUnit.OutlineColor = SelectionHoverColor;
            else
                asUnit.OutlineColor = asUnit.DefaultOutlineColor; //Color.black;

        }
    }
    void SetHoverTarget(GameObject Target = null) {
        if (Target != _lastHoveredEntity) {
            if (_lastHoveredEntity)
                SetHoverStatus(_lastHoveredEntity, false);
            _lastHoveredEntity = Target;
            if (Target != null)
                SetHoverStatus(Target, true);
            else
                ClearTooltip();
        }
    }
    void ClearLastHoveredStatus(GameObject Target) {
        if(_lastHoveredEntity == Target) {
            _lastHoveredEntity = null;
            ClearTooltip();
        }
    }
    void ClearTooltip() {
        if(_currentTooltip != null) {
            GameObject.Destroy(_currentTooltip.gameObject);
            _currentTooltip = null;
        }
    }

    // Action Functions
    public void AttackUnit() {
        ActionMode = Unit.ActionType.AttackUnit;
    }
    public void ThrowItem() {
        ActionMode = Unit.ActionType.ThrowItem;
    }
    public void UseItem() {
        ActionMode = Unit.ActionType.UseItem;
    }
    public void CraftItem() {
        foreach(Unit sheep in Selection) {
            Item ResultItem = Crafting.Current.Craft(sheep.HeldTool, sheep.HeldItem);
            if (ResultItem != null) {
                Item itemToDelete1 = sheep.HeldTool;
                Item itemToDelete2 = sheep.HeldItem;

                Transform VFX1 = Instantiate<Transform>(CraftVFX);
                VFX1.parent = sheep.transform;
                VFX1.position = itemToDelete1.transform.position;
                Transform VFX2 = Instantiate<Transform>(CraftVFX);
                VFX2.parent = sheep.transform;
                VFX2.position = itemToDelete2.transform.position;

                sheep.DropItem(itemToDelete1);
                sheep.DropItem(itemToDelete2);
                GameObject.Destroy(itemToDelete1.gameObject);
                GameObject.Destroy(itemToDelete2.gameObject);
                
                Item newItem = Instantiate<Item>(ResultItem);
                sheep.PickupItem(newItem);

                Transform VFX3 = Instantiate<Transform>(CraftVFX);
                VFX3.parent = sheep.transform;
                VFX3.position = newItem.transform.position;
            }
        }
    }

}
