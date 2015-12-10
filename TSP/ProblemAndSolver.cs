using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Linq;

namespace TSP
{

    class ProblemAndSolver
    {

        public class TSPSolution
        {
            /// <summary>
            /// we use the representation [cityB,cityA,cityC] 
            /// to mean that cityB is the first city in the solution, cityA is the second, cityC is the third 
            /// and the edge from cityC to cityB is the final edge in the path.  
            /// You are, of course, free to use a different representation if it would be more convenient or efficient 
            /// for your node data structure and search algorithm. 
            /// </summary>
            public ArrayList Route;          

			public TSPSolution(ArrayList iroute)
            {
                Route = new ArrayList(iroute);
            }

            /// <summary>
            /// Compute the cost of the current route.  
            /// Note: This does not check that the route is complete.
            /// It assumes that the route passes from the last city back to the first city. 
            /// </summary>
            /// <returns></returns>
            public double costOfRoute()
            {
                // go through each edge in the route and add up the cost. 
                int x;
                City here;
                double cost = 0D;

                for (x = 0; x < Route.Count - 1; x++)
                {
                    here = Route[x] as City;
                    cost += here.costToGetTo(Route[x + 1] as City);
                }

                // go from the last city to the first. 
                here = Route[Route.Count - 1] as City;
                cost += here.costToGetTo(Route[0] as City);
                return cost;
            }
        }

        #region Private members 

        /// <summary>
        /// Default number of cities (unused -- to set defaults, change the values in the GUI form)
        /// </summary>
        // (This is no longer used -- to set default values, edit the form directly.  Open Form1.cs,
        // click on the Problem Size text box, go to the Properties window (lower right corner), 
        // and change the "Text" value.)
        private const int DEFAULT_SIZE = 25;

        private const int CITY_ICON_SIZE = 5;

        // For normal and hard modes:
        // hard mode only
        private const double FRACTION_OF_PATHS_TO_REMOVE = 0.20;

        /// <summary>
        /// the cities in the current problem.
        /// </summary>
        private City[] Cities;
        /// <summary>
        /// a route through the current problem, useful as a temporary variable. 
        /// </summary>
        private ArrayList Route;
        /// <summary>
        /// best solution so far. 
        /// </summary>
        private TSPSolution bssf; 

        /// <summary>
        /// how to color various things. 
        /// </summary>
        private Brush cityBrushStartStyle;
        private Brush cityBrushStyle;
        private Pen routePenStyle;


        /// <summary>
        /// keep track of the seed value so that the same sequence of problems can be 
        /// regenerated next time the generator is run. 
        /// </summary>
        private int _seed;
        /// <summary>
        /// number of cities to include in a problem. 
        /// </summary>
        private int _size;

        /// <summary>
        /// Difficulty level
        /// </summary>
        private HardMode.Modes _mode;

        /// <summary>
        /// random number generator. 
        /// </summary>
        private Random rnd;
        #endregion

        #region Public members
        public int Size
        {
            get { return _size; }
        }

        public int Seed
        {
            get { return _seed; }
        }

        public double greedySolutionCost { get; set; }

        public List<PriorityQueue<List<int>>> queues { get; set; }
        #endregion

        #region Constructors
        public ProblemAndSolver()
        {
            this._seed = 1; 
            rnd = new Random(1);
            this._size = DEFAULT_SIZE;

            this.resetData();
        }

        public ProblemAndSolver(int seed)
        {
            this._seed = seed;
            rnd = new Random(seed);
            this._size = DEFAULT_SIZE;

            this.resetData();
        }

        public ProblemAndSolver(int seed, int size)
        {
            this._seed = seed;
            this._size = size;
            rnd = new Random(seed); 
            this.resetData();
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Reset the problem instance.
        /// </summary>
        private void resetData()
        {

            Cities = new City[_size];
            Route = new ArrayList(_size);
            bssf = null;

            if (_mode == HardMode.Modes.Easy)
            {
                for (int i = 0; i < _size; i++)
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble());
            }
            else // Medium and hard
            {
                for (int i = 0; i < _size; i++)
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble() * City.MAX_ELEVATION);
            }

            HardMode mm = new HardMode(this._mode, this.rnd, Cities);
            if (_mode == HardMode.Modes.Hard)
            {
                int edgesToRemove = (int)(_size * FRACTION_OF_PATHS_TO_REMOVE);
                mm.removePaths(edgesToRemove);
            }
            City.setModeManager(mm);

