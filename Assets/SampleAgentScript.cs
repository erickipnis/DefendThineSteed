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
	
	bool isSeeking;
	
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
		
		isSeeking = false;
	}
	
	// Update is called once per frame
	void Update ()
	{		
		
		DetermineBehaviors();
		//UpdateForces();
	}
	
	void DetermineBehaviors()
	{	
		if (agent.gameObject.tag == "Steed")
		{
			GameObject[] trolls = GameObject.FindGameObjectsWithTag("Troll");
			GameObject[] hayBales = GameObject.FindGameObjectsWithTag("Hay");

			float shortestTrollDistance = float.MaxValue;

			//int shortestTrollPositionIndex;

			Vector3 shortestTrollPosition = new Vector3(0.0f, 0.0f, 0.0f);
			
			for (int i = 0; i < trolls.Length; i++)
			{
				Vector3 trollPosition = trolls[i].transform.position;
				Vector3 hayPosition = hayBales[i].transform.position;
				
				float trollDistance = Vector3.Distance(trollPosition, agent.transform.position);
				float hayDistance = Vector3.Distance(hayPosition, agent.transform.position);

				if (trollDistance < shortestTrollDistance)
				{
					shortestTrollDistance = trollDistance;
					shortestTrollPosition = trolls[i].transform.position;
					//shortestTrollPositionIndex = i;
				}
				
				if (shortestTrollDistance <= 100)
				{
					flee(trollPosition);
				}
				else if (shortestTrollDistance > 100)
				{
					//wander();
				}								
			}
		}
		else if (agent.gameObject.tag == "Troll")
		{
			GameObject[] steeds = GameObject.FindGameObjectsWithTag("Steed");

			float shortestSteedDistance = float.MaxValue;

			int shortestSteedPositionIndex = 0;

			Vector3 shortestSteedPosition = new Vector3(0.0f, 0.0f, 0.0f);

			for (int i = 0; i < steeds.Length; i++)
			{
				Vector3 steedPosition = steeds[i].transform.position;
				float steedDistance = Vector3.Distance(steedPosition, agent.transform.position);
				//Debug.Log (steedDistance);

				if (steedDistance < shortestSteedDistance)
				{
					shortestSteedDistance = steedDistance;
					shortestSteedPosition = steeds[i].transform.position;
					shortestSteedPositionIndex = i;
					//Debug.Log(shortestSteedDistance);
				}
				
				if (shortestSteedDistance <= 100)
				{
					//Debug.Log(shortestSteedDistance);
					//agent.ResetPath();
					seek(shortestSteedPosition);
					//Debug.Log(steedDistance);
					if (shortestSteedDistance <= 10)
					{
						Destroy(steeds[shortestSteedPositionIndex]);
					}
				}				
				else if (shortestSteedDistance > 100)
				{
					Vector3 wanderForce = wander();
					wanderForce.Normalize();
					//wanderForce = wanderForce * 10;
					//Debug.Log(wanderForce);
					seek (target.position + wanderForce);
					//agent.ResetPath();
					//seek(target.position);					
				}
			}
		}
	}
	
	void UpdateForces()
	{
		agent.velocity += acceleration;
		
		agent.velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
		
		agent.transform.position += velocity;
		
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
//		Vector3 desiredVelocity = target - agent.transform.position;
//		//Vector3 desiredVelocity = agent.desiredVelocity;
//				
//		desiredVelocity.Normalize();
//				
//		desiredVelocity *= maxSpeed;
//				
//		Vector3 steerVector = desiredVelocity - agent.velocity;
//				
//		steerVector = Vector3.ClampMagnitude(steerVector, maxForce);
//				
//		agent.SetDestination(steerVector);

		agent.SetDestination(target);
		
	}
	
	void flee(Vector3 target)
	{	
		Vector3 desiredVelocity = target - agent.transform.position;
		//Vector3 desiredVelocity = agent.desiredVelocity;
		
		desiredVelocity.Normalize();
		
		desiredVelocity *= -maxSpeed;
		
		Vector3 fleeVector = desiredVelocity - agent.velocity;
		
		fleeVector = Vector3.ClampMagnitude(fleeVector, maxForce);
		
		agent.SetDestination (fleeVector);
	}
	
	Vector3 wander()
	{
		float circleRadius = 20.0f; 
		float circleDistance = 20.0f;
		float angleChange = 0.5f;
		
		Vector3 circleCenter = agent.velocity;
		//Debug.Log (agent.velocity);
		circleCenter.Normalize();
		circleCenter *= circleDistance;
		
		Vector3 displacement = new Vector3 (0.0f, 0.0f, -1.0f);
		displacement *= circleRadius;
		
		displacement = setAngle(displacement, wanderAngle);
		
		float randomNum = Random.Range(0.0f, 1.0f);
		wanderAngle += (randomNum * angleChange) - (angleChange * .5f);
		
		Vector3 wanderForce = circleCenter + displacement;
		//Debug.Log (wanderAngle);
//		wanderForce = targetVector - wanderForce;
//		wanderForce = wanderForce.normalized;
//		wanderForce *= maxSpeed;
//		wanderForce = Vector3.ClampMagnitude (wanderForce, maxForce);
		//Debug.Log(wanderForce);
		//Debug.Log(agent.pathStatus);
		//agent.SetDestination(wanderForce);
		return wanderForce;
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

