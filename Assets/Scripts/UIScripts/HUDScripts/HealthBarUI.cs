using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Our basic ui display for a single player. This will display the player's current health
/// </summary>
public class HealthBarUI : MonoBehaviour
{


    #region main variables
    public int characterID = 0;

    public Slider healthSlider;
    public Slider comboDamageSlider;
    public Slider chipDamageSlider;

    private CharacterStats associatedCharacterStats;
    #endregion main variables

    #region monobehavouir methods

    private void Awake()
    {
        
    }

    private void Start()
    {
        associatedCharacterStats = Overseer.Instance.PlayerObjects[characterID].GetComponent<CharacterStats>();
        associatedCharacterStats.OnCharacterHealthChanged.AddListener(OnCharacterHealthUpdated);
        OnCharacterHealthUpdated();
    }

    private void OnDestroy()
    {
        if (associatedCharacterStats)
        {
            associatedCharacterStats.OnCharacterHealthChanged.RemoveListener(OnCharacterHealthUpdated);
        }
    }
    #endregion monobehaviour methods

    #region visually update methods
    private void OnCharacterHealthUpdated()
    {
        healthSlider.value = associatedCharacterStats.CurrentHealth / associatedCharacterStats.MaxHealth;
        chipDamageSlider.value = (associatedCharacterStats.CurrentHealth + associatedCharacterStats.CurrentChipDamage) / associatedCharacterStats.MaxHealth;
        comboDamageSlider.value = (associatedCharacterStats.CurrentHealth + associatedCharacterStats.ComboDamage) / associatedCharacterStats.MaxHealth;
    }

    /// <summary>
    /// When our combo ends, we will call this method to begin a coroutine that will drop the combo damage to match the current health
    /// </summary>
    private void OnComboEnded()
    {

    }
    #endregion visually update methods


}
