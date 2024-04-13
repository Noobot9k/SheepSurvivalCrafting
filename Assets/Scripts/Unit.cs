using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor;

public class Unit : MonoBehaviour {
    public enum ActionType { None, MoveTo, PickupItem, ThrowItem, AttackUnit, UseItem}
    public enum UnitType { Fleshy, Wood, Stone }

    [Header("References")]
    [HideInInspector] public NavMeshAgent NavAgent;
    public ThoughtBubbleScript ThoughtBubble;
    public Transform Visual;
    public Transform TwoHandedHold;
    public Transform ToolHold;
    public Transform ItemHold;
    Animator animator;

    [Header("Outline")]
    public List<MeshRenderer> HealthOverlayMaterialsToUpdate = new List<MeshRenderer>();
    public List<MeshRenderer> OutlineMaterialsToUpdate = new List<MeshRenderer>();
    public Color OutlineColor = Color.black;
    public Color DefaultOutlineColor = new Color(0, 0, 0, 1);
    Color _oldOutlineColor;

    [Header("Status")]
    public UnitType Type = UnitType.Fleshy;
    public float MaxHealth = -1;
    public float Health = 100;
    // actions;
    [HideInInspector] public ActionType CurrentAction = ActionType.None;
    [HideInInspector] public object ActionContext;
    public UnityEvent ActionSet = new UnityEvent();

    [Header("Items")]
    public float PickupRange = 2;
    public float BaseActionCooldown = 1;
    public float BaseDamage = 10;
    public float BaseAxePower = 0;
    public Vector3 DropItemOffset = Vector3.up * .1f;
    public Item HeldTool;
    public Item HeldItem;
    float _tickLastAction = -100;


    // Main
    protected virtual void Start() {
        NavAgent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        UpdateOutlines();

        if (MaxHealth == -1) {
            MaxHealth = Health;
        }
    }
    protected virtual void Update() {
        if (_oldOutlineColor != OutlineColor) {
            _oldOutlineColor = OutlineColor;
            UpdateOutlines();
        }
        if (animator && NavAgent) animator.SetFloat("MoveSpeed", NavAgent.velocity.magnitude / NavAgent.speed);

        if (CurrentAction == ActionType.PickupItem) { // PICKUP ITEM
            Item item = (Item)ActionContext;

            if(item.HoldingUnit) {
                SetAction();
            } else if ((transform.position - item.transform.position).magnitude < PickupRange) {
                SetAction();
                StopMoving();
                PickupItem(item);
            }
        } else if (CurrentAction == ActionType.ThrowItem) { // THROW ITEM
            Vector3 targetPos = (Vector3)ActionContext;
            if((transform.position - targetPos).magnitude < PickupRange) {
                Item itemToDrop = HeldTool;
                if(itemToDrop == null) itemToDrop = HeldItem; // this needs changing.
                if(itemToDrop) {
                    StopMoving();
                    DropItem(itemToDrop);
                    itemToDrop.transform.position = targetPos + DropItemOffset;
                }
                if(HeldItem != null && HeldItem.IsTool) {
                    DropItem(HeldItem);
                    PickupItem(HeldItem);
                }
                SetAction();
            }
        } else if (CurrentAction == ActionType.AttackUnit) {
            Unit targetUnit = (Unit)ActionContext;
            float range = PickupRange;
            float damage = BaseDamage;
            float axePower = BaseAxePower;
            float cooldown = BaseActionCooldown;
            if(HeldTool) { 
                range = HeldTool.Range;
                damage += HeldTool.Damage;
                axePower += HeldTool.AxePower;
                cooldown = HeldTool.ActionCooldown;
            }
            if ((transform.position - targetUnit.transform.position).magnitude < range) {
                StopMoving();
                if(Time.time - _tickLastAction > cooldown) {
                    _tickLastAction = Time.time;

                    animator.SetTrigger("Attack");
                    if(targetUnit.Type == UnitType.Fleshy)
                        targetUnit.TakeDamage(damage);
                    else if(targetUnit.Type == UnitType.Wood)
                        targetUnit.TakeDamage(axePower);
                    SetAction();
                }
            } else {
                SetDestination(((Unit)ActionContext).transform.position);
            }
        }
    }
    private void OnCollisionStay(Collision collision) {
        if(collision.gameObject.GetComponent<Unit>()) {//(collision.gameObject.layer == LayerMask.GetMask("Unit")) {
            if (NavAgent && (collision.transform.position - NavAgent.destination).magnitude < 1) {
                StopMoving();
                //NavAgent.isStopped = true;
            }
        }
    }
    private void OnTriggerStay(Collider other) {
        if(other.GetComponent<Unit>() && NavAgent) {
            if((other.transform.position - NavAgent.destination).magnitude < 1) {
                StopMoving();
                //NavAgent.isStopped = true;
            }
        }
    }

