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
        public event Action<Task> OnTaskStarted;
        public event Action<Task> OnTaskCompleted;
        public event Action<Task> OnTaskFailed;


        private List<TaskData> _taskTemplates = new List<TaskData>();
        private readonly Dictionary<DayOfWeek, List<Task>> _weeklyTasks = new Dictionary<DayOfWeek, List<Task>>();
        private Task _currentTrackedTask;
        private DateTime _lastCheckTime = DateTime.MinValue;

        private const string SAVE_KEY = "WeeklyTasks";

        public void Initialize()
        {
            LoadTasks();
            _monoFeature.OnApplicationQuitEvent += SaveTasks;
        }

        public override void Dispose()
        {
            if (_monoFeature != null)
                _monoFeature.OnApplicationQuitEvent -= SaveTasks;
            base.Dispose();
        }

        #region Public API - Методи для керування завданнями

        public List<Task> GetTasksForDay(DayOfWeek day)
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

            return timeline;
        }

        public bool AddTask(TaskData newTaskData)
        {
            if (!IsTimeSlotAvailable(newTaskData.RecurrenceDays.First(), newTaskData.StartTimeOfDay,
                    newTaskData.EndTimeOfDay, null))
            {
                Debug.LogWarning("Time slot is not available!");
                return false;
            }

            _taskTemplates.Add(newTaskData);

            OnTaskListUpdated?.Invoke();
            return true;
        }
        
        public bool UpdateTask(TaskData updatedTaskData)
        {
            if (!IsTimeSlotAvailable(updatedTaskData.RecurrenceDays.First(), updatedTaskData.StartTimeOfDay, updatedTaskData.EndTimeOfDay, updatedTaskData.Id))
            {
                Debug.LogWarning("Updated time slot is not available!");
                return false;
            }

            var taskIndex = _taskTemplates.FindIndex(t => t.Id == updatedTaskData.Id);
            if (taskIndex != -1)
            {
                _taskTemplates[taskIndex] = updatedTaskData;
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
                OnTaskListUpdated?.Invoke();
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
        
        public void CompleteTask(Task task)
        {
            if(task == null || task.Data.IsFreeTime || task.TodayStatus != TaskStatus.InProgress) return;
    
            task.Complete();
    
            var definition = _taskTypeFeature.GetDefinition(task.Data.Type);
            if(definition != null)
            {
                foreach(var reward in definition.CompletionRewards)
                {
                    _playerStatsFeature.AddStat(reward.Type, reward.Value);
                }
            }
            OnTaskListUpdated?.Invoke();
        }

        public void FailTask(Task task)
        {
            if(task == null || task.Data.IsFreeTime || task.TodayStatus != TaskStatus.InProgress) return;

            task.Fail();

            var definition = _taskTypeFeature.GetDefinition(task.Data.Type);
            if(definition != null)
            {
                foreach(var penalty in definition.FailurePenalties)
                {
                    _playerStatsFeature.AddStat(penalty.Type, penalty.Value);
                }
            }
            OnTaskListUpdated?.Invoke();
        }

        #endregion

        #region Core Logic - Основна логіка роботи фічі


        private void TrackTasks(float deltaTime)
        {
            var now = DateTime.Now;


            if (now.Date != _lastCheckTime.Date)
            {
                RegenerateWeeklyTasks();
            }

            _lastCheckTime = now;

            var today = now.DayOfWeek;
            if (!_weeklyTasks.ContainsKey(today)) return;

            var tasksForToday = _weeklyTasks[today];

            foreach (var task in tasksForToday)
            {
                var taskStartTime = now.Date + task.Data.StartTimeOfDay;
                var taskEndTime = now.Date + task.Data.EndTimeOfDay;

                // Перевірка на початок завдання
                if (task.TodayStatus == TaskStatus.Pending && now >= taskStartTime && now < taskEndTime)
                {
                    task.Start();
                    _currentTrackedTask = task;
                    OnTaskStarted?.Invoke(task);
                }

                else if (task.TodayStatus == TaskStatus.InProgress && now >= taskEndTime)
                {

                }
            }
        }


        private void RegenerateWeeklyTasks()
        {
            _weeklyTasks.Clear();
            _currentTrackedTask = null;

            foreach (var day in Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>())
            {
                _weeklyTasks[day] = new List<Task>();
            }

            foreach (var template in _taskTemplates)
            {
                template.RestoreTimeSpans();
                foreach (var day in template.RecurrenceDays)
                {
                    var newTask = new Task(template);
                    _weeklyTasks[day].Add(newTask);
                }
            }


            foreach (var dayTasks in _weeklyTasks.Values)
            {
                dayTasks.Sort((a, b) => a.Data.StartTimeOfDay.CompareTo(b.Data.StartTimeOfDay));
            }

            OnTaskListUpdated?.Invoke();
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
