using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script handles all the mechanics related to Yukari's Persona Assistant, Isis.
/// </summary>
public class IsisMechanics : InteractionHandler
{
    [Tooltip("Set the speed of the hover frequency. This is how fast isis will move up and down")]
    public float hoverSpeed = 1;

    private YukariMechanics associatedYukariMechanics;

    

    #region monobehaviour methods
    public override void Awake()
    {
        
    }

    private void Update()
    {
        
    }
    #endregion monobehaviour methods
    #region animator events
    public void OnLaunchAirProjectile()
    {

    }
    #endregion animator events

    #region helper methods
    /// <summary>
    /// Call this method on creating our yukari mechanics to properly setup Isis
    /// </summary>
    /// <param name="associatedYukariMechanics"></param>
    public void SetupIsis(YukariMechanics associatedYukariMechanics)
    {
        AssociatedCharacterStats = associatedYukariMechanics.GetComponent<CharacterStats>();
        this.associatedYukariMechanics = associatedYukariMechanics;
        this.gameObject.SetActive(false);//Turn off isis on start
    }


    /// <summary>
    /// This method updates the hover position of our character. 
    /// </summary>
    private void UpdateHover()
    {

    }
    #endregion helper methods

}
