using Core.Feature;
using Features.Tasks.Model;
using System;
using System.Collections.Generic;
using Unity.Notifications.Android;
using UnityEngine;

namespace Features.Notifications
{
    public class NotificationFeature : BaseFeature
    {
        private const string CHANNEL_ID = "task_notifications_channel";

        public void Initialize()
        {
            var channel = new AndroidNotificationChannel()
            {
                Id = CHANNEL_ID,
                Name = "Task Notifications",
                Importance = Importance.Default,
                Description = "Pushes reminders about upcoming tasks",
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);
        }

        public void ScheduleNotification(Task task)
        {
            var taskData = task.Data;
            var fireTime = DateTime.Today + taskData.StartTimeOfDay;

            if (fireTime < DateTime.Now) return;

            var notification = new AndroidNotification
            {
                Title = "Upcoming Task!",
                Text = $"Your task '{taskData.Name}' is about to start.",
                FireTime = fireTime
            };

            var notificationId = GenerateNotificationId(task.Id);
            AndroidNotificationCenter.SendNotificationWithExplicitID(notification, CHANNEL_ID, notificationId);
        }

        public void CancelNotification(Guid taskId)
        {
            var notificationId = GenerateNotificationId(taskId);
            AndroidNotificationCenter.CancelNotification(notificationId);
        }

        public void CancelAllNotifications()
        {
            AndroidNotificationCenter.CancelAllScheduledNotifications();
        }
        
        private int GenerateNotificationId(Guid taskId)
        {
            return Math.Abs(taskId.GetHashCode());
        }
    }
}