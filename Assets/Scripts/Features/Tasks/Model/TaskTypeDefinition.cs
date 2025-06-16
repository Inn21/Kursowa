using System;
using System.Collections.Generic;

namespace Features.Tasks.Model
{
    [Serializable]
    public class TaskTypeDefinition
    {
        public Features.Tasks.Model.TaskType Type;
        public string Name;
        public string Description;
        public List<RewardPoint> CompletionRewards;
        public List<RewardPoint> FailurePenalties;
    }
}