using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	
	//Movement
		public int moveSpeed;
		float initZ;
		public Vector3 velocity;
		
		//Jumping
			public bool isJumping;
			public int jumpSpeed;
			bool wallJump = false;
			public float currentGravity,wallGravity,maxGravity;
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
		public GameObject sprite,slashHolder,slash,hitbox;
		public LayerMask playerLayer,swordLayer;
	
	//Network
		public int playerNumber;
		float lastSyncTime = 0f,syncDelay = 0f, syncTime = 0f;
		Vector3 syncStartPos = Vector3.zero, syncEndPos = Vector3.zero;
		//Vector3 syncPosition = Vector3.zero;
		
	
	// Use this for initialization
	void Start () 
	{
		initZ = transform.position.z;
		initGravity = currentGravity;
		controller = GetComponent<CharacterController>();
		
	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		if(networkView.isMine)
		{
			Character(velocity.x,controller.isGrounded);
			Movement();
		}
//		else
//			SyncedMovement();
		//DebugStuff();
//		else
//		{
//			SyncedMovement();	
//		}
	}
	
	void Movement()
	{
		if(transform.position.z != initZ)
			transform.position = new Vector3(transform.position.x,transform.position.y,initZ);
		if(!slashHolder.activeSelf)
		{
			velocity = new Vector3(Input.GetAxis("Horizontal")+currentWall,velocity.y,0);
			Controls();
		}
		else
		{
			if(controller.isGrounded)
			{
				if(velocity.x <	.02f)
					velocity.x += 1f*Time.deltaTime;
				if(velocity.x >	-.02f)
					velocity.x -= 1f*Time.deltaTime;
			}
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
	
	
	
	[RPC] void Character(float xVelocity,bool isGrounded)
	{
		
		if(xVelocity < 0)
			sprite.transform.localScale = new Vector3(-1,1,1);
		if(xVelocity > 0)
			sprite.transform.localScale = new Vector3(1,1,1);
		
		if(networkView.isMine)
			networkView.RPC("Character",RPCMode.OthersBuffered,xVelocity,isGrounded);
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
		{
			SwordAnimation(Input.GetAxis("Vertical"));
			StartCoroutine(slashCountdown(.3f,.5f));
		}
		
		if(networkView.isMine)
			networkView.RPC ("Slash",RPCMode.OthersBuffered);
	}
	
	void SwordAnimation(float vAxis)
	{
		if( vAxis > 0)
		{
			print ("iambutthurt");
			slash.animation["UpSlash"].speed = 2;
			slash.animation.Play("UpSlash");
		}
		else if(vAxis < 0)
		{
			print ("iambutthurt2");
			slash.animation["DownSlash"].speed = 2;
			slash.animation.Play("DownSlash");
		}
		else
		{
			if(sprite.transform.localScale.x < 0)
			{
				print ("iambutthurt3");
				slash.animation["LeftSlash"].speed = 2;
				slash.animation.Play("LeftSlash");
			}
			else
			{
				print ("iambutthurt4");
				slash.animation["RightSlash"].speed = 2;
				slash.animation.Play("RightSlash");
			}
		}	
	}
	
	IEnumerator slashCountdown(float waitTime,float waitTime2)
	{
		hitbox.SetActive(false);
		slashHolder.SetActive(true);
		yield return new WaitForSeconds(waitTime);
		slash.collider.enabled = false;
		hitbox.SetActive(true);
		yield return new WaitForSeconds(waitTime2);
		slashHolder.SetActive(false);
		slash.collider.enabled = true;
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
	
//	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)//This should act the same as before, but now we're in control of the movement and sync
//	{
//		Vector3 syncPosition = transform.position;
//		if(stream.isWriting)
//		{
//			syncPosition = transform.position;
//			stream.Serialize(ref syncPosition);
//		}
//		else
//		{
//			stream.Serialize(ref syncPosition);
//			
//			syncTime = 0f;
//			syncDelay = Time.time - lastSyncTime;
//			lastSyncTime = Time.time;
//			
//			syncStartPos = transform.position;
//			syncEndPos = syncPosition;
//			//transform.position = syncPosition;
//		}
//	}
//	
//	void SyncedMovement()
//	{
//		syncTime += Time.deltaTime;
//		transform.position = Vector3.Lerp(syncStartPos,syncEndPos,syncTime/syncDelay);
//	}
	
	void OnDestroy()
	{
		
	}
}
