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
    static SemaphoreSlim taskSemaphore; // Семафор для обробки завдань
    static Queue<(int, int)> tasksQueue; // Черга завдань для потоків
    static object lockObject = new object(); // Для синхронізації доступу до черги завдань

    // Потік, який обробляє пару елементів масиву
    static void Worker()
    {
        while (true)
        {
            (int, int) task;
            lock (lockObject)
            {
                if (tasksQueue.Count > 0)
                {
                    task = tasksQueue.Dequeue(); // Бере завдання з черги
                }
                else
                {
                    break; // Якщо завдань більше немає, завершуємо роботу потоку
                }
            }

            int i = task.Item1;
            int oppositeIndex = task.Item2;

            matrix[i] += matrix[oppositeIndex];  // Додаємо пару елементів

            taskSemaphore.Release();  // Вивільняємо слот для наступного потоку
        }
    }

    static void CalculateSum()
    {
        while (currentLength > 1)
        {
            int halfLength = currentLength / 2;
            tasksQueue = new Queue<(int, int)>();  // Очищаємо чергу завдань

            // Додаємо завдання до черги
            for (int i = 0; i < halfLength; i++)
            {
                int oppositeIndex = currentLength - i - 1;
                tasksQueue.Enqueue((i, oppositeIndex));  // Додаємо пару індексів
            }

            // Створюємо пул потоків і кожен потік працює із завданнями
            taskSemaphore = new SemaphoreSlim(0, halfLength);  // Початкове обмеження на кількість завдань

            // Створюємо потоки
            var tasks = new Task[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                tasks[i] = Task.Run(() => Worker()); // Кожен потік виконує Worker
            }

            // Чекаємо, поки всі потоки оброблять пари
            for (int i = 0; i < halfLength; i++)
            {
                taskSemaphore.Wait();  // Очікуємо, поки потік звільнить слот
            }

            // Якщо довжина непарна, додаємо центральний елемент
            if (currentLength % 2 == 1)
            {
                matrix[halfLength] += matrix[currentLength - 1];
            }

            // Оновлюємо довжину масиву
            currentLength = (currentLength + 1) / 2;

            // Виводимо масив після кожної хвилі
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
            matrix[i] = i + 1;  // Заповнюємо масив значеннями
        }
        currentLength = matrix.Length;

        threadCount = 4;  // Використовуємо 4 потоки (можна налаштувати)

        CalculateSum();

        Console.WriteLine("Загальна сума елементів масиву: " + matrix[0]);
    }
}
