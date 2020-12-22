using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class GravityZone : MonoBehaviour
{
    public float gravityForce = 0f;
    [Range(0.0F, 360)]
    public int angle = 0;
    [HideInInspector] public int _lastAngle = 0;
    [HideInInspector] public float _lastGravitForce = 0;

    BoxCollider2D _boxCollider;
    bool active = false;

    public void Start() {
        _boxCollider = (BoxCollider2D)GetComponent<BoxCollider2D>();
    }

    public void SetLastGravity(int lastAngle, float lastGravitForce)
    {
        _lastGravitForce = lastGravitForce;
        _lastAngle = lastAngle;
    }

    public void OnCollisionStayWithCharacter (GameObject character)
    {
        if (VerifyCharacterCenterInZone(character)) {
            CharacterGravity _characterGravity = (CharacterGravity)character.GetComponent<CharacterGravity>();
            if ( ! active )
            {
                active = true;
               SetLastGravity(_characterGravity._angle, _characterGravity._gravitForce);
               _characterGravity.SetGravity(angle, gravityForce);
            }
        }
    }

    public void OnCollisionExitWithCharacter (GameObject character)
    {
        CharacterGravity _characterGravity = (CharacterGravity)character.GetComponent<CharacterGravity>();
        if ( active )
        {
            _characterGravity.SetGravity(_lastAngle, _lastGravitForce);
            SetLastGravity(0, 0);
            active = false;
        }
    }

    bool VerifyCharacterCenterInZone (GameObject character)
    {
        float top = _boxCollider.offset.y + (_boxCollider.size.y / 2);
        float bottom = _boxCollider.offset.y - (_boxCollider.size.y / 2);
        float left = _boxCollider.offset.x - (_boxCollider.size.x / 2);
        float right = _boxCollider.offset.x + (_boxCollider.size.x / 2);
        
        Vector2 topLeft = transform.TransformPoint(new Vector2(left, top));
        Vector2 bottomRight = transform.TransformPoint(new Vector2(right, bottom));

        if (character.transform.position.x >= topLeft.x &&         // right of the left edge AND
            character.transform.position.x <= bottomRight.x &&    // left of the right edge AND
            character.transform.position.y >= bottomRight.y &&         // below the top AND
            character.transform.position.y <= topLeft.y) {    // above the bottom
            return true;
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Vector2 gravityDirection = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.down;
        Gizmos.color = Color.green;
        Vector3 target = gravityDirection * 4;
        Vector3 arrowRight = Quaternion.AngleAxis(angle, Vector3.forward) * new Vector2(1, 1);
        Vector3 arrowLeft = Quaternion.AngleAxis(angle, Vector3.forward) * new Vector2(-1, 1);
        Gizmos.DrawRay(transform.position, target);
        Gizmos.DrawRay(transform.position + target, arrowRight * 0.5f);
        Gizmos.DrawRay(transform.position + target, arrowLeft * 0.5f);
    }
}
