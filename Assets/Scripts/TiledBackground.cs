using System.Collections;
using UnityEngine;

public class TiledBackground : MonoBehaviour {

	public int textureSize = 32;

	public bool scaleHorizontialy = true;
	public bool scaleVerticaly = true;

	// Use this for initialization
	void Start () {
		var xTile = scaleHorizontialy ? Mathf.Ceil(transform.localScale.x / textureSize) : 1;
		var yTile = scaleVerticaly ? Mathf.Ceil(transform.localScale.y / textureSize) : 1;

		GetComponent<Renderer>().material.mainTextureScale = new Vector3 (xTile, yTile, 1);
	}
}
