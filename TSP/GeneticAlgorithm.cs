using System;
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

        public GeneticAlgorithm(City[] cities)
        {
            this.cities = cities;
        }

        public ArrayList solve()
        {
            var population = InitializePopulation(1000);
            
            for (int i = 0; i < 1000; i++)
            {
                var best = SelectBest(population);

                var children = new SortedList<double, List<int>>();

                var parents = PickTwo(best);

                var mom = parents.Item1;
                var dad = parents.Item2;

                var son = Mutate(CrossOver(mom, dad));
                var daughter = Mutate(CrossOver(mom, dad));

                children.Add(ComputeCost(son), son);
                children.Add(ComputeCost(daughter), daughter);

                var candidateCost = children.Keys.Min();
                var candidatePath = children[candidateCost];

                if (candidateCost < bestCost)
                {
                    bestCost = candidateCost;
                    bestPath = candidatePath;
                }
            }

            return null; // TODO CHANGE THIS MATT!!!!
        }

        private Tuple<List<int>, List<int>> PickTwo(SortedList<double, List<int>> population)
        {
            return Tuple.Create(new List<int>(), new List<int>());
        }

        private SortedList<double, List<int>> InitializePopulation(int popSize)
        {
			//List<List<int>> randomResults = new List<List<int>>();
			SortedList<double, List<int>> randomResults = new SortedList<double, List<int>>();
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
				Double finalCost = new ProblemAndSolver.TSPSolution(new ArrayList(generateFinalTour(Route).ToArray())).costOfRoute();
				randomResults.Add(finalCost, Route);
			}
			return randomResults;
        }

		public List<City> generateFinalTour(List<int> tour)
		{
			List<City> newList = new List<City>();
			foreach (int index in tour)
			{
				newList.Add(cities[index - 1]);
			}
			return newList;
		}

		private SortedList<double, List<int>> SelectBest(SortedList<double, List<int>> population)
        {
            return null;
        }

        private List<int> CrossOver(List<int> _mom, List<int> _dad)
        {
            Random random = new Random();

            int[] mom = _mom.ToArray();
            int[] dad = _dad.ToArray();

            HashSet<int> availableCities = new HashSet<int>(mom);
            // crossover at a random position, up to a random length
            int startPos = random.Next(mom.Length);
            int crossCount = random.Next(mom.Length - startPos);

            foreach (var item in mom)
            {
                Console.Write(item + " ");

            }
            Array.Copy(dad, startPos, mom, startPos, crossCount);


            List<int> indicesWithDuplicates = null;

            // we find out where the duplicates are and which cities have not crossed over.
            for (int i = 0; i < mom.Length; i++)
            {
                if (!availableCities.Remove(mom[i]))
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
                            mom[indexIter.Current] = cityIter.Current;
                        }
                    }
                }

            }
            Console.WriteLine();
            foreach (var item in mom)
            {
                Console.Write(item + " ");

            }
            Console.WriteLine();


            return null;
        }

        private List<int> Mutate(List<int> gene)
        {
            return null;
        }

        private double ComputeCost(List<int> gene)
        {
            return 0;
        }
    }
}
