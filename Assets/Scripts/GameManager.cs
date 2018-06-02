using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityStandardAssets.CrossPlatformInput;
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Entities.Variables;
using Sfs2X.Requests;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            trace("Instance is null!");
            SceneManager.LoadScene("Connection");
        }
        else
            trace("WARNING!: multiple instances of game manager!");
    }
    //----------------------------------------------------------
    // Public properties
    //----------------------------------------------------------

    public LayerMask layerMask;
    public float speed = 100f, jumpVelocity = 350f;
    public GameObject localPlayer = null;
    public Dictionary<User, GameObject> remotePlayers = new Dictionary<User, GameObject>();
    public GameObject[] playerPrefabs;
    public int playerPrefabNumber;

    //----------------------------------------------------------
    // Private properties
    //----------------------------------------------------------

    private Text log;
    private static SmartFox sfs;
    private Boolean localPlayerIsGrounded = true;
    private Boolean localPlayerDirection = true;

    //----------------------------------------------------------
    // Unity calback methods
    //----------------------------------------------------------
    void Start()
    {
        //log = GameObject.Find("Canvas/ScrollArea/TextContainer/Log").GetComponent<Text>();
        //sfs = startSmartFox();

        if (localPlayer == null)
        {
            //SpawnLocalPlayer();
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
                localPlayerIsGrounded = true;

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
        else if(SmartFoxConnection.IsInitialized)
        {
            sfs = SmartFoxConnection.Connection;
        }
    }

    public SmartFox startSmartFox()
    {
        if (!SmartFoxConnection.IsInitialized)
        {
            trace("SFS not initialized!");

            // if connection is not initialized we start it by loading connection scene
            SceneManager.LoadScene("_preload");
            return null;
        }

        // Set the SFS singeleton instance to sfs variable for later use
        return SmartFoxConnection.Connection;
    }

    // ------------------------ Player Spawn Methods ------------------------
    public void SpawnLocalPlayer()
    {
        localPlayer = GameObject.Instantiate(playerPrefabs[playerPrefabNumber]);
        localPlayer.transform.position = new Vector3(UnityEngine.Random.Range(-230f, 75f), 80f, 0);
        localPlayer.transform.FindChild("name").GetComponent<Text>().text = Connection.localUser.Name;

        // Set user position variables when local player spawned
        List<UserVariable> userVariables = new List<UserVariable>();
        userVariables.Add(new SFSUserVariable("px", (double)localPlayer.transform.position.x));
        userVariables.Add(new SFSUserVariable("py", (double)localPlayer.transform.position.y));
        userVariables.Add(new SFSUserVariable("mo", playerPrefabNumber));
        sfs.Send(new SetUserVariablesRequest(userVariables));
    }
    public void SpawnRemotePlayer(User user)
    {
        GameObject remotePlayer;

        // Get random position for new user
        playerPrefabs[user.GetVariable("mo").GetIntValue()].transform.position = new Vector3( (float)user.GetVariable("px").GetDoubleValue(), (float)user.GetVariable("py").GetDoubleValue() );

        remotePlayer = GameObject.Instantiate(playerPrefabs[user.GetVariable("mo").GetIntValue()]);
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
        userVariables.Add(new SFSUserVariable("px", (double)localPlayer.transform.position.x));
        userVariables.Add(new SFSUserVariable("py", (double)localPlayer.transform.position.y));
        sfs.Send(new SetUserVariablesRequest(userVariables));
    }

    public void jump()
    {
        Debug.Log("Jump!!!!!");
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
        userVariables.Add(new SFSUserVariable("px", (double)localPlayer.transform.position.x));
        userVariables.Add(new SFSUserVariable("py", (double)localPlayer.transform.position.y));
        sfs.Send(new SetUserVariablesRequest(userVariables));
    }

    // Destroy and exit player and show Game Over Message
    public void killLocalPlayer()
    {
        var gameOverTxt = GameObject.Find("Canvas/GameOverTxt").GetComponent<Text>();
        localPlayer.transform.Rotate(0f, 0f, -90f);
        sfs.Send(new LeaveRoomRequest());
        localPlayer.transform.FindChild("name").GetComponent<RectTransform>().Rotate(0, 0, -90);
        localPlayer.transform.FindChild("name").GetComponent<RectTransform>().anchoredPosition = new Vector3(20f, -16f, 0);
        localPlayer.transform.FindChild("name").GetComponent<Text>().color = Color.red;
        Destroy(localPlayer.GetComponent<Animator>());
        localPlayer = null;
        gameOverTxt.color = new Color(1f, 0f, 0f, 1f);
    }
    internal void killRemotePlayer(SFSUser user)
    {
        remotePlayers[user].transform.Rotate(0f, 0f, -90f);
        remotePlayers[user].transform.FindChild("name").GetComponent<RectTransform>().Rotate(0, 0, -90);
        remotePlayers[user].transform.FindChild("name").GetComponent<RectTransform>().anchoredPosition = new Vector3(20f, -16f, 0);
        remotePlayers[user].transform.FindChild("name").GetComponent<Text>().color = Color.red;
        Destroy(remotePlayers[user].GetComponent<Animator>());
        remotePlayers[user] = null;
    }

    // Special btn function
    public void specialBtn()
    {
        trace("Speciaaaaaaaaaaal!!!");
    }
    // A function to log on debugger and in game log viewer
    public void trace(string textString)
    {
        Debug.Log(textString);
        if(log != null)
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
}
