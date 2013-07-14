using UnityEngine;
using System.Collections;

public class Sword : MonoBehaviour {

	public GameObject splatParticle;
	public GameObject network;
	
	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
	
	void OnTriggerEnter(Collider other)
	{
		if(other.tag=="Hitbox")
		{
			KillPlayer(other);
		}
	}
	
	[RPC] void KillPlayer(Collider other)
	{
		print("hit");
		Network.Instantiate(splatParticle,other.transform.position,other.transform.rotation,0);
		//Network.Destroy(other.transform.parent.gameObject);
		other.transform.parent.gameObject.SetActive(false);
//		if(networkView.isMine)
//			networkView.RPC("KillPlayer",RPCMode.OthersBuffered,other);
			
	}
}
