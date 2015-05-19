using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

// Messed with an enum to avoid magic numbers
// May not be worth it...
public enum Action
{
	wander,
	flee,
	seek,
	error
};

// Made a struct because it's faster/smaller
public struct Observation
{
	public Action action;		// 3 legal choices
	public int trollDistance;	// troll distance
	public int humanDistance;	// human distance
	//public bool windy;			// Windy or not
	public bool takeAction;			// takeAction or not
}

public class BayesScript
{
	// Calculate the constant once
	public static double sqrt2PI = Math.Sqrt(2.0 * Math.PI);
	
	// List of observations.  Initialized from the data file
	// Added to with new observations during program run
	public static List<Observation> obsTab = new List<Observation> ();
	
	// All the little tables to store the counts, proportions,
	// sums, sums of squares, means, and standard deviations
	// for the 4 conditions and the action.  Used doubles for the proportions,
	// means and standard deviations to mitigate roundoff errors for products
	// of small probabilities.
	public static int [,] actionCt = new int[3,2];			// Action condition (discrete)
	public static double [,] actionPrp = new double[3,2];
	//int[] actionCt = new int[3];			// Action condition (discrete)
	//double[] actionPrp = new double[3];
	
	public static int [] trollDistanceSum = new int[2];				// trollDistanceerature condition (continuous)
	public static double [] trollDistanceMean = new double[2];
	public static double [] trollDistanceStdDev = new double[2];
	public static int [] trollDistanceSumSq = new int[2];
	
	public static int [] humanDistanceSum = new int[2];				// humanDistanceity condition (continuous)
	public static int [] humanDistanceSumSq = new int[2];
	public static double [] humanDistanceMean = new double[2];
	public static double [] humanDistanceStdDev = new double[2];
	
	//int [,] windyCt = new int[2,2];				// Windy condition (Boolean)
	//double [,] windyPrp = new double[2,2];
	
	public static int[] takeActionCt = new int[2];					// takeAction (Boolean)
	public static double[] takeActionPrp = new double[2];

	public static string filePath = "./BayesTheromData/steedBayes.txt";

	public static double wanderYesOdds;
	public static double wanderNoOdds;
	public static double fleeYesOdds;
	public static double fleeNoOdds;
	public static double seekYesOdds;
	public static double seekNoOdds;

	public static double totalYes;
	public static double wanderTotalOdds;
	public static double fleeTotalOdds;
	public static double seekTotalOdds;
	
	public BayesScript ()
	{
	}
	
	public static void LoadAndBuildData()
	{
		// Read the table in and build the stats
		ReadObsTab(filePath);
		BuildStats();
	}

	public static void GetBayesOdds(int tDistance, int pDistance)
	{		
		double wanderYes = CalcBayes(Action.wander, tDistance, pDistance, true);
		double wanderNo = CalcBayes(Action.wander, tDistance, pDistance, false);
		double wanderYesNno = wanderYes + wanderNo;
		
		wanderYesOdds = wanderYes / wanderYesNno;
		wanderNoOdds = wanderNo / wanderYesNno;
		
		Debug.Log (wanderYes + " " + wanderNo + "  Wander Yes: " +
		                   wanderYesOdds + "  Wander No:" + wanderNoOdds);
		
		// Test fleeing scenario
		double fleeYes = CalcBayes(Action.flee, tDistance , pDistance, true);
		double fleeNo = CalcBayes(Action.flee, tDistance, pDistance, false);
		double fleeYesNno = fleeYes + fleeNo;
		
		fleeYesOdds = fleeYes / fleeYesNno;
		fleeNoOdds = fleeNo / fleeYesNno;
		
		Debug.Log(fleeYes + " " + fleeNo + "  Flee Yes: " +
		                  fleeYesOdds + "  Flee No:" + fleeNoOdds);
		
		// Test seeking scenario
		double seekYes = CalcBayes(Action.seek, tDistance, pDistance, true);
		double seekNo = CalcBayes(Action.seek, tDistance, pDistance, false);
		double seekYesNno = seekYes + seekNo;
		
		seekYesOdds = seekYes / seekYesNno;
		seekNoOdds = seekNo / seekYesNno;
		
		Debug.Log(seekYes + " " + seekNo + "  Seek Yes: " +
		                  seekYesOdds + "  Seek No:" + seekNoOdds);	

		totalYes = wanderYesOdds + fleeYesOdds + seekYesOdds;
		
		wanderTotalOdds = (wanderYesOdds / totalYes) * 100;
		fleeTotalOdds = (fleeYesOdds / totalYes) * 100;
		seekTotalOdds = (seekYesOdds / totalYes) * 100;
	}
	
