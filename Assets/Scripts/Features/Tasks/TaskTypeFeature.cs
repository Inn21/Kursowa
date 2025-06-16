using System.Collections.Generic;
using System.Linq;
using Core.Feature;
using Features.Tasks.Model;
using UnityEngine;

namespace Features.Tasks
{
    public class TaskTypeFeature : BaseFeature
    {
        private List<TaskTypeDefinitionSO> _taskTypeDefinitions;
        private const string TASK_TYPES_RESOURCES_PATH = "TaskTypes";

        public void Initialize()
        {
            _taskTypeDefinitions = Resources.LoadAll<TaskTypeDefinitionSO>(TASK_TYPES_RESOURCES_PATH).ToList();
        }

        public List<TaskTypeDefinitionSO> GetAllTaskTypes()
        {
            return _taskTypeDefinitions;
        }

        public TaskTypeDefinitionSO GetDefinition(TaskType type)
        {
            return _taskTypeDefinitions.FirstOrDefault(t => t.Type == type);
        }
    }
}
