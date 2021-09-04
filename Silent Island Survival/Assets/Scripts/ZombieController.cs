using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieController : MonoBehaviour
{
    #region Variables Attributes

    // Zombies will not regenerate hit points and all action points will be spend every turn, (If Possible)
    // So we do not need limits.

    public int actionPoints = 4;
    public int hitPoints = 10;
    public int hitPointLimit = 10;
    public int attack = 3;
    public int attackRange = 1;
    public int defense = 3;
    public GameObject target; // This target will only be used to track the unit they are trying to map to and destroy.
    public List<GameObject> zombiePath = new List<GameObject>();
    public List<GameObject> tempPath01 = new List<GameObject>();
    private List<GameObject> tempPath02 = new List<GameObject>();
    private List<GameObject> movementGrid = new List<GameObject>();
    private GameObject startGridElement;
    private GameObject endGridElement;
    public bool startTurn; // This bool will be switched to True when the main script activates the zombies turn.
    private bool getNextPos; // Used when zombie needs next position to move.
    private bool zombieIsMoving; // This bool will help to move the zombie.
    public GameObject gridPrefab;

    // For testing purposes we are going to checnge the color of certain grid elements.
    //Even though they will be invisible in game play.
    public Material startMat;
    public Material endMat;
    public Material selectedMat;

    // These variables are used in Get Next Element Function.
    GameObject leftGridElemn;
    GameObject rightGridElemn;
    GameObject upGridElemn;
    GameObject downGridElemn;
    float distanceLeftGridElemnToTarget = 0;
    float distanceRightGridElemnToTarget = 0;
    float distanceUpGridElemnToTarget = 0;
    float distanceDownGridElemnToTarget = 0;


    // We will use these next variables in the while loop.
    int GridElementRow = 0;
    int GridElementCol = 0;
    int actionPointsRemaining;

    // Variables used durning zombie movement.
    int nextPositionIndex = 0;
    Vector3 NextPosition = Vector3.zero;
    float zombieMovementSpeed = 1f;

    private Animator anim;
    #endregion


    void Start()
    {
        // Get animator contoller from unit.
        anim = GetComponentInChildren<Animator>();
    }


    void Update()
    {
        // The zombie will get switched to start Turn by the main controller scripts or the previous zombie on the current zombie list.
        if (startTurn)
        {
            startTurn = false;
            target = AcquireTarget();
            CreateZombieMovementPath();
            SetAllBoolsToFalse();
            nextPositionIndex = 0;
            getNextPos = true;
        }

        else if (getNextPos)
        {
            // Update the next position index.
            nextPositionIndex += 1;
            GetNextPosition();
        }

        else if (zombieIsMoving)
        {
            MoveZombie();
        }

    }

    #region Functions

    public GameObject AcquireTarget()
    {
        // This function returns a game object that is one of the current units in play.
        // The unit that is the shortest distance away from the zombie.
        var copyOfUnitsInPlay = GameObject.Find("MainController").GetComponent<MainController>().unitsInPlay;
        var tempTarget = copyOfUnitsInPlay[0];
        var distanceToCurrentTarget = Vector3.Distance(this.transform.position, tempTarget.transform.position);

        for (int unitsIndex = 0; unitsIndex < copyOfUnitsInPlay.Count; unitsIndex++)
        {
            // check to see if the new unit is closer.
            if (Vector3.Distance(this.transform.position, copyOfUnitsInPlay[unitsIndex].transform.position) <= distanceToCurrentTarget)
            {
                tempTarget = copyOfUnitsInPlay[unitsIndex];
                distanceToCurrentTarget = Vector3.Distance(this.transform.position, tempTarget.transform.position);
            }
        }

        return tempTarget;
    }

    public void CreateZombieMovementPath()
    {
        // This function will create a path that will get the zombie to, or close to the target.
        // The path is stored as zombiePath.

        //1. Clear old path and grid.
        for (int listIndex = 0; listIndex < tempPath01.Count; listIndex++)
        {
            GameObject.Destroy(tempPath01[listIndex]);
        }
        tempPath01.Clear();

        for (int listIndex = 0; listIndex < movementGrid.Count; listIndex++)
        {
            GameObject.Destroy(movementGrid[listIndex]);
        }
        movementGrid.Clear();


        //2. Create a grid of size actionPoints by action point centered on the zombie.
        var gridSize = actionPoints;

        for (int row = - gridSize; row <= gridSize; row++)
        {
            for (int col = - gridSize; col <= gridSize; col++)
            {
                var temp = Instantiate(gridPrefab, new Vector3((Mathf.RoundToInt(this.transform.position.x + row)), .3f, (Mathf.RoundToInt(this.transform.position.z + col))), Quaternion.identity);
                movementGrid.Add(temp);
            }
        }

        //3. Mark Start and End grid element.
        // The start is always the middle grid element as the grid is centered on the zombie.
        startGridElement = GetgridElementAtPosition(Mathf.RoundToInt(this.transform.position.x), Mathf.RoundToInt(this.transform.position.z));
        startGridElement.GetComponent<MeshRenderer>().material = startMat;

        // The end grid element will be whatever grid element is closest to the target.
        endGridElement = movementGrid[0];

        for (int elements = 1; elements < movementGrid.Count; elements++)
        {
            // We compare the next elements distance from target to the current endGridElement and trade if nessesscary.
            if (Vector3.Distance(movementGrid[elements].transform.position, target.transform.position) < Vector3.Distance(endGridElement.transform.position, target.transform.position))
            {
                endGridElement = movementGrid[elements];
            }

        }

        endGridElement.GetComponent<MeshRenderer>().material = endMat;

        //4. Create a path and save it to tempPath01.
        tempPath01.Add(startGridElement);

        actionPointsRemaining = actionPoints - 2;

        while (true)
        {
            tempPath01.Add(GetNextGridElemn(tempPath01[tempPath01.Count - 1]));

            // Set the row and col.
            GridElementRow = Mathf.RoundToInt(tempPath01[tempPath01.Count - 1].transform.position.x);
            GridElementCol = Mathf.RoundToInt(tempPath01[tempPath01.Count - 1].transform.position.z);

            // Check to see if the ground element is passable.
            // If it is not, we will need to end this path.
            if (GameObject.Find("MainController").GetComponent<MainController>().groundTiles[GameObject.Find("MainController").GetComponent<MainController>().
                LocateIndexOfGroundTile(GridElementRow, GridElementCol)] != null)
            {
                if (GameObject.Find("MainController").GetComponent<MainController>().groundTiles[GameObject.Find("MainController").GetComponent<MainController>().
                LocateIndexOfGroundTile(GridElementRow, GridElementCol)].GetComponent<GroundTileController>().terrainIsPassable != true)
                {
                    break;
                }
            }

            else break;


            // Check to see if the zombie has any action points left.
            if (actionPointsRemaining <= 0) break;
            else actionPointsRemaining -= 1;

        }

        // Change the material of all elements on the path for testing.
        ChangePathMaterial(tempPath01, selectedMat);

        // Set zombie to moving.
        zombieIsMoving = true;


    }

    #region Helper Functions For CreateZombieMovementPath

    public GameObject GetNextGridElemn(GameObject currentElement)
    {
        // This function is only called when creating the zombieMovementPath.
        // It checks all 4 adjacent ground tiles and returns the grid element that is the closest to the target.
        // So we start by guessing the left grid element is closest...
        leftGridElemn = GetgridElementAtPosition(Mathf.RoundToInt(currentElement.transform.position.x - 1), Mathf.RoundToInt(currentElement.transform.position.z));
        rightGridElemn = GetgridElementAtPosition(Mathf.RoundToInt(currentElement.transform.position.x + 1), Mathf.RoundToInt(currentElement.transform.position.z));
        upGridElemn = GetgridElementAtPosition(Mathf.RoundToInt(currentElement.transform.position.x), Mathf.RoundToInt(currentElement.transform.position.z - 1));
        downGridElemn = GetgridElementAtPosition(Mathf.RoundToInt(currentElement.transform.position.x), Mathf.RoundToInt(currentElement.transform.position.z + 1));

        distanceLeftGridElemnToTarget = Vector3.Distance(leftGridElemn.transform.position, endGridElement.transform.position);
        distanceRightGridElemnToTarget = Vector3.Distance(rightGridElemn.transform.position, endGridElement.transform.position);
        distanceUpGridElemnToTarget = Vector3.Distance(upGridElemn.transform.position, endGridElement.transform.position);
        distanceDownGridElemnToTarget = Vector3.Distance(downGridElemn.transform.position, endGridElement.transform.position);

        // Check to see which one is closest.
        if (distanceLeftGridElemnToTarget <= distanceRightGridElemnToTarget && distanceLeftGridElemnToTarget <= distanceUpGridElemnToTarget && distanceLeftGridElemnToTarget <= distanceDownGridElemnToTarget)
        {
            return leftGridElemn;
        }

        else if (distanceRightGridElemnToTarget <= distanceUpGridElemnToTarget && distanceRightGridElemnToTarget <= distanceDownGridElemnToTarget)
        {
            return rightGridElemn;
        }

        else if (distanceUpGridElemnToTarget <= distanceDownGridElemnToTarget)
        {
            return upGridElemn;
        }

        else return downGridElemn;
    }

    public GameObject GetgridElementAtPosition(int row, int col)
    {
        for (int elemIndex = 0; elemIndex < movementGrid.Count; elemIndex++)
        {
            if (Mathf.Abs(movementGrid[elemIndex].transform.position.x - row) < 0.5f && Mathf.Abs(movementGrid[elemIndex].transform.position.z - col) < 0.5f)
            {
                return movementGrid[elemIndex];
            }
        }

        Debug.Log("GetgridElementAtPosition has had an invalid input.");
        return null;
    }

    public void ChangePathMaterial(List<GameObject> path, Material material)
    {
        for (int indexOfElement = 0; indexOfElement < path.Count; indexOfElement++)
        {
            path[indexOfElement].GetComponent<MeshRenderer>().material = material;
        }
    }

    #endregion

    public void MoveZombie()
    { 
        // Check to see if the zombie has reached the position.
        if (Vector3.Distance(this.transform.position, NextPosition) < .1)
        {
            SetAllBoolsToFalse();
            getNextPos = true;
        }
        else
        {
            this.transform.LookAt(NextPosition);
            this.transform.position = Vector3.MoveTowards(this.transform.position, NextPosition, zombieMovementSpeed * Time.deltaTime);
        }
        
    }

    public void GetNextPosition()
    {
        // This function will take the next element of the zombie movement path
        // and use the ZombieInteract with ground tile to decide if the zombie should
        // 1. Move on top of the tile,
        // 2. Attack the tile,
        // 3. or end the turn because the path is impossible to complete. EX: Void, building.

        // Be setting all bools to false we ensure the code does not loop back to here.
        SetAllBoolsToFalse();

        // First we need to check if there are still movement elements left in this list.
        if (nextPositionIndex < tempPath01.Count && actionPoints > 1)
        {
            // Reduce action point by 1.
            actionPoints -= 1;

            NextPosition = new Vector3(tempPath01[nextPositionIndex].transform.position.x, 0f, tempPath01[nextPositionIndex].transform.position.z);

            // 1. Move on top of the tile.
            if (ZombieInteractsWithNextGroundTileOnMove(this.gameObject, GameObject.Find("MainController").GetComponent<MainController>().LocateIndexOfGroundTile(Mathf.RoundToInt(tempPath01[nextPositionIndex].transform.position.x), Mathf.RoundToInt(tempPath01[nextPositionIndex].transform.position.z))))
            {
                PlayWalk();
                zombieIsMoving = true;
            }

            // These cases are handled by the Zombie Interacte with Ground Tile.
            //2.Attack the tile,
            // 3. or end the turn because the path is impossible to complete. EX: Void, building.
        }

        // Else we need to activate the next zombie.
        else
        {
            ActivateNextZombie();
        }
           
    }

    public bool ZombieInteractsWithNextGroundTileOnMove(GameObject unit, int indexForTileArray)
    {
        // Returns a bool of true if the zombie should move onto the selected ground tile.

        // If the movement selected list is not empty that means we need to move the unit.

        if (GameObject.Find("MainController").GetComponent<MainController>().groundTiles[indexForTileArray] == null)
        {
            StartCoroutine(DumbWalkAnimation());
            return false;
        }

        // If the ground title is passable then we can move the unit on to it.

        else if (GameObject.Find("MainController").GetComponent<MainController>().groundTiles[indexForTileArray].transform.GetComponent<GroundTileController>().terrainIsPassable)
        {
            // Set the old ground tile to passable.
            unit.transform.parent.GetComponent<GroundTileController>().terrainIsPassable = true;


            // Set the new ground tiles to be the parent.
            unit.transform.SetParent(GameObject.Find("MainController").GetComponent<MainController>().groundTiles[indexForTileArray].transform);

            // Set the new ground tile to unpassable.
            unit.transform.parent.GetComponent<GroundTileController>().terrainIsPassable = false;

            return true;
        }

        else
        {
            // We need to handle the unit's interation with that object.

            var childCount = GameObject.Find("MainController").GetComponent<MainController>().groundTiles[indexForTileArray].transform.transform.childCount;
            for (var i = 0; i < childCount; i++)
            {
                var child = GameObject.Find("MainController").GetComponent<MainController>().groundTiles[indexForTileArray].transform.GetChild(i);
                var childTag = child.tag;

                // If the ground tile contains a tree or rock we will harvest.
                if (childTag == "Tree" || childTag == "Rock" || childTag == "Loot Box" || childTag == "Farm Plot" || childTag == "Wall" || childTag == "Living Quarters" || childTag == "Medical Facility" || childTag == "Town Hall")
                {
                    ZombieAttack(child.transform.gameObject);
                }

                // If the ground title is a Abandoned Structure or Structure we will bring up Interact with Structure Menu.
                else if (childTag == "Abandoned House" || childTag == "Abandoned Factory"
                    || childTag == "Abandoned Vehicle" || childTag == "Holding Factory" || childTag == "Zombie")
                {
                    StartCoroutine(DumbWalkAnimation());
                }

                // If the ground title contains a unit we will have the player attack.
                else if (childTag == "Unit") ZombieAttack(child.transform.gameObject);

            }

            return false;
        }
    }

    public void ZombieAttack(GameObject target)
    {
        // Have the zombie look at the target.
        this.transform.LookAt(target.transform);

        if (target.transform.tag == "Rock" || target.transform.tag == "Tree" || target.transform.tag == "Loot Box" )
        {
            // Set the ground title to passable and destory the tree, or rock.
            target.transform.parent.GetComponent<GroundTileController>().terrainIsPassable = true;

            StartCoroutine(AttackUnitAnimation(target));
        }

        else if (target.transform.tag == "Farm Plot" || target.transform.tag == "Wall" || target.transform.tag == "Living Quarters" || target.transform.tag == "Medical Facility" || target.transform.tag == "Town Hall")
        {
            // Check the health of the object. We only need to destory it if the health is lower than the zombie attack.
            if (target.GetComponent<StructureContoller>().hitPoints < attack)
            {
                // Set the ground title to passable and destory the tree, or rock.
                target.transform.parent.GetComponent<GroundTileController>().terrainIsPassable = true;
            }

            // Reduce the hit points of the object.
            target.GetComponent<StructureContoller>().hitPoints -= attack;

            StartCoroutine(AttackUnitAnimation(target));
        }

        else if (target.transform.tag == "Unit")
        {
            StartCoroutine(AttackUnitAnimation(target));
        }

    }

    public void ZombieTakeDamge(int damageAmount)
    {
        StartCoroutine(TakeDamageAnimation(damageAmount));
    }

    public void ActivateNextZombie()
    {
        SetAllBoolsToFalse();

        int currentZombieIndex = GameObject.Find("MainController").GetComponent<MainController>().zombiesInPlay.IndexOf(gameObject);

        if (currentZombieIndex < GameObject.Find("MainController").GetComponent<MainController>().zombiesInPlay.Count - 1)
        {
            GameObject.Find("MainController").GetComponent<MainController>().selectedZombie = GameObject.Find("MainController").GetComponent<MainController>().zombiesInPlay[currentZombieIndex + 1];
            GameObject.Find("MainController").GetComponent<MainController>().updateCameraForZombieTurn();
            GameObject.Find("MainController").GetComponent<MainController>().zombiesInPlay[currentZombieIndex + 1].GetComponent<ZombieController>().startTurn = true;

        }

        else GameObject.Find("MainController").GetComponent<MainController>().UpdateTurn();
    }

    public void SetAllBoolsToFalse()
    {
        // There are lots of times we need to set all the rest of the bools to false
        // Then turn one back on. This function can help clean up the code.
        startTurn = false;
        getNextPos = false;
        zombieIsMoving = false;
    }

    #region Coroutines For Animations

    IEnumerator TakeDamageAnimation(int damageAmount)
    {
        if ((damageAmount - defense) > hitPoints)
        {
            // Play the death animation.
            anim.Play("Z_dead_A");
        }

        else
        {
            // Play the damage animation.
            anim.Play("Z_damage");
        }
        

        // suspend execution the length of animations
        yield return new WaitForSeconds(anim.GetCurrentAnimatorClipInfo(0).Length);

        hitPoints -= (damageAmount - defense);
    }

    IEnumerator DieAnimation()
    {
        Debug.Log("Play Die Animation");

        // suspend execution the length of animations
        yield return new WaitForSeconds(anim.GetCurrentAnimatorClipInfo(0).Length);

        //GameObject.Find("MainController").GetComponent<MainController>().RemoveDeceasedUnit(gameObject);
    }

    IEnumerator AttackUnitAnimation(GameObject target)
    {

        if (target.transform.tag == "Unit")
        {
            anim.Play("Z_attack_A");
        }

        else
        {
            anim.Play("Z_attack_A");
        }
        

        // suspend execution the length of animations
        yield return new WaitForSeconds(anim.GetCurrentAnimatorClipInfo(0).Length);


        if (target.transform.tag == "Unit")
        {
            // Call the take damage function on the unit script.
            target.GetComponent<UnitController>().TakeDamage(attack);
        }

        else if (target.transform.tag == "Rock" || target.transform.tag == "Tree" || target.transform.tag == "Loot Box")
        {
            Destroy(target);
        }

        else if (target.transform.tag == "Farm Plot" || target.transform.tag == "Wall" || target.transform.tag == "Living Quarters" || target.transform.tag == "Medical Facility" || target.transform.tag == "Town Hall")
        {
            // Check the hit points of the object and destroy it if it is below 0.
            if (target.GetComponent<StructureContoller>().hitPoints <= 0)
            {
                // Remove the target from the list of structures.
                GameObject.Find("MainController").GetComponent<MainController>().currentStructuresInGame.Remove(target);

                // Destory Game Object.
                Destroy(target);
            }
        }


        // Set bools.
        startTurn = true;

    }

    IEnumerator DumbWalkAnimation()

    {
        anim.Play("Z_dumb_walk_A");

        // suspend execution the length of animations
        yield return new WaitForSeconds(anim.GetCurrentAnimatorClipInfo(0).Length);

        // Set bools.
        actionPoints = 0;
        ActivateNextZombie();
    }

    public void PlayWalk()
    {
        anim.Play("Z_walk");
    }

    #endregion

    #endregion
}
