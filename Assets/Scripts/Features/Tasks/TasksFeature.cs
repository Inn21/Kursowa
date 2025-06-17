using System;
using System.Collections.Generic;
using System.Linq;
using _PROJECT.Scripts.Application.Features.Save;
using Core.Feature;
using Core.Feature.PlayerStats;
using Core.Feature.Tasks;
using Core.Utils.MonoUtils;
using Features.Tasks.Model;
using UnityEngine;
using Zenject;

namespace Features.Tasks
{
    public class TasksFeature : BaseFeature
    {
        public event Action OnTaskListUpdated;
        public event Action<Task> OnTaskStateChanged;
        
        [Inject] private readonly ISaveFeature _saveFeature;
        [Inject] private readonly MonoFeature _monoFeature;
        [Inject] private readonly PlayerStatsFeature _playerStatsFeature;
        [Inject] private readonly TaskTypeFeature _taskTypeFeature;

        private List<TaskData> _taskTemplates = new List<TaskData>();
        private readonly Dictionary<DayOfWeek, List<Task>> _weeklyTasks = new Dictionary<DayOfWeek, List<Task>>();
        private TaskStatusHistory _taskStatusHistory = new TaskStatusHistory();
        private DateTime _lastDayGenerated = DateTime.MinValue;

        private const string TEMPLATES_SAVE_KEY = "TaskTemplatesData";
        private const string STATUS_SAVE_KEY = "TaskStatusHistory";
        private readonly TimeSpan GracePeriod = TimeSpan.FromMinutes(30);

        public void Initialize()
        {
            LoadData();
            RegenerateAllWeeklyTasks();
            ApplySavedStatusesForToday();
            _monoFeature.OnPerSecondUpdate += TrackTasks;
            _monoFeature.OnApplicationQuitEvent += SaveData;
        }

        public override void Dispose()
        {
            if (_monoFeature != null)
            {
                _monoFeature.OnPerSecondUpdate -= TrackTasks;
                _monoFeature.OnApplicationQuitEvent -= SaveData;
            }
            base.Dispose();
        }

        public bool AddTask(TaskData newTaskData)
        {
            if (!IsTimeSlotAvailable(newTaskData.RecurrenceDays.First(), newTaskData.StartTimeOfDay, newTaskData.EndTimeOfDay, null))
                return false;
            
            _taskTemplates.Add(newTaskData);
            RegenerateAllWeeklyTasks();
            ApplySavedStatusesForToday();
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
                ApplySavedStatusesForToday();
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
                ApplySavedStatusesForToday();
                OnTaskListUpdated?.Invoke();
            }
        }
        
        public List<Task> GetTasksForDay(DayOfWeek day)
        {
            return _weeklyTasks.ContainsKey(day) ? _weeklyTasks[day] : new List<Task>();
        }

        public Task GetCurrentOrNextTask()
        {
            var now = DateTime.Now;
            var dayOfWeek = now.DayOfWeek;
            var timeOfDay = now.TimeOfDay;

            if (_weeklyTasks.ContainsKey(dayOfWeek))
            {
                var activeTask = _weeklyTasks[dayOfWeek]
                    .FirstOrDefault(t => !t.Data.IsFreeTime && (t.TodayStatus == TaskStatus.InProgress || t.TodayStatus == TaskStatus.AwaitingConfirmation));
                if (activeTask != null)
                {
                    return activeTask;
                }
            }

            for (int i = 0; i < 7; i++)
            {
                var checkDay = now.AddDays(i).DayOfWeek;
                if (!_weeklyTasks.ContainsKey(checkDay)) continue;

                var tasksForDay = _weeklyTasks[checkDay];
                
                var upcomingTask = tasksForDay
                    .Where(t => !t.Data.IsFreeTime && t.TodayStatus == TaskStatus.Pending)
                    .OrderBy(t => t.Data.StartTimeOfDay)
                    .FirstOrDefault(t => i > 0 || t.Data.StartTimeOfDay > timeOfDay);

                if (upcomingTask != null)
                {
                    return upcomingTask;
                }
            }
            
            return null;
        }

        public void CompleteTask(Task task)
        {
            if(task == null || task.Data.IsFreeTime || task.IsActionHandled || (task.TodayStatus != TaskStatus.InProgress && task.TodayStatus != TaskStatus.AwaitingConfirmation)) return;
            
            task.Complete();
            task.SetActionHandled();
            
            var definition = _taskTypeFeature.GetDefinition(task.Data.Type);
            if(definition != null)
            {
                foreach(var reward in definition.CompletionRewards)
                    _playerStatsFeature.AddStat(reward.Type, reward.Value);
            }
            OnTaskStateChanged?.Invoke(task);
        }

