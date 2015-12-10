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

        private List<int> best;

        public GeneticAlgorithm(City[] cities)
        {
            this.cities = cities;
        }

        public ArrayList solve()
        {
            List<int> parent1 = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 };
            List<int> parent2 = new List<int>() { 3, 4, 5, 1, 2, 8, 7, 6 };

            CrossOver(parent1, parent2);
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
    }
}
