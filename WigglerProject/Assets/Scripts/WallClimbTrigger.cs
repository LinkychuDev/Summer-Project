using System;
using UnityEngine;



public class WallClimbTrigger : CollisionBlock
{
	public bool isTrigger = true;

	private Collider collider;
	private bool hasSwitched;
	private PlayerReferenceManager playerReferenceManager;
	
	private  Vector3 inverseDirection = Vector3.down ;
	private Vector3 gravityDirection;

	public GameObject wall;

	public LayerMask groundMask;

	public Transform inverseSetPosition;
	public Transform wallContactPos;
	public Direction targetDir;


	private Vector3 WallContactPos;

	
	

	private void Start()
	{
		collider = wall.GetComponent<Collider>();
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
				{
					Debug.Log("Is Stretching and in contact");
					return;
				}
				
				//controller.ChangeGravity(transform.forward);


				
				
				GameManager.CameraClimbSwitch?.Invoke(gravityDirection);
				playerRb.linearVelocity = Vector3.zero;

				Vector3 targetPos = controller.climbCheckOffset.position;

				RaycastHit hit;

				if (Physics.Raycast(playerRb.position, gravityDirection, out hit, controller.climbDetectionDistance,
					    controller.silkMask))
				{
					Debug.Log("Target Succeeded forward");
					Vector3 targetForward = hit.point - (gravityDirection *  controller.climbOffset);
					controller.ChangeGravity(gravityDirection, true, true, true, targetForward, wall.transform);

				}
				
				
				
				else if(RotaryHeart.Lib.PhysicsExtension.Physics.Raycast(targetPos,  gravityDirection,  out  hit, controller.climbDetectionDistance, controller.silkMask ))
				{
					controller.ChangeGravity(gravityDirection, true, true, true, hit.point, wall.transform);
					Debug.Log("Target Succeeded");
					hasSwitched = true;
				}

				else
				{
					Debug.Log("Failed to find target");
				}
				//playerRb.AddForce(transform.forward * controller.climbTriggerOffset, ForceMode.VelocityChange);
				
				
			
				
				//FlipDirection(transform.forward, playerRb);
			}
		}
	}

	


	private void OnTriggerExit(Collider other)
	{
		if (!other.TryGetComponent(out PlayerMovement controller))
			return;
		if(PlayerReferenceManager.instance.currentState == PlayerState.Stretching)
		{
			Debug.Log("Is Stretching and in contact");
			return;
		}

		if(!controller.isClimbing)
			return;
		//controller.ChangeGravity(transform.forward);
		var playerRb = other.gameObject.GetComponent<Rigidbody>();


		Debug.Log("Exiting Collider");
		WallContactPos = collider.ClosestPointOnBounds(controller.transform.position);

		//GameManager.CameraClimbSwitch?.Invoke(gravityDirection);
		controller.ChangeGravity(inverseDirection, false, true, false, WallContactPos, wall.transform);
		playerRb.AddForce(transform.up * controller.climbForce);
		playerRb.AddForce(transform.forward * controller.climbForce);
		hasSwitched = false;

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