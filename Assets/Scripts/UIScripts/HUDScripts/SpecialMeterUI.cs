using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SpecialMeterUI : MonoBehaviour
{

    #region main variables

    public Slider MeterSlider;

    private CharacterStats AssociatedCharacterStats;

    public int CharacterId = 0;

    #endregion

    #region monobehaviour methods
    void Start()
    {
        AssociatedCharacterStats = Overseer.Instance.PlayerObjects[CharacterId].GetComponent<CharacterStats>();
        AssociatedCharacterStats.OnCharacterHealthChanged.AddListener(CharacterMeterUpdate);
        AssociatedCharacterStats.OnMoveExecuted.AddListener(CharacterMeterUpdate);
        AssociatedCharacterStats.OnMoveHit.AddListener(CharacterMeterUpdate);
        CharacterMeterUpdate();
    }

    #endregion

    #region event methods

    private void CharacterMeterUpdate()
    {
        MeterSlider.value = AssociatedCharacterStats.SpecialMeter / AssociatedCharacterStats.MaxSpecialMeter;
    }

    #endregion
}
