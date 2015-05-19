using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	void OnGUI()
	{
		const int buttonWidth = 100;
		const int buttonHeight = 75;

		Rect resetButton = new Rect (Screen.width - (buttonWidth * 2), Screen.height -
						(buttonHeight + 10), buttonWidth, buttonHeight);

		Rect endButton = new Rect (Screen.width - buttonWidth, Screen.height - 
						(buttonHeight + 10), buttonWidth, buttonHeight);

		if(GUI.Button (resetButton, "RESTART"))
		{
			//BayesScript.DumpToFile(100, true, false, true);
			TrollGeneticAlgorithm.ShutdownGA();
			TrollGeneticAlgorithm.WritePhenotypes();		
			TrollGeneticAlgorithm.Reset();		

			SteedGeneticAlgorithm.ShutdownGA();
			SteedGeneticAlgorithm.WritePhenotypes();
			SteedGeneticAlgorithm.Reset();

			BayesScript.DumpToFile();

			Application.LoadLevel(Application.loadedLevel);
		}
		if (GUI.Button (endButton, "END"))
		{
			TrollGeneticAlgorithm.ShutdownGA();
			TrollGeneticAlgorithm.WritePhenotypes();
			TrollGeneticAlgorithm.Reset();

			SteedGeneticAlgorithm.ShutdownGA();
			SteedGeneticAlgorithm.WritePhenotypes();
			SteedGeneticAlgorithm.Reset();

			BayesScript.DumpToFile();

			Application.Quit();
		}
	}
}