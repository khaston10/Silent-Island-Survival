using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackFeedbackCont : MonoBehaviour
{
    int timeVisible;
    float VisibilityTimer;

    // Start is called before the first frame update
    void Start()
    {
        timeVisible = 3;
        VisibilityTimer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (VisibilityTimer > timeVisible)
        {
            timeVisible = 3;
            VisibilityTimer = 0;
            this.gameObject.SetActive(false);
        }
        else
        {
            VisibilityTimer += Time.deltaTime;
        }
            
    }
}
