using UnityEngine;
using UnityEngine.AI;

namespace Features.Character
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
    public class CharacterMovementController : MonoBehaviour
    {
        private NavMeshAgent _agent;
        private Animator _animator;
        
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        private string _currentTaskAnimation;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            bool isMoving = _agent.velocity.sqrMagnitude > 0.01f;
            _animator.SetBool(IsMoving, isMoving);
        }

        public bool HasReachedDestination()
        {
            if (!_agent.pathPending)
            {
                if (_agent.remainingDistance <= _agent.stoppingDistance)
                {
                    if (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void MoveToPoint(Vector3 destination)
        {
            StopTaskAnimation();
            if (_agent.isOnNavMesh)
            {
                _agent.SetDestination(destination);
            }
        }

        public void PlayTaskAnimation(string triggerName)
        {
            if (string.IsNullOrEmpty(triggerName) || _animator == null) return;
            
            StopTaskAnimation();
            
            _currentTaskAnimation = triggerName;
            _animator.SetTrigger(_currentTaskAnimation);
        }

        public void StopTaskAnimation()
        {
            if (!string.IsNullOrEmpty(_currentTaskAnimation) && _animator != null)
            {
                _animator.ResetTrigger(_currentTaskAnimation);
                _currentTaskAnimation = null;
            }
        }
    }
}