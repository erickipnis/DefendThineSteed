using UnityEngine;
using System.Collections;

public class PrefabGenerator : MonoBehaviour {
	public static int steeds;
	// Use this for initialization
	void Start() 
	{
		GameObject trollPrefab = (GameObject)Resources.Load ("Troll");
		GameObject steedPrefab = (GameObject)Resources.Load ("Steed");

		GameObject[] trolls; 
		GameObject[] steeds;
		
		for (int i = 0; i < 5; i++)
		{
			Vector3 position = new Vector3(Random.Range(50, 450), 0.0f, Random.Range(50, 450));
		
			Instantiate (trollPrefab, position, Quaternion.identity);
		}

		steeds = 3;

		for (int j = 0; j < steeds; j++)
		{
			Vector3 position = new Vector3(Random.Range(50, 450), 0.0f, Random.Range(50, 450));
			
			Instantiate (steedPrefab, position, Quaternion.identity);

		}

		trolls = GameObject.FindGameObjectsWithTag("Troll");
		steeds = GameObject.FindGameObjectsWithTag("Steed");

		for (int k = 0; k < 5; k++)
		{
			while (trolls[k].transform.position.y > 0)
			{
				Vector3 position = new Vector3(Random.Range(50, 450), 0.0f, Random.Range(50, 450));

				Destroy(trolls[k]);
				trolls[k] = trollPrefab;
				Instantiate(trolls[k], position, Quaternion.identity); 
			}

			while (steeds[k].transform.position.y > 0)
			{
				Vector3 position = new Vector3(Random.Range(50, 450), 0.0f, Random.Range(50, 450));

				Destroy(steeds[k]);
				steeds[k] = trollPrefab;
				Instantiate(steeds[k], position, Quaternion.identity); 
			}
		}

		TrollGeneticAlgorithm.SetUpGA ();
		SteedGeneticAlgorithm.SetUpGA();

		BayesScript.LoadAndBuildData();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
