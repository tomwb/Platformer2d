using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ThroughPlatform : MonoBehaviour
{
    public bool top = false;
    public bool left = false;
    public bool right = false;
    public bool bottom = true;

    public bool fallingThroughPlatform = false;
    public FallingDirections fallingDirection = FallingDirections.below;

    BoxCollider2D _collider;

    public enum FallingDirections
    {
        above,
        below
    };

    public bool VerifyFallingThroughPlatform ()
    {
        float moveVertical = Input.GetAxis("Vertical");
        if (fallingDirection == FallingDirections.below && moveVertical < 0 && fallingThroughPlatform == true)
        {
            return true;
        }
        if (fallingDirection == FallingDirections.above && moveVertical > 0 && fallingThroughPlatform == true)
        {
            return true;
        }

        return false;
    }

    void OnDrawGizmosSelected()
    {
        _collider = (BoxCollider2D)GetComponent<BoxCollider2D>();
        Bounds bounds = _collider.bounds;
        Vector2 position = Vector2.zero;
        float size = 0.1f;
        Gizmos.color = Color.green;
        if (bottom)
        {
            position = new Vector2(transform.position.x, bounds.min.y - size);
            Gizmos.DrawCube(position, new Vector2(bounds.size.x, size));
        }
        if (top)
        {
            position = new Vector2(transform.position.x, bounds.max.y + size);
            Gizmos.DrawCube(position, new Vector2(bounds.size.x, size));
        }
        if (right)
        {
            position = new Vector2(bounds.max.x + size, transform.position.y);
            Gizmos.DrawCube(position, new Vector2(size, bounds.size.y));
        }
        if (left)
        {
            position = new Vector2(bounds.min.x - size, transform.position.y);
            Gizmos.DrawCube(position, new Vector2(size, bounds.size.y));
        }
    }
}
