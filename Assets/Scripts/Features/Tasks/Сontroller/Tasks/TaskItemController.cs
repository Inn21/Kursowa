using System;
using Features.Tasks;
using Features.Tasks.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Core.Feature.Tasks.UI
{
    public class TaskItemController : MonoBehaviour
    {
        [Inject] TasksFeature _tasksFeature;
        [Inject] TaskTypeFeature _taskTypeFeature;
        
        [Header("Основні елементи")]
        [SerializeField] private TextMeshProUGUI _taskTitleText;
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private Image _taskIcon; 
        [SerializeField] private GameObject _disableOverlay; 
        [SerializeField] private Button _editButton;
        
        // TODO: Додати логіку для відображення нагород (Stats)

        private Task _task;
        private TasksUIController _ownerController;

        
       
        public void Initialize(Task task, TasksUIController owner)
        {
            _task = task;
            _ownerController = owner;

            _taskTitleText.text = _task.Data.Name;
            
            var startTime = _task.Data.StartTimeOfDay.ToString(@"hh\:mm");
            var endTime = _task.Data.EndTimeOfDay.ToString(@"hh\:mm");
            var durationMinutes = _task.Data.Duration.TotalMinutes.ToString("F0");
            _timeText.text = $"{startTime} - {endTime} ({durationMinutes} хв)";
            
            var definition = _taskTypeFeature.GetDefinition(_task.Data.Type);
            if(definition != null && definition.Icon != null)
            {
                _taskIcon.sprite = definition.Icon;
                _taskIcon.color = Color.white;
            }
            else
            {
                _taskIcon.color = Color.clear;
            }

            bool isRealTask = !_task.Data.IsFreeTime;
            _editButton.gameObject.SetActive(isRealTask);
            if(isRealTask)
            {
                _editButton.onClick.AddListener(OnEditClicked);
            }
        }

        private void OnEditClicked()
        {
            _ownerController.ShowEditPopup(_task.Data);
        }

        private void OnDestroy()
        {
            if (_editButton != null)
            {
                _editButton.onClick.RemoveListener(OnEditClicked);
            }
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
                    _disableOverlay.SetActive(true); 
                    break;
            }
        }

        #region Interaction Handlers
       
        public void MarkAsDone()
        {
            if (_task.TodayStatus == TaskStatus.InProgress || _task.TodayStatus == TaskStatus.Pending)
            {
                Debug.Log($"Task '{_task.Data.Name}' marked as DONE.");
            }
        }
        
        public void MarkAsFailed()
        {
            if (_task.TodayStatus == TaskStatus.InProgress || _task.TodayStatus == TaskStatus.Pending)
            {
                Debug.Log($"Task '{_task.Data.Name}' marked as FAILED.");
            }
        }

        #endregion
    }
}
