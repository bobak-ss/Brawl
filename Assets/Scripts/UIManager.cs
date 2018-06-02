using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{    
    public String[] heroDesc;
    public GameObject[] heroes;
    public GameObject heroesMenu;
    public Text heroInfo;

    private int myHeroNum;

    void Awake ()
    {
        // if connection is not initialized we start it by loading connection scene
        if (!SmartFoxConnection.IsInitialized)
            SceneManager.LoadScene("Connection");
	}

    public void chooseHeroBtn()
    {
        heroesMenu.SetActive(true);
    }
    public void backToMainMenuBtn()
    {
        heroesMenu.SetActive(false);
    }
    public void startRandom()
    {
        GameManager.Instance.playerPrefabNumber = UnityEngine.Random.Range(0, heroDesc.Length - 1);
        SceneManager.LoadScene("GameStaging");
    }
    public void selectHero(int heroNumber)
    {
        heroInfo.text = heroDesc[heroNumber];
        myHeroNum = heroNumber;
    }
    public void startGame()
    {
        GameManager.Instance.playerPrefabNumber = myHeroNum;
        SceneManager.LoadScene("GameStaging");
    }
}
