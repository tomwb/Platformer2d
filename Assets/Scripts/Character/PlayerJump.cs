using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : CharacterHabilityBase
{
    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;
    public float timeToJumpApex = .4f;
    float maxJumpVelocity;
    float minJumpVelocity;

    public int extraJumps = 1;
    bool isJumping = false;
    bool ableToJump = true;
    int _curentJump = 0;
    float _jumpForce;
    CharacterGravity _characterGravity;

    public override void Initialization()
    {
        base.Initialization();
        _characterGravity = GetComponent<CharacterGravity>();

        float gravity = (2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        Physics2D.gravity = new Vector2(0, gravity * Time.fixedDeltaTime * -1);
        _characterGravity.SetGravityDefault();
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
    }

    public override void EverFrame()
    {
        base.EverFrame();

        if (ableToJump && Input.GetKeyDown(KeyCode.Space))
        {
            isJumping = true;
            ableToJump = false;
            _curentJump += 1;
            _jumpForce = maxJumpVelocity;
            _characterGravity.ClearCurrentGravity();
        }
        // pulo minimo caso solte rapido
        if (Input.GetKeyUp(KeyCode.Space))
        {
            _jumpForce = minJumpVelocity;
        }

    }

    public override void Move()
    {
        base.Move();
        if (isJumping)
        {
            Vector2 speed = (_characterGravity._gravityDirection * -1) * _jumpForce;
            _physicsController.AddVelocity(speed);

            if ((_jumpForce - _characterGravity._currentGravitForce) <= 0)
            {
                _characterGravity.ClearCurrentGravity();
                isJumping = false;
            }
        }

    }

    public override void AfterMove()
    {
        base.AfterMove();
        // caso tocar no chão ou no teto para de pular
        if (_collision.collisions.below || _collision.collisions.above)
        {
            isJumping = false;
            _characterGravity.ClearCurrentGravity();
        }

        // caso tocar no chão zera os pulos
        if (_collision.collisions.below)
        {
            ableToJump = true;
            _curentJump = 0;
        }
        else
        {
            ableToJump = false;
        }

        // caso tenha pulos liberado habilita para pular
        if (_curentJump >= 1 && _curentJump <= extraJumps)
        {
            ableToJump = true;
        }
    }
}
