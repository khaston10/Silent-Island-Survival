using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
    public int turnsUntilCropsMature = 0;
    public int cropsAtHarvest = 5;

    #endregion

    #region Variables for Living Quarters
    // Living Quarters heal action points for units in range.
    public int amountOfActionPointsHealed = 3;
    public int actionPointHealRange = 3;
    #endregion

    #region Variables for Wall

    // The wall's position can be fine adjusted by the user when creating, so we need two variables;
    // These values will always be between -.4 and .4 and will be updated by the main script when the wall is created.
    float wallOffsetX;
    float wallOffsetZ;
    Quaternion wallRot;

    #endregion

    #region Variables for Medical Facilities
    // Medical Facilities heal hit points for units in range.
    public int amountOfHitPointsHealed = 10; // This value should come in multiplies or 5.
    public int hitPointHealRange = 2;
    #endregion

    #region Variables for Traps

    private string[] PlayAnimationNames = { "Trap01Attack", "Trap02Attack"};
    public int[] trapDamge = { 3, 5, 10 };

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        // Create the most basic structure be instantiating the first object on the list.
        var temp = Instantiate(structureObjects[0], this.transform.position, this.transform.rotation);

        // If the structure is a wall we need to fine adjust the wall prefab's position.
        if (structureType == "Wall")
        {
            // Set the wall offsets from the main script.
            wallOffsetX = GameObject.Find("MainController").GetComponent<MainController>().wallOffsetX;
            wallOffsetZ = GameObject.Find("MainController").GetComponent<MainController>().wallOffsetZ;
            wallRot = GameObject.Find("MainController").GetComponent<MainController>().wallOnWallSelector.transform.rotation;

            temp.transform.position = new Vector3(this.transform.position.x + wallOffsetX, 0f, this.transform.position.z + wallOffsetZ);

            // Update the rotation.
            temp.transform.rotation = wallRot;
        }

        temp.transform.SetParent(this.transform);

        // Set radius of effect for Living Quarters and Medical Facilities.
        if (structureType == "Living Quarters" || structureType == "Medical Facility")
        {
            // Update the radius of effect.
            this.transform.GetChild(0).transform.localScale = Vector3.one * actionPointHealRange * .3f;
        }

        // If the structure is a trap we need to get the animator.
        
        if (structureType == "Trap")
        {
            PlayTrapAnim();
        }
        

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

            #region Upgrades for specific structure type.

            if (structureType == "Farm Plot")
            {
                //UntilCropsMature -= 1;
                cropsAtHarvest += 1;
            }

            else if (structureType == "Living Quarters")
            {
                amountOfActionPointsHealed += 1;
                actionPointHealRange += 1;
            }

            else if (structureType == "Medical Facility")
            {
                amountOfHitPointsHealed += 10;
                hitPointHealRange += 1;
            }

            #endregion

            // Because certain structures are setup differently we need to split by case to destory the old prefab.
            if (structureType == "Farm Plot")
            {
                Destroy(this.transform.GetChild(2).gameObject);
            }

            else if (structureType == "Living Quarters" || structureType == "Medical Facility")
            {
                Destroy(this.transform.GetChild(1).gameObject);

                // Update the radius of effect.
                this.transform.GetChild(0).transform.localScale = Vector3.one * actionPointHealRange * .3f;
            }

            else Destroy(this.transform.GetChild(0).gameObject);


            // Create new child with the upgraded structure.
            var temp = Instantiate(structureObjects[currentStructureLevel + 1], this.transform.position, this.transform.rotation);
            temp.transform.SetParent(this.transform);

            // If the structure is a wall we will need to set the wall's location more precisely.
            temp.transform.position = new Vector3(this.transform.position.x + wallOffsetX, 0f, this.transform.position.z + wallOffsetZ);

            // Update the rotation.
            temp.transform.rotation = wallRot;

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

        turnsUntilCropsMature = GameObject.Find("MainController").GetComponent<MainController>().selectedUnit.GetComponent<UnitController>().turnsUntilCropsMature;
    }

    public void CropsAreReadyForHarvest()
    {
        // This function returns true if crops have matured and are ready for harvest.
        if (daysSinceCropsPlanted >= turnsUntilCropsMature)
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
            return cropsAtHarvest * GameObject.Find("MainController").GetComponent<MainController>().selectedUnit.GetComponent<UnitController>().cropsAtHarvestMultiplier;

        }

        else return 0;
        
    }

    public void HealActionPoints(GameObject unit)
    {
        // Only Living Quarters can heal action points.
        if (structureType == "Living Quarters")
        {
            if ((unit.transform.GetComponent<UnitController>().actionPointsLimit - unit.transform.GetComponent<UnitController>().actionPoints) > amountOfActionPointsHealed)
            {
                unit.transform.GetComponent<UnitController>().actionPoints += amountOfActionPointsHealed;
            }

            else unit.transform.GetComponent<UnitController>().actionPoints = unit.transform.GetComponent<UnitController>().actionPointsLimit;
        } 
    }

    public void HealHitPoints(GameObject unit)
    {
        // Only Medical facilities can heal hit points.
        if (structureType == "Medical Facility")
        {
            if ((unit.transform.GetComponent<UnitController>().hitPointLimit - unit.transform.GetComponent<UnitController>().hitPoints) > amountOfHitPointsHealed)
            {
                unit.transform.GetComponent<UnitController>().hitPoints += amountOfHitPointsHealed;
            }

            else unit.transform.GetComponent<UnitController>().hitPoints = unit.transform.GetComponent<UnitController>().hitPointLimit;
        }
    }

    public void PlayTrapAnim()
    {
        StartCoroutine(PlayTrapAnimation());
    }

    public IEnumerator PlayTrapAnimation()
    {
        if (currentStructureLevel < 2)
        {
            var anim = GetComponentInChildren<Animator>();
            anim.Play(PlayAnimationNames[currentStructureLevel]);
        }

        else
        {
            GetComponentInChildren<ParticleSystem>().Play();
        }
        

        yield return new WaitForSeconds(1);


    }
    #endregion
}
