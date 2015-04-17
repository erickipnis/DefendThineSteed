using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public static GameManager instance = null;
	private int level = 0;

	void Awake(){
		if (instance == null) {
			instance = this;
		} 
		else if (instance != this) {
			Destroy (gameObject);
			DontDestroyOnLoad (gameObject);
			InitGame ();
		}
	}
	// Use this for initialization
	void InitGame () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
