using System;
using UnityEngine;

namespace Nextwin.Client.UI
{
    public abstract class UIBase<TUI> : MonoBehaviour where TUI : Enum
    {
        public TUI ID
        {
            get { return _id; }
        }
        [SerializeField]
        private TUI _id;

        protected delegate void Callback();

        [SerializeField, Range(1, 100), Tooltip("If this rate is high, the UI will appear and disappear fast")]
        protected int _showSpeedRate = 50;
        protected float _rate;

        protected float _yieldTime = 0.01f;

        public virtual void Show(bool isActive = true)
        {
            gameObject.SetActive(isActive);
        }

        protected void SetRate()
        {
            _rate = _showSpeedRate / (float)1000f;
        }
    }
}
