using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Requests;
using Sfs2X.Entities;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class Connection : MonoBehaviour {
	public string configFile = "sfs-config.xml";
	public bool UseConfigFile = true;
	public string serverIP = "127.0.0.1";
	public int serverPort = 9933;
	public int defaultWsPort = 8080;
	public string UserName = "bss";
	public string Password = "";
	public string ZoneName = "brawl";

	public static SmartFox sfs;
    
    static private Text log;
    private GameObject popUp;
    private Text errorText;
    //private Button okBtn;
    private InputField inputField;


    void Start ()
    {
        log = GameObject.Find("LogsObject").AddComponent<Text>();
        popUp = GameObject.Find("PopUp");
        errorText = GameObject.Find("ErrorTxt").GetComponent<Text>();
        //okBtn = GameObject.Find("Button").GetComponent<Button>();
        inputField = GameObject.Find("InputField").GetComponent<InputField>();
        inputField.text = serverIP;

        sfs = new SmartFox();

        // Set ThreadSafeMode explicitly, or Windows Store builds will get a wrong default value (false)
        sfs.ThreadSafeMode = true;

        // SmartFox Event listeners
        sfs.AddEventListener(SFSEvent.CONNECTION, ConnectionHandler);
        sfs.AddEventListener(SFSEvent.CONNECTION_LOST, ConnectionLostHandler);
        sfs.AddEventListener(SFSEvent.CONFIG_LOAD_SUCCESS, ConfigLoadHandler);
        sfs.AddEventListener(SFSEvent.CONFIG_LOAD_FAILURE, ConfigLoadFailHandler);
        sfs.AddEventListener(SFSEvent.LOGIN, LoginHandler);
        sfs.AddEventListener(SFSEvent.LOGIN_ERROR, LoginErrorHandler);
        sfs.AddEventListener(SFSEvent.ROOM_ADD, RoomAddHandler);
        sfs.AddEventListener(SFSEvent.ROOM_CREATION_ERROR, RoomCreationErrorHandler);
        sfs.AddEventListener(SFSEvent.ROOM_JOIN, RoomJoinHandler);
        sfs.AddEventListener(SFSEvent.ROOM_JOIN_ERROR, RoomJoinErrorHandler);
        
        if (UseConfigFile)
            sfs.LoadConfig(Application.dataPath + "/Resources/sfs-config.xml", true);
        else
            sfs.Connect(serverIP, serverPort);
    }

    void Update () 
	{
        if (sfs != null)
            sfs.ProcessEvents();
        else if (!popUp.activeInHierarchy)
            popUp.SetActive(true);
    }
    public void startSmartfox()
    {
        sfs = new SmartFox();

        // Set ThreadSafeMode explicitly, or Windows Store builds will get a wrong default value (false)
        sfs.ThreadSafeMode = true;

        // SmartFox Event listeners
        sfs.AddEventListener(SFSEvent.CONNECTION, ConnectionHandler);
        sfs.AddEventListener(SFSEvent.CONNECTION_LOST, ConnectionLostHandler);
        sfs.AddEventListener(SFSEvent.CONFIG_LOAD_SUCCESS, ConfigLoadHandler);
        sfs.AddEventListener(SFSEvent.CONFIG_LOAD_FAILURE, ConfigLoadFailHandler);
        sfs.AddEventListener(SFSEvent.LOGIN, LoginHandler);
        sfs.AddEventListener(SFSEvent.LOGIN_ERROR, LoginErrorHandler);
        sfs.AddEventListener(SFSEvent.ROOM_ADD, RoomAddHandler);
        sfs.AddEventListener(SFSEvent.ROOM_CREATION_ERROR, RoomCreationErrorHandler);
        sfs.AddEventListener(SFSEvent.ROOM_JOIN, RoomJoinHandler);
        sfs.AddEventListener(SFSEvent.ROOM_JOIN_ERROR, RoomJoinErrorHandler);

        serverIP = inputField.text;
        if (UseConfigFile)
            sfs.LoadConfig(Application.dataPath + "/Resources/sfs-config.xml", true);
        else
            sfs.Connect(serverIP, serverPort);

        popUp.SetActive(false);
    }
    // Handlers

    // Connection Handlers
    void ConnectionHandler(BaseEvent e)
    {
        if ((bool)e.Params["success"])
        {
            trace("Successfully connected");
            if (UseConfigFile)
                ZoneName = sfs.Config.Zone;

            // Save reference to the SmartFox instance in a static field, to share it among different scenes
            SmartFoxConnection.Connection = sfs;

            //startig login phase
            sfs.Send(new LoginRequest(UserName, Password, ZoneName));
        }
        else
        {
            RetrySrvFail("Failed to connect");
        }
    }

    void ConnectionLostHandler(BaseEvent e)
    {
        RetrySrvFail("Connection lost!");
    }

    // Config file load Handlers
    void ConfigLoadHandler(BaseEvent e)
    {
        trace("Loaded Config File Successfully");
        sfs.Connect(sfs.Config.Host, sfs.Config.Port);
    }
    void ConfigLoadFailHandler(BaseEvent e)
    {
        RetrySrvFail("Loading Config File Failed!");
    }

    // Login Handlers
    void LoginHandler(BaseEvent e)
    {
        trace("Successfully logged into zone! user: " + e.Params["user"]);

        string roomName = "arena";

        // We either create the Game Room or join it if it exists already
        if (sfs.RoomManager.ContainsRoom(roomName))
        {
            trace("GameRoom already exists! Joining the room");
            sfs.Send(new JoinRoomRequest(roomName));
        }
        else
        {
            trace("GameRoom does not exists! creating the room... Joining the room...");
            RoomSettings settings = new RoomSettings(roomName);
            settings.MaxUsers = 2;
            sfs.Send(new CreateRoomRequest(settings, true));
        }
    }
    void LoginErrorHandler(BaseEvent e)
    {
        RetrySrvFail("Failed to login! Errore Code:" + e.Params["errorCode"] + " - " + e.Params["errorMessage"]);
    }
    // Room Handlers
    private void RoomJoinHandler(BaseEvent e)
    {
        trace("Joined Room: " + e.Params["room"]);

        // Remove SFS2X listeners
        sfs.RemoveAllEventListeners();
        
        SceneManager.LoadScene("GameStaging");
    }
    private void RoomJoinErrorHandler(BaseEvent e)
    {
        RetrySrvFail("ERROR: Join room failed! ErrorCode:" + e.Params["error"] + " - " + e.Params["errorMessage"]);
    }
    private void RoomAddHandler(BaseEvent e)
    {
        RetrySrvFail("Adding room[" + e.Params["room"] + "] was successfull!");
    }
    private void RoomCreationErrorHandler(BaseEvent e)
    {
        RetrySrvFail("ERROR: Creating room[" + e.Params["room"] + "] was NOT successfull!\nErrorCode:" + e.Params["error"] + " - " + e.Params["errorMessage"]);
    }

    public static Text trace(string textString)
    {
        Debug.Log(textString);
        log.text += "\n-" + textString;

        Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        log.font = ArialFont;
        log.material = ArialFont.material;
        log.fontSize = 30;
        log.color = new Color(0.058f, 1f, 0f);
        log.verticalOverflow = VerticalWrapMode.Overflow;
        log.alignByGeometry = true;

        return log;
    }

    private void RetrySrvFail(String errorMsg)
    {
        trace(errorMsg);
        errorText.text = errorMsg;
        sfs.RemoveAllEventListeners();
        popUp.SetActive(true);
    }
}
