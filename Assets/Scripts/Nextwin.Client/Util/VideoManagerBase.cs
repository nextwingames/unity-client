using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Nextwin.Client.Util
{
    public abstract class VideoManagerBase<TScreen, TVideoPlayer> : Singleton<VideoManagerBase<TScreen, TVideoPlayer>> where TScreen : Enum where TVideoPlayer : Enum
    {
        public delegate void Callback();

        [SerializeField, Header("Set your video resources path")]
        [Tooltip("If your videos are in Assets/Resources/Videos directory, set as \"Videos\"")]
        protected string _videoResourcesPath;

        [SerializeField, Header("Key: Screen name / Value: Screen to play video (RawImage)")]
        protected SerializableDictionary<TScreen, RawImage> _screens;
        [SerializeField, Header("Key: VideoPlayer name / Value: VideoPlayer component")]
        protected SerializableDictionary<TVideoPlayer, VideoPlayer> _videoPlayers;
        protected Dictionary<string, Texture> _textures = new Dictionary<string, Texture>();

        protected float _waitForCallbackExecute = 3f;

        protected virtual void Start()
        {
            CheckComponentsAssigned();
        }

        /// <summary>
        /// 비디오를 재생, 비디오가 끝나면 특정 작업을 실행 시킬 수 있음
        /// </summary>
        /// <param name="videoNameWithDirectoryName">재생할 비디오의 이름(설정한 VideoResourcesPath의 하위 폴더 안에 파일이 있다면 SubDir/VideoName과 같이 명시)</param>
        /// <param name="videoPlayerName">비디오가 재생될 VideoPlayer 이름</param>
        /// <param name="callback">비디오가 끝난 후 실행할 작업</param>
        public virtual void PlayVideo(string videoNameWithDirectoryName, TVideoPlayer videoPlayerName, TScreen screenName, Callback callback = null)
        {
            VideoPlayer player = GetVideoPlayer(videoPlayerName);
            if(player == null)
            {
                return;
            }

            VideoClip clip = GetVideoClip(videoNameWithDirectoryName);
            if(clip == null)
            {
                return;
            }

            RawImage screen = GetScreen(screenName);
            if(screen == null)
            {
                return;
            }

            ActionManager.Instance.ExecuteWithDelay(() =>
            {
                player.Play();
            }, () =>
            {
                OnScreenOn(screen);
            }, 0.5f);

            player.loopPointReached += (videoPlayer) =>
            {
                // 비디오가 끝난 후 waitAndCallback초 후에 callback 실행
                ActionManager.Instance.ExecuteWithDelay(() =>
                {
                    OnScreenOff(screen);
                }, () =>
                {
                    callback?.Invoke();
                }, _waitForCallbackExecute);
            };
        }

        /// <summary>
        /// 모든 VideoPlayer 일시정지
        /// </summary>
        public virtual void PauseAll()
        {
            foreach(KeyValuePair<TVideoPlayer, VideoPlayer> item in _videoPlayers)
            {
                item.Value.Pause();
            }
        }

        /// <summary>
        /// 특정 VideoPlayer 일시정지
        /// </summary>
        /// <param name="videoPlayerName">일시정지 시키려는 VideoPlayer 이름</param>
        public virtual void Pause(TVideoPlayer videoPlayerName)
        {
            _videoPlayers[videoPlayerName].Pause();
        }

        /// <summary>
        /// 모든 VideoPlayer 재생
        /// </summary>
        public virtual void ResumeAll()
        {
            foreach(KeyValuePair<TVideoPlayer, VideoPlayer> item in _videoPlayers)
            {
                item.Value.Play();
            }
        }

        /// <summary>
        /// 특정 VideoPlayer 재생
        /// </summary>
        /// <param name="videoPlayerName">일시정지 시키려는 VideoPlayer 이름</param>
        public virtual void Resume(TVideoPlayer videoPlayerName)
        {
            _videoPlayers[videoPlayerName].Play();
        }

        protected virtual VideoPlayer GetVideoPlayer(TVideoPlayer videoPlayerName)
        {
            if(videoPlayerName == null)
            {
                foreach(KeyValuePair<TVideoPlayer, VideoPlayer> item in _videoPlayers)
                {
                    return item.Value;
                }
            }

            if(!_videoPlayers.ContainsKey(videoPlayerName))
            {
                Debug.LogError($"There is no VideoPlayer which key is {videoPlayerName}");
                return null;
            }

            return _videoPlayers[videoPlayerName];
        }

        protected virtual VideoClip GetVideoClip(string videoNameWithDirectoryName)
        {
            VideoClip clip = Resources.Load<VideoClip>($"{_videoResourcesPath}/{videoNameWithDirectoryName}");

            if(clip == null)
            {
                Debug.LogError($"There is no video named {videoNameWithDirectoryName}");
            }

            return clip;
        }

        protected virtual RawImage GetScreen(TScreen screenName)
        {
            if(screenName == null)
            {
                foreach(KeyValuePair<TScreen, RawImage> item in _screens)
                {
                    return item.Value;
                }
            }

            if(!_screens.ContainsKey(screenName))
            {
                Debug.LogError($"There is no screen which key is {screenName}");
                return null;
            }

            return _screens[screenName];
        }

        protected virtual void OnScreenOn(RawImage screen)
        {
            StartCoroutine(FadeIn(screen));
        }

        protected virtual void OnScreenOff(RawImage screen)
        {
            StartCoroutine(FadeOut(screen));
        }

        protected virtual IEnumerator FadeIn(RawImage screen)
        {
            screen.gameObject.SetActive(true);
            Color32 color = screen.color;

            WaitForSeconds waitForSeconds = new WaitForSeconds(0.0001f);
            while(color.a < 255)
            {
                color.a += 3;
                screen.color = color;
                yield return waitForSeconds;
            }
        }

        protected virtual IEnumerator FadeOut(RawImage screen)
        {
            Color32 color = screen.color;

            WaitForSeconds waitForSeconds = new WaitForSeconds(0.0001f);
            while(color.a > 0)
            {
                color.a -= 3;
                screen.color = color;
                yield return waitForSeconds;
            }

            screen.gameObject.SetActive(false);
        }

        protected virtual void CheckComponentsAssigned()
        {
            if(_screens.Count == 0)
            {
                Debug.LogError("There is no screen assgined for VideoManager.");
            }
            if(_videoPlayers.Count == 0)
            {
                Debug.LogError("There is no VideoPlayer assgined for VideoManager.");
            }

            CheckTargetTextureAssigned();
        }

        protected virtual void CheckTargetTextureAssigned()
        {
            foreach(KeyValuePair<TVideoPlayer, VideoPlayer> item in _videoPlayers)
            {
                Texture texture = item.Value.targetTexture;
                if(texture == null)
                {
                    Debug.LogError($"Assign target texture to {item.Key} VideoPlayer.");
                }

                _textures.Add(item.Key.ToString(), texture);
            }
        }
    }
}
