using UnityEngine;
using System.Collections.Generic;
using Features.Tasks.Model;

[CreateAssetMenu(fileName = "TaskType_", menuName = "Time Tracker/Task Type Definition")]
public class TaskTypeDefinitionSO : ScriptableObject
{
    public TaskType Type;
    public new string Name;
    [TextArea] public string Description;
    public Sprite Icon;
    public string AnimationTriggerName; 
    public List<RewardPoint> CompletionRewards;
    public List<RewardPoint> FailurePenalties;
}