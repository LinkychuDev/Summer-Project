using UnityEngine;

public class WallClimbTrigger : MonoBehaviour
{
	public bool isTrigger = true;
	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			if (other.TryGetComponent(out Rigidbody playerRb))
			{
				if(!playerRb.TryGetComponent(out PlayerMovement controller))
					return;
				if(controller.isClimbing)
					return;
				//controller.ChangeGravity(transform.forward);
				PlayerController.ClimbEvent?.Invoke(isTrigger);
				controller.ChangeGravity(transform.forward);
				//FlipDirection(transform.forward, playerRb);
			}
		}
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
}