using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureContoller : MonoBehaviour
{
    public GameObject[] structureObjects; // This will hold all the avaliable strutures that this can upgrade into.
    public string structureType = "Default";
    public int hitPoints = 100;
    public int hitPointLimit = 100;
    public int[] woodToBuild = new int[] {0, 0, 0}; // Wood material required to build.
    public int[] stoneToBuild = new int[] { 0, 0, 0 }; // Stone material required to build.
    public int[] foodToBuild = new int[] { 0, 0, 0 }; // Food material required to build.
    public int currentStructureLevel = 0;

    #region Variables for Farm Plots

    public bool cropsPlanted = false;
    public bool cropsReadyForHarvest = false;
    public int daysSinceCropsPlanted = 0;
    public int daysUntilCropsMature = 2;
    public int cropsAtHarvest = 5;

    #endregion

    #region Variables for Living Quarters

    // Living Quarters heal action points for units in range.
    int amountOfActionPointsHealed = 3;
    public int actionPointHealRange = 3;


    #endregion

    #region Variables for Medical Facilities



    #endregion

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

        if (currentStructureLevel < structureObjects.Length - 1)
        {

            // Because certain structures are setup differently we need to split by case to destory the old prefab.
            if (structureType == "Farm Plot")
            {
                Destroy(this.transform.GetChild(2).gameObject);
            }

            else Destroy(this.transform.GetChild(0).gameObject);


            // Create new child with the upgraded structure.
            var temp = Instantiate(structureObjects[currentStructureLevel + 1], this.transform.position, this.transform.rotation);
            temp.transform.SetParent(this.transform);

            // Increment the current structure level.
            currentStructureLevel += 1;

            return true;
        }

        else return false;
    }


    public int RepairStructure(int repairAmount)
    {
        if (hitPoints + repairAmount <= hitPointLimit)
        {
            hitPoints += repairAmount;
            return repairAmount;
        }

        else
        {
            var repairAmountToReturn = hitPointLimit - hitPoints;
            hitPoints = hitPointLimit;
            return repairAmountToReturn;
        }

    }

    public void PlantCrops()
    {
        // Turn the planted crops to visible.
        transform.GetChild(0).gameObject.SetActive(true);

        cropsPlanted = true;
        daysSinceCropsPlanted = 0;
    }

    public void CropsAreReadyForHarvest()
    {
        // This function returns true if crops have matured and are ready for harvest.
        if (daysSinceCropsPlanted >= daysUntilCropsMature)
        {
            cropsReadyForHarvest = true;

            // Turn the planted crops to invisible.
            transform.GetChild(0).gameObject.SetActive(false);

            // Turn the matured crops to visible. 
            transform.GetChild(1).gameObject.SetActive(true);
        }

    }

    public int HarvestCrops()
    {
        if (cropsReadyForHarvest)
        {
            // Turn the matured crops to visible. 
            transform.GetChild(1).gameObject.SetActive(false);

            daysSinceCropsPlanted = 0;
            cropsPlanted = false;
            cropsReadyForHarvest = false;
            return cropsAtHarvest;

        }

        else return 0;
        
    }

    public void HealActionPoints(GameObject unit)
    {
        // Only Living Quarters can heal.
        if (structureType == "Living Quarters")
        {
            if ((unit.transform.GetComponent<UnitController>().actionPointsLimit - unit.transform.GetComponent<UnitController>().actionPoints) > amountOfActionPointsHealed)
            {
                unit.transform.GetComponent<UnitController>().actionPoints += amountOfActionPointsHealed;
            }

            else unit.transform.GetComponent<UnitController>().actionPoints = unit.transform.GetComponent<UnitController>().actionPointsLimit;
        }

        
    }
    #endregion
}
