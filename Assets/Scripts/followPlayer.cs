using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class followPlayer : MonoBehaviour {

    public float depthOfView = -10;

    private Transform cameraTrans;
    private Transform playerTrans;

	void Start () {
        cameraTrans = GetComponent<Transform>();
        playerTrans = GameObject.Find("localPlayer(Clone)").GetComponent<Transform>();
	}
	
	void Update () {
        cameraTrans.position = new Vector3(playerTrans.position.x, 0, depthOfView);
	}
}