            cityBrushStyle = new SolidBrush(Color.Black);
            cityBrushStartStyle = new SolidBrush(Color.Red);
            routePenStyle = new Pen(Color.Blue,1);
            routePenStyle.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// make a new problem with the given size.
        /// </summary>
        /// <param name="size">number of cities</param>
        //public void GenerateProblem(int size) // unused
        //{
        //   this.GenerateProblem(size, Modes.Normal);
        //}

        /// <summary>
        /// make a new problem with the given size.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size, HardMode.Modes mode)
        {
            this._size = size;
            this._mode = mode;
            resetData();
        }

        /// <summary>
        /// return a copy of the cities in this problem. 
        /// </summary>
        /// <returns>array of cities</returns>
        public City[] GetCities()
        {
            City[] retCities = new City[Cities.Length];
            Array.Copy(Cities, retCities, Cities.Length);
            return retCities;
        }

        /// <summary>
        /// draw the cities in the problem.  if the bssf member is defined, then
        /// draw that too. 
        /// </summary>
        /// <param name="g">where to draw the stuff</param>
        public void Draw(Graphics g)
        {
            float width  = g.VisibleClipBounds.Width-45F;
            float height = g.VisibleClipBounds.Height-45F;
            Font labelFont = new Font("Arial", 10);

            // Draw lines
            if (bssf != null)
            {
                // make a list of points. 
                Point[] ps = new Point[bssf.Route.Count];
                int index = 0;
                foreach (City c in bssf.Route)
                {
                    if (index < bssf.Route.Count -1)
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[index+1]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    else 
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[0]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    ps[index++] = new Point((int)(c.X * width) + CITY_ICON_SIZE / 2, (int)(c.Y * height) + CITY_ICON_SIZE / 2);
                }

                if (ps.Length > 0)
                {
                    g.DrawLines(routePenStyle, ps);
                    g.FillEllipse(cityBrushStartStyle, (float)Cities[0].X * width - 1, (float)Cities[0].Y * height - 1, CITY_ICON_SIZE + 2, CITY_ICON_SIZE + 2);
                }

                // draw the last line. 
                g.DrawLine(routePenStyle, ps[0], ps[ps.Length - 1]);
            }

            // Draw city dots
            foreach (City c in Cities)
            {
                g.FillEllipse(cityBrushStyle, (float)c.X * width, (float)c.Y * height, CITY_ICON_SIZE, CITY_ICON_SIZE);
            }

        }

        /// <summary>
        ///  Helper method that updates the appropriate text fields on the form. 
        /// </summary>
        /// <returns></returns>
        private void updateForm(Stopwatch timer = null)
        {
            Program.MainForm.tbCostOfTour.Text = " " + this.costOfBssf();

            if (timer != null)
                Program.MainForm.tbElapsedTime.Text = timer.Elapsed.TotalSeconds.ToString();

            Program.MainForm.Invalidate();
        }

        /// <summary>
        ///  return the cost of the best solution so far. 
        /// </summary>
        /// <returns></returns>
        public double costOfBssf ()
        {
            if (bssf != null)
                return (bssf.costOfRoute());
            else
                return -1D; 
        }

        public void initQueues()
        {
            queues = new List<PriorityQueue<List<int>>>();
            List<Double> multipliers = new List<Double>() { .6, .8, 1.0, 1.2, 1.4 };
            for (int i = 0; i < 5; i++)
            {
                queues.Add(createQueue(multipliers[i]));
            }
        }

        public PriorityQueue<List<int>> createQueue(Double multiplier)
        {
            PriorityQueue<List<int>> pQueue = new PriorityQueue<List<int>>();
            pQueue.shelf = (int)Math.Floor(this.greedySolutionCost * multiplier);
            return pQueue;
        }

        public List<List<int>> crossOver(List<List<int>> parents, int numSwaps)
        {
            Random r = new Random();
            List<List<int>> children = new List<List<int>>();
            for(int i = 0; i < parents.Count-1; i++)
            {
                List<int> child1 = new List<int>(parents[i]);
                List<int> child2 = new List<int>(parents[i+1]);

                for (int j = 0; j < numSwaps; j++)
                {
                    int swapIndex = r.Next(Cities.Length);
                    int temp = 0;

                    temp = child1[swapIndex];
                    child1[swapIndex] = child2[swapIndex];
                    child2[swapIndex] = temp;
                }
                if(child1.Distinct().Count() == child1.Count())
                    children.Add(child1);
                if (child2.Distinct().Count() == child2.Count())
                    children.Add(child2);
            } 
            return children;
        }

