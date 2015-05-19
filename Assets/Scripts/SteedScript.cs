using UnityEngine;
using System.Collections;

public class SteedScript: MonoBehaviour
{
	NavMeshAgent agent;

	GameObject[] trolls; 
	//GameObject wanderSphere;
	GameObject player;
	GameObject safeZone;

	Vector3 previousPosition;
	Vector3 velocity;
	Vector3 acceleration;
	Vector3 trollPosition;
	Vector3 playerPosition;


	float wanderAngle = 15.0f;

	Vector3 previousPosition;
	Vector3 velocity;
	Vector3 acceleration;
	Vector3 seekForce;
	Vector3 fleeForce;


	private float maxSpeed;
	private float maxForce;	

	float seekWeight;
	float fleeWeight;
	float wanderWeight;
	float trollDistance;
	float playerDistance;
	float startingTrollDistance;
	float startingPlayerDistance;

	float timer;
	float successTimer;
	float fitness;


	int index;

bool isFleeing;
bool isWandering;
bool isSeekingChar;
bool isSeekingSafe;

// Use this for initialization
void Start () 
{
	agent = GetComponent<NavMeshAgent> ();
	
	previousPosition = new Vector3(0.0f, 0.0f, 0.0f);
	
	velocity = agent.velocity;		
	
	acceleration = new Vector3(0.0f, 0.0f, 0.0f);
	
	maxForce = 0.2f;
	maxSpeed = 2.0f;
	
	trolls = GameObject.FindGameObjectsWithTag("Troll");
	//Debug.Log(steeds);
	player = GameObject.FindGameObjectWithTag("Player");
	safeZone = GameObject.FindGameObjectWithTag("safe");
	targetSeek = new Vector3(1254.473f, 0.0f, 793.6649f);
	
	//wanderSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
	//wanderSphere.transform.position = agent.transform.position;
	
}


	float steedFleeDistance;

	uint chromosome;

	bool escaped;
	bool isWandering;
	bool isSeeking;
	bool isFleeing;

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
		
		maxForce = 0.2f;
		maxSpeed = 2.0f;
		
		trolls = GameObject.FindGameObjectsWithTag("Troll");
		//Debug.Log(steeds);
		player = GameObject.FindGameObjectWithTag("Player");
		safeZone = GameObject.FindGameObjectWithTag("safe");

		index = SteedGeneticAlgorithm.index;

		if (SteedGeneticAlgorithm.index < 5)
		{
			chromosome = SteedGeneticAlgorithm.chroms[index];
			//steedFleeDistance = SteedGeneticAlgorithm.phenos[index];
			steedFleeDistance = 248; // locked down value determined by GA
			SteedGeneticAlgorithm.index = SteedGeneticAlgorithm.index + 1;
		}		

		escaped = false;

		timer = 0;
		successTimer = 0;
		fitness = 200;

		//wander();

		isWandering = false;
		isSeeking = false;
		isFleeing = false;

