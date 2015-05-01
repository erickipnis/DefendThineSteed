using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public struct Observation
{			
	public int distance;		// troll or steed distance
	public bool wandering;      // Currently wandering or not
	public bool seeking;        // Currently seeking or not
	public bool seek;			// Start to seek, continue to seek or don't seek
}

public class BayesScript : MonoBehaviour 
{
	public static string filePath = "./BayesTheromData/trollData.txt";

	// Calculate the constant once
	public static float sqrt2PI = Mathf.Sqrt(2.0f * Mathf.PI);
	
	// List of observations.  Initialized from the data file
	// Added to with new observations during program run
	public static List<Observation> obsTab = new List<Observation> ();

	public static double yesSeekOdds;
	public static double noSeekOdds;
	
	// All the little tables to store the counts, proportions,
	// sums, sums of squares, means, and standard deviations
	// for the 4 conditions and the action.  Used doubles for the proportions,
	// means and standard deviations to mitigate roundoff errors for products
	// of small probabilities.
	
	public static int [] distanceSum = new int[2];				// Distance condition (constant)
	public static double [] distanceMean = new double[2];
	public static double [] distanceStdDev = new double[2];
	public static int [] distanceSumSq = new int[2];
	
	public static int[,] wanderingCt = new int[2, 2];				// wandering condition (Boolean)
	public static double[,] wanderingPrp = new double[2, 2];
	
	public static int[,] seekingCt = new int[2, 2];				// seeking condition (Boolean)
	public static double[,] seekingPrp = new double[2, 2];
	
	public static int [] seekCt = new int[2];					// Seek action (Boolean) This is what we're trying to determine.
	public static double [] seekPrp = new double[2];

