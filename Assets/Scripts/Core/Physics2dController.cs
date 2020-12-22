using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RaycastCollision))]
[RequireComponent(typeof(Rigidbody2D))]
public class Physics2dController : MonoBehaviour
{
    RaycastCollision _collision;
    CharacterHabilityBase[] _allCharacterHabilitys;
    [HideInInspector] public Vector2 _speed;
    float _time = 1f;

    void Start()
    {
        _collision = GetComponent<RaycastCollision>();
        _allCharacterHabilitys = GetComponents<CharacterHabilityBase>();
    }

    public void AddVelocity(Vector2 force)
    {
        _speed += force;
    }

    public void RemoveVelocity(Vector2 force)
    {
        _speed -= force;
    }

    public void SetVelocity(Vector2 force)
    {
        _speed = force;
    }

    public Vector2 GetVelocity()
    {
        return _speed;
    }

    void FixedUpdate()
    {
        if (_allCharacterHabilitys.Length > 0)
        {
            foreach (CharacterHabilityBase characterHability in _allCharacterHabilitys)
            {
                characterHability.BeforeMove();
            }
            foreach (CharacterHabilityBase characterHability in _allCharacterHabilitys)
            {
                characterHability.Move();
            }
        }
        Move();
        if (_allCharacterHabilitys.Length > 0)
        {
            foreach (CharacterHabilityBase characterHability in _allCharacterHabilitys)
            {
                characterHability.AfterMove();
            }
        }

        
        _speed = Vector2.zero;
    }

    public void Move()
    {
        Vector2 moveAmount = (_speed * Time.fixedDeltaTime) / Time.timeScale / _time;
        
        _speed = _collision.CalculateCollisions(moveAmount);
        transform.Translate(_speed);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<TimeZone>() != null)
        {
            TimeZone timeZone = collision.gameObject.GetComponent<TimeZone>();
            _time = timeZone.time;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<TimeZone>() != null)
        {
            _time = 1f;
        }
    }

}
