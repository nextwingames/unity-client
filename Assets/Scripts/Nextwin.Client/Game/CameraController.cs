using UnityEngine;

namespace Nextwin.Client.Game
{
    public class CameraController : MonoBehaviour
    {
        public Transform Target { get; set; }
        public Transform TargetPivot { get; set; }

        [Header("Camera Setting")]
        [SerializeField, Range(0f, 50f)]
        protected float _cameraDistance = 10f;
        [SerializeField, Range(0f, 50f)]
        protected float _cameraHeight = 5f;
        [SerializeField, Range(0f, 90f)]
        protected float _cameraAngle = 20f;
        [SerializeField]
        protected float _cameraSpeed = 2f;

        /// <summary>
        /// Target을 따라 카메라 이동
        /// </summary>
        /// <param name="useMouse"></param>
        public virtual void Move(bool useMouse)
        {
            if(useMouse)
            {
                MoveUsingMouse();
            }
            else
            {
                MoveUsingKeyboard();
            }
        }

        /// <summary>
        /// 카메라 회전(바라보는 방향 설정)
        /// </summary>
        public virtual void Rotate(bool useMouse)
        {
            Quaternion rotation = Quaternion.Euler(new Vector3(_cameraAngle, 0f, 0f));

            if(useMouse)
            {
                RotateCameraUsingMouse(rotation);
            }
            else
            {
                RotateCameraUsingKeyboard(rotation);
            }
        }

        protected virtual void MoveUsingMouse()
        {
            transform.localPosition = new Vector3(0, _cameraHeight, -_cameraDistance);
        }

        protected virtual void MoveUsingKeyboard()
        {
            Vector3 backward = -TargetPivot.forward;
            backward *= _cameraDistance;

            Vector3 camPos = new Vector3(Target.position.x, Target.position.y + _cameraHeight, Target.position.z + backward.z);
            transform.position = Vector3.Lerp(transform.position, camPos, Time.deltaTime * _cameraSpeed);
        }

        protected virtual void RotateCameraUsingMouse(Quaternion rotation)
        {
            transform.localRotation = rotation;
        }

        protected virtual void RotateCameraUsingKeyboard(Quaternion rotation)
        {
            transform.rotation = rotation;
        }
    }
}