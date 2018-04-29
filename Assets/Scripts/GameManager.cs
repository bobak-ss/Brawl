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
    private bool localPlayerIsGrounded = true;
	private Rigidbody2D localPlayerBody = null;
    private Transform localPlayerTrans = null;
    private Transform localPlayerGround = null;
    private Animator localPlayerAnimator = null;
    private GameObject remotePlayer = null;
    //private bool remotePlayerIsGrounded = true;
    private Rigidbody2D remotePlayerBody = null;
    //private Transform remotePlayerTrans = null;
    //private Transform remotePlayerGround = null;
    private Animator remotePlayerAnimator = null;
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
            
            sfs.AddEventListener(SFSEvent.USER_ENTER_ROOM, UserEnterRoomHandler);
            sfs.AddEventListener(SFSEvent.USER_EXIT_ROOM, UserExitRoomHandler);
            sfs.AddEventListener(SFSEvent.USER_VARIABLES_UPDATE, UserVarUpdateHandler);
            sfs.AddEventListener(SFSEvent.PUBLIC_MESSAGE, PublicMessageHandler);
            sfs.AddEventListener(SFSEvent.OBJECT_MESSAGE, MessageHandler);
            sfs.AddEventListener(SFSEvent.CONNECTION_LOST, LostConnectionHandler);
        }

        if (localPlayer == null)
        {
            SpawnLocalPlayer();

            // Initializations
            localPlayerBody = localPlayer.GetComponent<Rigidbody2D>();
            localPlayerTrans = localPlayer.GetComponent<Transform>();
            localPlayerAnimator = localPlayer.GetComponent<Animator>();
            localPlayerGround = GameObject.Find(localPlayer.name + "/isGrounded").transform;
        }
    }

    void Update()
    {
        if(sfs != null)
        {
            sfs.ProcessEvents();
            if (localPlayer != null)
            {
                localPlayerIsGrounded = Physics2D.Linecast(localPlayerTrans.position, localPlayerGround.position, layerMask);

                move(CrossPlatformInputManager.GetAxisRaw("Horizontal"));
                if (CrossPlatformInputManager.GetButtonDown("Jump"))
                    jump();

                if (localPlayerBody.velocity.x == 0 && localPlayerBody.velocity.y == 0)
                    localPlayerAnimator.SetBool("Running", false);
                else
                    localPlayerAnimator.SetBool("Running", true);
            }
            if (remotePlayer != null)
            {
                if (remotePlayerBody.velocity.x == 0 && remotePlayerBody.velocity.y == 0)
                    remotePlayerAnimator.SetBool("Running", false);
                else
                    remotePlayerAnimator.SetBool("Running", true);
            }
        }
    }

    // ------------------------ Player Spawn Methods ------------------------
    private void SpawnLocalPlayer()
    {
        localPlayer = GameObject.Instantiate(localPlayerObj);
        localPlayer.transform.position = new Vector3(UnityEngine.Random.Range(-190f, 190f), 150f, 0);

        List<UserVariable> userVariables = new List<UserVariable>();
        userVariables.Add(new SFSUserVariable("posx", (double)localPlayer.transform.position.x));
        userVariables.Add(new SFSUserVariable("posy", (double)localPlayer.transform.position.y));
        sfs.Send(new SetUserVariablesRequest(userVariables));
    }
    private void SpawnRemotePlayer(SFSUser user)
    {
        remotePlayerObj.transform.position = new Vector3( (float)user.GetVariable("posx").GetDoubleValue(), (float)user.GetVariable("posy").GetDoubleValue() );
        remotePlayer = GameObject.Instantiate(remotePlayerObj);

        // Initializations
        remotePlayerBody = remotePlayer.GetComponent<Rigidbody2D>();
        //remotePlayerTrans = remotePlayer.GetComponent<Transform>();
        remotePlayerAnimator = remotePlayer.GetComponent<Animator>();
        //remotePlayerGround = GameObject.Find(remotePlayer.name + "/isGrounded").transform;
    }


    // ------------------------ Player Action Methods ------------------------
    public void move(float input)
    {
        Vector2 moveVel = localPlayerBody.velocity;
        moveVel.x = speed * input;
        localPlayerBody.velocity = moveVel;

        List<UserVariable> userVariables = new List<UserVariable>();
        userVariables.Add(new SFSUserVariable("velx", (double)moveVel.x));
        userVariables.Add(new SFSUserVariable("vely", (double)moveVel.y));
        sfs.Send(new SetUserVariablesRequest(userVariables));
    }
    public void jump()
    {
        Vector2 moveVel = localPlayerBody.velocity;
        
        if (localPlayerIsGrounded)
            moveVel = new Vector3(localPlayerBody.velocity.x, localPlayerBody.velocity.y + jumpVelocity);
        localPlayerBody.velocity = moveVel;
        
        List<UserVariable> userVariables = new List<UserVariable>();
        userVariables.Add(new SFSUserVariable("vely", (double)moveVel.y));
        sfs.Send(new SetUserVariablesRequest(userVariables));
    }

    //----------------------------------------------------------
    // SmartFoxServer event listeners
    //----------------------------------------------------------

    private void UserEnterRoomHandler(BaseEvent evt)
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

    private void UserExitRoomHandler(BaseEvent evt)
    {
        SFSUser user = (SFSUser)evt.Params["user"];
        trace("(" + user.Name + ") Exited the room!");
        Destroy(remotePlayer);
    }
    
    private void UserVarUpdateHandler(BaseEvent evt)
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
    }

    private void PublicMessageHandler(BaseEvent e)
    {
        //Debug.Log("!!! New PublicMessage !!!");
        Room room = (Room)e.Params["room"];
        User sender = (User)e.Params["sender"];
        String msg = "[" + room.Name + "] " + sender.Name + ": " + e.Params["message"];
        trace(msg);
    }

    private void LostConnectionHandler(BaseEvent evt)
    {
        trace("Lost Connection");
        sfs.RemoveAllEventListeners();
        SceneManager.LoadScene("Connection");
    }

    private void MessageHandler(BaseEvent evt)
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
