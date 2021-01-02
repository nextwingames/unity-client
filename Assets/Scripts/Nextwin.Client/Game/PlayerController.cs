using Nextwin.Client.Util;
using System.Collections;
using UnityEngine;

namespace Nextwin.Client.Game
{
    #region enum { FallDownAxis, TagOrLayer }
    public enum FallDownAxis
    {
        X,
        Z
    }

    public enum TagOrLayer
    {
        Tag,
        Layer
    }
    #endregion

    /// <summary>
    /// Use Mouse에 체크되었다면
    /// 왼쪽이 최상위 부모 오른쪽이 최하위 자식일 때
    /// _body - _cameraArm - _camera의 계층 구조를 가져야 하고
    /// 체크되지 않았다면
    /// _camera(_body와 별개) // _cameraArm - _body의 계층 구조를 가져야 함
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        #region Control
        [Header("Control This Character")]
        [SerializeField]
        protected bool _onControl = true;
        #endregion

        #region Transform(GameObject) Setting
        [Header("Transform(GameObject) Setting")]
        [SerializeField]
        protected Transform _body;
        [SerializeField]
        protected Transform _pivot;
        [SerializeField]
        protected Camera _camera;
        protected CameraController _cameraController;
        [SerializeField, Tooltip("Only need one foot")]
        protected Rigidbody _foot;
        #endregion

        #region Mouse Setting
        [Header("Mouse Setting")]
        [SerializeField]
        protected bool _useMouse;
        [SerializeField, Range(1, 25), Tooltip("If you do not check Use Mouse, Mouse Sensitivity is not used")]
        protected int _mouseSensitivity = 12;
        #endregion

        #region Cotnrol Basic Key Setting
        [Header("Control Basic Key Setting")]
        [SerializeField]
        protected KeyCode _upKey = KeyCode.UpArrow;
        [SerializeField]
        protected KeyCode _downKey = KeyCode.DownArrow;
        [SerializeField]
        protected KeyCode _leftKey = KeyCode.LeftArrow;
        [SerializeField]
        protected KeyCode _rightKey = KeyCode.RightArrow;
        [SerializeField]
        protected KeyCode _jumpKey = KeyCode.Space;
        [SerializeField]
        protected KeyCode _runKey = KeyCode.LeftShift;
        #endregion

        #region Player Setting
        [Header("Player Setting")]
        [SerializeField]
        protected float _walkSpeed = 4f;
        [SerializeField]
        protected float _runSpeed = 8f;
        protected float _speed;
        [SerializeField, Range(1f, 20f)]
        protected float _rotateSpeed = 10f;
        [SerializeField]
        protected float _jumpPower = 5f;
        [SerializeField]
        protected float _wakeUpSpeed = 3f;
        #endregion

        #region Ground Setting
        [Header("Ground Setting")]
        [SerializeField]
        protected TagOrLayer _checkGroundByTagOrLayer;
        [SerializeField]
        protected string _groundTagOrLayer;
        #endregion

        #region Components
        protected Animator _animator;
        protected Rigidbody _rigidBody;
        #endregion

        #region Transforms
        protected Vector3 _destPos;
        protected Vector3 _curPos;
        protected Vector3 _lookDir;
        #endregion

        #region States
        protected bool _isMoving;
        protected bool _isJumping;
        protected bool _isFallDown;
        protected bool _isWakingUp;
        protected bool _isOnGround;
        protected FallDownAxis _fallDownAxis;
        #endregion

        protected delegate void Callback();

        protected virtual void Awake()
        {
            CheckHierarchy();
            SetCameraController();
        }

        protected virtual void Start()
        {
            _animator = _body.GetComponent<Animator>();
            _rigidBody = _body.GetComponent<Rigidbody>();

            _speed = _walkSpeed;
            _destPos = _body.localPosition;

            SetFootCollisionChecker();
        }

        protected virtual void Update()
        {
            InputKey();
            RotateWithMouse();
            CheckFallDown();
        }

        protected virtual void FixedUpdate()
        {
            WakeUp();
            RotateWithKeyboard();
            Move();
            Jump();
        }

        protected virtual void LateUpdate()
        {
            _cameraController.Move(_useMouse);
            _cameraController.Rotate(_useMouse);
        }

        /// <summary>
        /// 상, 하, 좌, 우 움직임 및 달리기, 점프의 입력을 받음
        /// </summary>
        protected virtual void InputKey()
        {
            if(!_onControl || _isFallDown)
            {
                return;
            }

            if(Input.GetKey(_runKey))
            {
                OnInputRunKey();
            }
            if(Input.GetKeyUp(_runKey))
            {
                OnReleaseRunKey();
            }
            if(Input.GetKey(_upKey))
            {
                OnInputUpKey();
            }
            if(Input.GetKey(_downKey))
            {
                OnInputDownKey();
            }
            if(Input.GetKey(_leftKey))
            {
                OnInputLeftKey();
            }
            if(Input.GetKey(_rightKey))
            {
                OnInputRightKey();
            }
            if(Input.GetKeyDown(_jumpKey))
            {
                OnInputJumpKey();
            }
        }

