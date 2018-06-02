using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public LayerMask layerMask;
    public static bool isGrounded = false;

    private GameObject player;

    void Start()
    {
        player = gameObject;
    }

	void Update ()
    {
        isGrounded = Physics2D.Linecast(player.GetComponent<Transform>().position, player.transform.FindChild("isGrounded").transform.position, layerMask);
    }
}
