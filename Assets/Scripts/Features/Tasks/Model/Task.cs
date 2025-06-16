using System;
using System.Collections.Generic;
using UnityEngine;

namespace Features.Tasks.Model
{
    [Serializable]
    public enum TaskStatus
    {
        Pending,
        InProgress,
        Completed,
        Failed
    }

    [Serializable]
    public enum RewardType
    {
        Strength,
        Intelligence,
        Health,
        Xp
    }
    
    [Serializable]
    public enum TaskType
    {
        PhysicalExercise,
        Sleep,
        Rest,
        Eating,
        WorkAndStudy,
        Housework,
        Creative,
        Hygiene
    }

    [Serializable]
    public class RewardPoint
    {
        public int Value;
        public RewardType Type;

        public RewardPoint(int value, RewardType type)
        {
            Value = value;
            Type = type;
        }
    }

    [Serializable]
    public class TaskData
    {
        public string Id;
        public string Name;
        public TaskType Type;
        
        public Color TaskColor;
        
        public bool IsFreeTime;

        public long StartTimeTicks;
        public long DurationTicks;
        
        [NonSerialized]
        public TimeSpan StartTimeOfDay;
        [NonSerialized]
        public TimeSpan Duration;
        
        public TimeSpan EndTimeOfDay => StartTimeOfDay + Duration;

        public List<DayOfWeek> RecurrenceDays = new List<DayOfWeek>();

        public TaskData()
        {
            Id = Guid.NewGuid().ToString();
            IsFreeTime = false;
            TaskColor = Color.white;
        }
        
        public void RestoreTimeSpans()
        {
            StartTimeOfDay = TimeSpan.FromTicks(StartTimeTicks);
            Duration = TimeSpan.FromTicks(DurationTicks);
        }
        
        public void PrepareForSerialization()
        {
            StartTimeTicks = StartTimeOfDay.Ticks;
            DurationTicks = Duration.Ticks;
        }
    }
    
    public class Task
    {
        public Guid Id { get; private set; }
        public TaskData Data { get; private set; }
        public TaskStatus TodayStatus { get; private set; }

        public Task(TaskData data)
        {
            Id = Guid.NewGuid();
            Data = data;
            TodayStatus = TaskStatus.Pending;
        }
        
        public void Start()
        {
            if (TodayStatus != TaskStatus.Pending) return;
            TodayStatus = TaskStatus.InProgress;
        }

        public void Complete()
        {
            if (TodayStatus != TaskStatus.InProgress) return;
            TodayStatus = TaskStatus.Completed;
        }

        public void Fail()
        {
            if (TodayStatus != TaskStatus.InProgress) return;
            TodayStatus = TaskStatus.Failed;
        }

        public void ResetStatus()
        {
            TodayStatus = TaskStatus.Pending;
        }
        
    }
}
