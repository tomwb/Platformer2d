using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : CharacterHabilityBase
{
    SpriteRenderer _spriteRenderer;
    BoxCollider2D _boxCollider2D;
    Animator _animator;
    bool directionRight = true;

    public override void Initialization()
    {
        base.Initialization();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _animator = GetComponent<Animator>();
    }

    public override void Move()
    {
        base.Move();
        Vector2 moveAmount = _physicsController.GetVelocity();
        bool tmpDirectionRight;
        if (moveAmount.x < 0)
        {
            _spriteRenderer.flipX = true;
            tmpDirectionRight = false;
        }
        else
        {
            tmpDirectionRight = true;
            _spriteRenderer.flipX = false;
        }

        // caso mudou a direção ele ajusta o boxcollider
        if (directionRight != tmpDirectionRight)
        {
            directionRight = tmpDirectionRight;
            _boxCollider2D.offset = new Vector2(_boxCollider2D.offset.x * -1, _boxCollider2D.offset.y);
        }
    }

    public override void AfterMove()
    {
        _animator.SetBool("IsGrounded ", _collision.collisions.below);
        _animator.SetFloat("xSpeed", Mathf.Abs(_physicsController._speed.x));
        _animator.SetFloat("ySpeed", _physicsController._speed.y);
    }
}