		// Determine initial behavior with bayes
		DetermineBehaviors();
	}


	// Update is called once per frame
	void Update () 
	{
//		Debug.Log ("Steed Transform: " + transform.position.y + " Steed Agent: " + agent.transform.position.y);
//
//		if (transform.position.y > 0.2f || agent.transform.position.y > 0.2f)
//		{
//			Vector3 randomPosition = new Vector3(Random.Range(50, 450), 0.0f, Random.Range(50, 450));
//			transform.position = randomPosition;
//			agent.transform.position = randomPosition;
//		}

		if (trolls == null)
		{
			trolls = GameObject.FindGameObjectsWithTag("Trolls");
		}
		
		//Debug.Log("Update is being called!");

		// Determine a new behavior every 5 seconds
		if (timer >= 5.0f)
		{	
			DetermineBehaviors();		
			timer = 0.0f;
		}
		

		UpdateForces();
		calculateVelocity();

		timer += Time.deltaTime;
		successTimer += Time.deltaTime;
		fitness -= Time.deltaTime * 0.5f;
	}

	void DetermineBehaviors()
	{
		GameObject troll = findClosestTroll();
		
		trollPosition = troll.transform.position;
		playerPosition = player.transform.position;

		Vector3 safePosition = safeZone.transform.position;
		
		trollDistance = Vector3.Distance(agent.transform.position, trollPosition);
		playerDistance = Vector3.Distance(agent.transform.position, playerPosition);
		startingTrollDistance = trollDistance;
		startingPlayerDistance = playerDistance;

		float safeDistance = Vector3.Distance(agent.transform.position, safePosition);
		
		BayesScript.GetBayesOdds((int)trollDistance, (int)playerDistance);

		int chanceToWander = (int)BayesScript.wanderTotalOdds;
		int chanceToFlee = (int)BayesScript.fleeTotalOdds;
		int chanceToSeek = (int)BayesScript.seekTotalOdds;

		Debug.Log ("Chance to Wander: " + chanceToWander);
		Debug.Log ("Chance to Flee: " + chanceToFlee);
		Debug.Log("Chance to Seek: " + chanceToSeek);

		int bayesChoice = Random.Range(1, 100);

		if (bayesChoice >= 1 && bayesChoice <= chanceToWander)
		{
			wander();
			isWandering = true;
			isFleeing = false;
			isSeeking = false;
		}
		else if (bayesChoice >= chanceToWander && bayesChoice <= (chanceToWander + chanceToFlee))
		{
			flee(trollPosition);
			isWandering = false;
			isFleeing = true;
			isSeeking = false;
		}
		else if (bayesChoice >= (chanceToWander + chanceToFlee) && bayesChoice <= 100)
		{
			seek(playerPosition);
			isWandering = false;
			isFleeing = false;
			isSeeking = true;
		}

//		// Check bayes odds here for wander, seek and flee odds
//		if ((chanceToWander == chanceToFlee) && (chanceToFlee == chanceToSeek))
//		{
//			// randomly wander, seek or flee
//			int randomNum = Random.Range(0, 2);
//
//			if (randomNum == 0)
//			{
//				wander();
//				isWandering = true;
//				isFleeing = false;
//				isSeeking = false;
//			}
//			else if (randomNum == 1)
//			{
//				flee(trollPosition);
//				isWandering = false;
//				isFleeing = true;
//				isSeeking = false;
//			}
//			else if (randomNum == 2)
//			{
//				seek(playerPosition);
//				isWandering = false;
//				isFleeing = false;
//				isSeeking = true;
//			}
//		}
//		else if (chanceToWander == chanceToFlee)
//		{
//			// randomly wander or flee
//			int randomNum = Random.Range(0, 1);
//
//			if (randomNum == 0)
//			{
//				wander();
//				isWandering = true;
//				isFleeing = false;
//				isSeeking = false;
//			}
//			else if (randomNum == 1)
//			{
//				flee(trollPosition);
//				isWandering = false;
//				isFleeing = true;
//				isSeeking = false;
//			}
//		}
//		else if (chanceToWander == chanceToSeek)
//		{
//			// randomly wander or seek
//			int randomNum = Random.Range(0, 1);
//			
//			if (randomNum == 0)
//			{
//				wander();
//				isWandering = true;
//				isFleeing = false;
//				isSeeking = false;
//			}
//			else if (randomNum == 1)
//			{
//				seek(playerPosition);
//				isWandering = false;
//				isFleeing = false;
//				isSeeking = true;
//			}
//		}
//		else if (chanceToFlee == chanceToSeek)
//		{
//			// randomly flee or seek
//			int randomNum = Random.Range(0, 1);
//			
//			if (randomNum == 0)
//			{
//				flee(trollPosition);
//				isWandering = false;
//				isFleeing = true;
//				isSeeking = false;
//			}
//			else if (randomNum == 1)
//			{
//				seek(playerPosition);
//				isWandering = false;
//				isFleeing = false;
//				isSeeking = true;
//			}
//		}
//		else if ((chanceToWander > chanceToFlee) && (chanceToWander > chanceToSeek))
//		{
//			// wander
//			wander();
//			isWandering = true;
//			isFleeing = false;
//			isSeeking = false;
//		}
//		else if ((chanceToFlee > chanceToWander) && (chanceToFlee > chanceToSeek))
//		{
//			// flee
//			flee (trollPosition);
//			isWandering = false;
//			isFleeing = true;
//			isSeeking = false;
//		}
//		else if ((chanceToSeek > chanceToWander) && (chanceToSeek > chanceToFlee))
//		{
//			// seek
//			seek(playerPosition);
//			isWandering = false;
//			isFleeing = false;
//			isSeeking = true;
//		}

void DetermineBehaviors()
{
	GameObject troll = findClosestTroll ();
	
	Vector3 trollPosition = troll.transform.position;
	Vector3 playerPosition = player.transform.position;
	Vector3 safePosition = safeZone.transform.position;
	
	float trollDistance = Vector3.Distance (agent.transform.position, trollPosition);
	float playerDistance = Vector3.Distance (agent.transform.position, playerPosition);
	float safeDistance = Vector3.Distance (agent.transform.position, safePosition);
	
	/*if (trollDistance <= 100) 
		{
		if (playerDistance <= 100) 
			{
			if (safeDistance <= 25) 
			{
				seekForce = seek (safePosition);
				applyForce (seekForce);
				Destroy (agent.gameObject);
			} // go to safe
			seekForce = seek (safePosition);
			applyForce (playerPosition);
		} // follow player
		fleeForce = flee (trollPosition);
		applyForce (fleeForce);
	} // flee troll
	else 
	{
		Vector3 wanderForce = wander ();
	}*/
		if (playerDistance <= 100)
		{
			Vector3 seekForce = seek (playerPosition);
			//Debug.Log(playerDistance);
			
			if (trollDistance <= 100)
			{
				//Debug.Log(trollDistance);
				Vector3 fleeForce = flee(trollPosition);
				
				applyForce(fleeForce);			
				//agent.SetDestination(seekForce);
				
			}
			if(safeDistance <= 25)
			{
				seekForce = seek (safePosition);
				//applyForce (seekForce);
				Save.score += 1;
				Destroy (agent.gameObject);
				
			}
			applyForce (seekForce);

		}
		else if (playerDistance > 100)
		{
			if (trollDistance <= 100)
			{
				//Debug.Log(trollDistance);
				Vector3 fleeForce = flee(trollPosition);
				
				applyForce(fleeForce);			
				//agent.SetDestination(seekForce);
				
			}	
			else if (trolls == null || trollDistance > 100)
			{	
				
				Vector3 wanderForce = wander();
				//Debug.Log(trollDistance);
				
				//agent.SetDestination(wanderForce);			
			}
		}
	
}


private void calculateVelocity()
{
	velocity = (agent.transform.position - previousPosition) / Time.deltaTime; 
	velocity.Normalize();
	
	previousPosition = agent.transform.position;
}


		// Did steed survive based on its decision?
		if (successTimer >= 5.0f)
		{
			// Add the observations to the table
			if (isWandering)
			{
				BayesScript.AddObs(Action.wander, (int) startingTrollDistance, (int) startingPlayerDistance, true);
				BayesScript.AddObs(Action.flee, (int) startingTrollDistance, (int) startingPlayerDistance, false);
				BayesScript.AddObs(Action.seek, (int) startingTrollDistance, (int) startingPlayerDistance, false);
			}
			else if (isFleeing)
			{
				BayesScript.AddObs(Action.flee, (int) startingTrollDistance, (int) startingPlayerDistance, true);
				BayesScript.AddObs(Action.seek, (int) startingTrollDistance, (int) startingPlayerDistance, false);
				BayesScript.AddObs(Action.wander, (int) startingTrollDistance, (int) startingPlayerDistance, false);
			}
			else if (isSeeking)
			{
				BayesScript.AddObs(Action.seek, (int) startingTrollDistance, (int) startingPlayerDistance, true);
				BayesScript.AddObs(Action.wander, (int) startingTrollDistance, (int) startingPlayerDistance, false);
				BayesScript.AddObs(Action.flee, (int) startingTrollDistance, (int) startingPlayerDistance, false);
			}

			successTimer = 0;
		}

		// ToDo: add the first percentage to the second and make the second the max range. Should work?

		// 0 to first percentage, then 1st percentage to 1st + 2nd percentage. then 1st + 2nd to 1st + 2nd + 3rd percentage

		// Decision Tree
		//Debug.Log(trollDistance);
		
//		if (playerDistance <= 100)
//		{
//			Vector3 seekForce = seek (playerPosition);
//			applyForce (seekForce);
//			//Debug.Log(playerDistance);
//
//			if (trollDistance <= steedFleeDistance)
//			{
//				//Debug.Log(trollDistance);
//				Vector3 fleeForce = flee(trollPosition);
//					
//				applyForce(fleeForce);
//
//				//agent.SetDestination(seekForce);
//
//				if (timer >= 7.0f && escaped == false)
//				{
//					SteedGeneticAlgorithm.fitnessArray[index] = fitness;
//					escaped = true;
//				}
//					
//			}
//			else if (trollDistance > steedFleeDistance && timer >= 7.0f && escaped == false)
//			{
//				SteedGeneticAlgorithm.fitnessArray[index] = fitness;
//				escaped = true;
//			}
//
			if(safeDistance <= 10)
			{
				Vector3 seekForce = seek (safePosition);
				Destroy (this);

				SteedGeneticAlgorithm.fitnessArray[index] = fitness;
				SteedGeneticAlgorithm.shelterBonuses[index] = fitness * 0.25f;
				escaped = true;

				if (isWandering)
				{
					BayesScript.AddObs(Action.wander, (int) startingTrollDistance, (int) startingPlayerDistance, true);
					BayesScript.AddObs(Action.flee, (int) startingTrollDistance, (int) startingPlayerDistance, false);
					BayesScript.AddObs(Action.seek, (int) startingTrollDistance, (int) startingPlayerDistance, false);
				}
				else if (isFleeing)
				{
					BayesScript.AddObs(Action.flee, (int) startingTrollDistance, (int) startingPlayerDistance, true);
					BayesScript.AddObs(Action.seek, (int) startingTrollDistance, (int) startingPlayerDistance, false);
					BayesScript.AddObs(Action.wander, (int) startingTrollDistance, (int) startingPlayerDistance, false);
				}
				else if (isSeeking)
				{
					BayesScript.AddObs(Action.seek, (int) startingTrollDistance, (int) startingPlayerDistance, true);
					BayesScript.AddObs(Action.wander, (int) startingTrollDistance, (int) startingPlayerDistance, false);
					BayesScript.AddObs(Action.flee, (int) startingTrollDistance, (int) startingPlayerDistance, false);
				}

				successTimer = 0;
			}
//		}
//		else if (playerDistance > 100)
//		{
//			if (trollDistance <= steedFleeDistance)
//			{
//				//Debug.Log(trollDistance);
//				Vector3 fleeForce = flee(trollPosition);
//					
//				applyForce(fleeForce);			
//				//agent.SetDestination(seekForce);
//
//				if (timer >= 7.0f && escaped == false)
//				{
//					SteedGeneticAlgorithm.fitnessArray[index] = fitness;
//					escaped = true;
//				}
//					
//			}	
//			else if (trolls == null || trollDistance > steedFleeDistance)
//			{	
//				
//				Vector3 wanderForce = wander();
//				//Debug.Log(trollDistance);
//				
//				//agent.SetDestination(wanderForce);			
//			}
//		}
	}

	private void calculateVelocity()
	{
		velocity = (agent.transform.position - previousPosition) / Time.deltaTime; 
		velocity.Normalize();
		
		previousPosition = agent.transform.position;
	}

	private GameObject findClosestTroll()
	{
		float shortestTrollDistance = float.MaxValue;
		float distance = 0;
		
		GameObject closestTroll = null;

		trolls = GameObject.FindGameObjectsWithTag("Troll");

		for (int i = 0; i < trolls.Length; i++)
		{
			distance = Vector3.Distance(agent.transform.position, trolls[i].transform.position);
			
			if (distance < shortestTrollDistance)
			{
				shortestTrollDistance = distance;
				closestTroll = trolls[i];
			}
		}
		
		return closestTroll;
	}

	private Vector3 seek(Vector3 target)
	{
		Vector3 desiredVelocity = target - agent.transform.position;
		
		desiredVelocity.Normalize();
		
		desiredVelocity *= maxSpeed;
		
		Vector3 steerVector = desiredVelocity - velocity;
		
		steerVector = Vector3.ClampMagnitude(steerVector, maxForce);
		
		applyForce(steerVector);
		
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
		//Debug.DrawRay(agent.transform.position, wanderForce, Color.red);
		
		//wanderSphere.transform.position = agent.transform.position + wanderForce;
		
		wanderForce.Normalize();		
		
		applyForce(wanderForce);
		
		//Debug.Log("Wander was called!");
		
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
