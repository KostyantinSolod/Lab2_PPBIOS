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
            // Проходимо лише до середини, для кожної пари елементів
            for (int i = start; i < end; i++) {
                int oppositeIndex = currentLength - i - 1;
                if (i < oppositeIndex) {
                    synchronized (matrix) { // Захищаємо масив від одночасного доступу
                        matrix[i] += matrix[oppositeIndex]; // Додаємо елементи на протилежних індексах
                    }
                }
            }
            latch.countDown();
        }
    }

    static void calculateSum() throws InterruptedException {
        // Продовжуємо до тих пір, поки не зменшиться до одного елемента
        while (currentLength > 1) {
            System.out.println("=======================================================================================");
            int halfLength = currentLength / 2; // Поділяємо на дві половини
            CountDownLatch latch = new CountDownLatch(threadCount); // Лічильник завершення потоків

            System.out.println("Поточна довжина масиву: " + currentLength);

            int chunkSize = (halfLength + threadCount - 1) / threadCount; // Розподіляємо роботу між потоками

            Thread[] threads = new Thread[threadCount];
            for (int threadId = 0; threadId < threadCount; threadId++) {
                int start = threadId * chunkSize;
                int end = Math.min(start + chunkSize, halfLength);

                if (start >= halfLength) {
                    latch.countDown();  // Якщо потік не працює (всі індекси оброблені), просто відразу звільняємо
                    continue;
                }

                threads[threadId] = new Thread(new Worker(start, end, latch));
                threads[threadId].start();
            }

            latch.await(); // Чекаємо на завершення всіх потоків

            // Якщо довжина непарна, додаємо центральний елемент
            if (currentLength % 2 == 1) {
                matrix[halfLength] += matrix[currentLength - 1];
                System.out.printf("Додаємо центральний елемент: %d -> %d\n",
                        matrix[halfLength], matrix[halfLength]);
            }

            // Оновлюємо довжину для наступної ітерації
            currentLength = (currentLength + 1) / 2;

            System.out.println("Масив після обробки:");
            for (int i = 0; i < currentLength; i++) {
                System.out.print(matrix[i] + " ");
            }
            System.out.println();
        }
    }

    public static void main(String[] args) throws InterruptedException {
        int n = 50000; // Розмір масиву
        matrix = new int[n];
        for (int i = 0; i < n; i++) {
            matrix[i] = i + 1;  // Ініціалізуємо масив числами від 1 до 50000
        }
        currentLength = matrix.length;

        threadCount = Runtime.getRuntime().availableProcessors(); // Використання доступних ядер

        // Початкова перевірка суми
        int initialSum = 0;
        for (int i = 0; i < n; i++) {
            initialSum += matrix[i];
        }
        System.out.println("Очікувана сума елементів масиву (від 1 до " + n + "): " + initialSum);

        calculateSum();

        // Перевірка результату
        System.out.println("Загальна сума елементів масиву: " + matrix[0]);
    }
}
