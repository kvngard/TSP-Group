using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP
{
    class BranchAndBound
    {

        #region Auxiliary
        class MinHeap<T> where T : IComparable<T>
        {
            private List<T> array = new List<T>();

            public void Add(T element)
            {
                array.Add(element);
                int c = array.Count - 1;
                while (c > 0 && array[c].CompareTo(array[c / 2]) == -1)
                {
                    T tmp = array[c];
                    array[c] = array[c / 2];
                    array[c / 2] = tmp;
                    c = c / 2;
                }
            }

            public T RemoveMin()
            {
                T ret = array[0];
                array[0] = array[array.Count - 1];
                array.RemoveAt(array.Count - 1);

                int c = 0;
                while (c < array.Count)
                {
                    int min = c;
                    if (2 * c + 1 < array.Count && array[2 * c + 1].CompareTo(array[min]) == -1)
                        min = 2 * c + 1;
                    if (2 * c + 2 < array.Count && array[2 * c + 2].CompareTo(array[min]) == -1)
                        min = 2 * c + 2;

                    if (min == c)
                        break;
                    else
                    {
                        T tmp = array[c];
                        array[c] = array[min];
                        array[min] = tmp;
                        c = min;
                    }
                }

                return ret;
            }

            public T Peek()
            {
                return array[0];
            }

            public int Count
            {
                get
                {
                    return array.Count;
                }
            }
        }

        class PriorityQueue<T>
        {
            internal class Node : IComparable<Node>
            {
                public int Priority;
                public T O;
                public int CompareTo(Node other)
                {
                    return Priority.CompareTo(other.Priority);
                }
            }

            private MinHeap<Node> minHeap = new MinHeap<Node>();

            public void Add(int priority, T element)
            {
                minHeap.Add(new Node() { Priority = priority, O = element });
            }

            public T RemoveMin()
            {
                return minHeap.RemoveMin().O;
            }

            public T Peek()
            {
                return minHeap.Peek().O;
            }

            public int Count
            {
                get
                {
                    return minHeap.Count;
                }
            }
        }

        public class State
        {
            public double[][] matrix;
            public int[] edges;
            public double lowerbound;
            public int nextFrom, nextTo;
            public int[] entered, exited;

            public State(double[][] matrix, int[] edges, double lowerbound)
            {
                this.matrix = matrix;
                this.edges = edges;
                this.lowerbound = lowerbound;
            }

            // copy constructor
            public State(State original)
            {
                matrix = copyArray(original.matrix);
                edges = (int[])original.edges.Clone();
                lowerbound = original.lowerbound;
                nextFrom = original.nextFrom;
                nextTo = original.nextTo;
                entered = (int[])original.entered.Clone();
                exited = (int[])original.exited.Clone();
            }

            public int EdgesFound
            {
                get { return getNumOfEdgesFound(); }
            }

            public int getNumOfEdgesFound()
            {
                int count = 0;
                foreach (var edge in edges)
                {
                    if (edge != -1)
                    {
                        count++;
                    }
                }
                return count;
            }

            public double[][] copyArray(double[][] source)
            {
                var len = source.Length;
                var dest = new double[len][];

                for (int i = 0; i < len; i++)
                {
                    var inner = source[i];
                    var ilen = inner.Length;
                    var newer = new double[ilen];
                    Array.Copy(inner, newer, ilen);
                    dest[i] = newer;
                }

                return dest;
            }

            public void printMatrix()
            {
                for (int i = 0; i < matrix.Length; i++)
                {
                    for (int j = 0; j < matrix.Length; j++)
                    {
                        Console.Write(matrix[i][j] + "\t");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();

            }
        }
        #endregion

        private PriorityQueue<State> pq;
        private City[] cities;
        private ArrayList Route;
        private ProblemAndSolver.TSPSolution bssf;
        private int size;

        public BranchAndBound(City[] cities)
        {
            this.cities = cities;
            size = cities.Length;
            pq = new PriorityQueue<State>();
        }

        public void Initialize()
        {
            int x;
            Route = new ArrayList();
            // this is the trivial solution. 
            for (x = 0; x < cities.Length; x++)
            {
                Route.Add(cities[cities.Length - x - 1]);
            }
            bssf = new ProblemAndSolver.TSPSolution(Route);

            // create first state
            double[][] matrix = createMatrix(cities);
            int[] edges = new int[size];
            for (int i = 0; i < size; i++)
            {
                edges[i] = -1;
            }
            State state = new State(matrix, edges, 0);

            // arrays for removing premature cycles
            state.entered = new int[size];
            state.exited = new int[size];

            // initialize arrays
            for (int i = 0; i < size; i++)
            {
                state.entered[i] = -1;
                state.exited[i] = -1;
            }

            // calculate first state's lowerbound
            state.lowerbound = reduceMatrix(state);

            // add first state into queue
            pq.Add(0, state);
        }
        public ProblemAndSolver.TSPSolution Solve()
        {
            Initialize();
            while (pq.Count > 0)
            {
                expandChild(pq.RemoveMin());
            }

            return bssf;
        }

        public double[][] createMatrix(City[] cities)
        {
            double[][] matrix = new double[size][];

            for (int i = 0; i < size; i++)
            {
                double[] distances = new double[size];

                for (int j = 0; j < size; j++)
                {
                    double givenDist = cities[i].costToGetTo(cities[j]);
                    distances[j] = givenDist > 0 ? givenDist : Double.PositiveInfinity;
                }

                matrix[i] = distances;
            }

            return matrix;
        }

        private double reduceRow(double[] row, int index)
        {
            double min = Double.PositiveInfinity;

            for (int i = 0; i < row.Length; i++)
            {
                min = row[i] < min ? row[i] : min;
            }

            if (min != 0)
            {
                for (int i = 0; i < row.Length; i++)
                {
                    row[i] -= min;
                }
            }

            return min;
        }

        private double reduceRows(double[][] matrix, int[] edges)
        {
            double reduced = 0;
            for (int i = 0; i < size; i++)
            {
                if (edges[i] == -1)
                {
                    reduced += reduceRow(matrix[i], i);
                }
            }
            return reduced;
        }
        private void transposeMatrix(double[][] matrix)
        {
            for (int i = 1; i < size; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    double temp = matrix[i][j];
                    matrix[i][j] = matrix[j][i];
                    matrix[j][i] = temp;
                }
            }
        }

        private double reduceCols(double[][] matrix, int[] edges)
        {
            double reduced = 0;
            transposeMatrix(matrix);
            for (int i = 0; i < size; i++)
            {
                if (!Array.Exists(edges, el => el == i))
                {
                    reduced += reduceRow(matrix[i], i);
                }
            }
            transposeMatrix(matrix);
            return reduced;
        }

        private double reduceMatrix(State state)
        {
            double bound = 0;
            bound += reduceRows(state.matrix, state.edges);
            bound += reduceCols(state.matrix, state.edges);
            state.lowerbound += bound;
            return bound;
        }

        private double includeEdge(State state)
        {
            double[][] matrix = state.matrix;
            for (int i = 0; i < size; i++)
            {
                matrix[state.nextFrom][i] = Double.PositiveInfinity;
                matrix[i][state.nextTo] = Double.PositiveInfinity;
            }

            state.edges[state.nextFrom] = state.nextTo;
            if (state.EdgesFound < size)
            {
                deleteEdges(state, state.nextFrom, state.nextTo);
            }
            return reduceMatrix(state);
        }

        private double excludeEdge(State state)
        {
            state.matrix[state.nextFrom][state.nextTo] = Double.PositiveInfinity;
            return reduceMatrix(state);
        }

        public double calcIncludeEdgeBound(State state, int row, int col)
        {
            double bound = 0;
            double[][] matrix = state.matrix;
            for (int i = 0; i < size; i++)
            {
                double min = Double.PositiveInfinity;

                if (matrix[row][i] == 0 && i != col)
                {
                    for (int j = 0; j < size; j++)
                    {
                        min = (matrix[j][i] < min && j != row) ? matrix[j][i] : min;
                    }
                    bound += min;
                }

                min = Double.PositiveInfinity;
                if (matrix[i][col] == 0 && i != row)
                {
                    for (int j = 0; j < size; j++)
                    {
                        min = (matrix[i][j] < min && j != col) ? matrix[i][j] : min;
                    }
                    bound += min;
                }
            }
            //state.printMatrix();
            return bound;
        }

        public double calcExcludeEdgeBound(State state, int row, int col)
        {
            double[][] matrix = state.matrix;
            double rowMin = Double.PositiveInfinity, colMin = Double.PositiveInfinity;

            for (int i = 0; i < size; i++)
            {
                rowMin = (matrix[row][i] < rowMin && i != col) ? matrix[row][i] : rowMin;
                colMin = (matrix[i][col] < colMin && i != row) ? matrix[i][col] : colMin;
            }

            return rowMin + colMin;
        }

        public void findGreatestDifference(State state)
        {
            double[][] matrix = state.matrix;
            double max = Double.MinValue;

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (matrix[i][j] == 0)
                    {
                        double cur = calcExcludeEdgeBound(state, i, j) - calcIncludeEdgeBound(state, i, j);
                        if (cur > max)
                        {
                            max = cur;
                            state.nextFrom = i;
                            state.nextTo = j;
                        }
                    }
                }
            }
        }

        public void deleteEdges(State state, int row, int col)
        {
            state.entered[col] = row;
            state.exited[row] = col;
            int start = row, end = col;

            while (state.exited[end] != -1)
            {
                end = state.exited[end];
            }

            while (state.entered[start] != -1)
            {
                start = state.entered[start];
            }

            if (state.EdgesFound < size - 1)
            {
                while (start != col)
                {
                    state.matrix[end][start] = Double.PositiveInfinity;
                    state.matrix[col][start] = Double.PositiveInfinity;
                    start = state.exited[start];
                }
            }
        }

        public void expandChild(State state)
        {
            findGreatestDifference(state);
            State included = new State(state);
            State excluded = new State(state);

            includeEdge(included);
            excludeEdge(excluded);

            if (included.EdgesFound == size)
            {
                var edges = included.edges;
                Route = new ArrayList();
                int x = 0;
                while (edges[x] != 0)
                {
                    Route.Add(cities[edges[x]]);
                    x = edges[x];
                }
                // complete the tour
                Route.Add(cities[edges[x]]);

                bssf = new ProblemAndSolver.TSPSolution(Route);
            }
            else if (included.lowerbound < bssf.costOfRoute())
            {
                //add included to queue
                pq.Add((int)included.lowerbound / (1 + included.EdgesFound), included);
            }

            if (excluded.EdgesFound == size)
            {
                var edges = excluded.edges;
                Route = new ArrayList();
                int x = 0;
                while (edges[x] != 0)
                {
                    Route.Add(cities[edges[x]]);
                    x = edges[x];
                }

                Route.Add(cities[edges[x]]);

                bssf = new ProblemAndSolver.TSPSolution(Route);
            }
            else if (excluded.lowerbound < bssf.costOfRoute())
            {
                // add excluded to queue
                pq.Add((int)excluded.lowerbound / (1 + excluded.EdgesFound), excluded);
            }
        }
    }
}
