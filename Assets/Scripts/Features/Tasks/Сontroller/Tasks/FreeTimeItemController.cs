using System;
using Features.Tasks;
using Features.Tasks.Model;
using Features.Tasks.Сontroller.Global;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Feature.Tasks.UI
{
    public class FreeTimeItemController : MonoBehaviour
    {
        [SerializeField] private Button _selectButton;
        
        private Task _freeTimeTask;
        private TasksUIController _uiController;
        private DayOfWeek _day;

        public void Initialize(Task task, DayOfWeek day, TasksUIController uiController)
        {
            _freeTimeTask = task;
            _day = day;
            _uiController = uiController;
            _selectButton.onClick.AddListener(OnClicked);
        }

        private void OnClicked()
        {
            _uiController.ShowCreationPopup(_day, _freeTimeTask.Data.StartTimeOfDay, _freeTimeTask.Data.EndTimeOfDay);
        }

        private void OnDestroy()
        {
            _selectButton.onClick.RemoveListener(OnClicked);
        }
    }
}