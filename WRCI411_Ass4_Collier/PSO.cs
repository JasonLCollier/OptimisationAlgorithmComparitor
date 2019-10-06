using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;

namespace WRCI411_Ass4_Collier
{
    class PSO
    {
        private static Random rnd;
        private static double[,] NutInfoArr;
        private static double[] MinNutIntake;

        public PSO(double[,] Arr)
        {
            rnd = new Random();
            NutInfoArr = Arr;
            double[] input = { 1095, 25550, 292, 4380, 1825, 657, 985.5, 6570, 27375 }; //average annual minimum nutritional intake for an adult male
            MinNutIntake = input;
            //{Calories (Kcalories), Protein (g), Calcium (g), Iron (mg), Vitamin A (IU), 
            //Thiamine VB1 (mg), Riboflavin VB2 (mg), Niacin (mg), Ascorbic Acid VC (mg)}
        }

        public void RunPSO()
        {
            int t = 0; //generation counter initialised to 0
            int ns = 200; //population of ns chromosones
            double initRange = 10; //its allele is initialised with a value between 0 and (initRange-1) * grams/$
            double[,] population = new double[ns, 77]; //ns individuals with 77 genes each with real value representation
            double curGenFitness;
            double newGenFitness;
            int it_wo_change = 0;
            ArrayList FitnessvsGenerations = new ArrayList(); //used to store Fitness vs Generations data
            double[,] Y = new double[population.GetLength(0), population.GetLength(1)];
            double[,] X = new double[population.GetLength(0), population.GetLength(1)];
            double[] indivYi = new double[population.GetLength(1)];
            double[] indivXi = new double[population.GetLength(1)];
            double[] indivYhat = new double[population.GetLength(1)];
            double[,] V = new double[population.GetLength(0), population.GetLength(1)];
            double c1 = 0.5;
            double c2 = 0.5;
            double r1, r2;
            double w = 0.877; //Inertia Weight 0.877
            int nNi = 150; //neighbourhood size
            double[,] Ni = new double[nNi, population.GetLength(1)];
            double[,] Yhat = new double[ns, population.GetLength(1)];

            population = Initialisation(population, initRange);
            X = population;
            Y = X;
            for (int j = 0; j < population.GetLength(1); j++)
            {
                indivYhat[j] = Y[0, j];
            }

            Console.WriteLine(t);
            indivYhat = Elitism(X);
            DisplayInfo(indivYhat);
            newGenFitness = GenerationalFitness(X, t);
            FitnessvsGenerations.Add(newGenFitness);
            curGenFitness = newGenFitness;

            while (t < 1000 /*&& it_wo_change <= 100*/)
            {
                for (int i = 0; i < ns; i++)
                {
                    for (int j = 0; j < population.GetLength(1); j++)
                    {
                        indivYi[j] = Y[i, j];
                    }
                    for (int j = 0; j < population.GetLength(1); j++)
                    {
                        indivXi[j] = X[i, j];
                    }

                    //Set the personal best position
                    if (Fitness(indivXi).fitness < Fitness(indivYi).fitness)
                    {
                        for (int j = 0; j < population.GetLength(1); j++)
                        {
                            Y[i, j] = indivXi[j];
                        }
                    }

                    //Set the global best position
                   /*if (Fitness(indivYi).fitness < Fitness(indivYhat).fitness)
                    {
                        for (int j = 0; j < population.GetLength(1); j++)
                        {
                            indivYhat[j] = indivYi[j];
                        }
                    }*/

                    //Set the neigbourhood best position
                    int n = i - (nNi / 2);
                    if (n < 0)
                        n = ns + n;
                    for (int particle = 0; particle < nNi; particle++)
                    {
                        for (int j = 0; j < population.GetLength(1); j++)
                        {
                            Ni[particle, j] = Y[n, j];
                        }
                        n++;
                        if (n >= ns)
                            n = n - ns;
                    }
                    for (int j = 0; j < population.GetLength(1); j++)
                    {
                        indivYhat[j] = Ni[0, j];
                        Yhat[i, j] = indivYhat[j];
                    }
                    for (int particle = 0; particle < nNi; particle++)
                    {
                        for (int j = 0; j < population.GetLength(1); j++)
                        {
                            indivYi[j] = Ni[particle, j];
                            indivYhat[j] = Yhat[i, j];
                        }
                        double curFitness = Fitness(indivYi).fitness;
                        double neighbourhoodBest = Fitness(indivYhat).fitness;
                        if (curFitness < neighbourhoodBest)
                        {
                            for (int j = 0; j < population.GetLength(1); j++)
                            {
                                Yhat[i, j] = indivYi[j];
                            }
                        }
                    }
                }

                for (int i = 0; i < ns; i++)
                {
                    for (int j = 0; j < population.GetLength(1); j++)
                    {
                        //Update Velocity
                        r1 = rnd.NextDouble();
                        r2 = rnd.NextDouble();
                        //V[i, j] =  w * V[i, j] + (c1 * r1 * (Y[i, j] - X[i, j]) + c2 * r2 * (indivYhat[j] - X[i, j]));
                        V[i, j] = w * V[i, j] + (c1 * r1 * (Y[i, j] - X[i, j]) + c2 * r2 * (Yhat[i, j] - X[i, j]));
                        //Update Position
                        X[i, j] += V[i, j];
                    }
                }

                if ((t + 1) % 10 == 0)
                {
                    Console.WriteLine("\n" + (t + 1));
                    indivYhat = Elitism(X);
                    DisplayInfo(indivYhat);
                }
                newGenFitness = GenerationalFitness(X, t + 1);
                FitnessvsGenerations.Add(newGenFitness);
                if (Math.Round(newGenFitness) == Math.Round(curGenFitness))
                    it_wo_change++;
                curGenFitness = newGenFitness;
                t++;
            }

            Console.WriteLine("\nBest:");
            DisplayInfo(Elitism(X));

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
            fd.fitness = ((+c * IndivNutInfo[0]) + s * (SSE) - o * over);
            return fd;
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
            if (gen % 10 == 0)
            {
                Console.Write("  \t{0}", (Math.Round(aveFitness, 2)).ToString() + "\t" + (Math.Round(aveCost, 2)).ToString() + "\t" + (Math.Round(aveSSE, 3)).ToString());
            }
            return aveFitness;
        }

        public static void DisplayInfo(double[] individual)
        {
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

        public static double[] Elitism(double[,] population)
        {
            double[] individual = new double[population.GetLength(1)];
            double[] betIndiv = new double[population.GetLength(1)];
            double fitness;
            double bestFit = double.MaxValue;
            int bestFitIndex = -1;
            for (int i = 0; i < population.GetLength(0); i++)
            {
                for (int j = 0; j < population.GetLength(1); j++)
                {
                    individual[j] = population[i, j];
                }
                fitness = Fitness(individual).fitness;
                if (fitness < bestFit)
                {
                    bestFit = fitness;
                    bestFitIndex = i;
                }
            }
            for (int j = 0; j < population.GetLength(1); j++)
            {
                betIndiv[j] = population[bestFitIndex, j];
            }
            return betIndiv;
        }
    }
}