        #region OnInput
        protected virtual void OnInputUpKey()
        {
            SetDestPos(_pivot.forward);
            if(!_useMouse)
            {
                SetLookDir(0f, 1f);
            }
            _isMoving = true;
        }

        protected virtual void OnInputDownKey()
        {
            SetDestPos(-_pivot.forward);
            if(!_useMouse)
            {
                SetLookDir(0f, -1f);
            }
            _isMoving = true;
        }

        protected virtual void OnInputLeftKey()
        {
            SetDestPos(-_pivot.right);
            if(!_useMouse)
            {
                SetLookDir(-1f, 0f);
            }
            _isMoving = true;
        }

        protected virtual void OnInputRightKey()
        {
            SetDestPos(_pivot.right);
            if(!_useMouse)
            {
                SetLookDir(1f, 0f);
            }
            _isMoving = true;
        }

        protected virtual void OnInputRunKey()
        {
            _speed = _runSpeed;
        }

        protected virtual void OnReleaseRunKey()
        {
            _speed = _walkSpeed;
        }

        protected virtual void OnInputJumpKey()
        {
            _isJumping = true;
        }
        #endregion

        #region Character Move
        /// <summary>
        /// 캐릭터 이동
        /// </summary>
        protected virtual void Move()
        {
            if(!_isMoving || _isFallDown)
            {
                return;
            }
            OnMoveStart();

            _curPos = _destPos;
            _body.localPosition = _destPos;

            _isMoving = false;
            OnMoveFinish();
        }

        protected virtual void OnMoveStart() { }

        protected virtual void OnMoveFinish() { }

        /// <summary>
        /// 입력을 받고 이동 목적지 설정
        /// </summary>
        /// <param name="vector">움직이려는 방향</param>
        protected virtual void SetDestPos(Vector3 vector)
        {
            Vector3 dir = new Vector3(vector.x, 0f, vector.z).normalized;
            _destPos += dir * Time.deltaTime * _speed;
            _destPos.y = _body.localPosition.y;
        }
        #endregion

        #region Character Jump
        /// <summary>
        /// 캐릭터 점프
        /// </summary>
        protected virtual void Jump()
        {
            //Debug.Log(_isOnGround);
            if(!_isJumping || _isFallDown || !_isOnGround)
            {
                return;
            }
            OnJumpStart();

            _rigidBody.AddForce(Vector3.up * _jumpPower, ForceMode.Impulse);

            _isJumping = false;
            OnJumpFinish();
        }


        protected virtual void OnJumpStart() { }

        protected virtual void OnJumpFinish() { }
        #endregion

        #region Fall Down & Wake Up
        /// <summary>
        /// 넘어졌는지 검사
        /// </summary>
        protected virtual void CheckFallDown()
        {
            Vector3 euler = _body.eulerAngles;

            if(euler.x >= 90f && euler.x <= 270f)
            {
                ChangeStateFallDown(FallDownAxis.X);
            }
            else if(euler.z >= 90f && euler.z <= 270f)
            {
                ChangeStateFallDown(FallDownAxis.Z);
            }
        }

        /// <summary>
        /// 넘어진 상태로 변경
        /// </summary>
        /// <param name="axis"></param>
        protected virtual void ChangeStateFallDown(FallDownAxis axis)
        {
            if(!_isOnGround)
            {
                return;
            }

            _fallDownAxis = axis;
            _isFallDown = true;
        }

        /// <summary>
        /// 넘어졌을 때 일어남
        /// </summary>
        protected virtual void WakeUp()
        {
            if(!_isFallDown || _isWakingUp)
            {
                return;
            }
            _isMoving = false;
            _isJumping = false;

            Rigidbody rigidbody = _body.GetComponent<Rigidbody>();
            rigidbody.isKinematic = true;

            OnWakeUpStart();

            StartCoroutine(TryWakeUp(() =>
            {
                _isFallDown = false;
                rigidbody.isKinematic = false;

                OnWakeUpFinish();
            }));
        }

        protected virtual void OnWakeUpStart() { }

        protected virtual void OnWakeUpFinish() { }

        /// <summary>
        /// 실제로 일어나는 행동을 취하는 코루틴
        /// </summary>
        /// <param name="afterWakeUp">일어난 후 실행할 일</param>
        /// <returns></returns>
        protected virtual IEnumerator TryWakeUp(Callback afterWakeUp)
        {
            _isWakingUp = true;

            Vector3 euler = _body.eulerAngles;
            Vector3 wakeUpEuler = _fallDownAxis == FallDownAxis.X ? new Vector3(0, euler.y, euler.z) : new Vector3(euler.x, euler.y, 0);

            while(!IsWakeUpFinished(euler))
            {
                euler = Vector3.Lerp(euler, wakeUpEuler, _wakeUpSpeed * Time.fixedDeltaTime * 0.1f);
                _body.rotation = Quaternion.Euler(euler);

                yield return new WaitForSeconds(0.01f);
            }

            _isWakingUp = false;
            afterWakeUp.Invoke();
        }

