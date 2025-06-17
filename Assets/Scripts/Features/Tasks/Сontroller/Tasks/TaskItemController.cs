using System.Collections;
using Features.Tasks.Model;
using Features.Tasks.Сontroller.Global;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Features.Tasks.Controller.Tasks
{
    public class TaskItemController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("UI Елементи")]
        [SerializeField] private TextMeshProUGUI _taskTitleText;
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private Image _taskIcon;
        [SerializeField] private Button _editButton;

        [Header("Налаштування станів")]
        [SerializeField] private Color _defaultColor = Color.white;
        [SerializeField] private Color _inProgressColor = Color.yellow;
        [SerializeField] private Color _awaitingColor = Color.cyan;
        [SerializeField] private Color _completedColor = Color.green;
        [SerializeField] private Color _failedColor = Color.red;

        [Header("Елементи для свайпу")]
        [SerializeField] private RectTransform _baseRect;
        [SerializeField] private GameObject _rightSwipeVisual;
        [SerializeField] private GameObject _leftSwipeVisual;
        [SerializeField] private float _swipeThreshold = 200f;
        [SerializeField] private Image _baseBackground;
        
        [Inject] private readonly TasksFeature _tasksFeature;
        [Inject] private readonly TaskTypeFeature _taskTypeFeature;

        private Task _task;
        private TasksUIController _ownerController;
        private Vector2 _originalPosition;
        private bool _canSwipe = false;
        private Coroutine _resetCoroutine;
        
        private ScrollRect _parentScrollRect;
        private bool _isDraggingHorizontally = false;
        private bool _isDraggingVertically = false;

        private void Awake()
        {
            _parentScrollRect = GetComponentInParent<ScrollRect>();
        }

        public void Initialize(Task task, TasksUIController owner)
        {
            _task = task;
            _ownerController = owner;
            _taskTitleText.text = _task.Data.Name;
            _timeText.text = $"{_task.Data.StartTimeOfDay:hh\\:mm} - {_task.Data.EndTimeOfDay:hh\\:mm}";
            
            var definition = _taskTypeFeature.GetDefinition(_task.Data.Type);
            if(definition != null && definition.Icon != null)
            {
                _taskIcon.sprite = definition.Icon;
                _taskIcon.color = Color.white;
            }
            
            bool isRealTask = !_task.Data.IsFreeTime;
            _editButton.gameObject.SetActive(isRealTask);
            if(isRealTask)
                 _editButton.onClick.AddListener(OnEditClicked);
            
            UpdateVisualState();
        }

        private void OnEnable()
        {
            _tasksFeature.OnTaskStateChanged += HandleTaskStateChange;
        }

        private void OnDisable()
        {
            _tasksFeature.OnTaskStateChanged -= HandleTaskStateChange;
            if (_editButton != null) _editButton.onClick.RemoveListener(OnEditClicked);
        }
        
        private void HandleTaskStateChange(Task changedTask)
        {
            if (_task != null && _task.Id == changedTask.Id)
                UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            if (_task == null || _task.Data.IsFreeTime)
            {
                _canSwipe = false;
                _baseBackground.color = _defaultColor;
                return;
            }
            
            _canSwipe = _task.TodayStatus == TaskStatus.InProgress || _task.TodayStatus == TaskStatus.AwaitingConfirmation;

            switch (_task.TodayStatus)
            {
                case TaskStatus.Pending: _baseBackground.color = _defaultColor; break;
                case TaskStatus.InProgress: _baseBackground.color = _inProgressColor; break;
                case TaskStatus.AwaitingConfirmation: _baseBackground.color = _awaitingColor; break;
                case TaskStatus.Completed: _baseBackground.color = _completedColor; break;
                case TaskStatus.Failed: _baseBackground.color = _failedColor; break;
            }
        }
        
        private void OnEditClicked()
        {
            _ownerController.ShowEditPopup(_task.Data);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_resetCoroutine != null) StopCoroutine(_resetCoroutine);
            
            _originalPosition = _baseRect.anchoredPosition;
            _isDraggingHorizontally = false;
            _isDraggingVertically = false;
            
            if(_parentScrollRect != null)
                _parentScrollRect.OnBeginDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if(_isDraggingHorizontally)
            {
                float deltaX = eventData.position.x - eventData.pressPosition.x;
                _baseRect.anchoredPosition = new Vector2(_originalPosition.x + deltaX, _originalPosition.y);
                return;
            }
            
            if(_isDraggingVertically)
            {
                if(_parentScrollRect != null)
                    _parentScrollRect.OnDrag(eventData);
                return;
            }
            
            if(Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y))
            {
                if(_canSwipe)
                {
                    _isDraggingHorizontally = true;
                    _rightSwipeVisual.SetActive(true);
                    _leftSwipeVisual.SetActive(true);
                    if(_parentScrollRect != null)
                        _parentScrollRect.enabled = false;
                }
            }
            else
            {
                _isDraggingVertically = true;
                 if(_parentScrollRect != null)
                    _parentScrollRect.OnDrag(eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_parentScrollRect != null)
            {
                _parentScrollRect.OnEndDrag(eventData);
                _parentScrollRect.enabled = true;
            }
            
            if(!_isDraggingHorizontally) return;
            
            float deltaX = _baseRect.anchoredPosition.x - _originalPosition.x;

            if (deltaX > _swipeThreshold)
                _tasksFeature.CompleteTask(_task);
            else if (deltaX < -_swipeThreshold)
                _tasksFeature.FailTask(_task);
            
            _resetCoroutine = StartCoroutine(ResetPositionCoroutine());
        }
        
        private IEnumerator ResetPositionCoroutine()
        {
            float elapsedTime = 0f;
            float duration = 0.2f;
            Vector2 startPosition = _baseRect.anchoredPosition;

            while(elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                _baseRect.anchoredPosition = Vector2.Lerp(startPosition, _originalPosition, t);
                yield return null;
            }
            
            _baseRect.anchoredPosition = _originalPosition;
            if(_rightSwipeVisual) _rightSwipeVisual.SetActive(false);
            if(_leftSwipeVisual) _leftSwipeVisual.SetActive(false);
        }
    }
}
