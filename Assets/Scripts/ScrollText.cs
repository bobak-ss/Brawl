using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollText : MonoBehaviour {

    public ScrollRect scrollRect;
    public float max = 0;

	void Update () {
        scrollRect.verticalNormalizedPosition = max;
    }
}
