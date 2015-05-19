using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class SteedGeneticAlgorithm 
{	
	static int popSize = 5;				// Population size
	static int chromLeng = 8;              // Number of bits in a chromosome
	static int nChromVals = 1 << chromLeng; // Number of values for that many bits
	
	static ThreshPop tp;
	public static uint[] chroms;
	public static float[] phenos;
	public static float[] fitnessArray;
	public static float[] shelterBonuses;
	
	//public static Dictionary<uint, float> chromFitnessDictionary;
	//public static Dictionary<uint, float> chromPhenoDictionary;
	
	public static int index;
	//public static int currentCheckedInIndex;
	
	//public static float[] phenotypeArray; 
	
	public static void SetUpGA()
	{
		//chromFitnessDictionary = new Dictionary<uint, float>();
		//chromPhenoDictionary = new Dictionary<uint, float>();
		index = 0;
		
		// Create the population, either from the file or from scratch
		// Presumably the popSize would be the number of NPCs that will be
		// spawned for a round.  The data file name is set here as well by
		// passing it into the constructor.
		tp = new ThreshPop(chromLeng, popSize, "./GeneticAlgorithmData/steedData.txt");	 
		
		// Local storage for the chromosomes and fitness values to demonstrate
		// how the ThreshPop is used.  In this case, we'll just store an array
		// of chromosomes to represent the checked out population and manipulate
		// them in simple loops to make something happen.
		// In your game, a given threshold would be an attribute of an NPC,
		// and the fitness would be determined when that NPC is "done"
		chroms = new uint[popSize];
		
		phenos = new float[popSize];
		
		fitnessArray = new float[popSize];
		
		shelterBonuses = new float[popSize];
		
		
		
		// Check out all the individuals from the population to get their chroms
		// A CheckOut would be done when an NPC is spawned, one at a time
		int i = 0;
		
		while (! tp.AllCheckedOut())
		{	
			chroms[i] = tp.CheckOut();
			//Debug.Log(chroms[i]);
			
			phenos[i] = Gen2Phen(chroms[i]);
			
			fitnessArray[i] = 0.0f;
			
			shelterBonuses[i] = 0.0f;
			//chromPhenoDictionary.Add(chroms[i], Gen2Phen(chroms[i]));
			//chromFitnessDictionary.Add(chroms[i], (float)Util.rand.Next(20));
			
			i++;
		}
		
	}
	
	public static void ShutdownGA()
	{		
		// check out all remaining if they were not checked out during the game (game ended too soon)
		int i = 0;
		while (! tp.AllCheckedIn())
		{
			int fit = Fitness(fitnessArray[i], i);
			tp.CheckIn(chroms[i], fit);
			i++;
		}
		
		// Save the new population for next time
		// This would be done at the end of each "round"
		tp.WritePop();
	}
	
	public static void Reset()
	{
		//chromFitnessDictionary.Clear ();
		//chromPhenoDictionary.Clear ();
		index = 0;
	}
	
	//	public static void CheckInIndividual(float fitness, uint chrom)
	//	{
	//		if (! tp.AllCheckedIn())
	//		{
	//			int fit = Fitness(fitness);
	//			tp.CheckIn(chrom, fit);
	//		}
	//	}
	
	public static void WritePhenotypes()
	{
		StreamWriter outStream = new StreamWriter ("./GeneticAlgorithmData/steedDistances.txt");
		
		for (int i = 0; i < phenos.Length; i++)
		{
			outStream.WriteLine("Geno: " + chroms[i].ToString() + ", SteedFleeDistance: " + phenos[i].ToString());
		}
		
		outStream.Close();
	}
	
	// Use this for initialization
	void Start () 
	{		
		
	}		
	
	// Update is called once per frame
	void Update () {
		
	}
	
	/* ThreshPop - Highest level class for the threshold GA package
	 * Implements a single generation of evolution with an old population from
	 * a previous generation (or the initial generation 0), and a new population
	 * created for this generation. Provides all the methods called directly to
	 * implement a threshold population from a calling class.
	 * Uses the Population class to implement its two internal populations
	 * (oldP and newP). Uses the Individual class as well for handling single
	 * Individuals. 
	 */
	public class ThreshPop
	{
		int popSize;		// Number of Individuals in population
		int chromSize;		// Length of chromosome for each individual
		Population oldP;	// Old population read from file or generated randomly
		Population newP;	// New population filled  as Individuals get fitness
		string popPath;		// String for data file path name (in Bin/Debug folder)
		int nextCOut = 0;	// Counter for number of Individuals checked out of oldP
		int nextCIn = 0;	// Counter for number of Individuals checked into of newP
		bool isGeneration0 = false;	// Assume there's a data file from a previous run
		
		// Constructor sets up old and new populations
		public ThreshPop (int cSize, int size, string path)
		{
			popSize = size;
			chromSize = cSize;
			popPath = path;
			oldP = new Population (popSize, chromSize);	// Old population for check out
			FillPop();
			newP = new Population (popSize, chromSize);	// New population for check in
		}
		
		// Fill oldP either from data file or from scratch (new, random)
		void FillPop ()
		{
			StreamReader inStream = null;	// Open file if it's there
			try
			{
				inStream = new StreamReader(popPath);
				oldP.ReadPop(inStream);		// File opened so read it
				inStream.Close();
			}
			catch
			{
				oldP.InitPop();			// File didn't open so fill with newbies
				isGeneration0 = true;	// Set flag to show it's generation 0
			}
		}
		
		public void WritePop()
		{
			StreamWriter outStream = new StreamWriter(popPath, false);
			newP.WritePop(outStream);
			outStream.Close();
		}
		
		// Display either oldP (0) or newP (1) on Console window
		public void DisplayPop(int which)
		{
			if (which == 0)
				oldP.DisplayPop();
			else
				newP.DisplayPop();
		}
		
		// Check out an individual to use for a threshold in an NPC
		public uint CheckOut ()
		{
			if (isGeneration0)	// Brand new => don't breed
			{
				Individual dude = oldP.GetDude(nextCOut);
				nextCOut++;
				return dude.Chrom;
			}
			else
			{	// Came from file so breed new one
				Individual newDude;
				if (nextCOut == 0)	// First one needs to be Best (elitism)
					newDude = oldP.BestDude();
				else
					newDude = oldP.BreedDude();	// Rest are bred
				nextCOut++;						// Count it
				return newDude.Chrom;			// Return its chromosome
			}
		}
		
		// Returns true if we've checked out a population's worth
		public bool AllCheckedOut()
		{
			return nextCOut == popSize;
		}
		
		// Check in an individual that has now acquired a fitness value
		public void CheckIn (uint chr, int fit)
		{
			Individual NewDude = new Individual(chr, chromSize, fit);	// Make Individual
			newP.AddNewInd(NewDude);						// Add to newP
			nextCIn++;										// Count it
			//Console.WriteLine("In CheckIn chr fit: " + chr + " " + fit);
		}
		
		// Returns true if newP is full of checked in Individuals
		public bool AllCheckedIn()
		{
			return nextCIn == popSize;
		}
		
	}
	
	/* Population class implements a single population. Used by ThreshPop for both
	 * oldP and newP. 
	 */
	public class Population
	{
		const double CROSSOVER_PROB = 0.9;	// 90% chance of crossover in BreedDude()
		int popSize;			// Population size
		int nBits;           	// Number of bits per chromosome
		int nChromVals;      	// Number of different chromosome values (2 to the nBits)
		Individual [] dudes;	// Array of Individuals
		int nDudes = 0;			// Current number of Individuals
		int totFit = 0;      	// Total fitness for all individuals in population
		char[] delim = {' '};	// Used in ReadPop to split input lines
		
		// Constructor sets up an empty population with popN individuals
		//   and chromosome length of cLeng (sets iBits)
		public Population (int popN, int cLeng)
		{
			popSize = popN;
			dudes = new Individual[popSize];
			nDudes = 0;
			nBits = cLeng;
			nChromVals = 1 << nBits;
			totFit = 0;
		}
		
		// Returns true if population is full
		public bool Full
		{
			get { return nDudes == popSize; }
		}
		
		// Fills population with new random chromosomes for generation 0
		public void InitPop()
		{
			for (int i = 0; i < popSize; i++)
			{
				dudes[i] = new Individual ((uint) Util.rand.Next (nChromVals), nBits);
			}
			nDudes = popSize;
			totFit = popSize;      // Default fitness for each Individual == 1
		}
		
		// Fills population by reading individuals from a file already opened to inStream
		// Assumes file is correctly formatted with correct number of lines
		public void ReadPop(StreamReader inStream)
		{
			for (int i = 0; i < popSize; i++)
			{
				string line = inStream.ReadLine();		// Read a line
				string [] tokens = line.Split (delim);	// Split into "words"
				uint chr = UInt32.Parse(tokens[0]);		// Convert words to numbers
				int fit = int.Parse(tokens[1]);
				dudes [i] = new Individual (chr, nBits, fit); // Put Individual in population
				totFit += fit;							// Accumulate total fitness for selection
			}
			nDudes = popSize;							// Show the population full
		}
		
		// Write the population out to a data file that can be read by ReadPop
		public void WritePop(StreamWriter outStream)
		{
			for (int i = 0; i < nDudes; i++)
			{
				outStream.WriteLine (dudes[i]);
			}
		}
		
		// Display the Population on the Console
		public void DisplayPop()
		{
			for (int i = 0; i < nDudes; i++)
			{
				//Console.WriteLine (dudes [i]);
			}
			//Console.WriteLine ();
		}
		
		// Breed a new Individual using crossover and mutation
		public Individual BreedDude()
		{
			Individual p1 = Select ();					// Get 2 parents
			Individual p2 = Select ();
			uint c1 = p1.Chrom;							// Extract their chromosomes
			uint c2 = p2.Chrom;
			
			if (Util.rand.NextDouble () < CROSSOVER_PROB) // Probably do crossover
			{
				uint kidChrom = CrossOver (c1, c2);		// Make new chromosome
				Individual newDude = new Individual (kidChrom, nBits); // Make Individual
				newDude.Mutate ();						// Maybe mutate a bit
				return newDude;							// Send it back
			}
			else
				// No crossover => Pick one of the parents to return unchanged
				return (Util.rand.NextDouble() < 0.5 ? p1 : p2);
		}
		
		// Roulette-wheel selection selects in linear proportion to fitness
		// Uses totFit, which was accumulated when population was filled
		public Individual Select()
		{
			// Roll a random integer from 0 to totFit - 1
			int roll = Util.rand.Next (totFit);
			
			// Walk through the population accumulating fitness
			int accum = dudes[0].Fitness;	// Initialize to the first one
			int iSel = 0;
			// until the accumulator passes the rolled value
			while (accum <= roll && iSel < nDudes-1)
			{
				iSel++;
				accum += dudes[iSel].Fitness;
			}
			// Return the Individual where we stopped
			return dudes[iSel];
		}
		
		// Find the best (highest fitness) Individual in the population
		// Used to implement elitism => best of old Pop survives in new
		public Individual BestDude ()
		{
			// Initialize to the first Individual in the array
			int whereBest = 0;			// Initialze to the first one
			int bestFit = dudes[0].Fitness;
			
			// Walk through the rest to get the overall best one
			for (int i = 1; i < nDudes; i++)
				if (dudes [i].Fitness > bestFit)
			{
				whereBest = i;
				bestFit = dudes [i].Fitness;
			}
			return dudes[whereBest];
		}
		
		// Add a new Individual to the population in the next open spot
		public int AddNewInd (Individual newDude)
		{
			int wherePut = -1;			// -1 in case something breaks
			if (Full)
				Debug.Log("Panic!  Tried to add too many dudes");
			else
			{
				wherePut = nDudes;
				dudes[wherePut] = newDude;
				nDudes++;				// Increment for next time
			}
			return wherePut;			// Return offset in array where it landed
		}
		
		// Get Individual at offset where in the array
		public Individual GetDude (int where)
		{
			return dudes [where];
		}
		
		// Set fitness of Individual at offset where to fitVal
		public void SetFitness (int where, int fitVal)
		{
			dudes[where].Fitness = fitVal;
		}
		
		// Single-point crossover of two parents, returns new kid
		// Uses bit shift tricks to get each parent's contribution on opposite
		// sides of random crossover point
		uint CrossOver(uint p1, uint p2)
		{
			int xOverPt = Util.rand.Next (0, nBits);	// Pick random crossover point
			p1 = (p1 >> xOverPt) << xOverPt;			// Get p1's bits to the left
			p2 = (p2 << (32 - xOverPt)) >> (32 - xOverPt); // p2's to the right
			uint newKid = p1 | p2;						// Or them together
			return newKid;
		}
	}
	
	/* Individual class implements a single Individual from a population.
	 * In addition to being the base type for the Population array, it is
	 * used to pass Individuals around in Population and ThreshPop, but it
	 * is not used by the Main method.  Contains the mutation operator, since
 	 * mutation only involves a single individual.
	 */
	public class Individual
	{
		const float MUT_PROB = 0.2f;	// Mutation probability
		int fitness;
		uint chrom;		// up to 32-bit chromosome with an unsigned integer
		int nBits;		// Number of bits actually used (starting w/ least sig)
		
		// Called by InitPop() with no fitness value
		public Individual (uint newChrom, int nB)
		{
			chrom = newChrom;
			nBits = nB;
			fitness = 1;	// Default fitness must be non-zero
		}
		
		// Overload called by ReadPop() with fitness from last generation
		public Individual (uint newChrom, int nB, int fit)
		{
			chrom = newChrom;
			nBits = nB;
			fitness = fit;
		}
		
		// Getters and a setter
		public uint Chrom
		{
			get { return this.chrom; }
		}
		
		public int Fitness
		{
			get { return this.fitness; }
			set { this.fitness = value; }
		}
		
		// Mutates a random bit MUT_PROB of the time
		public void Mutate ()
		{
			if (Util.rand.NextDouble() < MUT_PROB)
			{
				int mutPt = Util.rand.Next(0, nBits);	// 0 to nBits - 1
				int mutMask = 1 << mutPt;		// Build mask of 1 at mutation point
				chrom = chrom ^ (uint)mutMask;	// xor the mask, which flips that bit
			}
		}
		
		// Make it easier to write an Individual
		public override string ToString()
		{
			return (chrom + " " + fitness);
		}
	}
	
	public static float Gen2Phen (uint gen)
	{
		float lb = 0.0f;			// Lower bound for threshold range in game
		float ub = 300.0f;			// Upper bound
		float step = (ub - lb) / nChromVals;	// Step size for chrom values
		return (gen * step + lb);
	}
	
	public static int Fitness (float fitness, int shelterBonusIndex)
	{
		return (int)(fitness + (2 * shelterBonuses[shelterBonusIndex]) + Util.rand.Next(50));	// S/N = 1:1
	}
}