	public static void DumpToFile()
	{
		// Add this case to the observation List using the predicted actions if "successful"
		//AddObs(distance, wandering, seeking, seek);
		DumpTab();
		
		StreamWriter writer = new StreamWriter(filePath);
		
		for (int i = 0; i < obsTab.Count; i++)
		{
			writer.WriteLine(obsTab[i].action.ToString() + ' ' + obsTab[i].trollDistance.ToString() + ' ' +
			                 obsTab[i].humanDistance.ToString() + ' ' + obsTab[i].takeAction.ToString());
		}
		
		writer.Close();
	}
	
	public static void ReadObsTab (string fName)
	{
		try {
			using (StreamReader rdr = new StreamReader (fName))
			{
				string lineBuf = null;
				while ((lineBuf = rdr.ReadLine ()) != null)
				{
					string[] lineAra = lineBuf.Split (' ');
					
					// Map strings to correct data types for conditions & action
					// and Add the observation to List obsTab
					//AddObs(MapAction ( lineAra[0]), int.Parse(lineAra[1]),
					//    int.Parse(lineAra[2]), (lineAra[3] == "True" ? true : false),
					//    (lineAra[4] == "True" ? true : false) );
					AddObs(MapAction(lineAra[0]), int.Parse(lineAra[1]),
					       int.Parse(lineAra[2]), (lineAra[3] == "True" ? true : false));
				}
			}
		} catch
		{
			Debug.Log ("Problem reading and/or parsing observation file");
			//Environment.Exit (-1);
		}
	}
	
	//public void AddObs(Action action, int trollDistance, int humanDistance,
	//    bool windy, bool flee)
	
	public static void AddObs(Action action, int trollDistance, int humanDistance, bool takeAction)
	{
		// Build an Observation struct
		Observation obs;
		obs.action = action;
		obs.trollDistance = trollDistance;
		obs.humanDistance = humanDistance;
		//obs.windy = windy;
		obs.takeAction = takeAction;
		
		// Add it to the List
		obsTab.Add (obs);
	}
	
	// Maps string to enum name
	public static Action MapAction (string s)
	{
		switch (s)
		{
		case "wander" :
			return Action.wander;
		case "flee" :
			return Action.flee;
		case "seek" :
			return Action.seek;
		default :
			Debug.Log ("Problem in converting Action");
			return Action.error;
		}
	}
	
	// Dump obsTab to the Console for debugging purposes
	public static void DumpTab ()
	{
		foreach (Observation obs in obsTab)
		{
			Debug.Log (obs.action);
			Debug.Log (" " + obs.trollDistance);
			Debug.Log (" " + obs.humanDistance);
			//Debug.Log(" " + obs.windy);
			Debug.Log(" " + obs.takeAction);
		}
	}
	
	// Build all the statistics need for Bayes from the observations
	// in obsTab.  Presumably, this would be called during initialization
	// and not after every new observation has been added durng game flee,
	// as it does a lot of crunching on doubles.  With a small obsTab
	// this may not be much of an issue, but as it grows O(n) with size of
	// obsTab, it could pork out with a lot of new observations added during
	// game flee.
	public static void BuildStats()
	{
		// Accumulate all the counts
		foreach (Observation obs in obsTab)
		{
			// Do this once
			int takeActionOff = obs.takeAction ? 0 : 1;
			
			actionCt[(int)obs.action, takeActionOff]++;
			
			trollDistanceSum[takeActionOff] += obs.trollDistance;
			trollDistanceSumSq[takeActionOff] += obs.trollDistance*obs.trollDistance;
			
			humanDistanceSum[takeActionOff] += obs.humanDistance;
			humanDistanceSumSq[takeActionOff] += obs.humanDistance*obs.humanDistance;
			
			//windyCt[obs.windy?0:1,takeActionOff]++;
			
			takeActionCt[takeActionOff]++;
		}
		
		// Calculate the statistics
		CalcProps(actionCt, takeActionCt, actionPrp);
		
		trollDistanceMean[0] = Mean(trollDistanceSum[0],takeActionCt[0]);
		trollDistanceMean[1] = Mean(trollDistanceSum[1],takeActionCt[1]);
		trollDistanceStdDev[0] = StdDev(trollDistanceSumSq[0],trollDistanceSum[0],takeActionCt[0]);
		trollDistanceStdDev[1] = StdDev(trollDistanceSumSq[1],trollDistanceSum[1],takeActionCt[1]);
		
		humanDistanceMean[0] = Mean(humanDistanceSum[0],takeActionCt[0]);
		humanDistanceMean[1] = Mean(humanDistanceSum[1],takeActionCt[1]);
		humanDistanceStdDev[0] = StdDev(humanDistanceSumSq[0],humanDistanceSum[0],takeActionCt[0]);
		humanDistanceStdDev[1] = StdDev(humanDistanceSumSq[1],humanDistanceSum[1],takeActionCt[1]);
		
		//CalcProps(windyCt, takeActionCt, windyPrp);
		
		takeActionPrp[0] = (double)takeActionCt[0] / obsTab.Count;
		takeActionPrp[1] = (double)takeActionCt[1] / obsTab.Count;
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
					props[i,j] = 0.1d/takeActionCt[j];	// Can't have 0
		else
			props[i,j] = (double)counts[i,j] / n[j];
	}
	
