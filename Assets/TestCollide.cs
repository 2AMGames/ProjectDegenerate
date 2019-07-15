using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCollide : MonoBehaviour
{
    void Update()
    {
        Vector2 originPoint = this.transform.position;
        Vector2 direction = -this.transform.up;
        float distance = 5;
        DebugSettings.DrawLine(originPoint, originPoint + distance * direction, Color.red);
        if (Overseer.Instance.ColliderManager.CheckLineIntersectWithCollider(originPoint, direction, distance))
        {
            print("Hit");
        }
    }
}
