using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureContoller : MonoBehaviour
{
    public GameObject[] structureObjects; // This will hold all the avaliable strutures that this can upgrade into.
    public int costToBuild = 5; // Action Points Needed to Build.
    public int woodToBuild = 0; // Wood material required to build.
    public int stoneToBuild = 0; // Stone material required to build.
    public int foodToBuild = 0; // Food material required to build.

    // Start is called before the first frame update
    void Start()
    {
        // Create the most basic structure be instantiating the first object on the list.
        var temp = Instantiate(structureObjects[0], this.transform.position, this.transform.rotation);
        temp.transform.SetParent(this.transform);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
