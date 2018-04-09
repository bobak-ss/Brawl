using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Requests;
using Sfs2X.Entities;

public class SFSconnect : MonoBehaviour {
	public string configFile = "sfs-config.xml";
	public bool UseConfigFile = true;
	public string serverIP = "127.0.0.1";
	public int serverPort = 9933;
	public int defaultWsPort = 8080;
	public string UserName = "bss";
	public string Password = "";
	public string ZoneName = "brawl";
	public string RoomName = "arena";

	SmartFox sfs;

	void Start () 
	{
		
		sfs = new SmartFox ();
		sfs.ThreadSafeMode = true;
		sfs.AddEventListener (SFSEvent.CONNECTION, OnConnection);
		sfs.AddEventListener (SFSEvent.CONNECTION_LOST, OnConnectionLost);
		sfs.AddEventListener (SFSEvent.LOGIN, OnLogin);
		sfs.AddEventListener (SFSEvent.LOGIN_ERROR, onLoginError);
		sfs.AddEventListener (SFSEvent.CONFIG_LOAD_SUCCESS, onConfigLoad);
		sfs.AddEventListener (SFSEvent.CONFIG_LOAD_FAILURE, onConfigFail);
		sfs.AddEventListener (SFSEvent.ROOM_JOIN, OnJoinRoom);
		sfs.AddEventListener (SFSEvent.ROOM_JOIN_ERROR, OnJoinRoomError);
		sfs.AddEventListener (SFSEvent.PUBLIC_MESSAGE, PublicMessageHandler);

		if (UseConfigFile) 
		{
			//var cfg = Resources.Load<TextAsset>(configFile);
			sfs.LoadConfig (Application.dataPath + "/Resources/sfs-config.xml", true);

		}
		else 
		{
			sfs.Connect (serverIP, serverPort);
		}
	}

	void Update () 
	{
		sfs.ProcessEvents ();	
	}

	void PublicMessageHandler(BaseEvent e)
	{
		Debug.Log ("PublicMessage");
		Room room = (Room)e.Params ["room"];
		User sender = (User)e.Params ["sender"];
		Debug.Log("[" + room.Name + "] " + sender.Name + ": " + e.Params["message"]); 
	}

	void OnJoinRoom(BaseEvent e)
	{
		Debug.Log ("Joined Room: " + e.Params ["room"]);
		sfs.Send(new PublicMessageRequest("Hello World"));
	}
	void OnJoinRoomError(BaseEvent e)
	{
		Debug.Log ("Join room failed! ErrorCode:" + e.Params ["error"] + " - " + e.Params ["errorMessage"]);
	}

	void OnConnectionLost(BaseEvent e)
	{
		Debug.Log ("Connection lost!");
	}

	void onConfigLoad(BaseEvent e)
	{
		Debug.Log ("Loaded Config File Successfully");
		sfs.Connect (sfs.Config.Host, sfs.Config.Port);
	}
	void onConfigFail(BaseEvent e)
	{
		Debug.Log ("Loading Config File Failed!");
	}

	void OnLogin(BaseEvent e)
	{
		Debug.Log ("Successfully loged into zone " +  e.Params["user"]);
		sfs.Send (new JoinRoomRequest (RoomName));
	}
	void onLoginError(BaseEvent e)
	{
		Debug.Log ("Failed to login! Errore Code:"+e.Params["errorCode"]+" - "+e.Params["errorMessage"]);
	}

	void OnConnection(BaseEvent e)
	{
		if ((bool)e.Params ["success"]) 
		{
			Debug.Log ("Successfully connected");
			if (UseConfigFile)
				ZoneName = sfs.Config.Zone;
			sfs.Send (new LoginRequest (UserName, Password, ZoneName));
		}
		else
			Debug.Log ("Failed to connect");
	}

	void OnApplicationOutput()
	{
		if (sfs.IsConnected)
			sfs.Disconnect ();
	}
}
