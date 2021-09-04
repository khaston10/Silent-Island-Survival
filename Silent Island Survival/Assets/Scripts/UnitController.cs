using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    #region Variables Attributes
    public string unitName = "Default Unit";
    public enum Class
    {
        Basic,     // == 0
        Farmer,    // == 1
        Soldier,   // == 2
    }
    public Class unitClass;
    public int actionPoints = 1;
    public int actionPointsLimit = 10;
    public int hitPoints = 100;
    public int hitPointLimit = 100;
    public int attack = 5;
    public int attackRange = 3;
    public int defense = 4;
    public int sight = 5;
    public int repairPoints = 10; // This stat will not deplete but sets the amount of repair in one repair session.
    public float criticalHitPercentage = .1f; // This value will be between 0 - and 1.
    public int cropsAtHarvestMultiplier = 1; // This will be used for upgrades and will either have a value of 1 or 2.
    public int turnsUntilCropsMature = 5; // This will only be updated on farmers.
    private Animator anim;
    #endregion

    void Start()
    {
        // Set the unit Class
        if (this.name == "Basic Unit(Clone)") unitClass = Class.Basic;
        else if (this.name == "Farmer Unit(Clone)") unitClass = Class.Farmer;
        else if (this.name == "Soldier Unit(Clone)") unitClass = Class.Soldier;
        else Debug.Log("Invalid Unit Type. No Class assigned.");

        // Get animator contoller from unit.
        anim = GetComponentInChildren<Animator>();

    }


    void Update()
    {
        
    }

    #region Functions

    public void TakeDamage(int damageAmount)
    {
        // Decided if the unit will die from damage.
        if (damageAmount >= hitPoints)
        {
            hitPoints = 0;
            StartCoroutine(DieAnimation());
        }

        else
        {
            StartCoroutine(TakeDamageAnimation(damageAmount));
        }
    }

    #region Coroutines For Animations

    IEnumerator TakeDamageAnimation(int damageAmount)
    {
        anim.Play("m_fight_damage");

        // suspend execution the length of animations
        yield return new WaitForSeconds(anim.GetCurrentAnimatorClipInfo(0).Length);

        hitPoints -= damageAmount;
    }

    IEnumerator DieAnimation()
    {
        anim.Play("m_death_A");

        // suspend execution the length of animations
        yield return new WaitForSeconds(anim.GetCurrentAnimatorClipInfo(0).Length);

        //GameObject.Find("MainController").GetComponent<MainController>().RemoveDeceasedUnit(gameObject);
    }

    public void PlayIdle()
    {
        // This function should only get called from the main contoller.
        anim.Play("m_idle_A");
    }

    public void PlayRun()
    {
        // This function should only get called from the main contoller.
        anim.Play("m_run");
    }

    public void PlayInteract()
    {
        // This function should only get called from the main contoller.
        anim.Play("m_interact_A");
    }

    public void PlayAttack()
    {
        // This function should only get called from the main contoller.
        anim.Play("m_fight_attack_A");
        
    }

    public void PlayRangedAttack()
    {
        // This function should only get called from the main contoller.
        anim.Play("m_pistol_draw");
        
    }

    #endregion

    #endregion
}
