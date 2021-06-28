using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    #region Variables Attributes
    public string unitName = "Default Unit";
    public int actionPoints = 1;
    public int actionPointsLimit = 10;
    public int hitPoints = 100;
    public int hitPointLimit = 100;
    public int attack = 5;
    public int attackRange = 3;
    public int defense = 4;
    public int sight = 5;
    public int repairPoints = 10; // This stat will not deplete but sets the amount of repair in one repair session.
    #endregion

    void Start()
    {
        
    }


    void Update()
    {
        
    }
}