	public static double Mean (int sum, int n)
	{
		return (double)sum / n;
	}
	
	public static double StdDev(int sumSq, int sum, int n)
	{
		return Math.Sqrt((sumSq - (sum*sum)/(double)n) / (n-1));
	}
	
	// Calculates probability of x in a normal distribution of
	// mean and stdDev.  This corrects a mistake in the pseudo-code,
	// used a power function instead of an exponential.
	public static double GauProb (double mean, double stdDev, int x)
	{
		double xMinusMean = x - mean;
		return (1.0d / (stdDev*sqrt2PI)) * 
			Math.Exp(-1.0d*xMinusMean*xMinusMean / (2.0d*stdDev*stdDev));
	}
	
	/*********************************************************/
	
	// Bayes likelihood for four condition values and one action value
	// For each possible action value, call this with a specific set of four
	// condition values, and pick the action that returns the highest
	// likelihood as the most likely action to take, given the conditions.
	//public double CalcBayes(Action action, int trollDistance, int humanDistance,
	//    bool windy, bool flee)
	
	public static double CalcBayes(Action action, int trollDistance, int humanDistance, bool takeAction)
	{
		int takeActionOff = takeAction ? 0 : 1;
		double like = actionPrp[(int)action,takeActionOff] *
			GauProb(trollDistanceMean[takeActionOff],trollDistanceStdDev[takeActionOff],trollDistance) *
				GauProb(humanDistanceMean[takeActionOff],humanDistanceStdDev[takeActionOff],humanDistance) *
				//windyPrp[windy?0:1,takeActionOff] *
				takeActionPrp[takeActionOff];
		return like;
	}
	
	// Dump all the statistics to the Console for debugging purposes
	public static void DumpStats()
	{
		Debug.Log ("Action ");
		for (int i = 0; i < 3; i++)
			for (int j = 0; j < 2; j++)
				Debug.Log(actionCt[i,j]+" "+actionPrp[i,j]+" ");
		
		Debug.Log ("trollDistance ");
		Debug.Log (trollDistanceSum[0]+" "+trollDistanceSum[1]+" ");
		Debug.Log (trollDistanceSumSq[0]+" "+trollDistanceSumSq[1]+" ");
		Debug.Log (trollDistanceMean[0]+" "+trollDistanceMean[1]+" ");
		Debug.Log (trollDistanceStdDev[0]+" "+trollDistanceStdDev[1]);
		
		Debug.Log ("humanDistance ");
		Debug.Log (humanDistanceSum[0]+" "+humanDistanceSum[1]+" ");
		Debug.Log (humanDistanceSumSq[0]+" "+humanDistanceSumSq[1]+" ");
		Debug.Log (humanDistanceMean[0]+" "+humanDistanceMean[1]+" ");
		Debug.Log (humanDistanceStdDev[0]+" "+humanDistanceStdDev[1]);
		
		//Debug.Log ("Windy ");
		//for (int i = 0; i < 2; i++)
		//    for (int j = 0; j < 2; j++)
		//        Debug.Log(windyCt[i,j]+" "+windyPrp[i,j]+" ");
		//Debug.Log ();
		
		Debug.Log ("takeAction ");
		Debug.Log (takeActionCt[0]+" "+takeActionPrp[0]+" ");
		Debug.Log (takeActionCt[1]+" "+takeActionPrp[1]);
	}
}
