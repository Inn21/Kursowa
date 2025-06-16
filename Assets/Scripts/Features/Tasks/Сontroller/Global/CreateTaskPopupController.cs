using System;
using System.Collections.Generic;
using Core.Feature.Tasks.Model;
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
        [SerializeField] private TMP_Text _errorText;
        [SerializeField] private Button _createButton;
        [SerializeField] private Button _closeButton;
        
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
        private Color _selectedColor;
        private DayOfWeek _currentDay;

        private TimeSpan _slotBoundaryStart;
        private TimeSpan _slotBoundaryEnd;

        private float _lastValidStartHour, _lastValidStartMinute, _lastValidEndHour, _lastValidEndMinute;
        private bool _isUpdating;

        private void Awake()
        {
            _createButton.onClick.AddListener(OnCreateClicked);
            _closeButton.onClick.AddListener(Hide);

            for (int i = 0; i < _colorButtons.Count; i++)
            {
                var button = _colorButtons[i];
                button.onClick.AddListener(() => SelectColor(button.GetComponent<Image>().color));
            }
            
            _popupRoot.SetActive(false);
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

        public void Show(DayOfWeek day, TimeSpan startTime, TimeSpan endTime)
        {
            
            _currentDay = day;
            _taskNameInput.text = "";
            SelectColor(_colorButtons.Count > 0 ? _colorButtons[0].GetComponent<Image>().color : Color.white);
            
            _slotBoundaryStart = startTime;
            _slotBoundaryEnd = endTime;

            _startHourSlider.value = startTime.Hours;
            _startMinuteSlider.value = startTime.Minutes;
            
            SubscribeSliders();
            
            if (endTime.TotalHours >= 24)
            {
                _endHourSlider.value = 23;
                _endMinuteSlider.value = 59;
            }
            else
            {
                _endHourSlider.value = endTime.Hours;
                _endMinuteSlider.value = endTime.Minutes;
            }

            ValidateAndCacheAll();
            
            _popupRoot.SetActive(true);
        }

        public void Hide()
        {
            UnsubscribeSliders();
            _popupRoot.SetActive(false);
        }

        #region Time Changed Handlers

        private void OnStartTimeChanged()
        {
            if (_isUpdating) return;

            var newStartTime = GetCurrentStartTime();
            var endTime = GetCurrentEndTime();

            
            if (newStartTime < _slotBoundaryStart)
            {
                
                SetStartTime(_slotBoundaryStart);
                newStartTime = _slotBoundaryStart;
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
                newEndTime = GetCurrentEndTime(); 
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
            
            bool isAvailable = _tasksFeature.IsTimeSlotAvailable(_currentDay, startTime, endTime, null);
            _createButton.interactable = isAvailable;
            _errorText.gameObject.SetActive(!isAvailable);
            _errorText.text = isAvailable ? "" : "Час перетинається з іншим завданням!";

            UpdateUIText(startTime, endTime);
            CacheLastValidTime();
        }

        #endregion

        #region Actions and Helpers
        
        private void OnCreateClicked()
        {
            if (string.IsNullOrWhiteSpace(_taskNameInput.text))
            {
                _errorText.text = "Назва завдання не може бути пустою";
                _errorText.gameObject.SetActive(true);
                return;
            }

            var startTime = GetCurrentStartTime();
            var endTime = GetCurrentEndTime();

            var newTaskData = new TaskData
            {
                Name = _taskNameInput.text,
                TaskColor = _selectedColor,
                StartTimeOfDay = startTime,
                Duration = endTime - startTime,
                RecurrenceDays = new List<DayOfWeek> { _currentDay }
            };
            
            if (_tasksFeature.AddTask(newTaskData))
            {
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

        #endregion
    }
}
