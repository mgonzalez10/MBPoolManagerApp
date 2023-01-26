using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace MBPoolManagerApp
{
    public class EnqueueManager
    {
        private const int queueLength = 50;
        private readonly SemaphoreSlim semaphore;
        private readonly object _RunningTasksLock = new();
        private int _RunningTasksCount = 0;

        public EnqueueManager(SemaphoreSlim semaphore)
        {
            this.semaphore = semaphore;
        }

        public event EventHandler<TaskFinishedEventArgs> TaskFinished;

        public ConcurrentQueue<Task> CreateFifoQueue()
        {
            ConcurrentQueue<Task> taskQueue = new();
            for (int i = 0; i < queueLength; i++)
            {
                int taskNumber = i;
                EnqueueNewTask(taskQueue, taskNumber, i == 5);
            }

            return taskQueue;
        }

        private void EnqueueNewTask(ConcurrentQueue<Task> taskQueue, int taskNumber, bool raiseUserErrorForTesting)
        {
            taskQueue.Enqueue(Task.Run(() => DoWork(taskNumber, raiseUserErrorForTesting)));
        }

        private void DoWork(int taskNumber, bool raiseUserErrorForTesting)
        {
            try
            {
                semaphore.Wait();

                Console.WriteLine($"Task {taskNumber} initilized on thread {Thread.CurrentThread.ManagedThreadId} (Running {IncreaseTaskCount()} tasks)");

                // Simulate error during execution:
                if (raiseUserErrorForTesting)
                {
                    throw new Exception($"Error provoked by user on task {taskNumber}, thread {Thread.CurrentThread.ManagedThreadId}");
                }

                Console.WriteLine($"Task {taskNumber} finished on thread {Thread.CurrentThread.ManagedThreadId} (Running {DecreaseTaskCount()} tasks)");

                // Raise event
                TaskFinished?.Invoke(this, new TaskFinishedEventArgs(taskNumber));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error raised: {e.Message}");
            }
            finally
            {
                semaphore.Release(1);
            }
        }
        private int IncreaseTaskCount()
        {
            int taskCount;
            lock (_RunningTasksLock)
            {
                taskCount = ++_RunningTasksCount;
            }
            return taskCount;
        }

        private int DecreaseTaskCount()
        {
            int taskCount;
            lock (_RunningTasksLock)
            {
                taskCount = --_RunningTasksCount;

            }
            return taskCount;
        }
    }
}
