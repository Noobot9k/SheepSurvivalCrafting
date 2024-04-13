using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour {

    Unit ThisUnit;
    Animator Animator;

    Unit CurrentTarget;
    public float SearchFrequency = 1;
    float _TickLastSearch = -100;
    public float ReEvaluateRange = 8;
    public float AttackRange = 1;
    public float AttackCooldown = 1;
    float _TickLastAttack = -100;

    public bool RunAway = false;

    void Start() {
        ThisUnit = GetComponent<Unit>();
        Animator = GetComponentInChildren<Animator>();
    }
    void Update() {
        if(RunAway) {
            if((Camera.main.GetComponent<CameraScript>().FocalPosition - transform.position).magnitude > 20)
                GameObject.Destroy(gameObject);
        }
        if (Time.time - _TickLastSearch > SearchFrequency) {
            _TickLastSearch = Time.time;
            if (CurrentTarget && (CurrentTarget.transform.position - transform.position).magnitude >= ReEvaluateRange) {
                RefreshTarget();
            }
        }
        if(CurrentTarget == null) {
            RefreshTarget();
            if(CurrentTarget == null && !RunAway){// ThisUnit.NavAgent.desiredVelocity.magnitude <= .1f) {
                RunAway = true;
                IEnumerator cor() {
                    yield return new WaitForSeconds(1);
                    ThisUnit.SetDestination(Tutorial.FindValidSpawn(transform.position));//transform.position + Vector3.ProjectOnPlane(Random.onUnitSphere * 1000, Vector3.up) );
                }
                StartCoroutine(cor());
            }
        }
        if (Time.time - _TickLastAttack > AttackCooldown) {
            if(CurrentTarget && (CurrentTarget.transform.position - transform.position).magnitude < AttackRange) {
                _TickLastAttack = Time.time;

                Animator.SetTrigger("Attack");
                CurrentTarget.TakeDamage(60);
                if(CurrentTarget.Health <= 0) CurrentTarget = null;

            } else if(CurrentTarget) {
                ThisUnit.SetDestination(CurrentTarget.transform.position);
            }
        } else {
            ThisUnit.StopMoving();
        }
        
    }
    void RefreshTarget() {
        float closest = 1000;
        foreach(GameObject EnemyUnit in GameObject.FindGameObjectsWithTag("ControllableUnit")) {
            Unit unitComp = EnemyUnit.GetComponent<Unit>();
            float distance = (EnemyUnit.transform.position - transform.position).magnitude;
            if(unitComp && unitComp.Health > 0 && distance <= closest) {
                closest = distance;
                CurrentTarget = unitComp;
            }
        }
    }
}
