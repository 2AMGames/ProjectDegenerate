using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionIconPointer : MonoBehaviour
{
    private Coroutine MovingTowardsPositionCoroutine = null;

    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public void SetGoalPositionToMoveTowards (Vector3 goalPosition, float timeToReachTarget = .15f)
    {
        if (MovingTowardsPositionCoroutine != null)
        {
            StopCoroutine(MovingTowardsPositionCoroutine);

        }
        MovingTowardsPositionCoroutine = StartCoroutine(MoveTowardsGoalPosition(goalPosition, timeToReachTarget));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="goalTransformToMoveTowards"></param>
    /// <param name="timeToReachTarget"></param>
    public IEnumerator MoveTowardsGoalPosition(Vector3 goalPosition, float timeToReachTarget = .15f)
    {
        float timeThatHasPassed = 0;

        Vector3 originalPosition = this.transform.position;

        while (timeThatHasPassed < timeToReachTarget)
        {
            timeThatHasPassed += Time.unscaledDeltaTime;
            this.transform.position = Vector3.Lerp(originalPosition, goalPosition, animationCurve.Evaluate(timeThatHasPassed / timeToReachTarget));
            yield return null;
        }
        this.transform.position = goalPosition;
        MovingTowardsPositionCoroutine = null;
    }
}
