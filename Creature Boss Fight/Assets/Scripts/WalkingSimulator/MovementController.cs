using UnityEngine;
using System;
using Avrahamy;
using Avrahamy.Math;

namespace WalkingSimulator {
    [RequireComponent(typeof(Rigidbody), typeof(InputModel))]
    public class MovementController : OptimizedBehaviour, IMoveable {
        private const float INPUT_THRESHOLD = 0.1f;
        
        public enum ControlType {
            UpIsCameraForward,
            UpIsActorForward,
            UpIsX,
            UpIsY,
            UpIsZ,
        }
        
        public enum Direction {
            Forward,
            Back,
            Right,
            Left,
        }
        
        [SerializeField] Camera gameCamera;
        [SerializeField] ControlType controlType;
        [SerializeField] Direction actorForwardIs;
        public float maxSpeed = 5f;
        public float acceleration = 10f;
        public float deceleration = 10f;
        public float rotationSpeed = 360f;
        [SerializeField] float minSqrSpeed = 0.1f;
        [SerializeField] bool inputIsTargetDirection;

        public Vector3 Position {
            get {
                return body.position;
            }
            set {
                body.position = value;
            }
        }
        
        public Vector3 Velocity {
            get {
                return body.velocity;
            }
            set {
                body.velocity = value;
            }
        }

        private Vector3 UpWorldDirection {
            get {
                switch (controlType) {
                    case ControlType.UpIsCameraForward: {
                        var forward = gameCamera.transform.forward;
                        forward.y = 0f;
                        return forward.normalized;
                    }
                    case ControlType.UpIsActorForward: {
                        Vector3 forward;
                        switch (actorForwardIs) {
                            case Direction.Forward:
                                forward = transform.forward;
                                break;
                            case Direction.Back:
                                forward = -transform.forward;
                                break;
                            case Direction.Right:
                                forward = transform.right;
                                break;
                            case Direction.Left:
                                forward = -transform.right;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        forward.y = 0f;
                        return forward.normalized;
                    }
                    case ControlType.UpIsX:
                        return Vector3.right;
                    case ControlType.UpIsY:
                        return Vector3.up;
                    case ControlType.UpIsZ:
                        return Vector3.forward;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private float RotationAngleFix {
            get {
                switch (actorForwardIs) {
                    case Direction.Forward:
                        return 90f;
                    case Direction.Back:
                        return -90f;
                    case Direction.Right:
                        return 180f;
                    case Direction.Left:
                        return 0f;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        private Rigidbody body;
        private InputModel inputModel;
        
        protected void Awake() {
            body = GetComponent<Rigidbody>();
            inputModel = GetComponent<InputModel>();
        }

        private void Start()
        {
            gameCamera = Camera.main;
        }

        protected void Reset() {
            if (gameCamera == null) {
                gameCamera = FindObjectOfType<Camera>();
            }
            body = GetComponent<Rigidbody>();
            body.useGravity = false;
            body.constraints = RigidbodyConstraints.FreezeRotation;
        }

        protected void FixedUpdate() {
            var velocity = Velocity;
            var moveInput = inputModel.MoveInput;
            if (moveInput.sqrMagnitude < INPUT_THRESHOLD) {
                // Stop.
                if (velocity.sqrMagnitude <= minSqrSpeed) {
                    velocity = Vector3.zero;
                } else {
                    velocity = Vector3.MoveTowards(
                        velocity,
                        Vector3.zero,
                        deceleration * Time.fixedDeltaTime);
                }
            } else {
                var targetVelocity = InputToTargetDirection(moveInput, 90f) * maxSpeed;
                velocity = Vector3.MoveTowards(
                    velocity,
                    targetVelocity,
                    acceleration * Time.fixedDeltaTime);
            }
            Velocity = velocity;
        }

        protected void Update() {
            if (inputModel.IsLockFacing) return;
            var moveInput = inputModel.MoveInput;
            if (controlType == ControlType.UpIsActorForward) {
                // Only forward or sideways input is considered for rotation.
                moveInput.y = Mathf.Max(moveInput.y, 0f);
            }
            if (moveInput.sqrMagnitude < INPUT_THRESHOLD) return;
            var targetDirection = InputToTargetDirection(moveInput, RotationAngleFix);
            DebugDraw.DrawLine(Position, Position + targetDirection * 5f, Color.green);
            var targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
            var rotation = Quaternion.RotateTowards(body.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            body.MoveRotation(rotation);
        }

        private Vector3 InputToTargetDirection(Vector2 moveInput, float angleFix) {
            if (inputIsTargetDirection) return moveInput.ToVector3XZ();
            var angle = moveInput.GetAngle() - angleFix;
            var upDirection = UpWorldDirection;
            return upDirection.RotateInDegreesAroundY(angle);
        }

        private void OnDisable()
        {
            Velocity = Vector3.zero;
        }
    }
}
