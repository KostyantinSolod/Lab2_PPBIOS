using System;
using System.Collections.Concurrent;
using System.Threading;

class Program
{
    static int[] matrix;
    static int currentLength;
    static int threadCount;
    static BlockingCollection<(int, int)> tasksQueue;
    static SemaphoreSlim waveSemaphore;
    static ManualResetEventSlim taskAvailable;  

    static void Worker()
    {
        while (true)
        {
            taskAvailable.Wait();  
            if (tasksQueue.TryTake(out var task))
            {
                if (task.Item1 == -1) break;

                int i = task.Item1;
                int oppositeIndex = task.Item2;

                matrix[i] += matrix[oppositeIndex];
                waveSemaphore.Release();  
            }
        }
    }

    static void CalculateSum()
    {
        while (currentLength > 1)
        {
            int halfLength = currentLength / 2;
            waveSemaphore = new SemaphoreSlim(0, halfLength);

            for (int i = 0; i < halfLength; i++)
            {
                int oppositeIndex = currentLength - i - 1;
                tasksQueue.Add((i, oppositeIndex));
            }

            taskAvailable.Set();

            for (int i = 0; i < halfLength; i++)
            {
                waveSemaphore.Wait();
            }

            if (currentLength % 2 == 1)
            {
                matrix[halfLength - 1] += matrix[halfLength];
            }

            currentLength = halfLength;
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

        threadCount = 9;  

        tasksQueue = new BlockingCollection<(int, int)>();
        taskAvailable = new ManualResetEventSlim(false);  

        
        Thread[] workers = new Thread[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            workers[i] = new Thread(Worker);
            workers[i].Start();  
        }

        
        CalculateSum();

        for (int i = 0; i < threadCount; i++)
        {
            tasksQueue.Add((-1, -1));
        }

        taskAvailable.Set();

        foreach (var worker in workers)
        {
            worker.Join();
        }

        Console.WriteLine("Загальна сума елементів масиву: " + matrix[0]);
    }
}
