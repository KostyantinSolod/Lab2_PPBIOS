using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static int[] matrix;
    static int currentLength;
    static int threadCount;
    static SemaphoreSlim taskSemaphore; 
    static Queue<(int, int)> tasksQueue; 
    static object lockObject = new object(); 

    static void Worker()
    {
        while (true)
        {
            (int, int) task;
            lock (lockObject)
            {
                if (tasksQueue.Count > 0)
                {
                    task = tasksQueue.Dequeue(); 
                }
                else
                {
                    break; 
                }
            }

            int i = task.Item1;
            int oppositeIndex = task.Item2;

            matrix[i] += matrix[oppositeIndex];  

            taskSemaphore.Release();  
        }
    }

    static void CalculateSum()
    {
        while (currentLength > 1)
        {
            int halfLength = currentLength / 2;
            tasksQueue = new Queue<(int, int)>();  

            for (int i = 0; i < halfLength; i++)
            {
                int oppositeIndex = currentLength - i - 1;
                tasksQueue.Enqueue((i, oppositeIndex)); 
            }

            taskSemaphore = new SemaphoreSlim(0, halfLength);  

            var tasks = new Task[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                tasks[i] = Task.Run(() => Worker()); 
            }

            for (int i = 0; i < halfLength; i++)
            {
                taskSemaphore.Wait();  
            }

            if (currentLength % 2 == 1)
            {
                matrix[halfLength] += matrix[currentLength - 1];
            }

            currentLength = (currentLength + 1) / 2;

            Console.WriteLine("Масив після обробки:");
            foreach (var item in matrix.Take(currentLength))
            {
                Console.Write(item + " ");
            }
            Console.WriteLine();
        }
    }

    static void Main(string[] args)
    {
        matrix = new int[50000];
        for (int i = 0; i < matrix.Length; i++)
        {
            matrix[i] = i + 1;  
        }
        currentLength = matrix.Length;

        threadCount = 4; 

        CalculateSum();

        Console.WriteLine("Загальна сума елементів масиву: " + matrix[0]);
    }
}
