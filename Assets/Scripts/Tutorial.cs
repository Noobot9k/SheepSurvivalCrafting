using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Tutorial : MonoBehaviour {

    Camera cam;
    public Unit StartSheep;
    public Unit SecondSheep;
    public Transform killerWolf;


    public static Vector3 FindValidSpawn(Vector3 focus) {
        float DebugVisibilityTime = 10;

        Vector3 foundPos = focus;
        bool foundLocation = false;

        int mask = LayerMask.GetMask("Unit");
        float spawnDistance = 22.5f;
        float verticalDistance = 30;
        //bool hit = Physics.Raycast(foundPos, Vector3.up * distance, out RaycastHit hitInfo, distance, mask);

        Quaternion randomOffset = Quaternion.LookRotation(Vector3.ProjectOnPlane(Random.onUnitSphere,Vector3.up));
        for(int i = 0; i < 8; i++) {
            
            Vector3 spawnPos = focus + (Vector3.up * verticalDistance) + (randomOffset * Quaternion.AngleAxis(45 * i, Vector3.up)) * (Vector3.forward * spawnDistance);
            
            Debug.DrawLine(focus, spawnPos, Color.gray, DebugVisibilityTime);
            
            bool hit = Physics.Raycast(spawnPos, Vector3.down * verticalDistance * 2, out RaycastHit hitInfo, verticalDistance * 2);//, mask);
            if(hit && hitInfo.transform.gameObject.layer != mask) {
                Debug.DrawLine(spawnPos, hitInfo.point, Color.yellow, DebugVisibilityTime);

                bool navFound = NavMesh.SamplePosition(hitInfo.point, out NavMeshHit navHit, .5f, ~0);
                if(navFound) {
                    Debug.DrawLine(spawnPos, navHit.position, new Color(0,.5f,0), DebugVisibilityTime);

                    foundLocation = true;
                    foundPos = navHit.position;
                    //break;
                }
            } else {
                Debug.DrawRay(spawnPos, Vector3.down * verticalDistance * 2, Color.red, DebugVisibilityTime);
            }
        }

        if(foundLocation == false) Debug.LogWarning("Unable to find location to spawn.");
        return foundPos;
    }

    IEnumerator Start() {
        cam = Camera.main;

        yield return new WaitUntil(()=> StartSheep.GetComponent<NavMeshAgent>().desiredVelocity.magnitude > 0.1);
        yield return new WaitForSeconds(10);
        killerWolf.position = FindValidSpawn(StartSheep.transform.position); //Vector3.ProjectOnPlane(cam.transform.position, Vector3.up);
        killerWolf.gameObject.SetActive(true);
        yield return new WaitUntil(()=> StartSheep.Health <= 0);
        yield return new WaitForSeconds(5);
        if (killerWolf) GameObject.Destroy(killerWolf.gameObject);
        SecondSheep.transform.position = FindValidSpawn(StartSheep.transform.position); //StartSheep.transform.position + new Vector3(15,0,5);
        SecondSheep.gameObject.SetActive(true);
        yield return new WaitForEndOfFrame();
        Vector3 toSecondSheepNormal = SecondSheep.transform.position - StartSheep.transform.position;
        SecondSheep.SetDestination(StartSheep.transform.position + toSecondSheepNormal.normalized * 3); // + new Vector3(2,0,1.5f));
        yield return new WaitForSeconds(2);
        yield return new WaitUntil(()=> SecondSheep.NavAgent.desiredVelocity.magnitude == 0);
        yield return new WaitForSeconds(1);
        yield return StartCoroutine(SecondSheep.Say("Wolves never come \n out in the day!", true));
        //yield return new WaitForSeconds(5.5f);
        yield return StartCoroutine(SecondSheep.Say("Times are changing. We \n must fight for survival!", true));
        //yield return new WaitForSeconds(4.5f);

        CameraScript camScript = cam.GetComponent<CameraScript>();
        yield return StartCoroutine(camScript.TweenTo(SecondSheep.transform.position, 2));
        camScript.PlayerHasControl = true;
        SecondSheep.tag = "ControllableUnit";
    }
    void Update() {

    }
}
