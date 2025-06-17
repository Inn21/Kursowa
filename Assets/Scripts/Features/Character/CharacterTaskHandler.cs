using System.Collections;
using Core.Feature;
using Features.Tasks;
using Features.Tasks.Model;
using UnityEngine;
using Zenject;
using Features.Room;

namespace Features.Character
{
    public class CharacterTaskHandler : BaseFeature
    {
        [Inject] private readonly TasksFeature _tasksFeature;
        [Inject] private readonly RoomInteractionFeature _roomInteractionFeature;
        [Inject] private readonly TaskTypeFeature _taskTypeFeature;
        private CharacterMovementController _characterController;

        private Coroutine _activeTaskCoroutine;

        public void Initialize()
        {
            _characterController = Object.FindObjectOfType<CharacterMovementController>();
            if (_characterController == null)
            {
                Debug.LogError("CharacterMovementController not found in scene!");
            }
            _tasksFeature.OnTaskStateChanged += HandleTaskStateChange;
        }

        public override void Dispose()
        {
            if (_tasksFeature != null)
            {
                _tasksFeature.OnTaskStateChanged -= HandleTaskStateChange;
            }
        }

        private void HandleTaskStateChange(Task task)
        {
            if (_activeTaskCoroutine != null)
            {
                _characterController.StopCoroutine(_activeTaskCoroutine);
                _activeTaskCoroutine = null;
            }

            if (task.TodayStatus == TaskStatus.InProgress)
            {
                if (_characterController != null)
                {
                    _activeTaskCoroutine = _characterController.StartCoroutine(PerformTaskCoroutine(task));
                }
            }
            else if (task.TodayStatus == TaskStatus.Completed || task.TodayStatus == TaskStatus.Failed)
            {
                 if (_characterController != null)
                {
                    _characterController.StopTaskAnimation();
                }
            }
        }

        private IEnumerator PerformTaskCoroutine(Task task)
        {
            var destinationTransform = _roomInteractionFeature.GetPointForTaskType(task.Data.Type);
            if (destinationTransform != null && _characterController != null)
            {
                Debug.Log($"[CharacterTaskHandler] Moving to point for task: {task.Data.Name}");
                _characterController.MoveToPoint(destinationTransform.position);
                
                yield return new WaitForSeconds(0.5f);

                while (!_characterController.HasReachedDestination())
                {
                    yield return null;
                }
                
                Debug.Log($"[CharacterTaskHandler] Reached destination for task: {task.Data.Name}");

                var definition = _taskTypeFeature.GetDefinition(task.Data.Type);
                if(definition != null && !string.IsNullOrEmpty(definition.AnimationTriggerName))
                {
                    Debug.Log($"[CharacterTaskHandler] Playing animation: {definition.AnimationTriggerName}");
                    _characterController.PlayTaskAnimation(definition.AnimationTriggerName);
                }
                else
                {
                    Debug.LogWarning($"[CharacterTaskHandler] No animation trigger defined for task type: {task.Data.Type}");
                }
            }
            else
            {
                Debug.LogError($"[CharacterTaskHandler] Destination or CharacterController is null for task: {task.Data.Name}");
            }
        }
    }
}
