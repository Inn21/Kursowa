using System;
using System.Collections.Generic;
using System.Linq;
using _PROJECT.Scripts.Application.Features.Save;
using Core.Feature.PlayerStats;
using Core.Utils.MonoUtils;
using Features.Tasks;
using Features.Tasks.Model;
using UnityEngine;
using Zenject;

namespace Core.Feature.Tasks
{
    public class TasksFeature : BaseFeature
    {
        [Inject] readonly ISaveFeature _saveFeature;
        [Inject] readonly MonoFeature _monoFeature;
        [Inject] readonly TaskTypeFeature _taskTypeFeature;
        [Inject] readonly PlayerStatsFeature _playerStatsFeature;
        
        
        public event Action OnTaskListUpdated;
        public event Action<Task> OnTaskStateChanged;

        private List<TaskData> _taskTemplates = new List<TaskData>();
        private readonly Dictionary<DayOfWeek, List<Task>> _weeklyTasks = new Dictionary<DayOfWeek, List<Task>>();
        private DateTime _lastDayGenerated = DateTime.MinValue;
        private DateTime _lastCheckTime = DateTime.MinValue;

        private const string SAVE_KEY = "WeeklyTasks";
        private readonly TimeSpan GracePeriod = TimeSpan.FromMinutes(30);

        public void Initialize()
        {
            LoadTasks();
            RegenerateAllWeeklyTasks();
            _monoFeature.OnPerSecondUpdate += TrackTasks;
            _monoFeature.OnApplicationQuitEvent += SaveTasks;
        }

        public override void Dispose()
        {
            if (_monoFeature != null)
            {
                _monoFeature.OnPerSecondUpdate -= TrackTasks;
                _monoFeature.OnApplicationQuitEvent -= SaveTasks;
            }
            base.Dispose();
        }

        #region Public API - Методи для керування завданнями

      public bool AddTask(TaskData newTaskData)
        {
            if (!IsTimeSlotAvailable(newTaskData.RecurrenceDays.First(), newTaskData.StartTimeOfDay, newTaskData.EndTimeOfDay, null))
                return false;
            
            _taskTemplates.Add(newTaskData);
            RegenerateAllWeeklyTasks();
            OnTaskListUpdated?.Invoke();
            return true;
        }

        public bool UpdateTask(TaskData updatedTaskData)
        {
            if (!IsTimeSlotAvailable(updatedTaskData.RecurrenceDays.First(), updatedTaskData.StartTimeOfDay, updatedTaskData.EndTimeOfDay, updatedTaskData.Id))
                return false;

            var taskIndex = _taskTemplates.FindIndex(t => t.Id == updatedTaskData.Id);
            if (taskIndex != -1)
            {
                _taskTemplates[taskIndex] = updatedTaskData;
                RegenerateAllWeeklyTasks();
                OnTaskListUpdated?.Invoke();
                return true;
            }
            return false;
        }

        public void RemoveTask(string taskId)
        {
            var itemsRemoved = _taskTemplates.RemoveAll(t => t.Id == taskId);
            if (itemsRemoved > 0)
            {
                RegenerateAllWeeklyTasks();
                OnTaskListUpdated?.Invoke();
            }
        }
        
        public List<Task> GetTasksForDay(DayOfWeek day)
        {
            return _weeklyTasks.ContainsKey(day) ? _weeklyTasks[day] : new List<Task>();
        }

        public void CompleteTask(Task task)
        {
            if(task == null || task.Data.IsFreeTime) return;
            if (task.TodayStatus != TaskStatus.InProgress && task.TodayStatus != TaskStatus.AwaitingConfirmation) return;
            
            task.Complete();
            
            var definition = _taskTypeFeature.GetDefinition(task.Data.Type);
            if(definition != null)
            {
                foreach(var reward in definition.CompletionRewards)
                {
                    _playerStatsFeature.AddStat(reward.Type, reward.Value);
                }
            }
            OnTaskStateChanged?.Invoke(task);
        }

        public void FailTask(Task task)
        {
            if(task == null || task.Data.IsFreeTime) return;
            if (task.TodayStatus != TaskStatus.InProgress && task.TodayStatus != TaskStatus.AwaitingConfirmation) return;

            task.Fail();

            var definition = _taskTypeFeature.GetDefinition(task.Data.Type);
            if(definition != null)
            {
                foreach(var penalty in definition.FailurePenalties)
                {
                    _playerStatsFeature.AddStat(penalty.Type, penalty.Value);
                }
            }
            OnTaskStateChanged?.Invoke(task);
        }
        
