using System;
using System.Collections.Generic;
using System.Linq;
using Core.Feature;
using Core.Feature.PlayerStats;
using Core.Feature.Save;
using Core.Feature.Tasks;
using Core.Utils.MonoUtils;
using Features.Notifications;
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
        [Inject] private readonly NotificationFeature _notificationFeature;

        private List<TaskData> _taskTemplates = new List<TaskData>();
        private readonly Dictionary<DayOfWeek, List<Task>> _weeklyTasks = new Dictionary<DayOfWeek, List<Task>>();
        private TaskStatusHistory _taskStatusHistory = new TaskStatusHistory();
        private DateTime _lastDayGenerated = DateTime.MinValue;

        private const string TEMPLATES_SAVE_KEY = "TaskTemplatesData";
        private const string STATUS_SAVE_KEY = "TaskStatusHistory";
        private readonly TimeSpan GracePeriod = TimeSpan.FromMinutes(30);
        
        private struct RuntimeTaskState
        {
            public string TaskTemplateId;
            public TaskStatus Status;
            public bool IsActionHandled;
        }

        public void Initialize()
        {
            _notificationFeature.Initialize();
            bool isNewUser = LoadData();
            
            RegenerateAllWeeklyTasks();
            ApplySavedStatusesForToday();
            
            if (isNewUser)
            {
                ForgivePastTasksForToday();
            }
            
            ScheduleAllNotifications();
            
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
            if (!IsTimeSlotAvailable(newTaskData.Day, newTaskData.StartTimeOfDay, newTaskData.EndTimeOfDay, null))
                return false;
            
            _taskTemplates.Add(newTaskData);
            RegenerateWithStatePreservation();
            ScheduleAllNotifications();
            OnTaskListUpdated?.Invoke();
            return true;
        }

        public bool UpdateTask(TaskData updatedTaskData)
        {
            if (!IsTimeSlotAvailable(updatedTaskData.Day, updatedTaskData.StartTimeOfDay, updatedTaskData.EndTimeOfDay, updatedTaskData.Id))
                return false;

            var taskIndex = _taskTemplates.FindIndex(t => t.Id == updatedTaskData.Id);
            if (taskIndex != -1)
            {
                _taskTemplates[taskIndex] = updatedTaskData;
                RegenerateWithStatePreservation();
                ScheduleAllNotifications();
                OnTaskListUpdated?.Invoke();
                return true;
            }
            return false;
        }

        public void RemoveTask(string taskId)
        {
            var taskToRemove = _taskTemplates.FirstOrDefault(t => t.Id == taskId);
            if (taskToRemove == null) return;
            
            var weeklyTaskInstance = _weeklyTasks[taskToRemove.Day].FirstOrDefault(t => t.Data.Id == taskId);
            if (weeklyTaskInstance != null)
            {
                _notificationFeature.CancelNotification(weeklyTaskInstance.Id);
            }

            _taskTemplates.Remove(taskToRemove);
            RegenerateWithStatePreservation();
            OnTaskListUpdated?.Invoke();
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
                    return activeTask;
            }

            for (int i = 0; i < 7; i++)
            {
                var checkDay = now.AddDays(i).DayOfWeek;
                if (!_weeklyTasks.ContainsKey(checkDay)) continue;
                
                var upcomingTask = _weeklyTasks[checkDay]
                    .Where(t => !t.Data.IsFreeTime && t.TodayStatus == TaskStatus.Pending)
                    .OrderBy(t => t.Data.StartTimeOfDay)
                    .FirstOrDefault(t => i > 0 || t.Data.StartTimeOfDay > timeOfDay);

                if (upcomingTask != null)
                    return upcomingTask;
            }
            
            return null;
        }

        public void CompleteTask(Task task)
        {
            if(task == null || task.Data.IsFreeTime || task.IsActionHandled || (task.TodayStatus != TaskStatus.InProgress && task.TodayStatus != TaskStatus.AwaitingConfirmation)) return;
            
            task.Complete();
            task.SetActionHandled();
            _notificationFeature.CancelNotification(task.Id);
            
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
            _notificationFeature.CancelNotification(task.Id);

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
                ScheduleAllNotifications();
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

        private void RegenerateWithStatePreservation()
        {
            var runtimeState = new List<RuntimeTaskState>();
            var today = DateTime.Today;
            if (_weeklyTasks.ContainsKey(today.DayOfWeek))
            {
                foreach (var task in _weeklyTasks[today.DayOfWeek])
                {
                    if (!task.Data.IsFreeTime)
                    {
                        runtimeState.Add(new RuntimeTaskState
                        {
                            TaskTemplateId = task.Data.Id,
                            Status = task.TodayStatus,
                            IsActionHandled = task.IsActionHandled
                        });
                    }
                }
            }

            RegenerateAllWeeklyTasks();

            foreach (var record in runtimeState)
            {
                var task = _weeklyTasks[today.DayOfWeek]
                    .FirstOrDefault(t => t.Data.Id == record.TaskTemplateId);
                
                if (task != null)
                    task.ApplyLoadedStatus(record.Status, record.IsActionHandled);
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
                .Where(t => !t.IsFreeTime && t.Day == day)
                .OrderBy(t => t.StartTimeOfDay)
                .ToList();

            var currentTime = TimeSpan.Zero;
            var endOfDay = new TimeSpan(23, 59, 0);
            
            foreach (var taskData in realTasksForDay)
            {
                if (taskData.StartTimeOfDay > endOfDay) continue;

                var gap = taskData.StartTimeOfDay - currentTime;
                if (gap >= TimeSpan.FromMinutes(1))
                    timeline.Add(CreateFreeTimeSlot(currentTime, gap));
                
                timeline.Add(new Task(taskData));
                currentTime = taskData.EndTimeOfDay;
            }
            
            if (currentTime < endOfDay)
            {
                var finalGap = endOfDay - currentTime;
                if (finalGap >= TimeSpan.FromMinutes(1))
                    timeline.Add(CreateFreeTimeSlot(currentTime, finalGap));
            }
            return timeline;
        }
        
        private void ScheduleAllNotifications()
        {
            _notificationFeature.CancelAllNotifications();
            for (int i = 0; i < 7; i++)
            {
                var day = DateTime.Today.AddDays(i).DayOfWeek;
                if (_weeklyTasks.ContainsKey(day))
                {
                    foreach (var task in _weeklyTasks[day])
                    {
                        if (!task.Data.IsFreeTime && task.TodayStatus == TaskStatus.Pending)
                        {
                            _notificationFeature.ScheduleNotification(task);
                        }
                    }
                }
            }
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
                    task.ApplyLoadedStatus(record.Status, true);
            }
        }

        private void ForgivePastTasksForToday()
        {
            var timeNow = DateTime.Now.TimeOfDay;
            foreach (var task in _weeklyTasks[DateTime.Today.DayOfWeek])
            {
                if (!task.Data.IsFreeTime && task.Data.EndTimeOfDay < timeNow)
                {
                    task.SetActionHandled();
                }
            }
        }

        public bool IsTimeSlotAvailable(DayOfWeek day, TimeSpan start, TimeSpan end, string excludeTaskId)
        {
            return !_taskTemplates.Any(t => !t.IsFreeTime && t.Id != excludeTaskId && 
                                            t.Day == day && 
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

        private bool LoadData()
        {
            string templatesJson = _saveFeature.Load(TEMPLATES_SAVE_KEY, "{}");
            if (string.IsNullOrEmpty(templatesJson) || templatesJson == "{}")
            {
                CreateDefaultSchedule();
                string historyJson = _saveFeature.Load(STATUS_SAVE_KEY, "{}");
                _taskStatusHistory = JsonUtility.FromJson<TaskStatusHistory>(historyJson) ?? new TaskStatusHistory();
                return true;
            }
            
            var templatesWrapper = JsonUtility.FromJson<TaskDataWrapper>(templatesJson);
            _taskTemplates = templatesWrapper?.TaskTemplates ?? new List<TaskData>();
            if (_taskTemplates.Count == 0)
            {
                CreateDefaultSchedule();
                string historyJson = _saveFeature.Load(STATUS_SAVE_KEY, "{}");
                _taskStatusHistory = JsonUtility.FromJson<TaskStatusHistory>(historyJson) ?? new TaskStatusHistory();
                return true;
            }

            foreach (var template in _taskTemplates)
                template.RestoreTimeSpans();

            string historyJsonOnLoad = _saveFeature.Load(STATUS_SAVE_KEY, "{}");
            _taskStatusHistory = JsonUtility.FromJson<TaskStatusHistory>(historyJsonOnLoad) ?? new TaskStatusHistory();
            return false;
        }

        private void CreateDefaultSchedule()
        {
            _taskTemplates = new List<TaskData>();
            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                bool isWorkDay = day != DayOfWeek.Saturday && day != DayOfWeek.Sunday;

                _taskTemplates.Add(new TaskData { Name = "Вечірній сон", Type = TaskType.Sleep, StartTimeOfDay = new TimeSpan(23, 0, 0), Duration = new TimeSpan(1, 0, 0), Day = day });
                _taskTemplates.Add(new TaskData { Name = "Ранковий сон", Type = TaskType.Sleep, StartTimeOfDay = new TimeSpan(0, 0, 0), Duration = new TimeSpan(7, 0, 0), Day = day });
                _taskTemplates.Add(new TaskData { Name = "Ранкова гігієна", Type = TaskType.Hygiene, StartTimeOfDay = new TimeSpan(7, 0, 0), Duration = new TimeSpan(0, 30, 0), Day = day });
                _taskTemplates.Add(new TaskData { Name = "Сніданок", Type = TaskType.Eating, StartTimeOfDay = new TimeSpan(7, 30, 0), Duration = new TimeSpan(0, 30, 0), Day = day });
                
                if (isWorkDay)
                {
                    _taskTemplates.Add(new TaskData { Name = "Робота/Навчання", Type = TaskType.WorkAndStudy, StartTimeOfDay = new TimeSpan(9, 0, 0), Duration = new TimeSpan(8, 0, 0), Day = day });
                    _taskTemplates.Add(new TaskData { Name = "Обід", Type = TaskType.Eating, StartTimeOfDay = new TimeSpan(13, 0, 0), Duration = new TimeSpan(1, 0, 0), Day = day });
                }

                _taskTemplates.Add(new TaskData { Name = "Вечеря", Type = TaskType.Eating, StartTimeOfDay = new TimeSpan(19, 0, 0), Duration = new TimeSpan(0, 45, 0), Day = day });
            }
            
            foreach (var template in _taskTemplates)
                template.PrepareForSerialization();
        }
    }
}
