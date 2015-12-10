﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP
{
    class GeneticAlgorithm
    {
        private City[] cities;

        private double bestCost = double.PositiveInfinity;
        private List<int> bestPath = new List<int>();
		private double mutationLikelihood;
        private double crossOverLikelihood;
        private int populationSize;
        private int generationCount;

        private List<int> genOfUdates = new List<int>();
        private List<int> bestCosts = new List<int>();
        private int genSansUpdate = 0;

        Random random = new Random();
        
        public GeneticAlgorithm(City[] cities)
        {
            this.cities = cities;
			this.mutationLikelihood = 0.1;
            crossOverLikelihood = 0.5;
            populationSize = cities.Count();
            generationCount = cities.Count();
        }

        public ArrayList solve()
        {
            var population = InitializePopulation(populationSize);

            //for (int i = 0; i < generationCount; i++)
            int generation = 0;
            while (genSansUpdate < 50)
            {
                var best = SelectBest(population);

                var children = new SortedList<double, List<List<int>>>();

                while (best.Count >= 2)
                {
                    var parents = PickTwo(best);

                    var mom = parents.Item1;
                    var dad = parents.Item2;

                    //for (int j = 0; j < 4; j++)
                    int childrenCreated = 0;
                    while (childrenCreated < 10)
                    {
                        var child = Mutate(CrossOver(mom, dad));
                        double cost = ComputeCost(child);
                        if (cost == double.PositiveInfinity) { continue; }
                        if (!children.ContainsKey(cost)) { children[cost] = new List<List<int>>(); }
                        children[cost].Add(child);
                        childrenCreated++;
                    }

                }

                var candidateCost = children.Keys.Min();
                var candidatePath = children[candidateCost].First();

                if (candidateCost < bestCost)
                {
                    bestCost = candidateCost;
                    bestPath = candidatePath;
                    genOfUdates.Add(generation);
                    bestCosts.Add((int) bestCost);
                    genSansUpdate = 0;
                } else
                {
                    genSansUpdate++;
                }

                foreach (var e in population)
                {
                    if (!children.ContainsKey(e.Key)) { children[e.Key] = new List<List<int>>(); }
                    foreach (var g in e.Value)
                    {
                        children[e.Key].Add(g);
                    }
                }

                population = children;
                generation++;
            }

            return GenerateFinalTour(bestPath); 
        }

        private Tuple<List<int>, List<int>> PickTwo(SortedList<double, List<List<int>>> population)
        {
            List<int> mom, dad;
            IList<double> keys;
            //int index;
            keys = population.Keys;
            
            var key = population.Keys[random.Next(population.Keys.Count)];
            mom = population[key].First();
            population[key].RemoveAt(0);
            if (population[key].Count == 0) { population.Remove(key); }

            key = population.Keys[random.Next(population.Keys.Count)];
            dad = population[key].First();
            population[key].RemoveAt(0);
            if (population[key].Count == 0) { population.Remove(key); }

            return Tuple.Create(mom, dad);
        }

        private SortedList<double, List<List<int>>> InitializePopulation(int popSize)
        {
			//List<List<int>> randomResults = new List<List<int>>();
			SortedList<double, List<List<int>>> randomResults = new SortedList<double, List<List<int>>>();
			Random generator = new Random();

			for (int i = 0; i < popSize; i++)
			{
				List<int> Route = new List<int>();
				Route.Add(generator.Next(cities.Length));

				int unreachableCities = 0;

				while (Route.Count < cities.Length)
				{
					int newCity = generator.Next(cities.Length);
					if (Route.Contains(newCity))
						continue;

					//Make sure that the node is reachable.
					double cost = cities[Route[Route.Count - 1]].costToGetTo(cities[newCity]);
					if (cost != double.PositiveInfinity)
					{
						Route.Add(newCity);
					}
					else
					{
						//If not, increment the unreachable cities counter.
						//If this exceeds the number of cities, start over again.
						unreachableCities++;
						if (unreachableCities > cities.Length)
						{
							unreachableCities = 0;
							Route.Clear();
							Route.Add(generator.Next(cities.Length));
						}
					}
				}
				Double finalCost = ComputeCost(Route);
                if (finalCost == double.PositiveInfinity) { continue; }
                //while (randomResults.ContainsKey(finalCost)) { finalCost += .001; }
                if (!randomResults.ContainsKey(finalCost)) { randomResults[finalCost] = new List<List<int>>(); }
                randomResults[finalCost].Add(Route);
			}
			return randomResults;
        }

		public ArrayList GenerateFinalTour(List<int> tour)
		{
			var newList = new ArrayList();
			foreach (int index in tour)
			{
				newList.Add(cities[index]);
			}
			return newList;
		}

        private List<int> CrossOver(List<int> _mom, List<int> _dad)
        {
            List<int> child1 = new List<int>(_mom);
            List<int> child2 = new List<int>(_dad);

            List<int> child = null;

            if (random.NextDouble() > crossOverLikelihood)
            {
                return random.Next(2) == 0 ? child1 : child2;
            }


            for (int j = 0; j < cities.Length / 10; j++)
            {
                int swapIndex = random.Next(cities.Length);
                //int temp = 0;

                int val1 = child1[swapIndex];
                int val2 = child2[swapIndex];

                int index1 = child1.IndexOf(val2);
                int index2 = child2.IndexOf(val1);
                
                child1[index1] = val1;
                child2[index2] = val2;

                int temp = child1[swapIndex];
                child1[swapIndex] = child2[swapIndex];
                child2[swapIndex] = temp;
            }

            child = random.Next(2) == 0 ? child1 : child2;

            if (child.Distinct().Count() != child.Count())
            {
                return new List<int>(_mom);
            }
            return child;

            /*
            int[] mom = _mom.ToArray();
            int[] dad = _dad.ToArray();
            int[] child = (int[]) mom.Clone();

            HashSet<int> availableCities = new HashSet<int>(mom);
            // crossover at a random position, up to a random length
            int startPos = random.Next(mom.Length);
            int crossCount = random.Next(mom.Length - startPos);
            Array.Copy(dad, startPos, child, startPos, crossCount);


            List<int> indicesWithDuplicates = null;

            // we find out where the duplicates are and which cities have not crossed over.
            for (int i = 0; i < child.Length; i++)
            {
                if (!availableCities.Remove(child[i]))
                {
                    if (indicesWithDuplicates == null)
                    {
                        indicesWithDuplicates = new List<int>();
                    }

                    indicesWithDuplicates.Add(i);
                }
            }

            if (indicesWithDuplicates != null)
            {
                // now we replace duplicates with cities still left in availableCities

                using (var indexIter = indicesWithDuplicates.GetEnumerator())
                {
                    using (var cityIter = availableCities.GetEnumerator())
                    {
                        while (true)
                        {
                            if (!indexIter.MoveNext())
                            {
                                // break if there all duplicates are accounted for
                                break;
                            }

                            if (!cityIter.MoveNext())
                            {
                                // should not get here if there are no more duplicates
                                // size of availableCities should equal indicesWithDuplicates
                                throw new Exception("Not enough available cities");
                            }
                            // replace duplicates with cities that are still available
                            child[indexIter.Current] = cityIter.Current;
                        }
                    }
                }

            }
            return new List<int>(child);
            */
        }

        private SortedList<double, List<List<int>>> SelectBest(SortedList<double, List<List<int>>> population)
        {
            var best = new SortedList<double, List<List<int>>>();
            int count = populationSize / 2;
            for (int i = 0; i < count * 3 / 4; i++)
            {
                var key = population.Keys.Min();
                if (!best.ContainsKey(key)) { best[key] = new List<List<int>>(); }
                best[key].Add(population[key].First());
                population[key].RemoveAt(0);
                if (population[key].Count == 0) { population.Remove(key); }
            }

            var rand = new Random();
            while (Count(population) > 0 && Count(best) < count)
            {
                var key = population.Keys[rand.Next(population.Count)];
                if (!best.ContainsKey(key)) { best[key] = new List<List<int>>(); }
                best[key].Add(population[key].First());
                population[key].RemoveAt(0);
                if (population[key].Count == 0) { population.Remove(key); }
            }
            return best;
        }

        /*
        private SortedList<double, List<List<int>>> TournamentSelect(SortedList<double, List<List<int>>> population)
        {
            var best = new SortedList<double, List<List<int>>>();

            while (Count(population) > 0 && Count(best) < populationSize / 2)
            {
                var bucketA = population[population.Keys[random.Next(population.Keys.Count())]];
                var bucketB = population[population.Keys[random.Next(population.Keys.Count())]];
            }

            return best;
        }

        private Tuple<List<int>, double> randomGene(SortedList<double, List<List<int>>> population)
        {

            var bucketA = population[population.Keys[random.Next(population.Keys.Count())]];
            return Tuple.Create(bucketA[random.Next(bucketA.Count())];
        }
        */
            


        private int Count(SortedList<double, List<List<int>>> pop)
        {
            int total = 0;
            foreach (var e in pop)
            {
                total += e.Value.Count();
            }
            return total;
        }


        private List<int> Mutate(List<int> gene_in)
        {
            List<int> gene = new List<int>(gene_in);
            Random r = new Random();
            if (r.NextDouble() < mutationLikelihood)
            {
                int swapIndex1 = r.Next(cities.Length);
                int swapIndex2 = r.Next(cities.Length);

                while (swapIndex1 == swapIndex2)
                {
                    swapIndex2 = r.Next(cities.Length);
                }

                int temp = gene[swapIndex1];
                gene[swapIndex1] = gene[swapIndex2];
                gene[swapIndex2] = temp;

            }
            return gene;
        }

        private double ComputeCost(List<int> gene)
        {
            return new ProblemAndSolver.TSPSolution(new ArrayList(GenerateFinalTour(gene).ToArray())).costOfRoute();
        }
    }
}
