using Project.Scripts.Interactions;
using Project.Scripts.Ship.Abstracts;
using UnityEngine;
using UnityEngine.Events;

namespace Project.Scripts.Ship.Components
{
    /// <summary>
    /// Ship1 seat handling.
    /// </summary>
    public class Seat : AShipComponent
    {
        [SerializeField] [Min(0)] private Vector2 slidingLimits = Vector2.zero;
        [SerializeField] [Min(0)] private float movementStep = 0.5f;
        [SerializeField] [Min(0)] private float speedDeltaTime = 0.2f;
        [SerializeField] private PhysicalButton forwardButton;
        [SerializeField] private PhysicalButton backButton;
        [SerializeField] private PhysicalButton topButton;
        [SerializeField] private PhysicalButton bottomButton;

        private Vector2 _defaultLocalPosition = Vector2.zero, _currentOffset = Vector2.zero;

        private bool _forwardButtonClicked, _backButtonClicked, _topButtonClicked, _bottomButtonClicked;
        private UnityAction<bool> _forwardAction, _backAction, _topAction, _bottomAction;
        private float _horizontalVelocity, _verticalVelocity;

        private void Awake()
        {
            //Set the actions
            _forwardAction = enable => _forwardButtonClicked = enable;
            _backAction = enable => _backButtonClicked = enable;
            _topAction = enable => _topButtonClicked = enable;
            _bottomAction = enable => _bottomButtonClicked = enable;

            //Initializing
            var localPosition = transform.localPosition;
            _defaultLocalPosition = new Vector2(localPosition.z, localPosition.y);

        }

        private void OnEnable()
        {
            forwardButton.onValueChanged.AddListener(_forwardAction);
            backButton.onValueChanged.AddListener(_backAction);
            topButton.onValueChanged.AddListener(_topAction);
            bottomButton.onValueChanged.AddListener(_bottomAction);
        }

        protected override void Update()
        {
            base.Update();

            if (!IsOn) 
                return;
            
            HandleInputs();
            HandleMovement();
        }

        private void OnDisable()
        {
            forwardButton.onValueChanged.RemoveListener(_forwardAction);
            backButton.onValueChanged.RemoveListener(_backAction);
            topButton.onValueChanged.RemoveListener(_topAction);
            bottomButton.onValueChanged.RemoveListener(_bottomAction);
        }
        
        private void HandleInputs()
        {
            var movement = Vector2.zero;

            //Horizontal movement
            if (_forwardButtonClicked)
                movement.x += movementStep;
            if (_backButtonClicked)
                movement.x -= movementStep;

            //Vertical movement
            if (_topButtonClicked)
                movement.y += movementStep;
            if (_bottomButtonClicked)
                movement.y -= movementStep;

            _currentOffset += movement * Time.deltaTime;
            _currentOffset.x = Mathf.Clamp(_currentOffset.x, -slidingLimits.x, slidingLimits.x);
            _currentOffset.y = Mathf.Clamp(_currentOffset.y, -slidingLimits.y, slidingLimits.y);
        }

        private void HandleMovement()
        {
            var localPosition = transform.localPosition;

            localPosition.z = Mathf.SmoothDamp(localPosition.z, _defaultLocalPosition.x + _currentOffset.x, ref _horizontalVelocity, speedDeltaTime);
            localPosition.y = Mathf.SmoothDamp(localPosition.y, _defaultLocalPosition.y + _currentOffset.y, ref _verticalVelocity, speedDeltaTime);

            transform.localPosition = localPosition;
        }
    }
}