	// Use this for initialization
	void Start () 
	{

	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	public static void StartBayes()
	{
		// Start with the table from the example
		ReadObsTab(filePath);
		//DumpTab();
		BuildStats();
		//DumpStats();
		
		// Test to see if distance is 100, is wandering, not seeking currently
		double seekYes = CalcBayes(100, true, false, true);
		double seekNo =  CalcBayes(100, true, false, false);
		double yesNno = seekYes + seekNo;
		
		// TODO: PASS THESE VALUES TO DETERMINEBEHAVIOURS TO CALCULATE RANDOM TO THEN SEEK OR WANDER
		// ALSO HAVE TO THEN WRITE ORIGINAL VALUES BACK OUT TO FILE UPON SUCCESS SOMEHOW
		// ONLY TRY TO MAKE A DECISION ONE TIME PER SECOND
		yesSeekOdds = seekYes / yesNno;
		noSeekOdds = seekNo / yesNno;
		
		Debug.Log(seekYes + " " + seekNo + " " +
		          yesSeekOdds + " " + noSeekOdds);		
		
//		// Rebuild the statistics
//		BuildStats();
//		DumpStats();
//		
//		// Test again to see if distance is 100, is wandering, not seeking currently is now more likely
//		seekYes = CalcBayes(100, true, false, true);
//		seekNo = CalcBayes(100, true, false, false);
//		yesNno = seekYes + seekNo;
//		
//		yesSeekOdds = seekYes / yesNno;
//		noSeekOdds = seekNo / yesNno;
//		
//		Debug.Log(seekYes + " " + seekNo + " " +
//		          yesSeekOdds + " " + noSeekOdds);
	}

	public static void DumpToFile(int distance, bool wandering, bool seeking, bool seek)
	{
		// Add this case to the observation List using the predicted actions if "successful"
		//AddObs(distance, wandering, seeking, seek);
		DumpTab();

		StreamWriter writer = new StreamWriter(filePath);

		for (int i = 0; i < obsTab.Count; i++)
		{
			writer.WriteLine(obsTab[i].distance.ToString() + ' ' + obsTab[i].wandering.ToString() + ' ' +
			                 obsTab[i].seeking.ToString() + ' ' + obsTab[i].seek.ToString());
		}

		writer.Close();
	}
	
	public static void ReadObsTab (string fName)
	{
		try 
		{
			using (StreamReader rdr = new StreamReader (fName))
			{
				string lineBuf = null;
				while ((lineBuf = rdr.ReadLine ()) != null)
				{
					string[] lineAra = lineBuf.Split (' ');
					
					// Map strings to correct data types for conditions & action
					// and Add the observation to List obsTab
					AddObs(int.Parse(lineAra[0]), 
					       (lineAra[1] == "True" ? true : false),
					       (lineAra[2] == "True" ? true : false),
					       (lineAra[3] == "True" ? true : false) );
				}

				rdr.Close();
			 }
		}
		catch (IOException e)
		{
			Debug.Log (e.Message);
		}
	}
	
	public static void AddObs(int distance, bool wandering,
	                   bool seeking, bool seek)
	{
		// Build an Observation struct
		Observation obs;
		obs.distance = distance;
		obs.wandering = wandering;
		obs.seeking = seeking;
		obs.seek = seek;
		
		// Add it to the List
		obsTab.Add (obs);
	}
	
	// Dump obsTab to the Console for debugging purposes
	public static void DumpTab ()
	{
		foreach (Observation obs in obsTab)
		{
			Debug.Log (" " + obs.distance);
			Debug.Log (" " + obs.wandering);
			Debug.Log (" " + obs.seeking);
			Debug.Log (" " + obs.seek);
		}
	}
	
	// Build all the statistics need for Bayes from the observations
	// in obsTab.  Presumably, this would be called during initialization
	// and not after every new observation has been added durng game play,
	// as it does a lot of crunching on doubles.  With a small obsTab
	// this may not be much of an issue, but as it grows O(n) with size of
	// obsTab, it could pork out with a lot of new observations added during
	// game play.
	public static void BuildStats()
	{
		// Accumulate all the counts
		foreach (Observation obs in obsTab)
		{
			// Do this once
			int seekOff = obs.seek ? 0 : 1;
			
			distanceSum[seekOff] += obs.distance;
			distanceSumSq[seekOff] += obs.distance*obs.distance;
			
			wanderingCt[obs.wandering ? 0 : 1, seekOff]++;
			seekingCt[obs.seeking ? 0 : 1, seekOff]++;
			
			seekCt[seekOff]++;
		}
		
		// Calculate the statistics				
		distanceMean[0] = Mean(distanceSum[0],seekCt[0]);
		distanceMean[1] = Mean(distanceSum[1],seekCt[1]);
		distanceStdDev[0] = StdDev(distanceSumSq[0],distanceSum[0],seekCt[0]);
		distanceStdDev[1] = StdDev(distanceSumSq[1],distanceSum[1],seekCt[1]);
		
		CalcProps(wanderingCt, seekCt, wanderingPrp);
		CalcProps(seekingCt, seekCt, seekingPrp);
		
		seekPrp[0] = (double)seekCt[0] / obsTab.Count;
		seekPrp[1] = (double)seekCt[1] / obsTab.Count;
	}
	
	/******************************************************/
	// Standard statistical functions.  These should be useful without modification.
	
	// Calculates the proportions for a discrete table of counts
	// Handles the 0-frequency problem by assigning an artificially
	// low value that is still greater than 0.
	public static void CalcProps (int[,] counts, int[] n, double[,] props)
	{
		for (int i = 0; i < counts.GetLength(0); i++)
			for (int j = 0; j < counts.GetLength(1); j++)
				// Detects and corrects a 0 count by assigning a proportion
				// that is 1/10 the size of a proportion for a count of 1
				if (counts[i,j] == 0)
					props[i,j] = 0.1d/seekCt[j];	// Can't have 0
		else
			props[i,j] = (double)counts[i,j] / n[j];
	}
	
	public static double Mean (int sum, int n)
	{
		return (double)sum / n;
	}
	
	public static double StdDev(float sumSq, float sum, float n)
	{
		return Mathf.Sqrt((sumSq - (sum * sum)/(float) n) / (n-1.0f));
	}
	
	// Calculates probability of x in a normal distribution of
	// mean and stdDev.  This corrects a mistake in the pseudo-code,
	// used a power function instead of an exponential.
	public static double GauProb (float mean, float stdDev, float x)
	{
		float xMinusMean = x - mean;

		return (1.0f / (stdDev*sqrt2PI)) * 	Mathf.Exp(-1.0f * xMinusMean * xMinusMean / (2.0f*stdDev*stdDev));
	}
	
	/*********************************************************/
	
	// Bayes likelihood for four condition values and one action value
	// For each possible action value, call this with a specific set of four
	// condition values, and pick the action that returns the highest
	// likelihood as the most likely action to take, given the conditions.
	public static double CalcBayes(int distance, bool wandering,
	                        bool seeking, bool seek)
	{
		int seekOff = seek? 0 : 1;
		double like = GauProb((float)distanceMean[seekOff], (float)distanceStdDev[seekOff], (float)distance) *
			wanderingPrp[wandering ? 0 : 1, seekOff] *
				seekingPrp[seeking ? 0 : 1, seekOff] *
				seekPrp[seekOff];
		
		return like;
	}
	
	// Dump all the statistics to the Console for debugging purposes
	public static void DumpStats()
	{				
		Debug.Log ("Distance ");
		Debug.Log (distanceSum[0]+" "+distanceSum[1]+" ");
		Debug.Log (distanceSumSq[0]+" "+distanceSumSq[1]+" ");
		Debug.Log (distanceMean[0]+" "+distanceMean[1]+" ");
		Debug.Log (distanceStdDev[0]+" "+distanceStdDev[1]);
						
		Debug.Log ("Wandering ");
		for (int i = 0; i < 2; i++)
			for (int j = 0; j < 2; j++)
				Debug.Log(wanderingCt[i,j]+" "+wanderingPrp[i,j]+" ");
		
		Debug.Log("Seeking ");
		for (int i = 0; i < 2; i++)
			for (int j = 0; j < 2; j++)
				Debug.Log(seekingCt[i, j] + " " + seekingPrp[i, j] + " ");
		
		Debug.Log ("Seek ");
		Debug.Log(seekCt[0]+" "+seekPrp[0]+" ");
		Debug.Log (seekCt[1]+" "+seekPrp[1]);
	}
}
