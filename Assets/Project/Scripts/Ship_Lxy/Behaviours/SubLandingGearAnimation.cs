using Project.Scripts.Ship.Components;
using UnityEngine;

namespace Project.Scripts.Ship_Lxy.Behaviours
{
    /// <summary>
    /// Allows a LandingGear oriented Animator to follow an other specified LandingGear oriented Animator (in terms of parameters).
    /// This script is useful to set multiple LandingGear animations for one LandingGear script.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class SubLandingGearAnimation : MonoBehaviour
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
            _animator.SetBool(LandingGear.OutAnimatorHash, parentAnimator.GetBool(LandingGear.OutAnimatorHash));
        }
    }
}
