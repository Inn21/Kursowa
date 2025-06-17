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
        private CharacterMovementController _characterController;

        public void Initialize()
        {
            _characterController = Object.FindObjectOfType<CharacterMovementController>();
            _tasksFeature.OnTaskStateChanged += HandleTaskStateChange;
        }

        public override void Dispose()
        {
            if (_tasksFeature != null)
            {
                _tasksFeature.OnTaskStateChanged -= HandleTaskStateChange;
            }
            base.Dispose();
        }

        private void HandleTaskStateChange(Task task)
        {
            if (task.TodayStatus == TaskStatus.InProgress)
            {
                var destinationTransform = _roomInteractionFeature.GetPointForTaskType(task.Data.Type);
                if (destinationTransform != null && _characterController != null)
                {
                    _characterController.MoveToPoint(destinationTransform.position);
                }
            }
        }
    }
}