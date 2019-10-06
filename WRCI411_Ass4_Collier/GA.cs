using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;

namespace WRCI411_Ass4_Collier
{
    class GA
    {
        private static Random rnd;
        private static double[,] NutInfoArr;
        private static double[] MinNutIntake;

        public GA(double[,] Arr)
        {
            rnd = new Random();
            NutInfoArr = Arr;
            double[] input = { 1095, 25550, 292, 4380, 1825, 657, 985.5, 6570, 27375 }; //average annual minimum nutritional intake for an adult male
            MinNutIntake = input;
            //{Calories (Kcalories), Protein (g), Calcium (g), Iron (mg), Vitamin A (IU), 
            //Thiamine VB1 (mg), Riboflavin VB2 (mg), Niacin (mg), Ascorbic Acid VC (mg)}
        }

        public void RunGA()
        {
            int t = 0; //generation counter initialised to 0
            int ns = 1000; //population of ns chromosones
            int nts = 40; //number of individuals for tournament selection
            double initRange = 10; //its allele is initialised with a value between 0 and (initRange-1) * grams/$
            double pm = 0.2; //mutation probability aka mutation rate
            double pc = 0.9; //crossover probability aka crossover rate
            double[,] population = new double[ns, 77]; //ns individuals with 77 genes each with real value representation
            double[,] offspring = new double[population.GetLength(0) - 1, population.GetLength(1)]; //ns-1 offspring to replace population (elite indiv remains to new population)
            int bestIndiv;
            double curGenFitness;
            double newGenFitness;
            int it_wo_change = 0;
            ArrayList FitnessvsGenerations = new ArrayList(); //used to store Fitness vs Generations data

            population = Initialisation(population, initRange);
            newGenFitness = GenerationalFitness(population, t);
            FitnessvsGenerations.Add(-newGenFitness);
            curGenFitness = newGenFitness;

            while (t < 1110 && it_wo_change <= 100)
            {
                /*if (t < 10)
                    pm = 0.8;
                else if (t < 25)
                    pm = 0.4;
                else if (t < 50)
                    pm = 0.3;
                else if (t < 200)
                    pm = 0.2;
                else
                    pm = 0.1;*/
                offspring = Reproduction(population, nts, pm, pc);
                population = SurvivorSelection(population, offspring);
                newGenFitness = GenerationalFitness(population, t + 1);
                FitnessvsGenerations.Add(-newGenFitness);
                if (Math.Round(newGenFitness, 1) == Math.Round(curGenFitness, 1))
                    it_wo_change++;
                curGenFitness = newGenFitness;
                t++;
            }

            Console.WriteLine("\nBest:");
            bestIndiv = Elitism(population);
            DisplayInfo(bestIndiv, population);

            StreamWriter SW = new StreamWriter("FitnessvsGenerations.csv");
            for (int k = 0; k < FitnessvsGenerations.Count; k++)
            {
                SW.WriteLine((k + 1).ToString() + "," + FitnessvsGenerations[k].ToString());
            }
            SW.Close();
            
            Console.ReadLine();
        }

        private static double[,] Initialisation(double[,] population, double range)
        {
            for (int i = 0; i < population.GetLength(0); i++)
            {
                for (int j = 0; j < population.GetLength(1); j++)
                {
                    population[i, j] = (rnd.NextDouble() * range) * NutInfoArr[j,0];
                    //population[i, j] = (rnd.Next() * range);
                }
            }
            return population;
        }

