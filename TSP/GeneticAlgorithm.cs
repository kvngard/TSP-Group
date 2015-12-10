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
            return null;
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
