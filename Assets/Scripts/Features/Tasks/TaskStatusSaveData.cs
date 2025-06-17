using System;
using System.Collections.Generic;
using Features.Tasks.Model;

namespace Features.Tasks
{
    [Serializable]
    public class TaskStatusRecord
    {
        public string TaskTemplateId;
        public string Date;
        public TaskStatus Status;
    }

    [Serializable]
    public class TaskStatusHistory
    {
        public List<TaskStatusRecord> Records = new List<TaskStatusRecord>();
    }
}