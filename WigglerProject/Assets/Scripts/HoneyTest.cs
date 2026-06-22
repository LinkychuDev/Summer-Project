using System;
using System.Collections;
using UnityEngine;


public interface IStickable
{
    
}
public class HoneyTest : MonoBehaviour
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
        Forward,
        Backward
    }
    
    public Direction direction;
    public float stickDuration = 4;
    public float coolDuration = 4;
    public bool isActive = true;
    public float bounceForce = 10;
    private Vector3 directionHit;
    private Rigidbody currentStuckObject;
    private void OnTriggerEnter(Collider other)
    {
        if(other.attachedRigidbody == null)
            return;
        if(other.attachedRigidbody.isKinematic)
            return;
        if(!(other.TryGetComponent(out IStickable stickable)))
            return;
        if(!isActive)
            return;
        if(currentStuckObject != null)
            return;
        currentStuckObject = other.attachedRigidbody;
        directionHit = transform.position - currentStuckObject.position;
        directionHit.y = 0;
        directionHit.Normalize();
        StartCoroutine(StickDuration());
    }



    IEnumerator StickDuration()
    {
        
        if (currentStuckObject.TryGetComponent(out PlayerController controller))
        {
            controller.SetState(PlayerState.Stuck);
            currentStuckObject.position = transform.position;
            yield return new WaitForSeconds(stickDuration);
            controller.SetState(controller.GetLastState());
            StartCoroutine(CoolDown());
        }

        else
        {
            currentStuckObject.isKinematic = true;
            currentStuckObject.position = transform.position;
            yield return new WaitForSeconds(stickDuration);
            currentStuckObject.isKinematic = false;
            StartCoroutine(CoolDown());
        }
        
        
    }

    IEnumerator CoolDown()
    {
        isActive = false;
        currentStuckObject.AddForce(directionHit* bounceForce * Time.deltaTime, ForceMode.VelocityChange);
        yield return new WaitForSeconds(coolDuration);
        currentStuckObject = null;
        isActive = true;
        
    }

    private void OnTriggerExit(Collider other)
    {
        
    }

    Vector3 DirectionToVector()
    {
        switch (direction)
        {
            case Direction.Up:
                return Vector3.up;
            case Direction.Down:
                return Vector3.down;
            case Direction.Left:
                return Vector3.left;
            case Direction.Right:
                return Vector3.right;
            case Direction.Forward:
                return Vector3.forward;
            case Direction.Backward:
                return Vector3.back;
            
        }
        
        return Vector3.zero;
    }
}
