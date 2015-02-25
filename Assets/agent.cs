using UnityEngine;
using System.Collections;

public class agent : MonoBehaviour {
	public Transform target;
	NavMeshAgent navAgent;
	float remainingDistance;
	// Use this for initialization
	void Start () {
		navAgent = GetComponent<NavMeshAgent> ();
		remainingDistance = navAgent.remainingDistance;
	}
	
	// Update is called once per frame
	void Update () {
		navAgent.SetDestination (target.position);
	if (gameObject.tag == "Troll")
	{
		GameObject[] steeds = GameObject.FindGameObjectsWithTag("Steed");
			for(int i =0; i < steeds.Length; i++)
			{
				Vector3 steedPosition = steeds[i].transform.position;
				float steedDistance = Vector3.Distance(steedPosition, navAgent.transform.position);

				if(remainingDistance > steedDistance)
				{
					navAgent.SetDestination (steedPosition);
				}
			}

	}
	}
}
