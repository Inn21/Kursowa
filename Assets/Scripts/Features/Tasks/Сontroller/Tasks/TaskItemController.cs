using System;
using Core.Feature.Tasks.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Core.Feature.Tasks.UI
{
    public class TaskItemController : MonoBehaviour
    {
        [Inject] TasksFeature _tasksFeature;
        
        [Header("Основні елементи")]
        [SerializeField] private TextMeshProUGUI _taskTitleText;
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private Image _taskIcon; 
        [SerializeField] private GameObject _disableOverlay; 
        
        // TODO: Додати логіку для відображення нагород (Stats)

        private Task _task;

       
        public void Initialize(Task task)
        {
            _task = task;

           
            _taskTitleText.text = _task.Data.Name;
            
           
            var startTime = _task.Data.StartTimeOfDay.ToString(@"hh\:mm");
            var endTime = _task.Data.EndTimeOfDay.ToString(@"hh\:mm") ?? "N/A";
            var durationMinutes = _task.Data.Duration.TotalMinutes.ToString("F0") ?? "0";
            _timeText.text = $"{startTime} - {endTime} ({durationMinutes} хв)";
            
            // TODO: Встановити іконку _taskIcon на основі _task.Data.Type

            UpdateVisualState();
        }

        private void OnEnable()
        {          
            _tasksFeature.OnTaskCompleted += HandleTaskStateChanged;
            _tasksFeature.OnTaskFailed += HandleTaskStateChanged;
            _tasksFeature.OnTaskStarted += HandleTaskStateChanged;
        }

        private void OnDisable()
        {
            // Відписуємось, щоб уникнути витоків пам'яті
            _tasksFeature.OnTaskCompleted -= HandleTaskStateChanged;
            _tasksFeature.OnTaskFailed -= HandleTaskStateChanged;
            _tasksFeature.OnTaskStarted -= HandleTaskStateChanged;
        }
        
        private void HandleTaskStateChanged(Task updatedTask)
        {
            if (_task != null && _task.Id == updatedTask.Id)
            {
                UpdateVisualState();
            }
        }

        private void UpdateVisualState()
        {
            if (_task == null) return;

            switch (_task.TodayStatus)
            {
                case TaskStatus.Pending:
                    _disableOverlay.SetActive(false);
                    // TODO: Додати візуал для поточного завдання, якщо воно активне
                    break;
                case TaskStatus.InProgress:
                    _disableOverlay.SetActive(false);
                    // TODO: Виділити завдання (напр. рамкою) як поточне
                    break;
                case TaskStatus.Completed:
                case TaskStatus.Failed:
                    _disableOverlay.SetActive(true); // Показуємо "заблокований" стан
                    break;
            }
        }

        #region Interaction Handlers
        // Ці методи будуть викликатися з логіки свайпів у майбутньому.
        // Поки що їх можна викликати з кнопок для тестування.

        /// <summary>
        /// Обробник для позначення завдання як "Виконано".
        /// </summary>
        public void MarkAsDone()
        {
            if (_task.TodayStatus == TaskStatus.InProgress || _task.TodayStatus == TaskStatus.Pending)
            {
                // Викликаємо метод з основної фічі
                // _tasksFeature.MarkTaskAsCompleted(_task);
                Debug.Log($"Task '{_task.Data.Name}' marked as DONE.");
            }
        }

        /// <summary>
        /// Обробник для позначення завдання як "Провалено".
        /// </summary>
        public void MarkAsFailed()
        {
            if (_task.TodayStatus == TaskStatus.InProgress || _task.TodayStatus == TaskStatus.Pending)
            {
                // Викликаємо метод з основної фічі
                // _tasksFeature.MarkTaskAsFailed(_task);
                Debug.Log($"Task '{_task.Data.Name}' marked as FAILED.");
            }
        }

        #endregion
    }
}
