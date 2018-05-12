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
    public GameObject localPlayer = null;
    public Dictionary<User, GameObject> remotePlayers = new Dictionary<User, GameObject>();

    //----------------------------------------------------------
    // Private properties
    //----------------------------------------------------------

    private static SmartFox sfs;
    private Boolean localPlayerIsGrounded = true;
    private Boolean localPlayerDirection = true;
    private static Text log;

    //----------------------------------------------------------
    // Unity calback methods
    //----------------------------------------------------------
        
    void Start()
    {
        log = GameObject.Find("LogsObject/Log").GetComponent<Text>();

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
        }
    }

    // ------------------------ Player Spawn Methods ------------------------
    public void SpawnLocalPlayer()
    {
        localPlayer = GameObject.Instantiate(localPlayerObj);
        localPlayer.transform.position = new Vector3(UnityEngine.Random.Range(-190f, 190f), 150f, 0);
        localPlayer.transform.FindChild("name").GetComponent<Text>().text = Connection.localUser.Name;

        List<UserVariable> userVariables = new List<UserVariable>();
        userVariables.Add(new SFSUserVariable("px", (double)localPlayer.transform.position.x));
        userVariables.Add(new SFSUserVariable("py", (double)localPlayer.transform.position.y));
        sfs.Send(new SetUserVariablesRequest(userVariables));
    }
    public void SpawnRemotePlayer(User user)
    {
        GameObject remotePlayer;

        // Get random position for new user
        remotePlayerObj.transform.position = new Vector3( (float)user.GetVariable("px").GetDoubleValue(), (float)user.GetVariable("py").GetDoubleValue() );
        remotePlayer = GameObject.Instantiate(remotePlayerObj);
        remotePlayer.transform.FindChild("name").GetComponent<Text>().text = user.Name;
        remotePlayers.Add(user, remotePlayer);
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
        userVariables.Add(new SFSUserVariable("vx", (double)moveVel.x));
        userVariables.Add(new SFSUserVariable("vy", (double)moveVel.y));
        sfs.Send(new SetUserVariablesRequest(userVariables));
    }

    public void jump()
    {
        Vector2 moveVel = localPlayer.GetComponent<Rigidbody2D>().velocity;
        moveVel = new Vector3(localPlayer.GetComponent<Rigidbody2D>().velocity.x, localPlayer.GetComponent<Rigidbody2D>().velocity.y + jumpVelocity);

        if (localPlayerIsGrounded)
            localPlayer.GetComponent<Rigidbody2D>().velocity = moveVel;
        
        List<UserVariable> userVariables = new List<UserVariable>();
        userVariables.Add(new SFSUserVariable("vy", (double)moveVel.y));
        sfs.Send(new SetUserVariablesRequest(userVariables));
    }

    public void trace(string textString)
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
    }
}
