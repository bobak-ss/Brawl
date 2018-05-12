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
        // Find log panel in game to write game logs
        log = GameObject.Find("LogsObject/Log").GetComponent<Text>();
        
        sfs = startSmartFox();

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
                // Cast a line from player to isGrounded game object to tell if the player is on something or not 
                // (removed player object from layer mask to avoid line cast with player coliders)
                localPlayerIsGrounded = Physics2D.Linecast(localPlayer.GetComponent<Transform>().position, localPlayer.transform.FindChild("isGrounded").transform.position, layerMask);

                // Get input for movement and jump
                move(CrossPlatformInputManager.GetAxisRaw("Horizontal"));
                if (CrossPlatformInputManager.GetButtonDown("Jump"))
                    jump();

                // Change between running and idle animation based on player velocity
                if (localPlayer.GetComponent<Rigidbody2D>().velocity.x == 0 && localPlayer.GetComponent<Rigidbody2D>().velocity.y == 0)
                    localPlayer.GetComponent<Animator>().SetBool("Running", false);
                else
                    localPlayer.GetComponent<Animator>().SetBool("Running", true);
            }
        }
        else
        {
            sfs = startSmartFox();
        }
    }

    public SmartFox startSmartFox()
    {
        if (!SmartFoxConnection.IsInitialized)
        {
            trace("SFS not initialized!");

            // if connection is not initialized we start it by loading connection scene
            SceneManager.LoadScene("Connection");
            return null;
        }

        // Set the SFS singeleton instance to sfs variable for later use
        return SmartFoxConnection.Connection;
    }

    // ------------------------ Player Spawn Methods ------------------------
    public void SpawnLocalPlayer()
    {
        localPlayer = GameObject.Instantiate(localPlayerObj);
        localPlayer.transform.position = new Vector3(UnityEngine.Random.Range(-190f, 190f), 150f, 0);
        localPlayer.transform.FindChild("name").GetComponent<Text>().text = Connection.localUser.Name;

        // Set user position variables when local player spawned
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

        // If player moved check player direction
        if (moveVel.x != 0)
            localPlayerDirection = (moveVel.x < 0) ? false : true;

        Vector3 trueDirection = new Vector3(localPlayerDirection ? 1 : -1, 1, 1);
        // Redirecting player direction
        localPlayer.GetComponent<Transform>().localScale = trueDirection;
        // Redirecting player name direction
        localPlayer.transform.FindChild("name").GetComponent<Transform>().localScale = trueDirection;

        // Set user velocity variables
        List<UserVariable> userVariables = new List<UserVariable>();
        userVariables.Add(new SFSUserVariable("vx", (double)moveVel.x));
        userVariables.Add(new SFSUserVariable("vy", (double)moveVel.y));
        sfs.Send(new SetUserVariablesRequest(userVariables));
    }

    public void jump()
    {
        // Get player velocity
        Vector2 moveVel = localPlayer.GetComponent<Rigidbody2D>().velocity;
        // Add jump to player current velocity
        moveVel = new Vector3(localPlayer.GetComponent<Rigidbody2D>().velocity.x, localPlayer.GetComponent<Rigidbody2D>().velocity.y + jumpVelocity);

        // Apply jump if the player is on smth to jump on
        if (localPlayerIsGrounded)
            localPlayer.GetComponent<Rigidbody2D>().velocity = moveVel;
        
        // Change vertical velocity by seting y velocity variable
        List<UserVariable> userVariables = new List<UserVariable>();
        userVariables.Add(new SFSUserVariable("vy", (double)moveVel.y));
        sfs.Send(new SetUserVariablesRequest(userVariables));
    }


    // A function to log on debugger and in game log viewer
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
