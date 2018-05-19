using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOffscreen : MonoBehaviour
{

    public float offScreenX = 300f;
    public float offScreenY = 300f;

    private float playerX;
    private float playerY;

    private GameManager gm = null;

    void Start()
    {
        gm = GetComponent<GameManager>();
    }

    void Update()
    {
        if(gm.localPlayer != null)
        {
            playerX = Mathf.Abs(gm.localPlayer.transform.position.x);
            playerY = Mathf.Abs(gm.localPlayer.transform.position.y);
            if (playerX > offScreenX || playerY > offScreenY)
            {
                gm.trace("You Died!");
                gm.destroyPlayer(gm.localPlayer);
            }
        }
    }
}
