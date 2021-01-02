using System.Collections;
using UnityEngine;

namespace Nextwin.Client.Util
{
    public class ActionManager : Singleton<ActionManager>
    {
        public delegate void Callback();

        /// <summary>
        /// callback1을 실행하고 delayTime 이후 callback2를 실행
        /// </summary>
        /// <param name="callback1">먼저 실행시킬 기능</param>
        /// <param name="callback2">나중에 실행시킬 기능</param>
        /// <param name="delayTime">두 기능 사이 딜레이 시간</param>
        public void ExecuteWithDelay(Callback callback1, Callback callback2, float delayTime)
        {
            callback1?.Invoke();
            StartCoroutine(WaitAndDo(callback2, delayTime));
        }

        /// <summary>
        /// delayTime 이후 callback을 실행
        /// </summary>
        /// <param name="callback">일정 시간 이후 실행할 기능</param>
        /// <param name="delayTime">딜레이 시간</param>
        public void ExecuteWithDelay(Callback callback, float delayTime)
        {
            StartCoroutine(WaitAndDo(callback, delayTime));
        }

        private IEnumerator WaitAndDo(Callback callback, float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
            callback?.Invoke();
        }
    }
}