        public static fitData Fitness(double[] individual)
        {
            int c = 18;
            int s = 2;
            int u = 85;
            int o = 10;
            int over = 0;
            double SSE = 0;
            double[] IndivNutInfo = new double[NutInfoArr.GetLength(1)]; //Cost, Calories, Protein, Calcium, Iron, VitaminA, VB1, VB2, Niacin, VC;
            for (int i = 0; i < individual.Length; i++)
            {
                double MassPerDollar = NutInfoArr[i, 0]; //Mass (g) per dollar of each food type
                IndivNutInfo[0] += Math.Max(individual[i], 0) * (1 / MassPerDollar); //cost
                for (int entry = 1; entry < NutInfoArr.GetLength(1); entry++)
                {
                    IndivNutInfo[entry] += Math.Max(individual[i], 0) * (NutInfoArr[i, entry] / MassPerDollar); //nutrients
                }
            }
            for (int entry = 1; entry < IndivNutInfo.Length; entry++)
            {
                double val = IndivNutInfo[entry] - MinNutIntake[entry - 1];

                if (val < 0)
                {
                    SSE += Math.Pow((u * val) / MinNutIntake[entry - 1], 2);
                }
                else
                    over++;
            }

            fitData fd = new fitData();
            fd.cost = IndivNutInfo[0];
            fd.sse = SSE;
            fd.fitness = ((-c * IndivNutInfo[0]) - s * (SSE) + o * over);
            return fd;
        }

        public static int TournamentSelection(double[,] population, int nts)
        {
            double[] individual = new double[population.GetLength(1)];
            double fitness;
            double bestFit = double.MinValue;
            int bestFitIndex = -1;
            int i;
            int count = 0;
            while (count < nts)
            {
                i = rnd.Next(0, population.GetLength(0));
                for (int j = 0; j < population.GetLength(1); j++)
                {
                    individual[j] = population[i, j];
                }
                fitness = Fitness(individual).fitness;
                if (fitness >= bestFit)
                {
                    bestFit = fitness;
                    bestFitIndex = i;
                }
                count++;
            }
            return bestFitIndex;
        }

        public static double[] UniformCrossover(double[,] population, int parent1, int parent2)
        {
            double[] offspring = new double[population.GetLength(1)];
            double px = 0.5;
            for (int j = 0; j < population.GetLength(1); j++)
            {
                if (rnd.NextDouble() <= px)
                {
                    offspring[j] = population[parent1, j];
                }
                else
                {
                    offspring[j] = population[parent2, j];
                }
            }
            return offspring;
        }

        public static double[] UniformMutation(double[] offspring, double[,] population, double pm)
        {
            int decision;
            double xmaxj;
            double xminj;
            for (int j = 0; j < population.GetLength(1); j++)
            {
                
                if (rnd.NextDouble() <= pm)
                {
                    xmaxj = double.MinValue;
                    xminj = double.MaxValue;
                    decision = rnd.Next(0, 2);
                    if (decision == 0)
                    {
                        for (int i = 0; i < population.GetLength(0); i++)
                        {
                            if (population[i, j] > xmaxj)
                                xmaxj = population[i, j];
                        }
                        offspring[j] -= Scale(rnd.NextDouble(), (xmaxj - offspring[j]), 0, 1, 0);
                    }
                    else if (decision == 1)
                    {
                        for (int i = 0; i < population.GetLength(0); i++)
                        {
                            if (population[i, j] < xminj)
                                xminj = population[i, j];
                        }
                        offspring[j] -= Scale(rnd.NextDouble(), (offspring[j] - xminj), 0, 1, 0);
                    }
                }
            }
            return offspring;
        }

        public static double[,] Reproduction(double[,] population, int nts, double pm, double pc)
        {
            double[,] offspring = new double[population.GetLength(0) - 1, population.GetLength(1)];
            double[] individual = new double[population.GetLength(0)];
            int parent1, parent2;
            int count = 0;
            while (count < population.GetLength(0) - 1)
            {
                parent1 = TournamentSelection(population, nts);
                parent2 = TournamentSelection(population, nts);
                if (rnd.NextDouble() < pc)
                {
                    individual = UniformCrossover(population, parent1, parent2);
                    individual = UniformMutation(individual, population, pm);
                    for (int j = 0; j < population.GetLength(1); j++)
                    {
                        offspring[count, j] = individual[j];
                    }
                    count++;
                }
            }
            return offspring;
        }

