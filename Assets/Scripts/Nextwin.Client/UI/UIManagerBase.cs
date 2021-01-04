using Nextwin.Client.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nextwin.Client.UI
{
    /// <summary>
    /// UIManager의 base 추상 클래스, TEFrame과 TEDialog에 대응하는 enum을 정의해야함
    /// </summary>
    /// <typeparam name="T">UIManagerBase를 상속받는 UIManager 클래스</typeparam>
    /// <typeparam name="TFrame">Frame 식별자</typeparam>
    /// <typeparam name="TDialog">Dialog 식별자</typeparam>
    public abstract class UIManagerBase<T, TFrame, TDialog> : Singleton<UIManagerBase<T, TFrame, TDialog>> 
        where TFrame : Enum where TDialog : Enum where T : UIManagerBase<T, TFrame, TDialog>
    {
        protected Dictionary<TFrame, UIFrame<TFrame>> _frames;
        protected Dictionary<TDialog, UIDialog<TDialog>> _dialogs;

        protected virtual void Start()
        {
            FindUIs(ref _frames);
            FindUIs(ref _dialogs);
        }

        public virtual UIFrame<TFrame> GetFrame(TFrame frameID)
        {
            if(!_frames.ContainsKey(frameID))
            {
                Debug.LogError($"There is no UI which id is {frameID}.");
                return null;
            }
            return _frames[frameID];
        }

        public virtual UIDialog<TDialog> GetDialog(TDialog dialogID)
        {
            if(!_dialogs.ContainsKey(dialogID))
            {
                Debug.LogError($"There is no UI which id is {dialogID}.");
                return null;
            }
            return _dialogs[dialogID];
        }

        /// <summary>
        /// 모든 UI를 찾아 Dictionary에 추가한 후 비활성화
        /// </summary>
        /// <typeparam name="TBase">UIBase의 하위 클래스</typeparam>
        /// <typeparam name="TUI">UIBase의 ID를 지정할 enum</typeparam>
        /// <param name="dic">UI를 담을 Dictionary</param>
        protected virtual void FindUIs<TBase, TUI>(ref Dictionary<TUI, TBase> dic) where TBase : UIBase<TUI> where TUI : Enum
        {
            dic = new Dictionary<TUI, TBase>();

            foreach(TBase ui in FindObjectsOfType(typeof(TBase)))
            {
                if(dic.ContainsKey(ui.ID))
                {
                    Debug.LogError($"Duplicated ID {ui.ID} of {ui.gameObject.name}.");
                    continue;
                }

                dic.Add(ui.ID, ui);
                ui.Show(false);
            }
        }
    }
}
