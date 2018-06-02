using UnityEngine;
public class DDOL : MonoBehaviour {
	void Awake () {
        DontDestroyOnLoad(gameObject);
	}
}