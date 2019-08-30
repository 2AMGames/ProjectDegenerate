using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DebugTestVelocity : MonoBehaviour
{
    public float maxVelocity = 5;
    public float acceleration = 25;
    CustomPhysics2D rigid;

    private void Awake()
    {
        rigid = GetComponent<CustomPhysics2D>();
    }

    private void Update()
    {
        Vector2 goalVelocity = new Vector2(Input.GetAxisRaw("Horizontal_P1") * maxVelocity, Input.GetAxisRaw("Vertical_P1") * maxVelocity);
        print(goalVelocity);
        rigid.velocity = Vector2.MoveTowards(rigid.velocity, goalVelocity, Overseer.DELTA_TIME * acceleration);
    }
}
