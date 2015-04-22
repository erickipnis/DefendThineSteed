using UnityEngine;
using System.Collections;

public class TrollScript : MonoBehaviour{
	
	NavMeshAgent agent;
	
	GameObject[] steeds; 
	GameObject[] trolls;
	//GameObject wanderSphere;
	
	public Vector3 targetSeek;

	Vector3 previousPosition;
	Vector3 velocity;
	Vector3 acceleration;
	
	float wanderAngle = 15.0f;
	
	private float maxSpeed;
	private float maxForce;	
	
	float seekWeight;
	float fleeWeight;
	float wanderWeight;
	float sepearationWeight;
	float cohesionWeight;
	float alignmentWeight;

	bool isWandering;
	bool isSeeking;
	bool isFleeing;
	bool isFlocking;
	
	// Use this for initialization
	void Start () 
	{
		agent = GetComponent<NavMeshAgent> ();
		
		previousPosition = new Vector3(0.0f, 0.0f, 0.0f);
		
		velocity = agent.velocity;		
		
		acceleration = new Vector3(0.0f, 0.0f, 0.0f);
		
		maxForce = 3.0f;
		maxSpeed = 2.0f;
		
		steeds = GameObject.FindGameObjectsWithTag("Steed");
		trolls = GameObject.FindGameObjectsWithTag("Troll");
		//Debug.Log(steeds);
		
		targetSeek = new Vector3(1254.473f, 0.0f, 793.6649f);
		
	//	wanderSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		//wanderSphere.transform.position = agent.transform.position;
		sepearationWeight = 0.5f;
		cohesionWeight = 0.2f;
		alignmentWeight = 0.2f;

		isFlocking = true;
		isWandering = true;
		isSeeking = false;
		isFleeing = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (steeds == null)
		{
			steeds = GameObject.FindGameObjectsWithTag("Steed");
		}

		if (trolls == null)
		{
			trolls = GameObject.FindGameObjectsWithTag("Troll");
		}
		//Debug.Log("Update is being called!");

		DetermineBehaviors();
		UpdateForces();
		calculateVelocity();
	}
	
	void DetermineBehaviors()
	{
		GameObject steed = findClosestSteed();
		
		Vector3 steedPosition = steed.transform.position;
		
		float steedDistance = Vector3.Distance(agent.transform.position, steedPosition);
		
		//Debug.Log(steedDistance);

		if (steedDistance <= 100)
		{
			isSeeking = true;
			isWandering = false;
		}
		else if (steedDistance > 100)
		{
			isWandering = true;
			isSeeking = false;
		}
		
		if (isSeeking)
		{
			//Debug.Log(steedDistance);
			Vector3 seekForce = seek(steedPosition);
			
			applyForce(seekForce);			
			//agent.SetDestination(seekForce);
			
			if (steedDistance < 10)
			{
				Destroy(steed);
			}
		}
		else if (isWandering)
		{	
			wander();
		}

		if (isFlocking)
		{
			flock();
		}
	}
	
	private void calculateVelocity()
	{
		velocity = (agent.transform.position - previousPosition) / Time.deltaTime; 
		velocity.Normalize();
		
		previousPosition = agent.transform.position;
	}
	
	private GameObject findClosestSteed()
	{
		float shortestSteedDistance = float.MaxValue;
		float distance = 0;
		
		GameObject closestSteed = null;
		
		steeds = GameObject.FindGameObjectsWithTag("Steed");
		
		for (int i = 0; i < steeds.Length; i++)
		{
		 	distance = Vector3.Distance(agent.transform.position, steeds[i].transform.position);
		 	
		 	if (distance < shortestSteedDistance)
		 	{
		 		shortestSteedDistance = distance;
		 		closestSteed = steeds[i];
		 	}
		}
		
		return closestSteed;
	}
	
	private Vector3 seek(Vector3 target)
	{
		Vector3 desiredVelocity = target - agent.transform.position;
		
		desiredVelocity.Normalize();
		
		desiredVelocity *= maxSpeed;
		
		Vector3 steerVector = desiredVelocity - velocity;
		
		steerVector = Vector3.ClampMagnitude(steerVector, maxForce);
		
		//applyForce(steerVector);
		
		return steerVector;
	}
	
	private Vector3 flee(Vector3 target)
	{
		Vector3 desiredVelocity = target - agent.transform.position;
		
		desiredVelocity.Normalize();
		
		desiredVelocity *= -maxSpeed;
		
		Vector3 fleeVector = desiredVelocity - velocity;
		
		fleeVector = Vector3.ClampMagnitude(fleeVector, maxForce);
		
		applyForce(fleeVector);
		
		return fleeVector;
	}
	
