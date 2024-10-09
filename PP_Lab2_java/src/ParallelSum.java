import java.util.concurrent.ArrayBlockingQueue;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.Semaphore;
import java.util.concurrent.atomic.AtomicBoolean;

public class ParallelSum {
    static int[] matrix;
    static int currentLength;
    static int threadCount;
    static BlockingQueue<int[]> tasksQueue;
    static Semaphore waveSemaphore;
    static AtomicBoolean taskAvailable;

    static void worker() {
        while (true) {
            try {
                while (!taskAvailable.get() && tasksQueue.isEmpty()) {
                    Thread.sleep(1);
                }

                int[] task = tasksQueue.take();
                if (task[0] == -1) break;

                int i = task[0];
                int oppositeIndex = task[1];

                matrix[i] += matrix[oppositeIndex];

                waveSemaphore.release();
            } catch (InterruptedException e) {
                Thread.currentThread().interrupt();
                break;
            }
        }
    }

    static void calculateSum() throws InterruptedException {
        while (currentLength > 1) {
            int halfLength = currentLength / 2;
            waveSemaphore = new Semaphore(0);

            for (int i = 0; i < halfLength; i++) {
                int oppositeIndex = currentLength - i - 1;
                tasksQueue.put(new int[]{i, oppositeIndex});
            }

            taskAvailable.set(true);

            for (int i = 0; i < halfLength; i++) {
                waveSemaphore.acquire();
            }

            if (currentLength % 2 == 1) {
                matrix[halfLength - 1] += matrix[halfLength];
            }

            currentLength = halfLength;
        }
    }

    public static void main(String[] args) throws InterruptedException {
        matrix = new int[50000];
        for (int i = 0; i < matrix.length; i++) {
            matrix[i] = i + 1;
        }
        currentLength = matrix.length;

        threadCount = 100;

        tasksQueue = new ArrayBlockingQueue<>(currentLength);
        taskAvailable = new AtomicBoolean(false);

        Thread[] workers = new Thread[threadCount];
        for (int i = 0; i < threadCount; i++) {
            workers[i] = new Thread(ParallelSum::worker);
            workers[i].start();
        }

        calculateSum();

        for (int i = 0; i < threadCount; i++) {
            tasksQueue.put(new int[]{-1, -1});
        }

        for (Thread worker : workers) {
            worker.join();
        }

        System.out.println("Загальна сума елементів масиву: " + matrix[0]);
    }
}
