using UnityEngine;
using System.Collections;
public class CharacterControl : MonoBehaviour {


	private CharacterController controller;
	private Vector3 moveDirection = Vector3.zero;
	private Vector3 forward = Vector3.zero;
	private Vector3 right = Vector3.zero;

	// Use this for initialization
	void Start () {
		controller = gameObject.GetComponent(CharacterController);
	

	}
	
	// Update is called once per frame
	void Update () {
		forward = transform.forward;
		right = Vector3 (forward.z, 0, -forward.x);
		var horizontalInput = Input.GetAxisRaw("Horizontal");
		var verticalInput = Input.GetAxisRaw("Vertical");

		var targetDirection = horizontalInput * right + verticalInput * forward;	
		moveDirection = Vector3.RotateTowards(moveDirection, targetDirection, 
		                                      200 * Mathf.Deg2Rad * Time.deltaTime, 1000);

		var movement = moveDirection  * Time.deltaTime * 2;
		controller.Move(movement);

		transform.rotation = Quaternion.LookRotation(moveDirection);

		if (targetDirection != Vector3.zero)
		{
			transform.rotation = Quaternion.LookRotation(moveDirection);
		}
		//@script RequireComponent(CharacterController)
	}
	
}
