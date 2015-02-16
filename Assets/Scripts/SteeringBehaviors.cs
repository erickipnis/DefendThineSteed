using UnityEngine;
using System.Collections;

public class SteeringBehaviors : MonoBehaviour {
	
	Vector3 position;
	Vector3 velocity;
	Vector3 acceleration;
	float wanderAngle = 15.0f;
	
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
		//arrive(targetSeek);
		//seek(targetSeek);
		//flee(targetSeek);
		wander();
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
	
	void flee(Vector3 target)
	{
		Vector3 desiredVelocity = target - position;
		
		desiredVelocity.Normalize();
		
		desiredVelocity *= -maxSpeed;
		
		Vector3 fleeVector = desiredVelocity - velocity;
		
		fleeVector = Vector3.ClampMagnitude(fleeVector, maxForce);
		
		applyForce(fleeVector);
	}
	
	void wander()
	{
		float circleRadius = 20.0f; 
		float circleDistance = 20.0f;
		float angleChange = 0.5f;

		Vector3 circleCenter = velocity;

		circleCenter.Normalize();
		circleCenter *= circleDistance;

		Vector3 displacement = new Vector3 (0.0f, 0.0f, -1.0f);
		displacement *= circleRadius;

		displacement = setAngle(displacement, wanderAngle);

		float randomNum = Random.Range(0.0f, 1.0f);
		wanderAngle += randomNum * angleChange - angleChange * .5f;

		Vector3 wanderForce = circleCenter + displacement;

		applyForce(wanderForce);
	}

	Vector3 setAngle(Vector3 displacement, float angle)
	{
		float length = displacement.magnitude;
		Vector3 directionVector = displacement;

		directionVector.x = Mathf.Cos(angle) * length;
		directionVector.z = Mathf.Sin(angle) * length;

		return directionVector;
	}
}