        public void FailTask(Task task)
        {
            if(task == null || task.Data.IsFreeTime || task.IsActionHandled) return;

            task.Fail();
            task.SetActionHandled();

            var definition = _taskTypeFeature.GetDefinition(task.Data.Type);
            if(definition != null)
            {
                foreach(var penalty in definition.FailurePenalties)
                    _playerStatsFeature.AddStat(penalty.Type, penalty.Value);
            }
            OnTaskStateChanged?.Invoke(task);
        }
        
        private void TrackTasks(float deltaTime)
        {
            if (DateTime.Today != _lastDayGenerated.Date)
            {
                RegenerateAllWeeklyTasks();
                ApplySavedStatusesForToday();
                OnTaskListUpdated?.Invoke();
            }

            var now = DateTime.Now;
            var today = now.DayOfWeek;

            if (!_weeklyTasks.ContainsKey(today)) return;

            foreach (var task in _weeklyTasks[today])
            {
                if (task.Data.IsFreeTime || task.IsActionHandled) continue;

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
                else if (now >= gracePeriodEndTime)
                {
                    FailTask(task);
                }
            }
        }

        private void RegenerateAllWeeklyTasks()
        {
            _lastDayGenerated = DateTime.Today;
            _weeklyTasks.Clear();

            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                _weeklyTasks[day] = GenerateTimelineForDay(day);
            }
        }
        
        private List<Task> GenerateTimelineForDay(DayOfWeek day)
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
                    timeline.Add(CreateFreeTimeSlot(currentTime, gap));
                
                timeline.Add(new Task(taskData));
                currentTime = taskData.EndTimeOfDay;
            }
            
            var endOfDay = TimeSpan.FromHours(24);
            if (currentTime < endOfDay)
            {
                var finalGap = endOfDay - currentTime;
                if (finalGap >= TimeSpan.FromMinutes(1))
                    timeline.Add(CreateFreeTimeSlot(currentTime, finalGap));
            }
            return timeline;
        }

        private void ApplySavedStatusesForToday()
        {
            var todayStr = DateTime.Today.ToString("yyyy-MM-dd");
            var todayRecords = _taskStatusHistory.Records.Where(r => r.Date == todayStr).ToList();

            foreach (var record in todayRecords)
            {
                var task = _weeklyTasks[DateTime.Today.DayOfWeek]
                    .FirstOrDefault(t => t.Data.Id == record.TaskTemplateId);
                
                if (task != null)
                    task.ApplyLoadedStatus(record.Status);
            }
        }

        public bool IsTimeSlotAvailable(DayOfWeek day, TimeSpan start, TimeSpan end, string excludeTaskId)
        {
            return !_taskTemplates.Any(t => !t.IsFreeTime && t.Id != excludeTaskId && 
                                            t.RecurrenceDays.Contains(day) && 
                                            start < t.EndTimeOfDay && end > t.StartTimeOfDay);
        }

        private Task CreateFreeTimeSlot(TimeSpan start, TimeSpan duration)
        {
            var freeTimeData = new TaskData { Name = "Вільний час", IsFreeTime = true, StartTimeOfDay = start, Duration = duration };
            return new Task(freeTimeData);
        }

        private void SaveData()
        {
            foreach (var template in _taskTemplates)
                template.PrepareForSerialization();
            
            var templatesWrapper = new TaskDataWrapper { TaskTemplates = _taskTemplates };
            _saveFeature.Save(JsonUtility.ToJson(templatesWrapper), TEMPLATES_SAVE_KEY);
            
            var todayStr = DateTime.Today.ToString("yyyy-MM-dd");
            _taskStatusHistory.Records.RemoveAll(r => r.Date == todayStr);
            
            foreach (var task in _weeklyTasks[DateTime.Today.DayOfWeek])
            {
                if (!task.Data.IsFreeTime && task.IsActionHandled)
                {
                    _taskStatusHistory.Records.Add(new TaskStatusRecord
                    {
                        TaskTemplateId = task.Data.Id,
                        Date = todayStr,
                        Status = task.TodayStatus
                    });
                }
            }
            _saveFeature.Save(JsonUtility.ToJson(_taskStatusHistory), STATUS_SAVE_KEY);
        }

        private void LoadData()
        {
            string templatesJson = _saveFeature.Load(TEMPLATES_SAVE_KEY, "{}");
            var templatesWrapper = JsonUtility.FromJson<TaskDataWrapper>(templatesJson);
            _taskTemplates = templatesWrapper?.TaskTemplates ?? new List<TaskData>();
            foreach (var template in _taskTemplates)
                template.RestoreTimeSpans();

            string historyJson = _saveFeature.Load(STATUS_SAVE_KEY, "{}");
            _taskStatusHistory = JsonUtility.FromJson<TaskStatusHistory>(historyJson) ?? new TaskStatusHistory();
        }
    }
}
