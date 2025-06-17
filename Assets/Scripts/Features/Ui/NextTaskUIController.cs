using Features.Tasks;
using Features.Tasks.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Features.UI
{
    public class NextTaskUIController : MonoBehaviour
    {
        [Header("UI Елементи")]
        [SerializeField] private GameObject _rootObject;
        [SerializeField] private TextMeshProUGUI _taskNameText;
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private Image _taskIcon;

        [Inject] private readonly TasksFeature _tasksFeature;
        [Inject] private readonly TaskTypeFeature _taskTypeFeature;

        private void OnEnable()
        {
            _tasksFeature.OnTaskListUpdated += UpdateDisplay;
            _tasksFeature.OnTaskStateChanged += (task) => UpdateDisplay();
            UpdateDisplay();
        }

        private void OnDisable()
        {
            if(_tasksFeature != null)
            {
                _tasksFeature.OnTaskListUpdated -= UpdateDisplay;
                _tasksFeature.OnTaskStateChanged -= (task) => UpdateDisplay();
            }
        }

        private void UpdateDisplay()
        {
            var nextTask = _tasksFeature.GetCurrentOrNextTask();

            if (nextTask == null)
            {
                _rootObject.SetActive(false);
                return;
            }
            
            _rootObject.SetActive(true);

            _taskNameText.text = nextTask.Data.Name;
            _timeText.text = $"{nextTask.Data.StartTimeOfDay:hh\\:mm} - {nextTask.Data.EndTimeOfDay:hh\\:mm}";

            var definition = _taskTypeFeature.GetDefinition(nextTask.Data.Type);
            if (definition != null && definition.Icon != null)
            {
                _taskIcon.sprite = definition.Icon;
            }
        }
    }
}