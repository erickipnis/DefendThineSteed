using UnityEngine;
using System.Collections;

public class WinScreenManager : MonoBehaviour {

	void OnGUI()
	{
		const int buttonWidth = 100;
		const int buttonHeight = 50;
		
		Rect buttonRect = new Rect 
			(Screen.width / 2 - (buttonWidth),
			 (2 * Screen.height / 3) - (buttonHeight / 2) + 50,
			 buttonWidth,
			 buttonHeight);
		
		Rect other = new Rect 
			(Screen.width / 2 + (buttonWidth/2),
			 (2 * Screen.height / 3) - (buttonHeight / 2) + 50,
			 buttonWidth,
			 buttonHeight);
		
		if(GUI.Button(buttonRect,"RESTART"))
		{
			Application.LoadLevel("MainScene");
		}
		if (GUI.Button (other, "END")) 
		{
			Application.Quit ();
		}
		
	}
}
