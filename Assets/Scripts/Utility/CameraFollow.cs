using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
  public GameObject target;
  public int pixelsPerUnit = 32;
  Camera _camera;
  BoxCollider2D _collider;
  public float verticalOffset;
  public float lookAheadDstX;
  public float lookSmoothTimeX;
  public float verticalSmoothTime;
  public Vector2 focusAreaSize;

  FocusArea focusArea;

  float currentLookAheadX;
  float targetLookAheadX;
  float lookAheadDirX;
  float smoothLookVelocityX;
  float smoothVelocityY;

  bool lookAheadStopped;

  void Start()
  {
    _collider = (BoxCollider2D)target.GetComponent<BoxCollider2D>();
    _camera = (Camera)transform.GetComponent<Camera>();
    focusArea = new FocusArea(_collider.bounds, focusAreaSize);

    // _camera.orthographicSize = (Screen.height) / (float)pixelsPerUnit / 2f;
    _camera.orthographicSize = Screen.width / (((Screen.width / Screen.height) * 2) * (float)pixelsPerUnit);
  }

  void LateUpdate()
  {
    focusArea.Update(_collider.bounds);
    float xInput = Input.GetAxisRaw("Horizontal");
    Vector2 focusPosition = focusArea.centre + Vector2.up * verticalOffset;

    if (focusArea.velocity.x != 0)
    {
      lookAheadDirX = Mathf.Sign(focusArea.velocity.x);
      if (Mathf.Sign(xInput) == Mathf.Sign(focusArea.velocity.x) && xInput != 0)
      {
        lookAheadStopped = false;
        targetLookAheadX = lookAheadDirX * lookAheadDstX;
      }
      else
      {
        if (!lookAheadStopped)
        {
          lookAheadStopped = true;
          targetLookAheadX = currentLookAheadX + (lookAheadDirX * lookAheadDstX - currentLookAheadX) / 4f;
        }
      }
    }


    currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, targetLookAheadX, ref smoothLookVelocityX, lookSmoothTimeX);

    focusPosition.y = Mathf.SmoothDamp(transform.position.y, focusPosition.y, ref smoothVelocityY, verticalSmoothTime);
    focusPosition += Vector2.right * currentLookAheadX;
    Vector3 tmpPosition = (Vector3)focusPosition + Vector3.forward * -10;

    tmpPosition.x = GetNearestMultiple(tmpPosition.x);
    tmpPosition.y = GetNearestMultiple(tmpPosition.y);

    transform.position = tmpPosition;
  }

  float GetNearestMultiple(float value)
  {
    float multiple = 1f / pixelsPerUnit;
    float rem = value % multiple;
    float result = value - rem;
    if (rem > (multiple / 2))
      result += multiple;

    return result;
  }

  void OnDrawGizmos()
  {
    Gizmos.color = new Color(1, 0, 0, .5f);
    Gizmos.DrawCube(focusArea.centre, focusAreaSize);
  }

  struct FocusArea
  {
    public Vector2 centre;
    public Vector2 velocity;
    float left, right;
    float top, bottom;


    public FocusArea(Bounds targetBounds, Vector2 size)
    {
      left = targetBounds.center.x - size.x / 2;
      right = targetBounds.center.x + size.x / 2;
      bottom = targetBounds.min.y;
      top = targetBounds.min.y + size.y;

      velocity = Vector2.zero;
      centre = new Vector2((left + right) / 2, (top + bottom) / 2);
    }

    public void Update(Bounds targetBounds)
    {
      float shiftX = 0;
      if (targetBounds.min.x < left)
      {
        shiftX = targetBounds.min.x - left;
      }
      else if (targetBounds.max.x > right)
      {
        shiftX = targetBounds.max.x - right;
      }
      left += shiftX;
      right += shiftX;

      float shiftY = 0;
      if (targetBounds.min.y < bottom)
      {
        shiftY = targetBounds.min.y - bottom;
      }
      else if (targetBounds.max.y > top)
      {
        shiftY = targetBounds.max.y - top;
      }
      top += shiftY;
      bottom += shiftY;
      centre = new Vector2((left + right) / 2, (top + bottom) / 2);
      velocity = new Vector2(shiftX, shiftY);
    }
  }
}
