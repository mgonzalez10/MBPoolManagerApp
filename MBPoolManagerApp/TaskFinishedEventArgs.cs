using System;

namespace MBPoolManagerApp
{
    public class TaskFinishedEventArgs: EventArgs
    {
        public int TaskNumber { get; set; }

        public TaskFinishedEventArgs(int taskNumber)
        {
            TaskNumber = taskNumber;
        }

    }
}