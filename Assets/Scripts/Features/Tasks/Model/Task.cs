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
        AwaitingConfirmation,
        Completed,
        Failed
    }
    
    [Serializable]
    public enum RewardType { Strength, Intelligence, Health, Xp }

    [Serializable]
    public class RewardPoint {
        public int Value;
        public RewardType Type;
        public RewardPoint(int value, RewardType type) { Value = value; Type = type; }
    }

    [Serializable]
    public class TaskData
    {
        public string Id;
        public string Name;
        public TaskType Type;
        public Color TaskColor;
        public DayOfWeek Day;
        public bool IsFreeTime;
        public long StartTimeTicks;
        public long DurationTicks;
        
        [NonSerialized]
        public TimeSpan StartTimeOfDay;
        [NonSerialized]
        public TimeSpan Duration;
        
        public TimeSpan EndTimeOfDay => StartTimeOfDay + Duration;

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
        public bool IsActionHandled { get; private set; }

        public Task(TaskData data)
        {
            Id = Guid.NewGuid();
            Data = data;
            TodayStatus = TaskStatus.Pending;
            IsActionHandled = false;
        }
        
        public void ApplyLoadedStatus(TaskStatus status, bool isHandled)
        {
            TodayStatus = status;
            IsActionHandled = isHandled;
        }

        public void SetActionHandled()
        {
            IsActionHandled = true;
        }

        public void Start()
        {
            if (TodayStatus == TaskStatus.Pending)
                TodayStatus = TaskStatus.InProgress;
        }

        public void SetAwaitingConfirmation()
        {
            if (TodayStatus == TaskStatus.InProgress)
                TodayStatus = TaskStatus.AwaitingConfirmation;
        }

        public void Complete()
        {
            if (TodayStatus == TaskStatus.InProgress || TodayStatus == TaskStatus.AwaitingConfirmation)
                TodayStatus = TaskStatus.Completed;
        }

        public void Fail()
        {
            if (TodayStatus == TaskStatus.Pending || TodayStatus == TaskStatus.InProgress || TodayStatus == TaskStatus.AwaitingConfirmation)
                TodayStatus = TaskStatus.Failed;
        }
    }
}
