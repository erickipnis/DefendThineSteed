using UnityEngine;
using System.Collections;

public class TrollScript : MonoBehaviour{
	
	NavMeshAgent agent;
	
	GameObject[] steeds; 
	GameObject[] trolls;
	//GameObject wanderSphere;
	GameObject safeZone;
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
	bool captured;

	float timer;
	float fitness;

	int index;

	float steedSeekDistance;
	uint chromosome;

	// Use this for initialization
	void Start () 
	{
		agent = GetComponent<NavMeshAgent> ();

//		while (transform.position.y > 0 || agent.transform.position.y > 0)
//		{
//			Vector3 randomPosition = new Vector3(Random.Range(50, 450), 0.0f, Random.Range(50, 450));
//			transform.position = randomPosition;
//			agent.transform.position = randomPosition;
//		}
		
		previousPosition = new Vector3(0.0f, 0.0f, 0.0f);
		
		velocity = agent.velocity;		
		
		acceleration = new Vector3(0.0f, 0.0f, 0.0f);
		
		maxForce = 3.0f;
		maxSpeed = 1.0f;
		
		steeds = GameObject.FindGameObjectsWithTag("Steed");
		trolls = GameObject.FindGameObjectsWithTag("Troll");
		//Debug.Log(steeds);
		safeZone = GameObject.FindGameObjectWithTag ("safe");
		targetSeek = new Vector3(1254.473f, 0.0f, 793.6649f);

		sepearationWeight = 0.5f;
		cohesionWeight = 0.2f;
		alignmentWeight = 0.2f;

		isFlocking = true;
		isWandering = true;
		isSeeking = false;
		isFleeing = false;

		timer = 0;
		fitness = 100;
		index = TrollGeneticAlgorithm.index;

		if (TrollGeneticAlgorithm.index < 5)
		{
			chromosome = TrollGeneticAlgorithm.chroms[index];
			//steedSeekDistance = TrollGeneticAlgorithm.phenos[index];
			steedSeekDistance = 148; // locked down value while steeds are evolved individually
			TrollGeneticAlgorithm.index = TrollGeneticAlgorithm.index + 1;
		}
	
		captured = false;

		wander();

		//BayesScript.LoadAndBuildData();
	}
	
	// Update is called once per frame
	void Update () 
	{
//		Debug.Log ("Troll Transform: " + transform.position.y + " Troll Agent: " + agent.transform.position.y);
//
//		if (transform.position.y > 0.2f || agent.transform.position.y > 0.2f)
//		{
//			Vector3 randomPosition = new Vector3(Random.Range(50, 450), 0.0f, Random.Range(50, 450));
//			transform.position = randomPosition;
//			agent.transform.position = randomPosition;
//		}

		if (steeds == null)
		{
			steeds = GameObject.FindGameObjectsWithTag("Steed");
		}

		if (trolls == null)
		{
			trolls = GameObject.FindGameObjectsWithTag("Troll");
		}

		//Debug.Log("Update is being called!");


		// If 1 second has gone by then make a new decision
		//if (timer >= 1.0f)
		//{
			//BayesScript.GetBayesOdds();
			//DetermineBehaviors((float)BayesScript.yesSeekOdds, (float)BayesScript.noSeekOdds);
		DetermineBehaviors ();

			//timer = 0.0f;
		//}

		UpdateForces();
		calculateVelocity();

		timer += Time.deltaTime;
		fitness -= Time.deltaTime * 0.5f;
		//Debug.Log (timer);
	}

	void DetermineBehaviors()
	{
//		float yesSeek = yesSeekOdds * 100;
//		float noSeek = noSeekOdds * 100;
//		int randomNum = Random.Range (0, 100);
//		Debug.Log (yesSeek.ToString() + "    " + noSeek.ToString());

		GameObject steed = findClosestSteed();
		
		Vector3 steedPosition = steed.transform.position;
		Vector3 safePosition = safeZone.transform.position;
		float steedDistance = Vector3.Distance(agent.transform.position, steedPosition);
		float safeDistance = Vector3.Distance (agent.transform.position, safePosition);
		//Debug.Log(steedDistance);

		// For first time random 50% chance to wander or seek
//		if (yesSeek == noSeek)
//		{
//			float startRandom = Random.Range(0, 2); // 0 or 1
//
//			if (startRandom == 1)
//			{
//				isSeeking = true;
//				isWandering = false;
//			}
//			else if (startRandom == 0)
//			{
//				isWandering = true;
//				isSeeking = false;
//			}
//		}
//		else if (randomNum >= 0 && randomNum <= yesSeek)
//		{
//			isSeeking = true;
//			isWandering = false;
//		}
//		else if (randomNum > yesSeek && randomNum < 100)
//		{
//			isWandering = true;
//			isSeeking = false;
//		}


		if (steedDistance <= steedSeekDistance)
		{
			isSeeking = true;
			isWandering = false;
		}
		else if (steedDistance > steedSeekDistance)
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

			if(safeDistance < 60)
			{
				Vector3 fleeForce = flee (safePosition);
				applyForce (fleeForce);
			}

			// Didn't catch anything within 5 seconds so fitness = 0
			if (timer > 5.0f)
			{
				finalFitness = 0;
				GeneticAlgorithm.CheckInIndividual(finalFitness, index);
			}
			
			if (steedDistance < 20)
			{
				Loss.score += 1;
				Destroy(steed);
				TrollGeneticAlgorithm.steedsCaptured[index] = TrollGeneticAlgorithm.steedsCaptured[index] + 1;

				// Caught the steed within 5 seconds, give it a fitness based on time passed
				if (timer <= 10.0f && captured == false)
				{
					TrollGeneticAlgorithm.fitnessArray[index] = fitness;
					captured = true;
				}

				//BayesScript.AddObs(100, true, false, true);
			}
		}
		else if (isWandering)
		{	

			if (safeDistance < 60)
			{
				Vector3 fleeForce = flee (safePosition);
				applyForce(fleeForce);

			}
			wander();
			/*if(safeDistance <= 15)
			{
				isFleeing = true;
				Vector3 fleeForce = flee(safePosition);
				applyForce (fleeForce);
			}
			else if(safeDistance > 15)
			{
				isFleeing = false;
			}
			*/
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
