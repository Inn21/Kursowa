using System;
using System.Collections.Generic;
using Features.Tasks;
using Features.Tasks.Model;

namespace Core.Feature.Tasks
{
    [Serializable]
    public class TaskDataWrapper
    {
        public List<TaskData> TaskTemplates;
    }
}