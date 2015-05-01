using UnityEngine;
using System.Collections;

public class controller : MonoBehaviour {
	private Vector3 moveDirection = Vector3.zero;
	public float speed = 5.0f;

	// Use this for initialization
	void Start () {

	}

	
	void Update()
	{
		CharacterController controller = GetComponent<CharacterController> ();
		
		moveDirection = transform.TransformDirection(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		moveDirection *= speed;
		
		controller.Move(moveDirection * Time.deltaTime);
		//angle = Mathf.Atan2(moveDirection.x, transform.rotation = Quaternion.Euler(new Vector3(0, angle, 0)));


	}
}
