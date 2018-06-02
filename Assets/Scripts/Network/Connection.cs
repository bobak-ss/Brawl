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
using Sfs2X.Requests.MMO;
using Sfs2X.Entities.Data;

public class Connection : MonoBehaviour {
	public string configFile = "sfs-config.xml";
	public bool UseConfigFile = true;
	public string serverIP = "127.0.0.1";
	public int serverPort = 9933;
	public int defaultWsPort = 8080;
	public string UserName = "bss";
	public string Password = "";
	public string ZoneName = "brawl";
    public string roomName = "arena";
    public Text log;
    public GameObject popUp;
    public Text errorText;
    public InputField inputField;

    public static SmartFox sfs;
    public static User localUser = null;

    void Start ()
    {
        inputField.text = serverIP;
        sfs = new SmartFox();

        // Set ThreadSafeMode explicitly, or Windows Store builds will get a wrong default value (false)
        sfs.ThreadSafeMode = true;

        // SmartFox Event listeners
        sfs.AddEventListener(SFSEvent.CONNECTION, connectionHandler);
        sfs.AddEventListener(SFSEvent.CONNECTION_LOST, connectionLostHandler);
        sfs.AddEventListener(SFSEvent.CONFIG_LOAD_SUCCESS, configLoadHandler);
        sfs.AddEventListener(SFSEvent.CONFIG_LOAD_FAILURE, configLoadFailHandler);
        sfs.AddEventListener(SFSEvent.LOGIN, loginHandler);
        sfs.AddEventListener(SFSEvent.LOGIN_ERROR, loginErrorHandler);
        sfs.AddEventListener(SFSEvent.ROOM_ADD, roomAddHandler);
        sfs.AddEventListener(SFSEvent.ROOM_CREATION_ERROR, roomCreationErrorHandler);
        sfs.AddEventListener(SFSEvent.ROOM_JOIN, roomJoinHandler);
        sfs.AddEventListener(SFSEvent.ROOM_JOIN_ERROR, roomJoinErrorHandler);
        
        if (UseConfigFile)
            sfs.LoadConfig(Application.dataPath + "/Resources/sfs-config.xml", true);
        else
            sfs.Connect(serverIP, serverPort);
    }

    void Update () 
	{
        if (sfs != null)
        {
            sfs.ProcessEvents();
        }
        else if (!popUp.activeInHierarchy)
            popUp.SetActive(true);
        
    }
    public void startSmartfox()
    {
        sfs = new SmartFox();

        // Set ThreadSafeMode explicitly, or Windows Store builds will get a wrong default value (false)
        sfs.ThreadSafeMode = true;

        // SmartFox Event listeners
        sfs.AddEventListener(SFSEvent.CONNECTION, connectionHandler);
        sfs.AddEventListener(SFSEvent.CONNECTION_LOST, connectionLostHandler);
        sfs.AddEventListener(SFSEvent.CONFIG_LOAD_SUCCESS, configLoadHandler);
        sfs.AddEventListener(SFSEvent.CONFIG_LOAD_FAILURE, configLoadFailHandler);
        sfs.AddEventListener(SFSEvent.LOGIN, loginHandler);
        sfs.AddEventListener(SFSEvent.LOGIN_ERROR, loginErrorHandler);
        sfs.AddEventListener(SFSEvent.ROOM_ADD, roomAddHandler);
        sfs.AddEventListener(SFSEvent.ROOM_CREATION_ERROR, roomCreationErrorHandler);
        sfs.AddEventListener(SFSEvent.ROOM_JOIN, roomJoinHandler);
        sfs.AddEventListener(SFSEvent.ROOM_JOIN_ERROR, roomJoinErrorHandler);

        serverIP = inputField.text;
        if (UseConfigFile)
            sfs.LoadConfig(Application.dataPath + "/Resources/sfs-config.xml", true);
        else
            sfs.Connect(serverIP, serverPort);

        popUp.SetActive(false);
    }
    // Handlers

    // Config file load Handlers
    void configLoadHandler(BaseEvent e)
    {
        trace("Loaded Config File Successfully");
        sfs.Connect(sfs.Config.Host, sfs.Config.Port);
    }
    void configLoadFailHandler(BaseEvent e)
    {
        retrySrvFail("Loading Config File Failed!");
    }

    // Connection Handlers
    void connectionHandler(BaseEvent e)
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
            retrySrvFail("Failed to connect");
        }
    }

    void connectionLostHandler(BaseEvent e)
    {
        retrySrvFail("Connection lost!");
    }

    // Login Handlers
    void loginHandler(BaseEvent e)
    {
        trace("user[" + e.Params["user"] + "] Successfully logged into zone!");

        // We either create the Game Room or join it if it exists already
        if (sfs.RoomManager.ContainsRoom(roomName))
        {
            trace("GameRoom already exists! Joining the room");
            sfs.Send(new JoinRoomRequest(roomName));
        }
        else
        {
            trace("GameRoom does not exists! creating the room... Joining the room...");
            MMORoomSettings settings = new MMORoomSettings(roomName);
            settings.DefaultAOI = new Vec3D(300f, 180f, 0f);
            settings.MapLimits = new MapLimits(new Vec3D(-500f, -220f, 0f), new Vec3D(500f, 220f, 0f));
            settings.MaxUsers = 10;
            settings.Extension = new RoomExtension("Brawl", "com.xplosion.BrawlExtension");
            sfs.Send(new CreateRoomRequest(settings, true));
        }

        localUser = sfs.UserManager.GetUserByName(UserName);
    }
    void loginErrorHandler(BaseEvent e)
    {
        retrySrvFail("Failed to login! Errore Code:" + e.Params["errorCode"] + " - " + e.Params["errorMessage"]);
    }
    // Room Handlers
    private void roomJoinHandler(BaseEvent e)
    {
        trace("Joined Room: " + e.Params["room"]);

        // Remove SFS2X listeners
        sfs.RemoveAllEventListeners();
        
        SceneManager.LoadScene("Menu");
    }
    private void roomJoinErrorHandler(BaseEvent e)
    {
        retrySrvFail("ERROR: Join room failed! ErrorCode:" + e.Params["error"] + " - " + e.Params["errorMessage"]);
    }
    private void roomAddHandler(BaseEvent e)
    {
        trace("Adding room[" + e.Params["room"] + "] was successfull!");
    }
    private void roomCreationErrorHandler(BaseEvent e)
    {
        retrySrvFail("ERROR: Creating room[" + e.Params["room"] + "] was NOT successfull!\nErrorCode:" + e.Params["error"] + " - " + e.Params["errorMessage"]);
    }

    // A function to log on debugger and in game log viewer
    public void trace(string textString)
    {
        Debug.Log(textString);
        if (log != null)
        {
            Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            log.text += "\n-" + textString;
            log.font = ArialFont;
            log.material = ArialFont.material;
            log.fontSize = 17;
            log.color = new Color(0.058f, 0.450f, 0f);
            log.verticalOverflow = VerticalWrapMode.Overflow;
            log.alignByGeometry = true;
        }
    }

    private void retrySrvFail(String errorMsg)
    {
        trace(errorMsg);
        errorText.text = errorMsg;
        sfs.RemoveAllEventListeners();
        popUp.SetActive(true);
    }
}
