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
		targetSeek = new Vector3(580f, 0f, 845f);

		maxSpeed = 4;
		maxForce = 0.1f;
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		arrive(targetSeek);
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

	void arrive(Vector3 target)
	{
		Vector3 desiredVelocity = target - position;

		float distance = desiredVelocity.magnitude;
		desiredVelocity.Normalize();

		if (distance < 100) {
			float m = scale (0, 100, 0, maxSpeed, distance);
			desiredVelocity *= m;
		} 
		else {
			desiredVelocity *= maxSpeed;
			}

		Vector3 steerVector = desiredVelocity - velocity;
		steerVector = Vector3.ClampMagnitude (steerVector, maxForce);
		applyForce (steerVector);				
	}

	/// <summary>
	/// Scale the specified OldMin, OldMax, NewMin, NewMax and OldValue. Like Processing's
	/// map function.
	/// </summary>
	/// <param name="OldMin">Old minimum.</param>
	/// <param name="OldMax">Old max.</param>
	/// <param name="NewMin">New minimum.</param>
	/// <param name="NewMax">New max.</param>
	/// <param name="OldValue">Old value.</param>
	float scale(float OldMin, float OldMax, float NewMin, float NewMax, float OldValue)
	{
		float oldRange = (OldMax - OldMin);
		float newRange = (NewMax - NewMin);
		float newValue = (((OldValue - OldMin) * newRange) / oldRange) + NewMin;

		return newValue;
	}
	
	void wander()
	{
	
	}
}
