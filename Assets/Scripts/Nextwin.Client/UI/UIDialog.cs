using Nextwin.Client.Util;
using System;
using System.Collections;
using UnityEngine;

namespace Nextwin.Client.UI
{
    public abstract class UIDialog<TDialog> : UIBase<TDialog> where TDialog : Enum
    {
        private RectTransform _rectTransform;
        private readonly float _waitForCallbackRate = 0.2f;

        protected virtual void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public override void Show(bool isActive = true)
        {
            SetRate();

            if(isActive)
            {
                _rectTransform.localScale = new Vector3(0, 0.01f, 1f);
                gameObject.SetActive(true);

                StartCoroutine(ChangeScaleWidth(1f, () =>
                {
                    StartCoroutine(ChangeScaleHeight(1f));
                }));
            }
            else
            {
                StartCoroutine(ChangeScaleHeight(0.01f, () =>
                {
                    StartCoroutine(ChangeScaleWidth(0f, () =>
                    {
                        gameObject.SetActive(false);
                    }));
                }));
            }
        }

        /// <summary>
        /// showTime동안 대화상자가 나타나고 사라짐
        /// </summary>
        /// <param name="showTimeSeconds">대화상자가 활성화 되어있을 시간(초)</param>
        public virtual void PopupForSeconds(float showTimeSeconds)
        {
            ActionManager.Instance.ExecuteWithDelay(() => { Show(true); }, () => { Show(false); }, showTimeSeconds);
        }

        /// <summary>
        /// 너비 변경
        /// </summary>
        /// <param name="scale">변경하려는 너비 스케일</param>
        /// <param name="callback">스케일 변경 후 실행되는 기능</param>
        /// <returns></returns>
        private IEnumerator ChangeScaleWidth(float scale, Callback callback = null)
        {
            float rate = GetLocalRate(scale, _rectTransform.localScale.x);
            Vector3 vector = new Vector3(rate, 0, 0);

            while(!IsSameScale(scale, _rectTransform.localScale.x))
            {
                _rectTransform.localScale += vector;
                yield return new WaitForSeconds(_yieldTime);
            }

            Vector3 curScale = _rectTransform.localScale;
            _rectTransform.localScale = new Vector3(scale, curScale.y, curScale.z);

            yield return new WaitForSeconds(_yieldTime * _showSpeedRate * _waitForCallbackRate);
            callback?.Invoke();
        }

        /// <summary>
        /// 높이 변경
        /// </summary>
        /// <param name="scale">변경하려는 높이 스케일</param>
        /// <param name="callback">스케일 변경 후 실행되는 기능</param>
        /// <returns></returns>
        private IEnumerator ChangeScaleHeight(float scale, Callback callback = null)
        {
            float rate = GetLocalRate(scale, _rectTransform.localScale.y);
            Vector3 vector = new Vector3(0, rate, 0);

            while(!IsSameScale(scale, _rectTransform.localScale.y))
            {
                _rectTransform.localScale += vector;
                yield return new WaitForSeconds(_yieldTime);
            }

            Vector3 curScale = _rectTransform.localScale;
            _rectTransform.localScale = new Vector3(curScale.x, scale, curScale.z);

            yield return new WaitForSeconds(_yieldTime * _showSpeedRate * _waitForCallbackRate);
            callback?.Invoke();
        }

        private float GetLocalRate(float scale, float curScale)
        {
            return scale > curScale ? _rate : -_rate;
        }

        private bool IsSameScale(float scale1, float scale2)
        {
            float aScale1 = Math.Abs(scale1);
            float aSclae2 = Math.Abs(scale2);

            if(Math.Abs(aScale1 - aSclae2) < 0.08f)
            {
                return true;
            }
            return false;
        }
    }
}