        public static double[,] SurvivorSelection(double[,] population, double[,] offspring)
        {
            for (int j = 0; j < offspring.GetLength(1); j++)
            {
                population[population.GetLength(0) - 1, j] = population[Elitism(population), j];
            }

            for (int i = 0; i < offspring.GetLength(0); i++)
            {
                for (int j = 0; j < offspring.GetLength(1); j++)
                {
                    population[i, j] = offspring[i, j];
                }
            }

            return population;
        }

        public static int Elitism(double[,] population)
        {
            double[] individual = new double[population.GetLength(1)];
            double fitness;
            double bestFit = double.MinValue;
            int bestFitIndex = -1;
            for (int i = 0; i < population.GetLength(0); i++)
            {
                for (int j = 0; j < population.GetLength(1); j++)
                {
                    individual[j] = population[i, j];
                }
                fitness = Fitness(individual).fitness;
                if (fitness > bestFit)
                {
                    bestFit = fitness;
                    bestFitIndex = i;
                }
            }
            return bestFitIndex;
        }

        public static double Scale(double tu, double tsmax, double tsmin, double tumax, double tumin)
        {
            return ((tu - tumin) / (tumax - tumin)) * (tsmax - tsmin) + (tsmin);
        }

        public static double GenerationalFitness(double[,] population, int gen)
        {

            double[] curIndiv = new double[population.GetLength(1)];
            double aveFitness, aveCost, aveSSE;
            double totalFitness = 0;
            double totalCost = 0;
            double totalSSE = 0;
            for (int i = 0; i < population.GetLength(0); i++)
            {
                for (int j = 0; j < population.GetLength(1); j++)
                {
                    curIndiv[j] = population[i, j];
                }
                totalFitness += Fitness(curIndiv).fitness;
                totalCost += Fitness(curIndiv).cost;
                totalSSE += Fitness(curIndiv).sse;
            }
            aveFitness = totalFitness / population.GetLength(0);
            aveCost = totalCost / population.GetLength(0);
            aveSSE = totalSSE / population.GetLength(0);
            int best;
            best = Elitism(population);
            if (gen % 10 == 0)
            {
                Console.WriteLine(gen);
                DisplayInfo(best, population);
                Console.WriteLine("\t{0}", (-Math.Round(aveFitness, 2)).ToString() + "\t" + (Math.Round(aveCost, 2)).ToString() + "\t" + (Math.Round(aveSSE, 3)).ToString());
            }
            return aveFitness;
        }

        public static void DisplayInfo(int indiv, double[,] population)
        {
            double[] individual = new double[population.GetLength(1)];
            for (int j = 0; j < population.GetLength(1); j++)
            {
                individual[j] = population[indiv, j];
            }
            double[] IndivNutInfo = new double[NutInfoArr.GetLength(1)]; //Cost, Calories, Protein, Calcium, Iron, VitaminA, VB1, VB2, Niacin, VC;
            for (int i = 0; i < individual.Length; i++)
            {
                double MassPerDollar = NutInfoArr[i, 0]; //Mass (g) per dollar of each food type
                IndivNutInfo[0] += Math.Max(individual[i], 0) * (1 / MassPerDollar); //cost
                for (int entry = 1; entry < NutInfoArr.GetLength(1); entry++)
                {
                    IndivNutInfo[entry] += Math.Max(individual[i], 0) * (NutInfoArr[i, entry] / MassPerDollar); //nutrients
                }
            }

            Console.Write("R" + Math.Round(IndivNutInfo[0], 2).ToString() + "\t");
            for (int entry = 1; entry < IndivNutInfo.Length; entry++)
            {
                Console.Write(Math.Round(IndivNutInfo[entry], 0).ToString() + "\t");
            }
            Console.Write("\nAim:\t");
            for (int entry = 0; entry < MinNutIntake.Length; entry++)
            {
                Console.Write(Math.Round(MinNutIntake[entry], 1).ToString() + "\t");
            }
        }
    }
}
