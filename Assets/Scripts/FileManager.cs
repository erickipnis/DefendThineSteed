using UnityEngine;
using System.Collections;
using System.IO;

public class FileManager : MonoBehaviour 
{
	public static StreamReader reader;
	public static StreamWriter writer;

	// Use this for initialization
	void Start () 
	{

	}

	// Update is called once per frame
	void Update () 
	{
	
	}

	public static void WriteData(float[] data)
	{
//		writer = new StreamWriter("./BayesTheromData/trollData.txt");
//		writer.WriteLine("Test2");
//		writer.Close();
	}
}
