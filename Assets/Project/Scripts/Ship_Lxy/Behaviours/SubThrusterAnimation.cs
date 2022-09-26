using Project.Scripts.Ship.Components;
using UnityEngine;

namespace Project.Scripts.Ship_Lxy.Behaviours
{
    /// <summary>
    /// Allows a Reactor oriented Animator to follow an other specified Reactor oriented Animator (in terms of parameters).
    /// This script is useful to set multiple Reactors animation for one Reactor script.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class SubThrusterAnimation : MonoBehaviour
    {
        [SerializeField] private Animator parentAnimator;

        private Animator _animator;

        private void Awake()
        {
            //Initializing
            _animator = gameObject.GetComponent<Animator>();
        }

        private void Update()
        {
            _animator.SetFloat(Thruster.PowerAnimatorHash, parentAnimator.GetFloat(Thruster.PowerAnimatorHash));
        }
    }
}
