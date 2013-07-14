using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {
	
	const string typeName = "UniqueGameName";
	const string gameName = "RoomName";
	public int maxPlayers;
	
	bool refreshingHost = false;
	HostData[] hostList;
	
	public GameObject splatParticle;
	public GameObject player;
	public GameObject[] players;
	public GameObject spawningPlayer;
	public int playerCount = 0;
	public Vector3 spawnPosition;
	NetworkViewID playerID;
	
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
		else
		{
			if(Network.isClient)
			{
				if(GUILayout.Button("Disconnect"))
					Disconnect();
			}
			else
			{
				if(GUILayout.Button("Close Server"))
					Disconnect();
			}
		}
	}
	
	void StartServer()
	{
		Network.InitializeServer(maxPlayers,25000,!Network.HavePublicAddress());
		MasterServer.RegisterHost(typeName,gameName);
	}
	
	void RefreshHostList()
	{
		if(!refreshingHost)
		{
			MasterServer.RequestHostList(typeName);
			refreshingHost = true;	
		}
	}
	
	void JoinServer(HostData hostData)
	{
		
		Network.Connect(hostData);
	}
	
	void Disconnect()
	{
		Network.Disconnect();		
	}
	
	void SpawnPlayer()
	{
		spawningPlayer = Network.Instantiate(player,spawnPosition,Quaternion.identity,0) as GameObject;
	//	currentPlayer = Network.Instantiate(player,spawnPosition,player.transform.rotation,0) as GameObject;
		players[playerCount] = spawningPlayer;
		playerCount++;
	}
	
	void DeletePlayer(NetworkPlayer netPlayer,int index)
	{
		//if(player[playerCount]!=null)
			//Network.Instantiate(splatParticle,player.transform.position,player.transform.rotation,0);
		Network.DestroyPlayerObjects(netPlayer);	
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
	
	void OnPlayerConnected(NetworkPlayer player)
	{

	}
	
	void OnPlayerDisconnected(NetworkPlayer player)
	{
		int i = System.Int32.Parse(player.ToString());
		DeletePlayer(player,i);
		Network.RemoveRPCs(player);
	}
	
	void OnDisconnectedFromServer()
	{
		//DeletePlayer();
		Application.LoadLevel(Application.loadedLevel);
		print ("Disconnected");
	}
}
