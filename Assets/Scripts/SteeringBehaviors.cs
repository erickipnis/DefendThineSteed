using UnityEngine;
using System.Collections;

public class SteeringBehaviors : MonoBehaviour {
	
	Vector3 position;
	Vector3 velocity;
	Vector3 acceleration;
	
	public Vector3 targetSeek;

	float maxSpeed;
	float maxForce;	

	// Use this for initialization
	void Start ()
	{
		position = transform.position;
		position.y = 0;
		
		velocity = new Vector3(0, 0, 0);
		acceleration = new Vector3(0, 0, 0);
		targetSeek = new Vector3(738.855f, 0f, 959.8741f);

		maxSpeed = 4;
		maxForce = 0.1f;
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		seek(targetSeek);
		UpdateForces();
	}
	
	void UpdateForces()
	{
		velocity += acceleration;
		
		velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
		
		position += velocity;
		
		transform.position = position;
		
		acceleration *= 0;
	}
	
	void applyForce(Vector3 force)
	{
		acceleration += force;
		acceleration.y = 0;
	}
	
	void seek(Vector3 target)
	{
		Vector3 desiredVelocity = target - position;
		
		desiredVelocity.Normalize();
		
		desiredVelocity *= maxSpeed;
		
		Vector3 steerVector = desiredVelocity - velocity;
		                        
		steerVector = Vector3.ClampMagnitude(steerVector, maxForce);
		
		applyForce(steerVector);
	}
	
	void flee(Vector3 target)
	{
	
	}
	
	void wander()
	{
	
	}
	
}
