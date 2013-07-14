using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {
	
	const string typeName = "UniqueGameName";
	const string gameName = "RoomName";
	public int maxPlayers;
	
	bool refreshingHost = false;
	HostData[] hostList;
	
	public GameObject player;
	public Vector3 spawnPosition;
	
	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(refreshingHost && MasterServer.PollHostList().Length > 0)
		{
			hostList = MasterServer.PollHostList();
			refreshingHost = false;	
		}
	}
	
	void OnGUI()
	{
		if(!Network.isClient && !Network.isServer)
		{
			if (GUILayout.Button("Start Server"))
				StartServer();
			if (GUILayout.Button("Refresh"))
				RefreshHostList();
			if(hostList != null)
			{
				for(int i=0; i < hostList.Length;i++)
				{
					if (GUILayout.Button(hostList[i].gameName))
						JoinServer(hostList[i]);	
				}
			}
		}
	}
	
	private void StartServer()
	{
		Network.InitializeServer(maxPlayers,25000,!Network.HavePublicAddress());
		MasterServer.RegisterHost(typeName,gameName);
	}
	
	private void RefreshHostList()
	{
		if(!refreshingHost)
		{
			MasterServer.RequestHostList(typeName);
			refreshingHost = true;	
		}
	}
	
	private void JoinServer(HostData hostData)
	{
		Network.Connect(hostData);
	}
	
	private void SpawnPlayer()
	{
		Network.Instantiate(player,spawnPosition,player.transform.rotation,0);	
	}
	
	void OnServerInitialized()
	{
		SpawnPlayer();
		print ("Server init");	
	}
	
	void OnConnectedToServer()
	{
		SpawnPlayer();
		print ("Joined server");	
	}

	
}
