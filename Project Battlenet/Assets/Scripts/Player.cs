using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	
	//Movement
		public int moveSpeed;
		public Vector3 velocity;
	
		//Jumping
			public bool isJumping;
			public int jumpSpeed;
			public float currentGravity;
			public float wallGravity;
			float initGravity;

		//WallJumping
			public bool isWalled;
			public float initWall;
			public float currentWall;
			public bool rightWall,leftWall;
		
		//Dodge
			bool isDodge = false;
			public int dodgeSpeed;
	
		//Slash
			bool isSlash = false;
	
	//Death
	
	//Respawn
	
	//Animation
	
	//Collisions
		Ray left,right,topLeft,topRight,bottomLeft,bottomRight;
		RaycastHit wallHit,ceilingHit;
	
	//Object Refs
		CharacterController controller;
		public GameObject sprite;
	
	// Use this for initialization
	void Start () 
	{
		initGravity = currentGravity;
		controller = GetComponent<CharacterController>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		Character();
		Movement();
		DebugStuff();
	}
	
	void Movement()
	{
		velocity = new Vector3(Input.GetAxis("Horizontal")+currentWall,velocity.y,0);
		Gravity ();
		Controls();
		Collisions();
		controller.Move(velocity*moveSpeed*Time.deltaTime);
	}
	
	void Gravity()
	{
		if(!controller.isGrounded)
			velocity.y -= currentGravity*Time.deltaTime;
		else
		{	
			currentWall = 0;
			leftWall=false;
			rightWall=false;
		}
		
		if(rightWall)
		{
			if(currentWall > 0)
				currentWall += -1*Time.deltaTime;
			else
				currentWall =0;
		}
		if(leftWall)
		{
			if(currentWall < 0)
				currentWall += 1*Time.deltaTime;
			else
				currentWall = 0;
		}
	}
	
	void Collisions()
	{
			
		if(Physics.Raycast(transform.position,Vector3.right,1.3f))
		{
			WallHit();
			rightWall = true;
		}
		else if(Physics.Raycast(transform.position,-Vector3.right,1.3f))
		{
			WallHit();
			leftWall = true;
		}
		else
		{
			currentGravity = initGravity;
			isWalled = false;
		}
	}

	void WallHit()
	{
		if(velocity.y < 0)
			currentGravity = wallGravity;
		isWalled = true;
	}
	
	void Character()
	{
		if(velocity.x < 0)
			sprite.transform.localScale = new Vector3(-1,1,1);
		else if(velocity.x > 0)
			sprite.transform.localScale = new Vector3(1,1,1);	
	}
	
	#region Controls
	
	void Controls()
	{
		if(Input.GetButtonDown("Jump"))
		{
			if(isWalled&&!controller.isGrounded)
				WallJump();
			else if(controller.isGrounded)
				Jump ();
		}
		if(Input.GetButton("Dodge"))
		{
			if(!isDodge&&!isSlash)
				Dodge ();
		}
	}
	
	void Jump()
	{
		velocity.y = 0;
		velocity.y += jumpSpeed*Time.deltaTime;
	}
	
	void WallJump()
	{
		currentGravity = initGravity;
		if(rightWall)
			currentWall = -initWall*Time.deltaTime;
		else
			currentWall = initWall*Time.deltaTime;
		velocity.y = 0;
		velocity.y = jumpSpeed/1.2f*Time.deltaTime;
		velocity.x = 0;
	}
	
	void Dodge()
	{
		isDodge = true;
	}
	
	void Slash()
	{
		isSlash = true;
	}
	
	#endregion
	
	void Death()
	{
		
	}
	
	void Respawn()
	{
		
	}
	
	void DebugStuff()
	{
	}
	
}