	private Vector3 wander()
	{
		float circleRadius = 20.0f; 
		float circleDistance = 20.0f;
		float angleChange = 0.5f;
		
		Vector3 circleCenter = velocity;
		//Debug.Log(velocity);
		
		circleCenter.Normalize();
		circleCenter *= circleDistance;
		
		Vector3 displacement = new Vector3 (0.0f, 0.0f, -1.0f);
		displacement *= circleRadius;
		
		displacement = setAngle(displacement, wanderAngle);
		
		float randomNum = Random.Range(0.0f, 1.0f);
		wanderAngle += randomNum * angleChange - angleChange * .5f;
		
		Vector3 wanderForce = circleCenter + displacement;
		Debug.DrawRay(agent.transform.position, wanderForce, Color.red);
		
		//wanderSphere.transform.position = agent.transform.position + wanderForce;
		
		wanderForce.Normalize();		
		
		applyForce(wanderForce);
		
		return wanderForce;
	}

	private void flock()
	{
		Vector3 alignment = Align();
		Vector3 seperation = Seperate();
		Vector3 cohesion = Cohesion();

		alignment *= alignmentWeight;
		seperation *= sepearationWeight;
		cohesion *= cohesionWeight;

		applyForce(alignment);
		applyForce(seperation);
		applyForce(cohesion);
	}

	private Vector3 Seperate()
	{
		float seperateDistance = 20.0f;

		Vector3 sumVector = new Vector3(0.0f, 0.0f, 0.0f);

		int count = 0;

		for (int i = 0; i < trolls.Length; i++)
		{
			float distance = Vector3.Distance(agent.transform.position, trolls[i].transform.position);

			if ((distance > 0) && (distance < seperateDistance))
			{
				Vector3 difference = agent.transform.position - trolls[i].transform.position;
				difference.Normalize();
				difference /= distance;

				sumVector += difference;

				count++;
			}
		}

		if (count > 0)
		{
			sumVector /= count;
			sumVector.Normalize();
			sumVector *= maxSpeed;

			Vector3 steerVector = sumVector - velocity;
			steerVector = Vector3.ClampMagnitude(steerVector, maxForce);

			return steerVector;
		}
		else
		{
			return new Vector3(0.0f, 0.0f, 0.0f);
		}
	}

	private Vector3 Align()
	{
		float neighborDistance = 50.0f;

		Vector3 sumVector = new Vector3(0.0f, 0.0f, 0.0f);

		int count = 0; 

		for (int i = 0; i < trolls.Length; i++)
		{
			float distance = Vector3.Distance(agent.transform.position, trolls[i].transform.position);

			if ((distance > 0) && (distance < neighborDistance))
			{
				sumVector += trolls[i].transform.position;
				count++;
			}
		}

		if (count > 0)
		{
			sumVector /= count;
			sumVector.Normalize();
			sumVector *= maxSpeed;

			Vector3 steerVector = sumVector - velocity;
			steerVector = Vector3.ClampMagnitude(steerVector, maxForce);

			return steerVector;
		}
		else 
		{
			return new Vector3(0.0f, 0.0f, 0.0f);
		}
	}

	private Vector3 Cohesion()
	{
		float neighborDistance = 50.0f;

		Vector3 sumVector = new Vector3(0.0f, 0.0f, 0.0f);

		int count = 0;

		for (int i = 0; i < trolls.Length; i++)
		{
			float distance = Vector3.Distance(agent.transform.position,trolls[i].transform.position);

			if ((distance > 0) && (distance < neighborDistance))
			{
				sumVector += trolls[i].transform.position;
				count++;
			}
		}

		if (count > 0)
		{
			sumVector /= count;
			return seek(sumVector);
		}
		else 
		{
			return new Vector3(0.0f, 0.0f, 0.0f);
		}
	}

	Vector3 setAngle(Vector3 displacement, float angle)
	{
		float length = displacement.magnitude;
		Vector3 directionVector = displacement;
		
		directionVector.x = Mathf.Cos(angle) * length;
		directionVector.z = Mathf.Sin(angle) * length;
		
		return directionVector;
	}
	
	public void UpdateForces()
	{
		velocity += acceleration;
		
		velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
		
		agent.transform.position += velocity;
		
		//transform.position = position;
		
		acceleration *= 0;
	}
	
	void applyForce(Vector3 force)
	{
		acceleration += force;
		acceleration.y = 0;
	}
	
	
	
}
