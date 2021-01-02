using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Nextwin.Client.UI
{
    public abstract class UIFrame<TFrame> : UIBase<TFrame> where TFrame : Enum
    {
        private GameObject _shadeImageObject;
        private Image _shadeImage;
        private MaskableGraphic[] _graphics;

        protected virtual void Awake()
        {
            _graphics = GetComponentsInChildren<MaskableGraphic>();
            CreateShadeImage();
        }

        public override void Show(bool isActive = true)
        {
            SetRate();

            if(isActive)
            {
                SetGraphicsTransparency(true);
                SetShadeTransparency(true);
                gameObject.SetActive(true);

                StartCoroutine(ChangeShadeTransparency(false, () =>
                {
                    SetGraphicsTransparency(false);
                    StartCoroutine(ChangeShadeTransparency(true, () =>
                    {
                        _shadeImageObject.SetActive(false);
                    }));
                }));
            }
            else
            {
                _shadeImageObject.SetActive(true);
                StartCoroutine(ChangeShadeTransparency(false, () =>
                {
                    SetGraphicsTransparency(true);
                    StartCoroutine(ChangeShadeTransparency(true, () =>
                    {
                        gameObject.SetActive(false);
                    }));
                }));
            }
        }

        /// <summary>
        /// 가리개 이미지의 투명도를 서서히 변경시킨 후 특정 작업 실행
        /// </summary>
        /// <param name="toTransparent">투명도 여부</param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private IEnumerator ChangeShadeTransparency(bool toTransparent, Callback callback = null)
        {
            Color color = _shadeImage.color;
            float rate = toTransparent ? -_rate : _rate;
            bool isLoop = true;

            while(isLoop)
            {
                color.a += rate;
                _shadeImage.color = color;

                yield return new WaitForSeconds(_yieldTime);

                isLoop = toTransparent ? color.a > 0f : color.a < 1f;
            }

            callback?.Invoke();
        }

        /// <summary>
        /// 가리개 UI의 투명도를 설정
        /// </summary>
        /// <param name="toTransparent"></param>
        private void SetShadeTransparency(bool toTransparent)
        {
            float alpha = toTransparent ? 0f : 1f;

            Color color = _shadeImage.color;
            color.a = alpha;
            _shadeImage.color = color;
        }

        /// <summary>
        /// Frame에 있는 모든 그래픽 요소의 투명도를 설정
        /// </summary>
        /// <param name="toTransparent">투명 여부</param>
        private void SetGraphicsTransparency(bool toTransparent)
        {
            float alpha = toTransparent ? 0f : 1f;

            Color[] graphicColors = GetGraphicColors();
            for(int i = 0; i < graphicColors.Length; i++)
            {
                graphicColors[i].a = alpha;
                _graphics[i].color = graphicColors[i];
            }
        }

        private Color[] GetGraphicColors()
        {
            Color[] colors = new Color[_graphics.Length];
            for(int i = 0; i < colors.Length; i++)
            {
                colors[i] = _graphics[i].color;
            }
            return colors;
        }

        /// <summary>
        /// 가리개 이미지 생성
        /// </summary>
        private void CreateShadeImage()
        {
            _shadeImageObject = new GameObject();
            _shadeImageObject.AddComponent<RectTransform>();
            _shadeImageObject.AddComponent<Image>();

            _shadeImageObject.transform.position = transform.position;

            RectTransform shadeRect = _shadeImageObject.GetComponent<RectTransform>();
            RectTransform rect = GetComponent<RectTransform>();

            shadeRect.SetParent(transform);
            shadeRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.rect.width);
            shadeRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect.rect.height);

            _shadeImage = _shadeImageObject.GetComponent<Image>();
            _shadeImage.color = new Color(0, 0, 0, 0);
        }
    }
}
