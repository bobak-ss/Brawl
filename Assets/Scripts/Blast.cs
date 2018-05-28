using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blast : MonoBehaviour {

    private Transform blastTrans;
    private float newScale = 0f;

    public float maxRange = 200f;
    
	void Start () {
        blastTrans = GetComponent<Transform>();
	}
	
	void Update () {
        if(newScale < maxRange)
        {
            newScale = Mathf.Lerp(20f, maxRange, 1 * Time.deltaTime);
            blastTrans.localScale = new Vector3(newScale, newScale, 1);
        }
        else
        {
            newScale = Mathf.Lerp(maxRange, 20f, 1 * Time.deltaTime);
            blastTrans.localScale = new Vector3(newScale, newScale, 1);
        }
        //Debug.Log(newScale);
    }
}
