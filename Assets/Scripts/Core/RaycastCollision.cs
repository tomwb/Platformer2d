using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastCollision : MonoBehaviour
{
    public LayerMask collisionMask;
    public float maxSlopeAngle = 45;
    const float skinWidth = .015f;
    const float dstBetweenRays = .25f;
    int horizontalRayCount = 4;
    int verticalRayCount = 4;
    float horizontalRaySpacing;
    float verticalRaySpacing;
    BoxCollider2D _collider;
    [HideInInspector] public RaycastOrigins _raycastOrigins;
    [HideInInspector] public CollisionInfo collisions;

    void Start()
    {
        _collider = (BoxCollider2D)GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    void UpdateRaycastOrigins()
    {
        float top = _collider.offset.y + (_collider.size.y / 2);
        float bottom = _collider.offset.y - (_collider.size.y / 2);
        float left = _collider.offset.x - (_collider.size.x / 2);
        float right = _collider.offset.x + (_collider.size.x / 2);
        // seta todos os cantos
        _raycastOrigins.topLeft = transform.TransformPoint(new Vector2(left + skinWidth * 2, top + skinWidth * -2));
        _raycastOrigins.topRight = transform.TransformPoint(new Vector2(right + skinWidth * -2, top + skinWidth * -2));
        _raycastOrigins.bottomLeft = transform.TransformPoint(new Vector2(left + skinWidth * 2, bottom + skinWidth * 2));
        _raycastOrigins.bottomRight = transform.TransformPoint(new Vector2(right + skinWidth * -2, bottom + skinWidth * 2));
        // pega os tamanhos
        _raycastOrigins.width = Vector2.Distance(_raycastOrigins.bottomLeft, _raycastOrigins.bottomRight);
        _raycastOrigins.height = Vector2.Distance(_raycastOrigins.bottomLeft, _raycastOrigins.topLeft);
        // pega os meios
        _raycastOrigins.centerTop = transform.TransformPoint(new Vector2(_collider.offset.x, top));
        _raycastOrigins.centerLeft = transform.TransformPoint(new Vector2(left, _collider.offset.y));
        _raycastOrigins.centerRight = transform.TransformPoint(new Vector2(right, _collider.offset.y));
        _raycastOrigins.centerBottom = transform.TransformPoint(new Vector2(_collider.offset.x, bottom));
        _raycastOrigins.center = transform.TransformPoint(new Vector2(_collider.offset.x, _collider.offset.y));
    }

    void CalculateRaySpacing()
    {
        UpdateRaycastOrigins();

        horizontalRayCount = Mathf.RoundToInt(_raycastOrigins.height / dstBetweenRays);
        verticalRayCount = Mathf.RoundToInt(_raycastOrigins.width / dstBetweenRays);

        horizontalRaySpacing = _raycastOrigins.height / (horizontalRayCount - 1);
        verticalRaySpacing = _raycastOrigins.width / (verticalRayCount - 1);
    }

    public Vector2 CalculateCollisions(Vector2 speed)
    {
        UpdateRaycastOrigins();
        collisions.Reset();
        collisions.oldSpeed = speed;

        collisions.directionVectorRight = Quaternion.Euler(new Vector3(0, 0, transform.rotation.eulerAngles.z)) * Vector2.right;
        collisions.directionVectorUp = Quaternion.Euler(new Vector3(0, 0, transform.rotation.eulerAngles.z)) * Vector2.up;

        if (speed.y < 0)
        {
            speed = DescendSlope(speed);
        }

        speed = CalculateHorizontalCollisions(speed);
        speed = CalculateVerticalCollisions(speed);

        return speed;
    }

    Vector2 CalculateHorizontalCollisions(Vector2 speed)
    {

        float directionX = Mathf.Sign(speed.x);
        float rayLength = Mathf.Abs(speed.x) + skinWidth;
        
        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? _raycastOrigins.bottomLeft : _raycastOrigins.bottomRight;
            rayOrigin += collisions.directionVectorUp * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, collisions.directionVectorRight * directionX, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, collisions.directionVectorRight * directionX * rayLength, Color.yellow);

            if (hit)
            {
                if (hit.distance == 0)
                {
                    //continue;
                }
                //if (hit.collider.gameObject.GetComponent<ThroughPlatform>() != null)
                //{
                //    ThroughPlatform throughPlatform = hit.collider.gameObject.GetComponent<ThroughPlatform>();
                //    if (throughPlatform.left && (directionX == 1 || hit.distance == 0))
                //    {
                //        continue;
                //    }
                //    if (throughPlatform.right && (directionX == -1 || hit.distance == 0))
                //    {
                //        continue;
                //    }
                //    if (collisions.fallingThroughPlatform)
                //    {
                //        continue;
                //    }
                //    //if (throughPlatform.VerifyFallingThroughPlatform() == true)
                //    //{
                //    //    collisions.fallingThroughPlatform = true;
                //    //    Invoke("ResetFallingThroughPlatform", .5f);
                //    //   continue;
                //    //}
                //}

                float slopeAngle = Vector2.Angle(hit.normal, collisions.directionVectorUp);
                
                if (i == 0 && slopeAngle <= maxSlopeAngle)
                {
                    if (collisions.descendingSlope)
                    {
                        collisions.descendingSlope = false;
                        speed = collisions.oldSpeed;
                    }
                    float distanceToSlopeStart = 0;
                    if (slopeAngle != collisions.slopeAngleOld)
                    {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        speed.x -= distanceToSlopeStart * directionX;
                    }
                    speed = ClimbSlope(speed, slopeAngle, hit.normal);
                    speed.x += distanceToSlopeStart * directionX;
                }

                if (!collisions.climbingSlope || slopeAngle > maxSlopeAngle)
                {
                    speed.x = (hit.distance - skinWidth) * directionX;
                    rayLength = hit.distance;

                    if (collisions.climbingSlope)
                    {
                        speed.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(speed.x);
                    }

                    collisions.left = directionX == -1;
                    collisions.right = directionX == 1;
                }

            }
        }

        return speed;
    }

    Vector2 CalculateVerticalCollisions(Vector2 speed)
    {
        float directionY = Mathf.Sign(speed.y);
        float rayLength = Mathf.Abs(speed.y) + skinWidth;
        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? _raycastOrigins.bottomLeft : _raycastOrigins.topLeft;
            rayOrigin += collisions.directionVectorRight * (verticalRaySpacing * i + speed.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, collisions.directionVectorUp * directionY, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, collisions.directionVectorUp * directionY * rayLength, Color.red);

            if (hit)
            {
                if (hit.distance == 0)
                {
                    //continue;
                }
                //if (hit.collider.gameObject.GetComponent<ThroughPlatform>() != null)
                //{
                //    ThroughPlatform throughPlatform = hit.collider.gameObject.GetComponent<ThroughPlatform>();
                //    if (throughPlatform.bottom && (directionY == 1 || hit.distance == 0))
                //    {
                //        continue;
                //    }
                //    if (throughPlatform.top && (directionY == -1 || hit.distance == 0))
                //    {
                //        continue;
                //    }
                //    if (collisions.fallingThroughPlatform)
                //    {
                //        continue;
                //    }
                //    if (throughPlatform.VerifyFallingThroughPlatform() == true)
                //    {
                //        collisions.fallingThroughPlatform = true;
                //        Invoke("ResetFallingThroughPlatform", .2f);
                //        continue;
                //    }
                //}

                speed.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;

                if (collisions.climbingSlope)
                {
                    speed.x = speed.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(speed.x);
                }

                collisions.below = directionY == -1;
                collisions.above = directionY == 1;
            }
        }

        if (collisions.climbingSlope)
        {
            float directionX = Mathf.Sign(speed.x);
            rayLength = Mathf.Abs(speed.x) + skinWidth;
            Vector2 rayOrigin = ((directionX == -1) ? _raycastOrigins.bottomLeft : _raycastOrigins.bottomRight) + collisions.directionVectorUp * speed.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, collisions.directionVectorRight * directionX, rayLength, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, collisions.directionVectorUp);
                if (slopeAngle != collisions.slopeAngle)
                {
                    speed.x = (hit.distance - skinWidth) * directionX;
                    collisions.slopeAngle = slopeAngle;
                    collisions.slopeNormal = hit.normal;
                }
            }
        }

        return speed;
    }

    Vector2 ClimbSlope(Vector2 speed, float slopeAngle, Vector2 slopeNormal)
    {
        float moveDistance = Mathf.Abs(speed.x);
        float climbmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        if (speed.y <= climbmoveAmountY)
        {
            speed.y = climbmoveAmountY;
            speed.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(speed.x);
            collisions.below = true;
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
            collisions.slopeNormal = slopeNormal;
        }

        return speed;
    }

    Vector2 DescendSlope(Vector2 speed)
    {

        RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(_raycastOrigins.bottomLeft, -collisions.directionVectorUp, Mathf.Abs(speed.y) + skinWidth, collisionMask);
        RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(_raycastOrigins.bottomRight, -collisions.directionVectorUp, Mathf.Abs(speed.y) + skinWidth, collisionMask);

        if (maxSlopeHitLeft ^ maxSlopeHitRight)
        {
            speed = SlideDownMaxSlope(maxSlopeHitLeft, speed);
            speed = SlideDownMaxSlope(maxSlopeHitRight, speed);
        }

        if (!collisions.slidingDownMaxSlope)
        {
            float directionX = Mathf.Sign(speed.x);
            Vector2 rayOrigin = (directionX == -1) ? _raycastOrigins.bottomRight : _raycastOrigins.bottomLeft;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -collisions.directionVectorUp, Mathf.Infinity, collisionMask);
           
            if (hit)
            {
                
                float slopeAngle = Vector2.Angle(hit.normal, collisions.directionVectorUp);
                
                if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle)
                {
                    if (Mathf.Sign(hit.normal.x) == directionX)
                    {
                        if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(speed.x))
                        {
                            float moveDistance = Mathf.Abs(speed.x);
                            float descendmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                            speed.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(speed.x);
                            speed.y -= descendmoveAmountY;

                            collisions.slopeAngle = slopeAngle;
                            collisions.descendingSlope = true;
                            collisions.below = true;
                            collisions.slopeNormal = hit.normal;
                        }
                    }
                }
            }
        }

        return speed;
    }

    Vector2 SlideDownMaxSlope(RaycastHit2D hit, Vector2 speed)
    {

        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, collisions.directionVectorUp);
            if (slopeAngle > maxSlopeAngle)
            {
                speed.x = Mathf.Sign(hit.normal.x) * (Mathf.Abs(speed.y) - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad);

                collisions.slopeAngle = slopeAngle;
                collisions.slidingDownMaxSlope = true;
                collisions.slopeNormal = hit.normal;
            }
        }

        return speed;
    }

    void ResetFallingThroughPlatform()
    {
        collisions.fallingThroughPlatform = false;
    }

    public struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
        public Vector2 centerTop, centerBottom;
        public Vector2 centerLeft, centerRight;
        public Vector2 center;
        public float height, width;
    }

    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public bool climbingSlope;
        public bool descendingSlope;
        public bool slidingDownMaxSlope;

        public float slopeAngle, slopeAngleOld;
        public Vector2 slopeNormal;
        public Vector2 oldSpeed;
        public bool fallingThroughPlatform;

        public Vector2 directionVectorRight, directionVectorUp;

        public void Reset()
        {
            above = below = false;
            left = right = false;
            climbingSlope = false;
            descendingSlope = false;
            slidingDownMaxSlope = false;
            slopeNormal = Vector2.zero;

            slopeAngleOld = slopeAngle;
            slopeAngle = 0;

            directionVectorRight = Vector2.right;
            directionVectorUp = Vector2.up;
        }
    }

    private void OnDrawGizmosSelected()
    {
        _collider = (BoxCollider2D)GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
        //Gizmos.color = Color.green;
        //Gizmos.DrawSphere(_raycastOrigins.topleft, 0.05f);
        //Gizmos.DrawSphere(_raycastOrigins.topright, 0.05f);
        //Gizmos.DrawSphere(_raycastOrigins.bottomleft, 0.05f);
        //Gizmos.DrawSphere(_raycastOrigins.bottomright, 0.05f);

        Gizmos.color = Color.yellow;
        //Gizmos.DrawSphere(_raycastOrigins.centertop, 0.05f);
        //Gizmos.DrawSphere(_raycastOrigins.centerbottom, 0.05f);
        //Gizmos.DrawSphere(_raycastOrigins.centerleft, 0.05f);
        //Gizmos.DrawSphere(_raycastOrigins.centerright, 0.05f);
        Gizmos.DrawSphere(_raycastOrigins.center, 0.05f);
    }
}