        public void mutate(List<List<int>> parents, int numSwaps, double likelihood = 0.5)
        {
            Random r = new Random();
            foreach (List<int> parent in parents)
            {
                if(r.NextDouble() < likelihood)
                {
                    for (int i = 0; i < numSwaps; i++)
                    {
                        int swapIndex1 = r.Next(Cities.Length);
                        int swapIndex2 = r.Next(Cities.Length);

                        if(swapIndex1 == swapIndex2)
                        {
                            i--;
                            continue;
                        }

                        int temp = parent[swapIndex1];
                        parent[swapIndex1] = parent[swapIndex2];
                        parent[swapIndex2] = temp;
                    }
                }
            }
        }

        /// <summary>
        ///  Iterates over a variety of different greedy solutions and selects the best one. 
        /// </summary>
        /// <returns></returns>
        public void greedySolution(bool usingForBssf = false)
        {
            Random r = new Random();

            for (int total = 0; total < 10; total++)
            {
                bool valid = false;
                TSPSolution temp = null;

                while (!valid)
                {
                    Route.Clear();
                    int currentIndex = r.Next(Cities.Length);
                    Route.Add(Cities[currentIndex]);

                    while (Route.Count != Cities.Length)
                    {
                        double minDist = double.MaxValue;

                        for (int i = 0; i < Cities.Length; i++)
                        {
                            if (Route.Contains(Cities[i]))
                                continue;

                            double dist = Cities[currentIndex].costToGetTo(Cities[i]);

                            if (dist < minDist)
                            {
                                minDist = dist;
                                currentIndex = i;
                            }
                        }
                        Route.Add(Cities[currentIndex]);
                    }
                    temp = new TSPSolution(Route);
                    valid = temp.costOfRoute() != double.PositiveInfinity;
                }
                if (bssf == null || bssf.costOfRoute() > temp.costOfRoute())
                    bssf = temp;
            }

            this.greedySolutionCost = bssf.costOfRoute();
            return;
            }

        public List<List<int>> generateRandom(int n)
        {
            List<List<int>> randomResults = new List<List<int>>();
            Random generator = new Random();

            for (int i = 0; i < n; i++)
            {
                List<int> Route = new List<int>();
                Route.Add(generator.Next(Cities.Length));

                int unreachableCities = 0;

                while (Route.Count < Cities.Length)
                {
                    int newCity = generator.Next(Cities.Length);
                    if (Route.Contains(newCity))
                        continue;

                    //Make sure that the node is reachable.
                    double cost = Cities[Route[Route.Count - 1]].costToGetTo(Cities[newCity]);
                    if (cost != double.PositiveInfinity)
                    {
                        Route.Add(newCity);
                    }
                    else
                    {
                        //If not, increment the unreachable cities counter.
                        //If this exceeds the number of cities, start over again.
                        unreachableCities++;
                        if (unreachableCities > Cities.Length)
                        {
                            unreachableCities = 0;
                            Route.Clear();
                            Route.Add(generator.Next(Cities.Length));
                        }
                    }
                }
                randomResults.Add(Route);
            }
            return randomResults;
        }

		public void initialize(List<List<int>> population)
		{
			for (int i = 0; i < population.Count; i++)
			{
				addToQueue(evaluate(population[i]), population[i]);
			}
		}

		public bool addToQueue(Double costOfRoute, List<int> route)
		{
            if (costOfRoute == double.PositiveInfinity)
                return false;

            bool update = false;
			for(int i=0; i < queues.Count; i++)
			{
				if(costOfRoute < queues[i].shelf)
				{
                    if (costOfRoute < bssf.costOfRoute())
					{
						ArrayList newSolution = new ArrayList();
						for (int j=0; j< route.Count; j++)
						{
							newSolution.Add(Cities[route[j]]);
						}
						bssf = new TSPSolution(newSolution);
                        double funCOst = bssf.costOfRoute();
                        update = true;
					}
					queues[i].Add((int)costOfRoute,route);
                    break;
				}
			}
            return update;

		}

		public Double evaluate(List<int> route)
		{
            Route.Clear();
			for (int i=0; i < route.Count; i++)
			{
                Route.Add(Cities[route[i]]);
			}
            TSPSolution temp = new TSPSolution(Route);
            return temp.costOfRoute();
		}