        private void TrackTasks(float deltaTime)
        {
            if (DateTime.Now.Date != _lastDayGenerated.Date)
            {
                RegenerateAllWeeklyTasks();
                OnTaskListUpdated?.Invoke();
            }

            var now = DateTime.Now;
            var today = now.DayOfWeek;

            if (!_weeklyTasks.ContainsKey(today)) return;

            foreach (var task in _weeklyTasks[today])
            {
                if (task.Data.IsFreeTime || task.TodayStatus == TaskStatus.Completed || task.TodayStatus == TaskStatus.Failed) continue;

                var taskStartTime = _lastDayGenerated.Date + task.Data.StartTimeOfDay;
                var taskEndTime = _lastDayGenerated.Date + task.Data.EndTimeOfDay;
                var gracePeriodEndTime = taskEndTime + GracePeriod;
                
                if (task.TodayStatus == TaskStatus.Pending && now >= taskStartTime && now < taskEndTime)
                {
                    task.Start();
                    OnTaskStateChanged?.Invoke(task);
                }
                else if (task.TodayStatus == TaskStatus.InProgress && now >= taskEndTime && now < gracePeriodEndTime)
                {
                    task.SetAwaitingConfirmation();
                    OnTaskStateChanged?.Invoke(task);
                }
                else if ((task.TodayStatus == TaskStatus.InProgress || task.TodayStatus == TaskStatus.AwaitingConfirmation) && now >= gracePeriodEndTime)
                {
                    FailTask(task);
                }
            }
        }

        private void RegenerateAllWeeklyTasks()
        {
            _lastDayGenerated = DateTime.Now;
            _weeklyTasks.Clear();

            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                 var timeline = new List<Task>();
                var realTasksForDay = _taskTemplates
                    .Where(t => !t.IsFreeTime && t.RecurrenceDays.Contains(day))
                    .OrderBy(t => t.StartTimeOfDay)
                    .ToList();

                var currentTime = TimeSpan.Zero;
                
                foreach (var taskData in realTasksForDay)
                {
                    var gap = taskData.StartTimeOfDay - currentTime;
                    if (gap >= TimeSpan.FromMinutes(1))
                    {
                        timeline.Add(CreateFreeTimeSlot(currentTime, gap));
                    }

                    timeline.Add(new Task(taskData));
                    currentTime = taskData.EndTimeOfDay;
                }

                var endOfDay = TimeSpan.FromHours(24);
                if (currentTime < endOfDay)
                {
                    var finalGap = endOfDay - currentTime;
                    if (finalGap >= TimeSpan.FromMinutes(1))
                    {
                        timeline.Add(CreateFreeTimeSlot(currentTime, finalGap));
                    }
                }
                _weeklyTasks[day] = timeline;
            }
        }

        public bool IsTimeSlotAvailable(DayOfWeek day, TimeSpan start, TimeSpan end, string excludeTaskId)
        {
            var realTasksForDay = _taskTemplates
                .Where(t => !t.IsFreeTime && t.RecurrenceDays.Contains(day) && t.Id != excludeTaskId);

            foreach (var taskData in realTasksForDay)
            {
                if (start < taskData.EndTimeOfDay && end > taskData.StartTimeOfDay)
                {
                    return false;
                }
            }
            return true;
        }

        private Task CreateFreeTimeSlot(TimeSpan start, TimeSpan duration)
        {
            var freeTimeData = new TaskData
            {
                Name = "Вільний час",
                IsFreeTime = true,
                StartTimeOfDay = start,
                Duration = duration
            };
            return new Task(freeTimeData);
        }
        
       

        #endregion



       
        #region Save/Load Logic - Логіка збереження та завантаження

        private void SaveTasks()
        {
            foreach (var template in _taskTemplates)
            {
                template.PrepareForSerialization();
            }

            var wrapper = new TaskDataWrapper { TaskTemplates = _taskTemplates };
            string json = JsonUtility.ToJson(wrapper);

            _saveFeature.Save(json, SAVE_KEY);
            Debug.Log("Tasks saved successfully!");
        }

        private void LoadTasks()
        {
            string json = _saveFeature.Load(SAVE_KEY, "{}");
            TaskDataWrapper wrapper = JsonUtility.FromJson<TaskDataWrapper>(json);

            if (wrapper != null && wrapper.TaskTemplates != null)
            {
                _taskTemplates = wrapper.TaskTemplates;
                foreach (var template in _taskTemplates)
                {
                    template.RestoreTimeSpans();
                }
            }
            else
            {
                _taskTemplates = new List<TaskData>();
            }

            OnTaskListUpdated?.Invoke();
            Debug.Log("Tasks loaded!");
        }

        #endregion
    }
}