    // Health
    public void TakeDamage(float damage) {
        Health -= damage;
        if(Health <= 0) {
            StartCoroutine(Die());
        }

        UpdateHealthOverlay();
    }
    protected virtual IEnumerator Die() {
        Health = 0;
        if (animator) animator.SetTrigger("Dead");
        Item item = HeldItem;
        Item tool = HeldTool;
        DropItem(HeldItem);
        DropItem(HeldTool);
        if(item) item.transform.position += Vector3.left * .25f;
        if(tool) tool.transform.position += Vector3.right * .25f;
        yield return null;
    }

    // Sub Actions
    public IEnumerator Say(string Message, bool YieldForInput = false, float MessageDisplayTime = 5) {
        print("Calling say on thoughtbubble");
        yield return StartCoroutine(ThoughtBubbleScript.Say(ThoughtBubble, Message, YieldForInput, MessageDisplayTime));
    }
    public void SetDestination(Vector3 targetPos) {
        if(Health <= 0) { StopMoving(); return; }

        NavAgent.isStopped = false;
        NavAgent.SetDestination(targetPos);
    }
    public void StopMoving() {
        if (NavAgent)
            NavAgent.isStopped = true;
    }
    public void DropItem(Item item) {
        if(item == null) return;

        bool isHeldTool = item == HeldTool;
        bool isHeldItem = item == HeldItem;
        if (isHeldItem || isHeldTool) {
            item.transform.parent = null;
            item.transform.position = transform.position + DropItemOffset;
            item.HoldingUnit = null;
            if(isHeldTool) HeldTool = null;
            if(isHeldItem) HeldItem = null;
        } else {
            Debug.LogWarning("Can't drop item Unit is not holding. " + item.ToString());
        }
    }
    public void PickupItem(Item item) {
        if(item.HoldingUnit) return;

        if((HeldTool && HeldTool.IsTwoHanded) || (HeldItem && HeldItem.IsTwoHanded)) { // drop twohanded item is picking something else up
            DropItem(HeldItem);
            DropItem(HeldTool);
        }
        if(item.IsTwoHanded) { // drop all items if picking up a twohanded item
            DropItem(HeldItem);
            DropItem(HeldTool);
        }
        if(item.IsTool) {
            if (HeldTool == null || HeldItem != null) { // equip to heldTool slot.
                DropItem(HeldTool);
                HeldTool = item;
                item.transform.parent = ToolHold;

            } else if (HeldItem == null) { // equip to heldItem slot.
                DropItem(HeldItem);
                HeldItem = item;
                item.transform.parent = ItemHold;
            }
        } else {
            if(HeldItem == null || HeldTool != null) { // equip to heldItem slot.
                DropItem(HeldItem);
                HeldItem = item;
                item.transform.parent = ItemHold;

            } else if(HeldTool == null) { // equip to heldTool slot.
                DropItem(HeldTool);
                HeldTool = item;
                item.transform.parent = ToolHold;
            }
        }
        if(item.IsTwoHanded) item.transform.parent = TwoHandedHold;
        if(item.transform.parent == null) item.transform.parent = Visual;
        item.HoldingUnit = this;
        item.transform.localPosition = Vector3.zero; //Vector3.up * 1;
        item.transform.localRotation = new Quaternion();

        //// old
        //if(item.IsTwoHanded) {
        //    DropItem(HeldItem); DropItem(HeldTool);
        //}
        //if(item.IsTool) { 
        //    DropItem(HeldTool);
        //    HeldTool = item;
        //    item.transform.parent = ToolHold;
        //} else {
        //    DropItem(HeldItem);
        //    HeldItem = item;
        //    item.transform.parent = ItemHold;
        //}
    }

    // Action
    public void SetAction(ActionType action = ActionType.None, object context = null) {
        if(Health <= 0) return;
        if (ActionSet.GetPersistentEventCount() > 0)
            ActionSet.Invoke();

        CurrentAction = action;
        ActionContext = context;

        StopMoving();

        if (action == ActionType.MoveTo) {
            SetDestination((Vector3) context);
        } else if (action == ActionType.PickupItem) {
            Item item = (Item)context;
            SetDestination(item.transform.position);
        } else if (action == ActionType.ThrowItem) {
            SetDestination((Vector3)ActionContext);
            //DropItem(HeldTool);
        } else if (action == ActionType.UseItem) {
            if(HeldTool)
                StartCoroutine(HeldTool.UseItem((ItemUseData)context));
        } else if (action == ActionType.AttackUnit) {
            Unit target = (Unit)context;
            SetDestination(target.transform.position);
        }
    }
    
    // Display
    void UpdateHealthOverlay() {
        float max = MaxHealth;
        float current = Health;
        if(max == -1f) max = 100;
        if(current <= 0) current = max;
        foreach(MeshRenderer renderer in HealthOverlayMaterialsToUpdate) {
            renderer.material.SetFloat("Health", 1 - Mathf.Clamp01(current / max));
        }
    }
    void UpdateOutlines() {
        foreach(MeshRenderer renderer in OutlineMaterialsToUpdate) {
            foreach(Material mat in renderer.materials) {
                mat.SetColor("OutlineColor", OutlineColor);
            }
        }
    }

    ////buggy
    //private void OnDrawGizmos() {
    //    UpdateOutlines();
    //    UpdateHealthOverlay();
    //}

}
