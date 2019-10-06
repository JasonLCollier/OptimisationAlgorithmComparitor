using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;

namespace WRCI411_Ass4_Collier
{
    class DE
    {
        private static Random rnd;
        private static double[,] NutInfoArr;
        private static double[] MinNutIntake;

        public DE(double[,] Arr)
        {
            rnd = new Random();
            NutInfoArr = Arr;
            double[] input = { 1095, 25550, 292, 4380, 1825, 657, 985.5, 6570, 27375 }; //average annual minimum nutritional intake for an adult male
            MinNutIntake = input;
            //{Calories (Kcalories), Protein (g), Calcium (g), Iron (mg), Vitamin A (IU), 
            //Thiamine VB1 (mg), Riboflavin VB2 (mg), Niacin (mg), Ascorbic Acid VC (mg)}
        }

        public void RunDA()
        {
            int t = 0; //generation counter initialised to 0
            int ns = 200; //population of ns chromosones
            double initRange = 10; //its allele is initialised with a value between 0 and (initRange-1) * grams/$
            double B = 0.25; //scale factor
            double pr = 0.3; //crossover probability aka crossover rate
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

            while (t < 15000 /*&& it_wo_change <= 1000*/)
            {
                for (int i = 0; i < ns; i++)
                {
                    double fitnessXi;
                    double fitnessOffspring;
                    double[] indivXi = new double[population.GetLength(1)];

                    for (int j = 0; j < population.GetLength(1); j++)
                    {
                        indivXi[j] = population[i, j];
                    }

                    //produce trial vector
                    
                    double[] indivUi = new double[population.GetLength(1)];
                    double[] indivXi1 = new double[population.GetLength(1)];
                    double[] indivXi2 = new double[population.GetLength(1)];
                    double[] indivXi3 = new double[population.GetLength(1)];
                    int i1, i2, i3;
                    
                    i1 = rnd.Next(0, ns);
                    i2 = rnd.Next(0, ns);
                    while (i2 == i1)
                        i2 = rnd.Next(0, ns);
                    i3 = rnd.Next(0, ns);
                    while (i3 == i1 || i3 == i2)
                        i3 = rnd.Next(0, ns);

                    for (int j = 0; j < population.GetLength(1); j++)
                    {
                        indivXi1[j] = population[i1, j];
                        indivXi2[j] = population[i2, j];
                        indivXi3[j] = population[i3, j];
                    }

                    indivUi = Mutation(indivXi1, indivXi2, indivXi3, B);

                    //Produce offspring
                    double[] indivOffspring = new double[population.GetLength(1)];
                    indivOffspring = BinomialCrossover(indivUi, indivXi, pr);

                    //Selection
                    fitnessXi = Fitness(indivXi).fitness;
                    fitnessOffspring = Fitness(indivOffspring).fitness;

                    if (fitnessOffspring > fitnessXi)
                    {
                        for (int j = 0; j < population.GetLength(1); j++)
                            population[i, j] = indivOffspring[j];
                    }
                }

                newGenFitness = GenerationalFitness(population, t + 1);
                FitnessvsGenerations.Add(-newGenFitness);
                if (Math.Round(newGenFitness, 2) == Math.Round(curGenFitness, 2))
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
                    population[i, j] = (rnd.NextDouble() * range) * NutInfoArr[j, 0];
                    //population[i, j] = (rnd.Next() * range);
                }
            }
            return population;
        }

        
        public static fitData Fitness(double[] individual)
        {
            int c = 12;
            int s = 2;
            int u = 30;
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
        
        public static double[] BinomialCrossover(double[] Ui, double[] Xi, double pr)
        {
            double[] offspring = new double[Xi.Length];
            for (int j = 0; j < Xi.Length; j++)
            {
                if (rnd.NextDouble() <= pr)
                {
                    offspring[j] = Ui[j];
                }
                else
                {
                    offspring[j] = Xi[j];
                }
            }
            return offspring;
        }

        public static double[] Mutation(double[] Xi1, double[] Xi2, double[] Xi3, double B)
        {
            double[] Ui = new double[Xi1.Length];
            for (int j = 0; j < Xi1.Length; j++)
            {
                Ui[j] = Xi1[j] + B * (Xi2[j] - Xi3[j]);
            }
            return Ui;
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
            if (gen % 100 == 0)
            {
                Console.WriteLine(gen);
                DisplayInfo(best, population);
                Console.WriteLine("\t\t{0}", (-Math.Round(aveFitness, 2)).ToString() + "\t" + (Math.Round(aveCost, 2)).ToString() + "\t" + (Math.Round(aveSSE, 3)).ToString());
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
