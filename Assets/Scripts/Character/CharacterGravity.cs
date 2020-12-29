using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterGravity : CharacterHabilityBase
{
    [Range(0.0F, 360)]
    public int angle = 0;
    [HideInInspector] public int _angle;
    [HideInInspector] public float _gravitForce;
    [HideInInspector] public float _currentGravitForce;
    [HideInInspector] public Vector2 _gravityDirection;
    [HideInInspector] public float _gravityResistenceForce = 0;
    Vector2 currentGravity;

    public bool allowRotation = false;
    float _lastEulerAnglesZ = 0;

    public override void Initialization()
    {
        base.Initialization();
        _lastEulerAnglesZ = transform.eulerAngles.z;
        SetGravityDefault();
    }

    public override void Move()
    {
        base.Move();
        CalcGravitDirection(_angle);
        float diferenceAngle = _lastEulerAnglesZ - transform.rotation.eulerAngles.z;
        currentGravity = Quaternion.Euler(new Vector3(0, 0, diferenceAngle)) * currentGravity;
        float tpmGravitForce = _gravitForce;
        if (_gravityResistenceForce > 0) {
            tpmGravitForce = _gravityResistenceForce;
        }
        currentGravity += (_gravityDirection * tpmGravitForce) / Time.timeScale;
        _currentGravitForce += tpmGravitForce / Time.timeScale;
        
        _physicsController.AddVelocity(currentGravity);
        RotateCharacter();
    }

    public override void AfterMove()
    {
        base.AfterMove();
        // caso a gravidade esteja pra baixo
        if (_collision.collisions.below || _collision.collisions.above)
        {
            ClearCurrentGravity();
        }
    }

    public void SetGravity(int tmpAngle, float tmpGravityForce)
    {
        _angle = tmpAngle;

        if (tmpGravityForce > 0)
        {
            _gravitForce = tmpGravityForce;
        }
    }

    public void SetGravityDefault()
    {
        _angle = angle;
        _gravitForce = Mathf.Abs(Physics2D.gravity.y);
    }

    public void ClearCurrentGravity()
    {
        currentGravity = Vector2.zero;
        _currentGravitForce = 0;
    }

    void CalcGravitDirection(int tmpAngle)
    {
        float diferenceAngle = tmpAngle - transform.rotation.eulerAngles.z;
        _gravityDirection = Quaternion.AngleAxis(diferenceAngle, Vector3.forward) * Vector3.down;
    }

    void RotateCharacter ()
    {
        if (allowRotation) {
            _lastEulerAnglesZ = transform.eulerAngles.z;
            transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, new Vector3(0, 0, _angle), 0.1f);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<GravityZone>() != null)
        {
            GravityZone gravityZone = collision.gameObject.GetComponent<GravityZone>();
            gravityZone.OnCollisionStayWithCharacter(gameObject);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<GravityZone>() != null)
        {
            GravityZone gravityZone = collision.gameObject.GetComponent<GravityZone>();
            gravityZone.OnCollisionExitWithCharacter(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        _physicsController = GetComponent<Physics2dController>();
        CalcGravitDirection(angle);
        Gizmos.color = Color.green;
        Vector3 target = _gravityDirection * 2.5f;
        Vector3 arrowRight = Quaternion.AngleAxis(angle, Vector3.forward) * new Vector2(1, 1);
        Vector3 arrowLeft = Quaternion.AngleAxis(angle, Vector3.forward) * new Vector2(-1, 1);
        Gizmos.DrawRay(transform.position, target);
        Gizmos.DrawRay(transform.position + target, arrowRight * 0.5f);
        Gizmos.DrawRay(transform.position + target, arrowLeft * 0.5f);
    }
}
