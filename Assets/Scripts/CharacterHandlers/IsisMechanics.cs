using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(ProjectileInteractionHandler))]
/// <summary>
/// This script handles all the mechanics related to Yukari's Persona Assistant, Isis.
/// </summary>
public class IsisMechanics : MonoBehaviour
{

    #region const variables
    private const string ISIS_ATTACK_STATE = "IsisAttack";
    #endregion const variables

    public enum IsisAttackAnimation
    {
        CROUCH_ATTACK = 0x00,
        JUMP_ATTACK = 0x01,
        JUMP_ATTACK_2 = 0x02,
    }

    [Header("Entry/Exit Animations")]
    [SerializeField]
    private float entranceAnimationScalingTime = .3f;
    [SerializeField]
    private float exitAnimationScalingTime = .3f;
    [SerializeField]
    private Vector3 entryExitGoalScaling;
    [SerializeField]
    private AnimationCurve entryAnimationCurve;
    [SerializeField]
    private AnimationCurve exitAnimationCurve;
    [Tooltip("Set the speed of the hover frequency. This is how fast isis will move up and down")]
    public float hoverSpeed = 1;
    /// <summary>
    /// The animator component for Isis
    /// </summary>
    private Animator isisAnim;
    [Tooltip("This is the container object where isis will be stored when returned to yukari. This is what determines Isis' placement when used")]
    private Transform isisContainer;
    [Tooltip("The sprite renderer component of Isis")]
    public SpriteRenderer isisSpriteRenderer;
    /// <summary>
    /// The associated animation speed controller for Isis
    /// </summary>
    private AnimationSpeedController isisAnimSpeedController;
    

    /// <summary>
    /// Associated Yukari Mechanics that will be used for Isis.
    /// </summary>
    private YukariMechanics associatedYukariMechanics;

    /// <summary>
    /// Since Isis is not a separate character, we will apply a projectile interaction handler to it
    /// </summary>
    private ProjectileInteractionHandler projectileInteractionHandler;
    #region monobehaviour methods

    private void Update()
    {
        UpdateHover();
    }
    #endregion monobehaviour methods
   

    #region helper methods
    /// <summary>
    /// Call this method on creating our yukari mechanics to properly setup Isis
    /// </summary>
    /// <param name="associatedYukariMechanics"></param>
    public void SetupIsis(YukariMechanics associatedYukariMechanics)
    {
        this.associatedYukariMechanics = associatedYukariMechanics;
        this.isisContainer = this.transform.parent;
        this.isisAnim = GetComponent<Animator>();
        this.isisAnimSpeedController = GetComponent<AnimationSpeedController>();
        this.projectileInteractionHandler = GetComponent<ProjectileInteractionHandler>();
        this.gameObject.SetActive(false);//Turn off isis on start
    }


    /// <summary>
    /// This method updates the hover position of our character. 
    /// </summary>
    private void UpdateHover()
    {

    }
    #endregion helper methods

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private IEnumerator EnterIsisScalingAnimation()
    {
        float timeThatHasPassed = 0;

        while (timeThatHasPassed < entranceAnimationScalingTime)
        {
            this.transform.localScale = Vector3.Lerp(entryExitGoalScaling, Vector3.one, entryAnimationCurve.Evaluate(timeThatHasPassed / entranceAnimationScalingTime));
            yield return null;
            timeThatHasPassed += Overseer.DELTA_TIME;
        }
        this.transform.localScale = Vector3.one;
        this.isisAnimSpeedController.enabled = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private IEnumerator ExitIsisScalingAnimation()
    {
        float timeThatHasPassed = 0;

        while (timeThatHasPassed < exitAnimationScalingTime)
        {
            this.transform.localScale = Vector3.Lerp(Vector3.one, entryExitGoalScaling, exitAnimationCurve.Evaluate((exitAnimationScalingTime - timeThatHasPassed) / exitAnimationScalingTime));
            yield return null;
            timeThatHasPassed += Overseer.DELTA_TIME;

        }
        EndIsisAttack();
    }

    /// <summary>
    /// This should be called 
    /// </summary>
    /// <param name="isisAttackAnimation"></param>
    public void BeginIsisAttack(IsisAttackAnimation isisAttackAnimation)
    {
        if (this.gameObject.activeSelf)
        {
            return;
        }

        this.gameObject.SetActive(true);
        this.isisAnimSpeedController.enabled = false;
        isisAnim.SetInteger(ISIS_ATTACK_STATE, (int)isisAttackAnimation);
        this.transform.SetParent(null);
        StartCoroutine(EnterIsisScalingAnimation());
    }

    /// <summary>
    /// 
    /// </summary>
    private void EndIsisAttack()
    {
        this.gameObject.SetActive(false);
        this.transform.SetParent(this.isisContainer);
        this.transform.localPosition = Vector3.zero;
        this.transform.localRotation = Quaternion.Euler(Vector3.zero);
    }

    #region animation events
    /// <summary>
    /// This event should be called inside our animation clip at the end of the animation
    /// </summary>
    public void OnIsisAttackAnimationEnd()
    {
        this.isisAnimSpeedController.enabled = false;
        StartCoroutine(ExitIsisScalingAnimation());

    }
    #endregion animation events
}
