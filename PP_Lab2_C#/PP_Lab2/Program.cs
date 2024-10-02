using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static int[] matrix;
    static int currentLength;
    static int threadCount;
    static BlockingCollection<(int, int)> tasksQueue;  
    static SemaphoreSlim waveSemaphore;

    static void Worker()
    {
        while (true)
        {
            if (tasksQueue.TryTake(out var task, Timeout.Infinite))
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

        threadCount = 6;

        tasksQueue = new BlockingCollection<(int, int)>();


        Task calculateTask = Task.Run(() => CalculateSum());

        Task[] workers = new Task[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            workers[i] = Task.Run(() => Worker());  
        }

        calculateTask.Wait();

        for (int i = 0; i < threadCount; i++)
        {
            tasksQueue.Add((-1, -1));  
        }

        Task.WaitAll(workers);

        Console.WriteLine("Загальна сума елементів масиву: " + matrix[0]);
    }
}
