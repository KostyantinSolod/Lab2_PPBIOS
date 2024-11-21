import java.util.concurrent.CountDownLatch;

public class ParallelSum {
    static int[] matrix;
    static int currentLength;
    static int threadCount;

    static class Worker implements Runnable {
        private final int start;
        private final int end;
        private final CountDownLatch latch;

        public Worker(int start, int end, CountDownLatch latch) {
            this.start = start;
            this.end = end;
            this.latch = latch;
        }

        @Override
        public void run() {
            for (int i = start; i < end; i++) {
                int oppositeIndex = currentLength - i - 1;
                if (i < oppositeIndex) {
                    synchronized (matrix) { 
                        matrix[i] += matrix[oppositeIndex]; 
                    }
                }
            }
            latch.countDown();
        }
    }

    static void calculateSum() throws InterruptedException {
        while (currentLength > 1) {
            System.out.println("=======================================================================================");
            int halfLength = currentLength / 2;
            CountDownLatch latch = new CountDownLatch(threadCount); 

            System.out.println("Поточна довжина масиву: " + currentLength);

            int chunkSize = (halfLength + threadCount - 1) / threadCount; 

            Thread[] threads = new Thread[threadCount];
            for (int threadId = 0; threadId < threadCount; threadId++) {
                int start = threadId * chunkSize;
                int end = Math.min(start + chunkSize, halfLength);

                if (start >= halfLength) {
                    latch.countDown(); 
                    continue;
                }

                threads[threadId] = new Thread(new Worker(start, end, latch));
                threads[threadId].start();
            }

            latch.await(); 

            if (currentLength % 2 == 1) {
                matrix[halfLength] += matrix[currentLength - 1];
                System.out.printf("Додаємо центральний елемент: %d -> %d\n",
                        matrix[halfLength], matrix[halfLength]);
            }

            currentLength = (currentLength + 1) / 2;

            System.out.println("Масив після обробки:");
            for (int i = 0; i < currentLength; i++) {
                System.out.print(matrix[i] + " ");
            }
            System.out.println();
        }
    }

    public static void main(String[] args) throws InterruptedException {
        int n = 50000; 
        matrix = new int[n];
        for (int i = 0; i < n; i++) {
            matrix[i] = i + 1;  
        }
        currentLength = matrix.length;

        threadCount = Runtime.getRuntime().availableProcessors(); 

        int initialSum = 0;
        for (int i = 0; i < n; i++) {
            initialSum += matrix[i];
        }
        System.out.println("Очікувана сума елементів масиву (від 1 до " + n + "): " + initialSum);

        calculateSum();

        System.out.println("Загальна сума елементів масиву: " + matrix[0]);
    }
}
