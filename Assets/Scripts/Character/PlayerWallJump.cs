using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallJump : CharacterHabilityBase
{
  public float wallSlideSpeedMax = 0.5f;
  public float wallStickTime = .25f;
  float timeToWallUnstick;
  public Vector2 wallJumpClimb = new Vector3(.4f, .8f);
  [HideInInspector] public bool wallSliding = false;

  bool ableToJump = true;
  bool isWallJumping = false;
  PlayerWalk _playerWalk;
  PlayerJump _playerJump;
  CharacterGravity _characterGravity;

  public override void Initialization()
  {
    base.Initialization();
    _playerWalk = GetComponent<PlayerWalk>();
    _playerJump = GetComponent<PlayerJump>();
    _characterGravity = GetComponent<CharacterGravity>();
  }

  public override void EverFrame()
  {
    base.EverFrame();

    float xInput = Input.GetAxisRaw ("Horizontal");
    int wallDirX = (_collision.collisions.left) ? -1 : 1;
    if (transform.rotation.eulerAngles.z > 90 && transform.rotation.eulerAngles.z < 270)
    {
      xInput = xInput * -1;
    }

    if (!_playerJump.isJumping && ((_collision.collisions.left && xInput == -1) ||
        (_collision.collisions.right && xInput == 1))
        && !_collision.collisions.below)
    {
      ableToJump = true;
      _playerJump._curentJump = 0;
      wallSliding = true;
      _characterGravity._gravityResistenceForce = wallSlideSpeedMax;

      // if (timeToWallUnstick > 0)
      // {
      //   _physicsController._speed.x = 0;

      //   if (xInput != wallDirX && xInput != 0)
      //   {
      //     timeToWallUnstick -= Time.deltaTime;
      //   }
      //   else
      //   {
      //     timeToWallUnstick = wallStickTime;
      //   }
      // }
      // else
      // {
      //   timeToWallUnstick = wallStickTime;
      // }
    }
    else
    {
      wallSliding = false;
      ableToJump = false;
      _characterGravity._gravityResistenceForce = 0;
    }

    if (ableToJump && !_playerJump.isJumping && Input.GetKeyDown(KeyCode.Space))
    {
      ableToJump = false;
      isWallJumping = true;
      Vector2 _jumpDirection;
      _jumpDirection.x = -wallDirX * wallJumpClimb.x;
      _jumpDirection.y = wallJumpClimb.y;
      _playerJump.StartJump(_jumpDirection, _playerJump.maxJumpVelocity);
    }

  }

  public override void BeforeCalculateCollisions()
  {
    base.BeforeCalculateCollisions();

    if (wallSliding && _physicsController._speed.y < -wallSlideSpeedMax)
    {
      _characterGravity.ClearCurrentGravity();
    }

    if (_playerJump.isJumping && isWallJumping)
    {
      // remove player walk movement
      _physicsController.AddVelocity(_playerWalk.movement * -1);
    }
    if (!_playerJump.isJumping)
    {
      isWallJumping = false;
    }
  }

  public override void AfterMove()
  {
    base.AfterMove();
    // caso tocar no chão ou no teto para de pular
    if (_playerJump.isJumping && isWallJumping && (_collision.collisions.left ||
      _collision.collisions.right))
    {
      _playerJump.StopJump();
      isWallJumping = false;
    }
  }
}
