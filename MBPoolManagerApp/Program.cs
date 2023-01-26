using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MBPoolManagerApp
{
    class Program
    {
        private static object lockMsg = new();
       

        static void Main(string[] args)
        {
            // Specify max degree of parallelism
            int? degreeOfParallelism = 10;
            int degreeOfParallelismMod = degreeOfParallelism == null || degreeOfParallelism == 0 ? int.MaxValue : degreeOfParallelism.Value;
            var semaphore = new SemaphoreSlim(degreeOfParallelismMod, degreeOfParallelismMod);

            // Creates a concurrent queue to host the tasks
            var enqueueManager = new EnqueueManager(semaphore);
            enqueueManager.TaskFinished += EnqueueManager_TaskFinished;
            var taskQueue = enqueueManager.CreateFifoQueue();
            
            var tasks = new List<Task>();
            
            Console.WriteLine($"Semaphore current count {semaphore.CurrentCount}.");

            // Ejecuta las tareas en paralelo
            while (taskQueue.Count > 0)
            {
                // Error raising for testing error handling
                if (taskQueue.TryDequeue(out Task task))
                {
                    Console.WriteLine($"Dequeued task {task.Id}.");
                    tasks.Add(task);
                }
            }

            // Wait for all tasks completion
            Task.WaitAll(tasks.ToArray());

            Console.WriteLine("All tasks finished.");
        }

        private static void EnqueueManager_TaskFinished(object sender, TaskFinishedEventArgs e)
        {
            lock (lockMsg)
            {
                Console.WriteLine($"Task finished event fired to task {e.TaskNumber}");
            }
        }

        
    }
}
