using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildFeedbackController : MonoBehaviour
{
    // This is the length of time the panel will be shown to the player, in seconds.
    float lifeTimer; 

    // Start is called before the first frame update
    void Start()
    {
        lifeTimer = 3;
    }

    // Update is called once per frame
    void Update()
    {
        if (lifeTimer > 0) lifeTimer -= Time.deltaTime;

        else
        {
            lifeTimer = 3;
            this.gameObject.SetActive(false);
        }
            
            
    }
}
