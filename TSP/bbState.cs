using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSP
{
    public class bbState
    {
        public double[][] matrix;
        public int[] edges;
        public double lowerbound;
        public int nextFrom, nextTo;
        public int[] entered, exited;

        public bbState(double[][] matrix, int[] edges, double lowerbound)
        {
            this.matrix = matrix;
            this.edges = edges;
            this.lowerbound = lowerbound;
        }

        // copy constructor
        public bbState(bbState original)
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
}
