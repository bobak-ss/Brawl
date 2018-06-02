using Sfs2X;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class followPlayer : MonoBehaviour {

    public float depthOfView = -10;

    private SmartFox sfs;
    private Transform cameraTrans;
    private Transform playerTrans;

	void Start ()
    {
        if (!SmartFoxConnection.IsInitialized)
        {
            Debug.Log("SFS not initialized!");
            SceneManager.LoadScene("Connection");
            return;
        }
        cameraTrans = GetComponent<Transform>();
        playerTrans = GameManager.Instance.localPlayer.GetComponent<Transform>();
	}
	
	void Update () {
        if(playerTrans != null)
            cameraTrans.position = new Vector3(playerTrans.position.x, 0, depthOfView);
	}
}
