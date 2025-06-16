using System;
using System.Collections.Generic;
using System.Linq;
using Core.Feature.Tasks.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Core.Feature.Tasks.UI
{
    public class TasksUIController : MonoBehaviour
    {
        [Header("Налаштування спауну")]
        [SerializeField] private GameObject _taskItemPrefab;
        [SerializeField] private GameObject _freeTimeItemPrefab; 
        [SerializeField] private Transform _tasksContainer;

        [Header("Навігація і прогрес")]
        [SerializeField] private List<Button> _dayButtons;
        [SerializeField] private Slider _dayProgressSlider;
        [SerializeField] private TextMeshProUGUI _dayProgressText;
        
        [Header("Попап створення")]
        [SerializeField] private CreateTaskPopupController _createTaskPopup;

        [Header("Кольори")]
        [SerializeField] private Color _selectedDayColor = Color.white;
        [SerializeField] private Color _defaultDayColor = Color.gray;

        [Inject] TasksFeature _tasksFeature;
        [Inject] DiContainer _diContainer;
        
        private List<GameObject> _spawnedTaskItems = new List<GameObject>();
        private DayOfWeek _selectedDay;

        private void Start()
        {
            for (int i = 0; i < _dayButtons.Count; i++)
            {
                int dayIndex = i; 
                _dayButtons[i].onClick.AddListener(() => SelectDay(ConvertButtonIndexToDayOfWeek(dayIndex)));
            }
            
            SelectDay(DateTime.Now.DayOfWeek);
        }

        private void OnEnable()
        {
            _tasksFeature.OnTaskListUpdated += UpdateFullView;
           
        }

        private void OnDisable()
        {
            _tasksFeature.OnTaskListUpdated -= UpdateFullView;
        }
        
        
        public void ShowCreationPopup(DayOfWeek day, TimeSpan startTime, TimeSpan endTime)
        {
            _createTaskPopup.Show(day, startTime, endTime);
        }
        
        private void SelectDay(DayOfWeek day)
        {
            _selectedDay = day;
            UpdateFullView();
        }
        
        private void UpdateFullView()
        {
            UpdateTasksList();
            UpdateDayProgress();
            UpdateDayButtonsVisuals();
        }
        
        private void UpdateTasksList()
        {
            foreach (var item in _spawnedTaskItems)
            {
                Destroy(item);
            }
            _spawnedTaskItems.Clear();

            var timeline = _tasksFeature.GetTasksForDay(_selectedDay);

            foreach (var task in timeline)
            {
                GameObject prefabToSpawn = task.Data.IsFreeTime ? _freeTimeItemPrefab : _taskItemPrefab;
                var newItemObject = _diContainer.InstantiatePrefab(prefabToSpawn, _tasksContainer);

                if (task.Data.IsFreeTime)
                {
                    var controller = newItemObject.GetComponent<FreeTimeItemController>();
                    controller.Initialize(task, _selectedDay, this);
                }
                else
                {
                    var controller = newItemObject.GetComponent<TaskItemController>();
                    controller.Initialize(task);
                }
                _spawnedTaskItems.Add(newItemObject);
            }
        }
        
        private void UpdateDayProgress()
        {
            var realTasks = _tasksFeature.GetTasksForDay(_selectedDay).Where(t => !t.Data.IsFreeTime).ToList();
            if (realTasks.Count == 0)
            {
                _dayProgressText.text = "Немає завдань";
                _dayProgressSlider.value = 0;
                return;
            }
            int completedCount = realTasks.Count(t => t.TodayStatus == TaskStatus.Completed);
            _dayProgressText.text = $"Прогрес дня {completedCount}/{realTasks.Count}";
            _dayProgressSlider.value = (float)completedCount / realTasks.Count;
        }

        private void UpdateDayButtonsVisuals()
        {
            int selectedButtonIndex = ConvertDayOfWeekToButtonIndex(_selectedDay);
            for (int i = 0; i < _dayButtons.Count; i++)
            {
                var buttonImage = _dayButtons[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = (i == selectedButtonIndex) ? _selectedDayColor : _defaultDayColor;
                }
            }
        }
        
        
      
        private DayOfWeek ConvertButtonIndexToDayOfWeek(int index)
        {
            if (index == 6) return DayOfWeek.Sunday; 
            return (DayOfWeek)(index + 1); 
        }
        
        private int ConvertDayOfWeekToButtonIndex(DayOfWeek day)
        {
            if (day == DayOfWeek.Sunday) return 6;
            return (int)day - 1;
        }
    }
}
