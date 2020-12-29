using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalk : CharacterHabilityBase
{
    public float maxSpeed = 5;
    public float slopeMultiplyPerAngle = 0.05f; //2 in 45
    public float accelerationTimeGrounded = 0.1f;
    public float accelerationTimeAirborne = 0.2f;
    Vector2 currentVelocity;
    [HideInInspector] public Vector2 movement;

    public override void EverFrame()
    {
        base.EverFrame();
        Vector2 direction = new Vector2(Input.GetAxisRaw("Horizontal"), 0).normalized;

        if (transform.rotation.eulerAngles.z > 90 && transform.rotation.eulerAngles.z < 270)
        {
            direction = direction * -1;
        }

        float speed = maxSpeed;
        if (_collision.collisions.climbingSlope)
        {
            speed = maxSpeed / (_collision.collisions.slopeAngle * slopeMultiplyPerAngle);
        }
        if (_collision.collisions.descendingSlope)
        {
            speed = maxSpeed * (_collision.collisions.slopeAngle * slopeMultiplyPerAngle);
        }

        
        movement = Vector2.SmoothDamp(movement, direction * speed, ref currentVelocity, accelerationTimeGrounded);
    }

    public override void Move()
    {
        base.Move();
        _physicsController.AddVelocity(movement);
    }
}
