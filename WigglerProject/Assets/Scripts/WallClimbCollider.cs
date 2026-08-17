using System;
using System.Collections;
using UnityEngine;


public enum Direction
{
    Forward,
    Back,
    Right,
    Left
	
}
public class WallClimbCollider : MonoBehaviour, IPlayerHint
{
    
    /*public Direction targetDir;
    private Vector3 gravityDirection;
    private Vector3 inverseGravityDirection = Vector3.down;

    private Collider collider;
    
    public bool hasSwitched = false;
    private void Start()
    {
        collider = GetComponent<Collider>();
        UpdateDirection();
    }

    private void OnValidate()
    {
        UpdateDirection();
    }

	
    void UpdateDirection()
    {
        switch (targetDir)
        {
            case Direction.Forward:
                gravityDirection = Vector3.forward;
                break;
            case Direction.Back:
                gravityDirection = Vector3.back;
                break;
            case Direction.Right:
                gravityDirection = Vector3.right;
                break;
            case Direction.Left:
                gravityDirection = Vector3.left;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.CompareTag("Player"))
        {
            if (other.transform.TryGetComponent(out Rigidbody playerRb))
            {
                if (!playerRb.TryGetComponent(out PlayerMovement controller))
                    return;
                if(PlayerReferenceManager.instance.currentState == PlayerState.Stretching)
                {
                    Debug.Log("Is Stretching and in contact");
                    return;
                }
				
                if(hasSwitched)
                    return;
                //controller.ChangeGravity(transform.forward);
                GameManager.CameraClimbSwitch?.Invoke(gravityDirection);
                Vector3 targetPosition = collider.ClosestPoint(transform.position);
                controller.ChangeGravity(gravityDirection, true, true, false, targetPosition);
                //playerRb.AddForce(transform.forward * controller.climbTriggerOffset);

                StartCoroutine(SetUpGravity(controller));
                hasSwitched = true;
				
				
				
				
				
				
				
				
				
				
                //FlipDirection(transform.forward, playerRb);
            }
        }
    }

    IEnumerator SetUpGravity(PlayerMovement controller)
    {
        yield return new WaitUntil(() => controller.isGrounded);
        hasSwitched = true;
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.transform.CompareTag("Player"))
        {
            if (other.transform.TryGetComponent(out Rigidbody playerRb))
            {
                if (!playerRb.TryGetComponent(out PlayerMovement controller))
                    return;
                if(PlayerReferenceManager.instance.currentState == PlayerState.Stretching)
                {
                    Debug.Log("Is Stretching and in contact");
                    return;
                }

                if (hasSwitched)
                {
                    
                    
                    GameManager.CameraClimbSwitch?.Invoke(inverseGravityDirection);
                    controller.ChangeGravity(inverseGravityDirection, false, true, false);
                    playerRb.AddForce(transform.up * controller.climbTriggerOffset);
                    hasSwitched = false;

                }
            }
        }
    }*/
    public PlayerActionEvent PlayerActionEvent { get; } = PlayerActionEvent.ClimbAction;
}