        private bool IsWakeUpFinished(Vector3 euler)
        {
            float rotateValue = _fallDownAxis == FallDownAxis.X ? euler.x : euler.z;

            if(rotateValue > 5f)
            {
                return false;
            }
            return true;
        }
        #endregion

        #region Character Rotate
        /// <summary>
        /// 마우스로 회전
        /// </summary>
        protected virtual void RotateWithMouse()
        {
            if(!_onControl || !_useMouse)
            {
                return;
            }

            float sensitivity = _mouseSensitivity / 10f;
            Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X") * sensitivity, Input.GetAxis("Mouse Y") * sensitivity);
            Vector3 camAngle = _pivot.rotation.eulerAngles;
            float x = camAngle.x - mouseDelta.y;
            x = x < 180f ? Mathf.Clamp(x, -1f, 70f) : Mathf.Clamp(x, 345f, 361f);

            _body.rotation = Quaternion.Euler(0, camAngle.y + mouseDelta.x, camAngle.z);
            _pivot.rotation = Quaternion.Euler(x, camAngle.y + mouseDelta.x, camAngle.z);
        }

        /// <summary>
        /// 키보드 입력으로만 회전
        /// </summary>
        protected virtual void RotateWithKeyboard()
        {
            if(!_isMoving || _useMouse)
            {
                return;
            }

            float angle = Mathf.Atan2(_lookDir.x, _lookDir.z) * Mathf.Rad2Deg;
            _body.rotation = Quaternion.Slerp(_body.rotation, Quaternion.Euler(0, angle, 0), _rotateSpeed * Time.fixedDeltaTime);

            _lookDir = new Vector3(0, 0, 0);
        }

        /// <summary>
        /// 입력을 받고 바라보는 방향 설정
        /// </summary>
        /// <param name="x">좌, 우</param>
        /// <param name="z">상, 하</param>
        protected virtual void SetLookDir(float x, float z)
        {
            if(x == 0)
            {
                _lookDir = new Vector3(_lookDir.x, 0, z);
            }
            else if(z == 0)
            {
                _lookDir = new Vector3(x, 0, _lookDir.z);
            }
        }
        #endregion

        #region Collision
        /// <summary>
        /// 땅 위에 있는지 검사
        /// </summary>
        /// <param name="collision">충돌체</param>
        /// <param name="isEnter">충돌체 충돌이 Enter인지 Exit인지</param>
        protected virtual void CheckStandOnGround(Collider collider, bool isEnter)
        {
            switch(_checkGroundByTagOrLayer)
            {
                case TagOrLayer.Tag:
                    CheckStandOnGround(collider.tag, isEnter);
                    break;

                case TagOrLayer.Layer:
                    CheckStandOnGround(collider.gameObject.layer, isEnter);
                    break;
            }
        }

        protected virtual void CheckStandOnGround(string tag, bool isEnter)
        {
            if(tag.Equals(_groundTagOrLayer))
            {
                _isOnGround = isEnter;
            }
        }

        protected virtual void CheckStandOnGround(int layer, bool isEnter)
        {
            if(layer == LayerMask.NameToLayer(_groundTagOrLayer))
            {
                _isOnGround = isEnter;
            }
        }
        #endregion

        #region Awake
        private void CheckHierarchy()
        {
            if(_useMouse)
            {
                if(!_pivot.IsChildOf(_body))
                {
                    Debug.LogError("Pivot should be child of Body.");
                }
                if(!_camera.transform.IsChildOf(_pivot))
                {
                    Debug.LogError("Camera should be child of Pivot.");
                }
            }
            else
            {
                if(!_body.IsChildOf(_pivot))
                {
                    Debug.LogError("Body should be child of Pivot.");
                }
            }
        }

        private void SetCameraController()
        {
            _cameraController = _camera.gameObject.GetComponent<CameraController>();
            if(_cameraController == null)
            {
                _camera.gameObject.AddComponent<CameraController>();
                _cameraController = _camera.gameObject.GetComponent<CameraController>();
            }

            _cameraController.Target = _body;
            _cameraController.TargetPivot = _pivot;
        }
        #endregion

        #region Start
        private void SetFootCollisionChecker()
        {
            if(_foot == null)
            {
                _foot = _rigidBody;
            }

            OtherCollisionChecker footCollisionChecker = _foot.gameObject.AddComponent<OtherCollisionChecker>();

            footCollisionChecker.AddCollisionEvent(CollisionEvent.OnCollisionEnter, (collider) =>
            {
                CheckStandOnGround(collider, true);
            });

            footCollisionChecker.AddCollisionEvent(CollisionEvent.OnCollisionExit, (collider) =>
            {
                CheckStandOnGround(collider, false);
            });
        }
        #endregion
    }
}