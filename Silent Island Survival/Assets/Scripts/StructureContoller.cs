using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureContoller : MonoBehaviour
{
    public GameObject[] structureObjects; // This will hold all the avaliable strutures that this can upgrade into.
    public int woodToBuild = 0; // Wood material required to build.
    public int stoneToBuild = 0; // Stone material required to build.
    public int foodToBuild = 0; // Food material required to build.
    public int currentStructureLevel = 1;

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

    #region Functions

    public bool UpgradeStructure()
    {
        // This function returns a bool as true if it upgraded.

        if (currentStructureLevel < structureObjects.Length)
        {
            // Destroy child.
            Destroy(this.transform.GetChild(0).gameObject);

            // Create new child with the upgraded structure.
            var temp = Instantiate(structureObjects[currentStructureLevel], this.transform.position, this.transform.rotation);
            temp.transform.SetParent(this.transform);

            // Increment the current structure level.
            currentStructureLevel += 1;

            return true;
        }

        else return false;
    }

    #endregion
}
