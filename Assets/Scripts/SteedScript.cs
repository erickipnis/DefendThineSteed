using UnityEngine;
using System.Collections;

public class SteedScript: MonoBehaviour{

NavMeshAgent agent;

GameObject[] trolls; 
//GameObject wanderSphere;
GameObject player;
GameObject safeZone;
public Vector3 targetSeek;

Vector3 previousPosition;
Vector3 velocity;
Vector3 acceleration;
Vector3 seekForce;
Vector3 fleeForce;

float wanderAngle = 15.0f;

private float maxSpeed;
private float maxForce;	

float seekWeight;
float fleeWeight;
float wanderWeight;

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

// Update is called once per frame
void Update () 
{
	if (trolls == null)
	{
		trolls = GameObject.FindGameObjectsWithTag("Trolls");
	}
	
	//Debug.Log("Update is being called!");
	
	DetermineBehaviors();
	UpdateForces();
	calculateVelocity();
}

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
