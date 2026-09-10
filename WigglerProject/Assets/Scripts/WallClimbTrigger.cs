using System;
using System.Collections;
using UnityEngine;



public class WallClimbTrigger : CollisionBlock, IPlayerHint
{
	public bool isTrigger = true;

	private Collider wallCollider;
	private bool hasSwitched;
	private PlayerReferenceManager playerReferenceManager;
	
	private  Vector3 inverseDirection = Vector3.down ;
	private Vector3 gravityDirection;

	public GameObject wall;

	public LayerMask groundMask;

	public Transform inverseSetPosition;
	public Transform wallContactPos;
	public Direction targetDir;


	[SerializeField] private float maxClimbAngle;

	private Vector3 WallContactPos;


	public bool usePreset;
	
	

	private void Start()
	{
		wallCollider = wall.GetComponent<Collider>();
		UpdateDirection();
	}

	private void OnValidate()
	{
		UpdateDirection();
	}

	
	void UpdateDirection()
	{
		if (usePreset)
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

		else
		{
			gravityDirection = transform.forward;
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
				if(controller.isClimbing)
					return;
				StartCoroutine(SetUpClimbing(playerRb, controller));
				
				
			

				
				
				//playerRb.AddForce(transform.forward * controller.climbTriggerOffset, ForceMode.VelocityChange);
				
				
			
				
				//FlipDirection(transform.forward, playerRb);
			}
		}
	}


	IEnumerator SetUpClimbing(Rigidbody playerRb, PlayerMovement controller)
	{
		
		yield return new WaitUntil(() =>
			PlayerReferenceManager.instance.playerStretch.stretchState == PlayerStretch.StretchState.None);
		GameManager.CameraClimbSwitch?.Invoke(gravityDirection);
		playerRb.linearVelocity = Vector3.zero;

		Vector3 targetPos = controller.climbCheckOffset.position;

		RaycastHit hit;

		if (Physics.Raycast(playerRb.position, gravityDirection, out hit, controller.climbDetectionDistance,
			    controller.silkMask))
		{
			Debug.Log("Target Succeeded forward");
			Vector3 targetForward = hit.point - (gravityDirection *  controller.climbOffset);
			yield return new WaitForFixedUpdate();
			controller.ChangeGravity(gravityDirection, true, true, true, targetForward, wall.transform);

		}
				
				
				
		else if(Physics.Raycast(targetPos,  gravityDirection,  out  hit, controller.climbDetectionDistance, controller.silkMask ))
		{
			yield return new WaitForFixedUpdate();
			controller.ChangeGravity(gravityDirection, true, true, true, hit.point, wall.transform);
			Debug.Log("Target Succeeded");
			hasSwitched = true;
		}

		else
		{
			Debug.Log("Failed to find target");
		}
		//controller.ChangeGravity(transform.forward);


				
		GameManager.CameraClimbSwitch?.Invoke(gravityDirection);
	}
	


	private void OnTriggerExit(Collider other)
	{
		if (!other.TryGetComponent(out PlayerMovement controller))
			return;
		if(PlayerReferenceManager.instance.currentState == PlayerState.Stretching)
		{
			
			return;
		}

		if(!controller.isClimbing)
			return;
		//controller.ChangeGravity(transform.forward);
		

		
		

		
		WallContactPos = wallCollider.ClosestPointOnBounds(controller.transform.position);

		
		controller.ChangeGravity(inverseDirection, false, true, false, WallContactPos, wall.transform);
		controller.rb.AddForce(transform.up * controller.climbForce);
		controller.rb.AddForce(transform.forward * controller.climbForce);
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

	public PlayerActionEvent PlayerActionEvent { get; } = PlayerActionEvent.ClimbAction;
}