using System;
using UnityEngine;


public enum Direction
{
	Forward,
	Back,
	Right,
	Left
	
}
public class WallClimbTrigger : CollisionBlock
{
	public bool isTrigger = true;

	
	private bool hasSwitched;
	private PlayerReferenceManager playerReferenceManager;
	
	private  Vector3 inverseDirection = Vector3.down ;
	private Vector3 gravityDirection;

	public GameObject wall;

	public LayerMask groundMask;

	public Transform inverseSetPosition;
	public Transform wallContactPos;
	public Direction targetDir;


	private void Start()
	{
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

	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			if (other.TryGetComponent(out Rigidbody playerRb))
			{
				if (!playerRb.TryGetComponent(out PlayerMovement controller))
					return;
				if(PlayerReferenceManager.instance.currentState == PlayerState.Stretching)
					return;
				//controller.ChangeGravity(transform.forward);


				wallContactPos.position = CalculateOffsetPos(wallContactPos.position, playerRb.transform.position);

				
				inverseSetPosition.position = CalculateOffsetPos(inverseSetPosition.position, playerRb.transform.position);
				

				if (controller.isClimbing)
				{
					hasSwitched = false;
					//GameManager.CameraClimbSwitch?.Invoke( inverseDirection);
					controller.ChangeGravity(inverseDirection, false, true, true, inverseSetPosition.position);
					
					playerRb.AddForce(-transform.up * controller.climbTriggerOffset);

				}

				else
				{
					GameManager.CameraClimbSwitch?.Invoke(gravityDirection);
					controller.ChangeGravity(gravityDirection, true, true, true, wallContactPos.position, wall.transform);
					playerRb.AddForce(transform.up * controller.climbTriggerOffset);
					hasSwitched = true;
				}
				
				
				
				
				
				
				
				
				
				
				//FlipDirection(transform.forward, playerRb);
			}
		}
	}

	private Vector3 CalculateOffsetPos(Vector3 offset, Vector3 playerPos)
	{
		Vector3 targetPos = offset;
		if (targetDir == Direction.Left ||  targetDir == Direction.Right)
		{
			targetPos.y = playerPos.y;
		}
		
		else if (targetDir == Direction.Forward ||  targetDir == Direction.Back)
		{
			targetPos.x = playerPos.x;
		}
		
		return targetPos;
	}

	void FlipDirection(Vector3 newUpDir, Rigidbody playerRb)
	{
		float angleBetween = Vector3.Angle(transform.forward, playerRb.transform.up);

		float angleThreshold = 0.0001f;

		if (angleBetween > angleThreshold)
		{
			Quaternion rotationDifference = Quaternion.FromToRotation(playerRb.transform.up, newUpDir);
			
			playerRb.transform.rotation = rotationDifference * playerRb.transform.rotation;
		}
	}
	
	
	/*else
	{
		if (Physics.Raycast(rb.position, transform.forward, out RaycastHit hit, climbDetectionDistance, silkMask,
			    QueryTriggerInteraction.Ignore))
		{
			//Debug.Log("Hit Object: " + hit.transform.gameObject);

			rb.position = hit.point;

			isClimbing = true;
                
			ChangeGravity(-hit.normal, true, true);
                
		}
	}*/

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(wallContactPos.position, 0.1f);
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(inverseSetPosition.position, 0.1f);
	}
}