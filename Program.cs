/************************************************************
 * File:                  Program.cs
 * Author:                Carlos del Sol
 * Date:                  10.02.2013
 * Description:           Create a .Net Console app that takes in a single number, n, 
 *                        and returns the n-th number in the Fibonacci sequence.
 * Comments:              Based on Thread Pool msdn best practice:  http://msdn.microsoft.com/en-us/library/3dasc8as(v=vs.100).aspx
*************************************************************/
using System;
using System.Threading;

namespace Fibonacci
{
    public interface IFibonacci
    {
        /// <summary>
        /// From value 
        /// </summary>
        int intNum { get; set; }
        /// <summary>
        /// Computed value
        /// </summary>
        int intValue { get; set; }
    }
    
    public class Sequence : IFibonacci
    {
        private int _intNum;
        private int _intValue;
        private ManualResetEvent _doneEvent;

        // Constructor.
        public Sequence(int intNum, ManualResetEvent doneEvent)
        {
            _intNum = intNum;
            _doneEvent = doneEvent;
        }

        // Wrapper method for use with thread pool. 
        public void ThreadPoolCallback(Object threadContext)
        {
            int threadIndex = (int)threadContext;
            Console.WriteLine("thread {0} started...", threadIndex);
            _intValue = Calculate(_intNum);
            Console.WriteLine("thread {0} result calculated...", threadIndex);
            _doneEvent.Set();
        }

        // Recursive method that calculates the Nth Fibonacci number. 
        public int Calculate(int n)
        {
            if (n <= 1)
            {
                return n;
            }

            return Calculate(n - 1) + Calculate(n - 2);
        }

        // Property implementation:
        public int intNum
        {
            get { return _intNum; }
            set { _intNum = value; }
        }

        public int intValue
        {
            get { return _intValue; }
            set { _intValue = value; }
        }
    }

    public class Program
    {
        //Get integers between 0 and 45 from console
        public static int getNum()
        {
            int num = int.MinValue;
            try
            {
                bool result = false;
                do
                {
                    Console.Write("Enter some integer: ");
                    result = int.TryParse(Console.ReadLine(), out num);
                    if (!result || num <= 0 || num > 64)
                    {
                        Console.WriteLine("You did not enter a valid integer. The number must be between 1 and 64");
                    }
                } while (!result || num <= 0 || num > 64);
            }
            catch (Exception ex) {
                Console.WriteLine("There was an error: {0}", ex.Message);
            }
            return num;
        }

        static void Main(string[] args)
        {
            try
            {
                int FibonacciCalculations = getNum();
                DateTime dtInit = new DateTime();
                DateTime dtEnd = new DateTime();

                // One event is used for each Fibonacci object.
                ManualResetEvent[] doneEvents = new ManualResetEvent[FibonacciCalculations];
                Sequence[] fibArray = new Sequence[FibonacciCalculations];

                // Configure and start threads using ThreadPool.
                Console.WriteLine("launching {0} tasks...", FibonacciCalculations);
                dtInit = DateTime.Now;
                for (int i = 0; i < FibonacciCalculations; i++)
                {
                    doneEvents[i] = new ManualResetEvent(false);
                    Sequence f = new Sequence(i+1, doneEvents[i]);
                    fibArray[i] = f;
                    ThreadPool.QueueUserWorkItem(f.ThreadPoolCallback, i);
                }

                // Wait for all threads in pool to calculate.
                WaitHandle.WaitAll(doneEvents);
                dtEnd = DateTime.Now;
                Console.WriteLine("All calculations are complete.");
                Console.WriteLine("Time: {0} ms", Math.Abs(dtEnd.Millisecond - dtInit.Millisecond));
                Console.WriteLine();
                
                string strSeq = string.Empty;
                // Display the results. 
                for (int i = 0; i < FibonacciCalculations; i++)
                {
                    Sequence f = fibArray[i];
                    Console.WriteLine("Fibonacci({0}) = {1}", f.intNum, f.intValue);
                    strSeq += string.Format("{0}   ", f.intValue.ToString());
                }

                // Display the results.
                Console.WriteLine();
                Console.WriteLine("================");
                Console.WriteLine("||  Sequence  ||");
                Console.WriteLine("================");
                Console.WriteLine(strSeq);
                Console.WriteLine("================");
                
            }
            catch (FormatException) {
                Console.WriteLine("You did not enter a valid integer");
            }
            catch (Exception ex)
            {
                Console.WriteLine("There was an error {0}", ex.Message);
            }
            
            Console.WriteLine();
            Console.WriteLine("Enter To Exit");
            Console.ReadLine();
        }
    }

}
