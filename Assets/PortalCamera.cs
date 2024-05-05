using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCamera : MonoBehaviour {

	public Transform playerCamera;
	public Transform portal;
	public Transform otherPortal;
	
	// Update is called once per frame
	void LateUpdate ()
	{
		Quaternion portalRotationDifference = Quaternion.Inverse(otherPortal.rotation) * portal.rotation;
		Vector3 playerOffsetFromPortal = portalRotationDifference * (playerCamera.position - otherPortal.position);
		transform.position = portal.position + playerOffsetFromPortal;

		Quaternion newCameraRotation = portalRotationDifference * playerCamera.rotation;
		transform.rotation = newCameraRotation;
	}
}