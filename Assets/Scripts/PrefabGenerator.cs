using UnityEngine;
using System.Collections;

public class PrefabGenerator : MonoBehaviour {

	// Use this for initialization
	void Start() 
	{
		GameObject trollPrefab = (GameObject)Resources.Load ("Troll");
		GameObject steedPrefab = (GameObject)Resources.Load ("Steed");
		
		for (int i = 0; i < 5; i++)
		{
			Vector3 position = new Vector3(Random.Range(50, 450), 0.0f, Random.Range(50, 450));
			
			Instantiate (trollPrefab, position, Quaternion.identity);
		}
		
		for (int j = 0; j < 3; j++)
		{
			Vector3 position = new Vector3(Random.Range(50, 450), 0.0f, Random.Range(50, 450));
			
			Instantiate (steedPrefab, position, Quaternion.identity);
		}
		
		GeneticAlgorithm.SetUpGA();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
