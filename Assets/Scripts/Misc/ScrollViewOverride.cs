using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewOverride : MonoBehaviour
{
    private ScrollRect scrollRect;
    // Start is called before the first frame update
    void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
        scrollRect.horizontal = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
