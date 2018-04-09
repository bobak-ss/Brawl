using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour 
{
	public float speed = 100f, jumpVelocity = 100f;
	public bool isGrounded = false;
	Rigidbody2D myBody;
	Transform myTrans;
	public LayerMask layerMask;
	GameObject ground;
	Transform groundTrans;
	Animator animator;

	// Use this for initialization
	void Start () 
	{
		myBody = GetComponent<Rigidbody2D> ();	
		myTrans = GetComponent<Transform> ();
		groundTrans = GameObject.Find (this.name + "/isGrounded").transform;
		animator = GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		isGrounded = Physics2D.Linecast (myTrans.position, groundTrans.position, layerMask);
		move(Input.GetAxisRaw("Horizontal"));
		if (Input.GetButtonDown ("Vertical"))
		{
			jump ();
		}

		if (myBody.velocity.x == 0 && myBody.velocity.y == 0)
			animator.SetBool ("Running", false);
		else
			animator.SetBool ("Running", true);
		//animator.SetBool ("Running", !isGrounded);
	}
	
	public void move(float input)
	{
		Vector2 moveVel = myBody.velocity;
		moveVel.x = speed * input;
		myBody.velocity = moveVel;
	}

	public void jump()
	{
		if(isGrounded)
			myBody.velocity += jumpVelocity * Vector2.up;
	}
}
