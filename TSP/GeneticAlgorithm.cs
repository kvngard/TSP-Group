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

        private List<int> best;

        public GeneticAlgorithm(City[] cities)
        {
            this.cities = cities;
        }

        public ArrayList solve()
        {
            return null;
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

        private List<int> CrossOver(List<int> mom, List<int> dad)
        {
            return null;
        }

        private List<int> Mutate(List<int> gene)
        {
            return null;
        }
    }
}
