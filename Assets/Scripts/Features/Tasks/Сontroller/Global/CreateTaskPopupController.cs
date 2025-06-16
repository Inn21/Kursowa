using System;
using System.Collections.Generic;
using System.Linq;
using Features.Tasks;
using Features.Tasks.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Core.Feature.Tasks.UI
{
    public class CreateTaskPopupController : MonoBehaviour
    {
        [Header("UI елементи")]
        [SerializeField] private GameObject _popupRoot;
        [SerializeField] private TMP_InputField _taskNameInput;
        [SerializeField] private TMP_Dropdown _taskTypeDropdown;
        [SerializeField] private TMP_Text _errorText;
        [SerializeField] private Button _createButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _deleteButton;

        [Header("Час Початку")]
        [SerializeField] private Slider _startHourSlider;
        [SerializeField] private Slider _startMinuteSlider;
        [SerializeField] private TMP_Text _startTimeText;

        [Header("Час Закінчення")]
        [SerializeField] private Slider _endHourSlider;
        [SerializeField] private Slider _endMinuteSlider;
        [SerializeField] private TMP_Text _endTimeText;

        [Header("Вибір кольору")]
        [SerializeField] private List<Button> _colorButtons;

        [Inject] private TasksFeature _tasksFeature;
        [Inject] private TaskTypeFeature _taskTypeFeature;
        
        
        private Color _selectedColor;
        private DayOfWeek _currentDay;
        private TasksUIController _ownerController;
        private bool _isEditMode;
        private string _editingTaskId;

        private TimeSpan _slotBoundaryStart;
        private TimeSpan _slotBoundaryEnd;

        private float _lastValidStartHour, _lastValidStartMinute, _lastValidEndHour, _lastValidEndMinute;
        private bool _isUpdating;

        private void Awake()
        {
            _createButton.onClick.AddListener(OnCreateOrUpdateClicked);
            _closeButton.onClick.AddListener(Hide);
            _deleteButton.onClick.AddListener(OnDeleteClicked);
        }
        
        private void SetupTaskTypeDropdown()
        {
            _taskTypeDropdown.ClearOptions();
            var typeDefinitions = _taskTypeFeature.GetAllTaskTypes();
            var options = typeDefinitions.Select(def => new TMP_Dropdown.OptionData(def.Name)).ToList();
            _taskTypeDropdown.AddOptions(options);
        }

        public void Show(DayOfWeek day, TimeSpan startTime, TimeSpan endTime, TasksUIController owner, TaskData taskToEdit = null)
        {
            _isEditMode = taskToEdit != null;
            _editingTaskId = _isEditMode ? taskToEdit.Id : null;
            _ownerController = owner;
            _currentDay = day;
            _taskNameInput.text = "";
            SelectColor(_colorButtons.Count > 0 ? _colorButtons[0].GetComponent<Image>().color : Color.white);

            SetupTaskTypeDropdown();
            
            _slotBoundaryStart = startTime;
            _slotBoundaryEnd = endTime;
            
            _deleteButton.gameObject.SetActive(_isEditMode);

            TimeSpan initialStartTime = _isEditMode ? taskToEdit.StartTimeOfDay : startTime;
            TimeSpan initialEndTime = _isEditMode ? taskToEdit.EndTimeOfDay : endTime;

            if(_isEditMode)
            {
                 _taskNameInput.text = taskToEdit.Name;
                 SelectColor(taskToEdit.TaskColor);
            }

            _startHourSlider.value = initialStartTime.Hours;
            _startMinuteSlider.value = initialStartTime.Minutes;
            
            if (initialEndTime.TotalHours >= 24)
            {
                _endHourSlider.value = 23;
                _endMinuteSlider.value = 59;
            }
            else
            {
                _endHourSlider.value = initialEndTime.Hours;
                _endMinuteSlider.value = initialEndTime.Minutes;
            }

            SubscribeSliders();
            ValidateAndCacheAll();
            _popupRoot.SetActive(true);
        }
        
        public void SubscribeSliders()
        {
            _startHourSlider.onValueChanged.AddListener(_ => OnStartTimeChanged());
            _startMinuteSlider.onValueChanged.AddListener(_ => OnStartTimeChanged());
            _endHourSlider.onValueChanged.AddListener(_ => OnEndTimeChanged());
            _endMinuteSlider.onValueChanged.AddListener(_ => OnEndTimeChanged());
        }

        public void UnsubscribeSliders()
        {
            _startHourSlider.onValueChanged.RemoveAllListeners();
            _startMinuteSlider.onValueChanged.RemoveAllListeners();
            _endHourSlider.onValueChanged.RemoveAllListeners();
            _endMinuteSlider.onValueChanged.RemoveAllListeners();
        }

        public void Hide()
        {
            UnsubscribeSliders();
            _popupRoot.SetActive(false);
        }

        private void OnStartTimeChanged()
        {
            if (_isUpdating) return;
            var newStartTime = GetCurrentStartTime();
            var endTime = GetCurrentEndTime();

            if (newStartTime < _slotBoundaryStart)
            {
                SetStartTime(_slotBoundaryStart);
            }
            else if (newStartTime >= endTime)
            {
                RevertStartTime();
                return;
            }
            ValidateAndCacheAll();
        }

        private void OnEndTimeChanged()
        {
            if (_isUpdating) return;
            var startTime = GetCurrentStartTime();
            var newEndTime = GetCurrentEndTime();

            if (newEndTime > _slotBoundaryEnd)
            {
                SetEndTime(_slotBoundaryEnd);
            }
            else if (newEndTime <= startTime)
            {
                RevertEndTime();
                return;
            }
            ValidateAndCacheAll();
        }

        private void ValidateAndCacheAll()
        {
            var startTime = GetCurrentStartTime();
            var endTime = GetCurrentEndTime();
            
            bool isAvailable = _tasksFeature.IsTimeSlotAvailable(_currentDay, startTime, endTime, _editingTaskId);
            _createButton.interactable = isAvailable;
            _errorText.gameObject.SetActive(!isAvailable);
            _errorText.text = isAvailable ? "" : "Час перетинається з іншим завданням!";

            UpdateUIText(startTime, endTime);
            CacheLastValidTime();
        }

        private void OnCreateOrUpdateClicked()
        {
            if (string.IsNullOrWhiteSpace(_taskNameInput.text))
            {
                _errorText.text = "Назва завдання не може бути пустою";
                _errorText.gameObject.SetActive(true);
                return;
            }

            var startTime = GetCurrentStartTime();
            var endTime = GetCurrentEndTime();
            
            var selectedTypeIndex = _taskTypeDropdown.value;
            var allTypes = _taskTypeFeature.GetAllTaskTypes();
            var selectedType = allTypes[selectedTypeIndex].Type;
            
            var taskData = new TaskData
            {
                Id = _editingTaskId ?? Guid.NewGuid().ToString(),
                Name = _taskNameInput.text,
                TaskColor = _selectedColor,
                StartTimeOfDay = startTime,
                Duration = endTime - startTime,
                RecurrenceDays = new List<DayOfWeek> { _currentDay },
                Type = selectedType
            };

            bool success = _isEditMode ? _tasksFeature.UpdateTask(taskData) : _tasksFeature.AddTask(taskData);
            
            if (success)
            {
                Hide();
            }
        }

        private void OnDeleteClicked()
        {
            if (_isEditMode)
            {
                _tasksFeature.RemoveTask(_editingTaskId);
                Hide();
            }
        }

        private void SelectColor(Color color)
        {
            _selectedColor = color;
        }

        private TimeSpan GetCurrentStartTime() => new TimeSpan((int)_startHourSlider.value, (int)_startMinuteSlider.value, 0);
        private TimeSpan GetCurrentEndTime() => new TimeSpan((int)_endHourSlider.value, (int)_endMinuteSlider.value, 0);

        private void SetStartTime(TimeSpan time)
        {
            _isUpdating = true;
            _startHourSlider.value = time.Hours;
            _startMinuteSlider.value = time.Minutes;
            _isUpdating = false;
        }
        
        private void SetEndTime(TimeSpan time)
        {
            _isUpdating = true;
            if (time.TotalHours >= 24)
            {
                 _endHourSlider.value = 23;
                 _endMinuteSlider.value = 59;
            }
            else
            {
                _endHourSlider.value = time.Hours;
                _endMinuteSlider.value = time.Minutes;
            }
            _isUpdating = false;
        }

        private void UpdateUIText(TimeSpan start, TimeSpan end)
        {
            _startTimeText.text = start.ToString(@"hh\:mm");
            _endTimeText.text = end.ToString(@"hh\:mm");
        }
        
        private void CacheLastValidTime()
        {
            _lastValidStartHour = _startHourSlider.value;
            _lastValidStartMinute = _startMinuteSlider.value;
            _lastValidEndHour = _endHourSlider.value;
            _lastValidEndMinute = _endMinuteSlider.value;
        }

        private void RevertStartTime()
        {
            _isUpdating = true;
            _startHourSlider.value = _lastValidStartHour;
            _startMinuteSlider.value = _lastValidStartMinute;
            _isUpdating = false;
        }

        private void RevertEndTime()
        {
            _isUpdating = true;
            _endHourSlider.value = _lastValidEndHour;
            _endMinuteSlider.value = _lastValidEndMinute;
            _isUpdating = false;
        }
    }
}
