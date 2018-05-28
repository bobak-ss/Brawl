using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public Dictionary<GameObject, String> heroDesc = new Dictionary<GameObject, String>();
    public String[] desc;
    public GameObject[] heroes;
    public GameObject heroesMenu;
    public Text heroInfo;

    private GameManager gm;
    private GameObject myHero;

    void Awake () {
        gm = GetComponent<GameManager>();

        for (int i = 0; i < heroes.Length; i++)
            heroDesc.Add(heroes[i], desc[i]);
	}

    private void disableObjects(GameObject[] objects)
    {
        foreach (var o in objects)
        {
            o.SetActive(false);
        }
    }
    private void enableObjects(GameObject[] objects)
    {
        foreach (var o in objects)
        {
            o.SetActive(true);
        }
    }

    public void chooseHeroBtn()
    {
        heroesMenu.SetActive(true);
    }

    public void startRandom()
    {
        GameManager.Instance.localPlayerPrefab = heroes[UnityEngine.Random.Range(0, heroes.Length)];
        SceneManager.LoadScene("GameStaging");
    }

    public void selectHero(GameObject hero)
    {
        heroInfo.text = heroDesc[hero];
        myHero = hero;
    }

    public void startGame()
    {
        GameManager.Instance.localPlayerPrefab = myHero;
        SceneManager.LoadScene("GameStaging");
    }

    public void backToMainMenuBtn()
    {
        heroesMenu.SetActive(false);
    }
}
