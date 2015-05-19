using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	void OnGUI()
	{
		const int buttonWidth = 100;
		const int buttonHeight = 75;

		Rect resetButton = new Rect (Screen.width - (buttonWidth + 1), Screen.height -
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
			GeneticAlgorithm.ShutdownGA();
		}
	}

	void Update()
	{
		if (Save.score + Loss.score == PrefabGenerator.steeds) 
		{
			TrollGeneticAlgorithm.ShutdownGA();
			TrollGeneticAlgorithm.WritePhenotypes();
			TrollGeneticAlgorithm.Reset();

			SteedGeneticAlgorithm.ShutdownGA();
			SteedGeneticAlgorithm.WritePhenotypes();
			SteedGeneticAlgorithm.Reset();

			BayesScript.DumpToFile();

			Application.Quit();

			if(Save.score > Loss.score)
			{
				Application.LoadLevel ("WinScene");
			}
			if(Save.score < Loss.score)
			{
				Application.LoadLevel("LoseScene");
			}
		}
	}
}