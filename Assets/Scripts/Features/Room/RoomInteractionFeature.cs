using System.Collections.Generic;
using System.Linq;
using Core.Feature;
using Features.Tasks.Model;
using UnityEngine;

namespace Features.Room
{
    public class RoomInteractionFeature : BaseFeature
    {
        private Dictionary<TaskType, Transform> _interactionPoints = new Dictionary<TaskType, Transform>();

        public void Initialize()
        {
            var points = Object.FindObjectsOfType<InteractionPoint>();
            foreach (var point in points)
            {
                if (!_interactionPoints.ContainsKey(point.AssociatedTaskType))
                {
                    _interactionPoints.Add(point.AssociatedTaskType, point.transform);
                }
                else
                {
                    Debug.LogWarning($"Duplicate interaction point for TaskType: {point.AssociatedTaskType}");
                }
            }
        }

        public Transform GetPointForTaskType(TaskType type)
        {
            _interactionPoints.TryGetValue(type, out var point);
            return point;
        }
    }
}