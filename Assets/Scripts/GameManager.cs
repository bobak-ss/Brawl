using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Entities.Variables;
using Sfs2X.Requests;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour 
{
    //----------------------------------------------------------
    // Public properties
    //----------------------------------------------------------

    public GameObject localPlayerObj;
    public GameObject remotePlayerObj;
    public LayerMask layerMask;
    public float speed = 100f, jumpVelocity = 50f;

    //----------------------------------------------------------
    // Private properties
    //----------------------------------------------------------

    private SmartFox sfs;
    private GameObject localPlayer = null;
    private Boolean localPlayerIsGrounded = true;
    private Boolean localPlayerDirection = true;
    private GameObject remotePlayer = null;
    private Boolean remotePlayerDirection = true;
    static private Text log;

    //----------------------------------------------------------
    // Unity calback methods
    //----------------------------------------------------------
        
    void Start()
    {
        log = GameObject.Find("LogsObject").AddComponent<Text>();

        if (!SmartFoxConnection.IsInitialized)
        {
            trace("SFS not initialized!");
            SceneManager.LoadScene("Connection");
			return;
        }

        sfs = SmartFoxConnection.Connection;

        if(sfs.IsConnected)
        {
            trace("sfs is connected in game manager");
            
            sfs.AddEventListener(SFSEvent.USER_ENTER_ROOM, userEnterRoomHandler);
            sfs.AddEventListener(SFSEvent.USER_EXIT_ROOM, userExitRoomHandler);
            sfs.AddEventListener(SFSEvent.USER_VARIABLES_UPDATE, userVarUpdateHandler);
            sfs.AddEventListener(SFSEvent.PROXIMITY_LIST_UPDATE, proximityListUpdateHandler);
            sfs.AddEventListener(SFSEvent.PUBLIC_MESSAGE, publicMessageHandler);
            sfs.AddEventListener(SFSEvent.OBJECT_MESSAGE, messageHandler);
            sfs.AddEventListener(SFSEvent.CONNECTION_LOST, lostConnectionHandler);
        }

        if (localPlayer == null)
        {
            SpawnLocalPlayer();
        }
    }

    void Update()
    {
        if(sfs != null)
        {
            sfs.ProcessEvents();
            if (localPlayer != null)
            {
                localPlayerIsGrounded = Physics2D.Linecast(localPlayer.GetComponent<Transform>().position, localPlayer.transform.FindChild("isGrounded").transform.position, layerMask);

                move(CrossPlatformInputManager.GetAxisRaw("Horizontal"));
                if (CrossPlatformInputManager.GetButtonDown("Jump"))
                    jump();

                if (localPlayer.GetComponent<Rigidbody2D>().velocity.x == 0 && localPlayer.GetComponent<Rigidbody2D>().velocity.y == 0)
                    localPlayer.GetComponent<Animator>().SetBool("Running", false);
                else
                    localPlayer.GetComponent<Animator>().SetBool("Running", true);
            }
            if (remotePlayer != null)
            {
                if (remotePlayer.GetComponent<Rigidbody2D>().velocity.x == 0 && remotePlayer.GetComponent<Rigidbody2D>().velocity.y == 0)
                    remotePlayer.GetComponent<Animator>().SetBool("Running", false);
                else
                {
                    remotePlayer.GetComponent<Animator>().SetBool("Running", true);
                }
            }
        }
    }

    // ------------------------ Player Spawn Methods ------------------------
    private void SpawnLocalPlayer()
    {
        localPlayer = GameObject.Instantiate(localPlayerObj);
        localPlayer.transform.position = new Vector3(UnityEngine.Random.Range(-190f, 190f), 150f, 0);
        localPlayer.transform.FindChild("name").GetComponent<Text>().text = Connection.localUser.Name;

        List<UserVariable> userVariables = new List<UserVariable>();
        userVariables.Add(new SFSUserVariable("posx", (double)localPlayer.transform.position.x));
        userVariables.Add(new SFSUserVariable("posy", (double)localPlayer.transform.position.y));
        sfs.Send(new SetUserVariablesRequest(userVariables));
    }
    private void SpawnRemotePlayer(User user)
    {
        remotePlayerObj.transform.position = new Vector3( (float)user.GetVariable("posx").GetDoubleValue(), (float)user.GetVariable("posy").GetDoubleValue() );
        remotePlayer = GameObject.Instantiate(remotePlayerObj);
        remotePlayer.transform.FindChild("name").GetComponent<Text>().text = user.Name;
    }


    // ------------------------ Player Action Methods ------------------------
    public void move(float input)
    {
        Vector2 moveVel = localPlayer.GetComponent<Rigidbody2D>().velocity;
        moveVel.x = speed * input;
        localPlayer.GetComponent<Rigidbody2D>().velocity = moveVel;

        if (moveVel.x != 0)
            localPlayerDirection = (moveVel.x < 0) ? false : true;

        Vector3 trueDirection = new Vector3(localPlayerDirection ? 1 : -1, 1, 1);
        // Redirecting player direction
        localPlayer.GetComponent<Transform>().localScale = trueDirection;
        // Redirecting player name direction
        localPlayer.transform.FindChild("name").GetComponent<Transform>().localScale = trueDirection;

        List<UserVariable> userVariables = new List<UserVariable>();
        userVariables.Add(new SFSUserVariable("velx", (double)moveVel.x));
        userVariables.Add(new SFSUserVariable("vely", (double)moveVel.y));
        sfs.Send(new SetUserVariablesRequest(userVariables));
    }
    public void jump()
    {
        Vector2 moveVel = localPlayer.GetComponent<Rigidbody2D>().velocity;
        
        if (localPlayerIsGrounded)
            moveVel = new Vector3(localPlayer.GetComponent<Rigidbody2D>().velocity.x, localPlayer.GetComponent<Rigidbody2D>().velocity.y + jumpVelocity);
        localPlayer.GetComponent<Rigidbody2D>().velocity = moveVel;
        
        List<UserVariable> userVariables = new List<UserVariable>();
        userVariables.Add(new SFSUserVariable("vely", (double)moveVel.y));
        sfs.Send(new SetUserVariablesRequest(userVariables));
    }

    //----------------------------------------------------------
    // SmartFoxServer event listeners
    //----------------------------------------------------------

    private void userEnterRoomHandler(BaseEvent evt)
    {
        SFSUser user = (SFSUser)evt.Params["user"];
        trace("(" + user.Name + ") Entered the room!");

        // we set local player vars to let new entered user
        List<UserVariable> userVariables = new List<UserVariable>();
        userVariables.Add(new SFSUserVariable("posx", (double)localPlayer.transform.position.x));
        userVariables.Add(new SFSUserVariable("posy", (double)localPlayer.transform.position.y));
        sfs.Send(new SetUserVariablesRequest(userVariables));

        SpawnRemotePlayer(user);
    }

    private void userExitRoomHandler(BaseEvent evt)
    {
        SFSUser user = (SFSUser)evt.Params["user"];
        trace("(" + user.Name + ") Exited the room!");
        Destroy(remotePlayer);
    }
    
    private void userVarUpdateHandler(BaseEvent evt)
    {
        List<string> changedVars = (List<string>)evt.Params["changedVars"];

        SFSUser user = (SFSUser)evt.Params["user"];

        if (user == sfs.MySelf) return;

        if (remotePlayer == null)
            SpawnRemotePlayer(user);

        //trace("(" + user.Name + ") Changed its vars!");
        if (changedVars.Contains("posx") || changedVars.Contains("posy"))
            remotePlayer.transform.position = new Vector3((float)user.GetVariable("posx").GetDoubleValue(), (float)user.GetVariable("posy").GetDoubleValue(), 0);
        if (changedVars.Contains("velx"))
            remotePlayer.GetComponent<Rigidbody2D>().velocity = new Vector3((float)user.GetVariable("velx").GetDoubleValue(), (float)user.GetVariable("vely").GetDoubleValue(), 0);
        if (changedVars.Contains("vely"))
            remotePlayer.GetComponent<Rigidbody2D>().velocity = new Vector3((float)user.GetVariable("velx").GetDoubleValue(), (float)user.GetVariable("vely").GetDoubleValue(), 0);

        // Redirecting player direction
        if (user.GetVariable("velx").GetDoubleValue() != 0)
            remotePlayerDirection = (user.GetVariable("velx").GetDoubleValue() < 0) ? false : true;
        Vector3 trueDirection = new Vector3(remotePlayerDirection ? 1 : -1, 1, 1);
        remotePlayer.GetComponent<Transform>().localScale = trueDirection;
        remotePlayer.transform.FindChild("name").GetComponent<Transform>().localScale = trueDirection;
    }
    private void proximityListUpdateHandler(BaseEvent e)
    {
        var addedUsers = (List<User>) e.Params["addedUsers"];
        var removedUsers = (List<User>) e.Params["removedUsers"];

        // Handle new players
        foreach (var user in addedUsers)
        {
            SpawnRemotePlayer(user);
        }

        // Handle removed players
        foreach (var user in removedUsers)
        {
            Destroy(remotePlayer);
        }
    }
    private void publicMessageHandler(BaseEvent e)
    {
        //Debug.Log("!!! New PublicMessage !!!");
        Room room = (Room)e.Params["room"];
        User sender = (User)e.Params["sender"];
        String msg = "[" + room.Name + "] " + sender.Name + ": " + e.Params["message"];
        trace(msg);
    }

    private void lostConnectionHandler(BaseEvent evt)
    {
        trace("Lost Connection");
        sfs.RemoveAllEventListeners();
        SceneManager.LoadScene("Connection");
    }

    private void messageHandler(BaseEvent evt)
    {
        trace("Message Handler");
    }

    public static Text trace(string textString)
    {
        Debug.Log(textString);
        log.text += "\n-" + textString;

        Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        log.font = ArialFont;
        log.material = ArialFont.material;
        log.fontSize = 7;
        log.color = new Color(0.058f, 0.450f, 0f);
        log.verticalOverflow = VerticalWrapMode.Overflow;
        log.alignByGeometry = true;

        return log;
    }
}
