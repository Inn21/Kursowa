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

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (_agent.remainingDistance > _agent.stoppingDistance)
            {
                _animator.SetBool(IsMoving, true);
            }
            else
            {
                _animator.SetBool(IsMoving, false);
            }
        }

        public void MoveToPoint(Vector3 destination)
        {
            _agent.SetDestination(destination);
        }
    }
}