		public List<List<int>> drawParents(int numParents)
		{
			List<List<int>> parents = new List<List<int>>();
			Random r = new Random();
			for (int i = 0; i < numParents; i++)
			{
				double swtch = r.NextDouble();

				if (swtch < 0.5 && queues[0].Count != 0) parents.Add(queues[0].RemoveMin());
				else if (swtch < 0.7 && queues[1].Count != 0) parents.Add(queues[1].RemoveMin());
				else if (swtch < 0.85 && queues[2].Count != 0) parents.Add(queues[2].RemoveMin());
				else if (swtch < 0.97 && queues[3].Count != 0) parents.Add(queues[3].RemoveMin());
				else if (swtch < 0.99 && queues[4].Count != 0) parents.Add(queues[4].RemoveMin());

			}
			return parents;
		}

		public bool Select(List<List<int>> children, int numSurvivors)//todo: possible refactor here. heavy cost. possible state will help here
		{
			if(numSurvivors > children.Count - 1)
			{
				numSurvivors = children.Count - 1;
			}

            SortedList<double, List<int>> ChildCosts = new SortedList<double, List<int>>();
            SortedList<double, List<int>> ChildPercentages = new SortedList<double, List<int>>();

            Double sum = 0;
            //List<Double> costs = new List<Double>();
            List<Double> percentages = new List<Double>();
            for (int i = 0; i < children.Count; i++)
            {
                double cost = evaluate(children[i]);
                if (ChildCosts.ContainsKey(cost) || cost == double.PositiveInfinity)
                {
                    numSurvivors--;
                    continue;
                }

                ChildCosts.Add(cost, children[i]);

                if (!double.IsPositiveInfinity(cost))
                    sum += cost;
            }
   //         double percentSum = 0;
   //         for (int i = 0; i < ChildCosts.Count; i++)
   //         {
   //             ChildPercentages.Add(1 - ChildCosts.Keys[i] / sum, children[i]);
   //         }

   //         List<List<int>> survivors = new List<List<int>>();
   //         //costs = new List<Double>();

   //         Random r = new Random();
   //         double tempValue = 0;

   //         while (survivors.Count < numSurvivors)
			//{
			//	tempValue = r.NextDouble();

   //             for (int i = 1; i < ChildCosts.Count; i++)
   //             {
   //                 if (tempValue > ChildPercentages.Keys[i])
   //                 {
   //                     if(survivors.Contains((ChildCosts[ChildCosts.Keys[i - 1]])))
   //                         break;
   //                     survivors.Add(ChildCosts[ChildCosts.Keys[i - 1]]);
   //                 }
   //             }
			//}

            bool updated = false;
			for(int i=0; i < numSurvivors; i++)
			{
                updated = addToQueue(ChildCosts.Keys[i], ChildCosts.Values[i]);
			}

            return updated;
		}

        public void geneticAlgorithm()
        {
            Stopwatch timer = new Stopwatch();
            GeneticAlgorithm algorithm = new GeneticAlgorithm(Cities);
            timer.Start();
            Route = algorithm.solve();
            timer.Stop();
            Program.MainForm.tbElapsedTime.Text = timer.Elapsed.TotalSeconds.ToString();
            
            bssf = new TSPSolution(Route);
            updateForm(); 
            Program.MainForm.Invalidate();

            /*
            greedySolution();
            initQueues();
            initialize(generateRandom(1000));
            int genWithoutChange = 0;

            while (genWithoutChange < 15)
            {
                List<List<int>> parents = drawParents(10);
                List<List<int>> children = crossOver(parents, 3);
                mutate(parents, 3, .75);
                children.AddRange(parents);
                //This returns a boolean value showing that the bssf was updated by one of the children we created.
                bool updated = Select(children, 12);

                if(!updated)
                    genWithoutChange++;
            }

            updateForm();
            */
        }

        /// <summary>
        ///  solve the problem.  This is the entry point for the solver when the run button is clicked
        /// right now it just picks a simple solution. 
        /// </summary>
        public void solveProblem()
        {
            int x;
            Route = new ArrayList(); 
            // this is the trivial solution. 
            for (x = 0; x < Cities.Length; x++)
            {
                Route.Add( Cities[Cities.Length - x -1]);
            }
            // call this the best solution so far.  bssf is the route that will be drawn by the Draw method. 
            bssf = new TSPSolution(Route);
            // update the cost of the tour. 
            Program.MainForm.tbCostOfTour.Text = " " + bssf.costOfRoute();
            // do a refresh. 
            Program.MainForm.Invalidate();

        }
        #endregion
    }

}
