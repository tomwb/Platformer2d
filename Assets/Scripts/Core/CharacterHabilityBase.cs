using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Physics2dController))]
[RequireComponent(typeof(RaycastCollision))]
public class CharacterHabilityBase : MonoBehaviour
{
    protected Physics2dController _physicsController;
    protected RaycastCollision _collision;

    void Start()
    {
        Initialization();
    }

    private void Update()
    {
        EverFrame();
    }

    public virtual void Initialization()
    {
        _physicsController = GetComponent<Physics2dController>();
        _collision = GetComponent<RaycastCollision>();
    }

    public virtual void EverFrame()
    {
    }

    public virtual void BeforeMove()
    {
    }

    public virtual void Move()
    {
    }

    public virtual void BeforeCalculateCollisions()
    {
        
    }

    public virtual void AfterCalculateCollisions()
    {
        
    }

    public virtual void AfterMove()
    {
    }

}
