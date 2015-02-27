using UnityEngine;
using System.Collections;

public class SampleAgentScript : MonoBehaviour {
	public Transform target;
	NavMeshAgent agent;
	// Use this for initialization
	Vector3 position;
	Vector3 velocity;
	Vector3 acceleration;
	
	float wanderAngle = 15.0f;
	float steedWanderWeight =0.001f;
	float steedFleeWeight = 0.2f;
	float trollSeekWeight = 0.2f;
	float trollWanderWeight = 0.001f;
	
	Vector3 targetSeek;
	
	float maxSpeed;
	float maxForce;	
	
	NavMeshPath currentPath;
	
	// Use this for initialization
	void Start ()
	{	
		agent = GetComponent<NavMeshAgent> ();
		
		position = transform.position;
		position.y = 0;
		
		velocity = agent.velocity;
		acceleration = new Vector3 (0.0f, 0.0f, 0.0f);
		targetSeek = agent.steeringTarget;
		
		maxSpeed = 2;
		maxForce = 0.1f;
		
		currentPath = agent.path;
	}
	
	// Update is called once per frame
	void Update ()
	{		
		
		DetermineBehaviors();
		//UpdateForces();
	}
	
	void DetermineBehaviors()
	{
		if (gameObject.tag == "Steed")
		{
			GameObject[] trolls = GameObject.FindGameObjectsWithTag("Troll");
			GameObject[] hayBales = GameObject.FindGameObjectsWithTag("Hay");
			
			for (int i = 0; i < trolls.Length; i++)
			{
				Vector3 trollPosition = trolls[i].transform.position;
				Vector3 hayPosition = hayBales[i].transform.position;
				
				float trollDistance = Vector3.Distance(trollPosition, position);
				float hayDistance = Vector3.Distance(hayPosition, position);
				
				if (trollDistance <= 100)
				{
					flee(trollPosition);
				}
				else if (trollDistance > 100)
				{
					//wander();
				}								
			}
		}
		else if (agent.gameObject.tag == "Troll")
		{
			GameObject[] steeds = GameObject.FindGameObjectsWithTag("Steed");
			
			for (int i = 0; i < steeds.Length; i++)
			{
				Vector3 steedPosition = steeds[i].transform.position;
				float steedDistance = Vector3.Distance(steedPosition, position);
				
				if (steedDistance <= 100)
				{
					seek(steedPosition);
				}
				else if (steedDistance > 100)
				{
					//wander();
					//agent.path = currentPath;
					
				}
			}
		}
	}
	
	void UpdateForces()
	{
		velocity += acceleration;
		
		velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
		
		position += velocity;
		
		agent.transform.position = position;
		
		acceleration *= 0;
	}
	
	void applyForce(Vector3 force)
	{
		acceleration += force;
		acceleration.y = 0;
	}
	
	void seek(Vector3 target)
	{
		agent.SetDestination (target);
		//		Vector3 desiredVelocity = target - position;
		//		
		//		desiredVelocity.Normalize();
		//		
		//		desiredVelocity *= maxSpeed;
		//		
		//		Vector3 steerVector = desiredVelocity - velocity;
		//		
		//		steerVector = Vector3.ClampMagnitude(steerVector, maxForce);
		//		
		//		applyForce(steerVector * trollSeekWeight);
	}
	
	void flee(Vector3 target)
	{	

		Vector3 desiredVelocity = target - agent.transform.position;
		
		desiredVelocity.Normalize();
		
		desiredVelocity *= -maxSpeed;
		
		Vector3 fleeVector = desiredVelocity - velocity;
		
		fleeVector = Vector3.ClampMagnitude(fleeVector, maxForce);
		
		agent.SetDestination (fleeVector);
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
		
		applyForce(wanderForce * trollWanderWeight);
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

