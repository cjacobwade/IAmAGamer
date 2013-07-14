using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	
	//Movement
		public int moveSpeed;
		public Vector3 velocity;
		
		//Jumping
			public bool isJumping;
			public int jumpSpeed;
			bool wallJump = false;
			public float currentGravity;
			public float wallGravity;
			public float maxGravity;
			float initGravity;

		//WallJumping
			public LayerMask platLayer;
			public bool isWalled,rightWall,leftWall;
			public float initWall;
			float currentWall;
		
		//Dodge
			bool isDodge = false;
			public int dodgeSpeed;
	
		//Slash
			public bool isSlash;
	
		//ScreenWrap
			public int[] screenBounds;
	
	//Death
	
	//Respawn
	
	//Animation
	
	//Collisions
		Ray left,right,topLeft,topRight,bottomLeft,bottomRight;
		RaycastHit wallHit,ceilingHit;
	
	//Object Refs
		CharacterController controller;
		public GameObject sprite;
		public GameObject slashHolder;
		public GameObject slash;
	
	// Use this for initialization
	void Start () 
	{
		initGravity = currentGravity;
		controller = GetComponent<CharacterController>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(networkView.isMine)
		{
			Character(velocity.x,slashHolder.activeSelf,Input.GetAxis("Vertical"),controller.isGrounded);
			Movement();
		}
		//DebugStuff();
//		else
//		{
//			SyncedMovement();	
//		}
	}
	
	void Movement()
	{
		if(!slashHolder.activeSelf)
		{
			velocity = new Vector3(Input.GetAxis("Horizontal")+currentWall,velocity.y,0);
			Controls();
		}
		Gravity ();
		Collisions();
		ScreenWrap();
		controller.Move(velocity*moveSpeed*Time.deltaTime);
	}
	
	void ScreenWrap()
	{
		if(transform.position.x < screenBounds[0])
			transform.position = new Vector3(screenBounds[1],transform.position.y,transform.position.z);
		if(transform.position.x > screenBounds[1])
			transform.position = new Vector3(screenBounds[0],transform.position.y,transform.position.z);
		if(transform.position.y > screenBounds[2])
			transform.position = new Vector3(transform.position.x,screenBounds[3],transform.position.z);
		if(transform.position.y < screenBounds[3])
			transform.position = new Vector3(transform.position.x,screenBounds[2],transform.position.z);
	}
	void Gravity()
	{
		if(!controller.isGrounded)
		{
			if(velocity.y > maxGravity)
				velocity.y -= currentGravity*Time.deltaTime;
		}
		else
		{	
			//wallJump = false;
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
		
		if(controller.collisionFlags == CollisionFlags.Above)
			velocity.y = -.5f;
		
		if(Physics.Raycast(transform.position,Vector3.right,1.2f,platLayer))
		{
			rightWall = true;
			WallHit();
		}
		else if(Physics.Raycast(transform.position,-Vector3.right,1.2f,platLayer))
		{
			leftWall = true;
			WallHit();
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
		else
			currentGravity = initGravity;
		if(!isWalled)
			velocity.y = 0;
		isWalled = true;
	}
	
	
	
	[RPC] void Character(float xVelocity,bool swordActive, float vAxis,bool isGrounded)
	{
		if(xVelocity < 0)
			sprite.transform.localScale = new Vector3(-1,1,1);
		if(xVelocity > 0)
			sprite.transform.localScale = new Vector3(1,1,1);
		if(!swordActive)
		{
			if(vAxis > 0)
				slashHolder.transform.eulerAngles = new Vector3(0,0,90);
			else if(vAxis<0)
			{
				if(!isGrounded)
					slashHolder.transform.eulerAngles = new Vector3(0,0,-90);
			}
			else
				slashHolder.transform.rotation = new Quaternion(0,0,0,0);
		}
		
		if(networkView.isMine)
			networkView.RPC("Character",RPCMode.OthersBuffered,xVelocity,swordActive,vAxis,isGrounded);
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
		
		if(Input.GetButtonDown("Slash"))
		{
			Slash ();
		}
			
		if(Input.GetButton("Dodge"))
		{
			if(!isDodge&&slash.activeSelf)
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
		if(!wallJump)
		{
			//currentGravity = initGravity;
			if(rightWall)
				currentWall = -initWall*Time.deltaTime;
			else
				currentWall = initWall*Time.deltaTime;
			velocity.y = 0;
			velocity.y = jumpSpeed/1.2f*Time.deltaTime;
			velocity.x = 0;
			StartCoroutine(wallCountdown(.2f));
			wallJump = true;
		}
	}
	
	IEnumerator wallCountdown(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);
		wallJump = false;
	}

	void Dodge()
	{
		isDodge = true;
	}
	
	[RPC] void Slash()
	{
		if(!slashHolder.activeSelf)
			StartCoroutine(slashCountdown(.5f));
		
		if(networkView.isMine)
			networkView.RPC ("Slash",RPCMode.OthersBuffered);
	}
	
	IEnumerator slashCountdown(float waitTime)
	{
		slashHolder.SetActive(true);
		slash.animation["Slash"].speed = 1.7f;
		slash.animation.Play("Slash");
		yield return new WaitForSeconds(waitTime);
		slashHolder.SetActive(false);
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
