using Nextwin.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nextwin.Client.Util
{
    /// <summary>
    /// 오디오 관리자, 모든 오디오클립의 이름은 고유해야함
    /// </summary>
    public abstract class AudioManagerBase<TAudioClip, TAudioSource> : Singleton<AudioManagerBase<TAudioClip, TAudioSource>> where TAudioClip : Enum where TAudioSource : Enum
    {
        protected Dictionary<TAudioClip, AudioClip> _audioClips;
        [SerializeField, Header("Key: AudioSource layer / Value: AudioSource object")]
        protected SerializableDictionary<TAudioSource, AudioSource> _audioSources;

        protected override void Awake()
        {
            base.Awake();
            LoadAudioClips();
        }

        protected virtual void Start()
        {
            CheckAudioSourcesAssinged();
        }

        /// <summary>
        /// 특정 오디오 소스를 통해 오디오 재생
        /// </summary>
        /// <param name="auidoClipName">재생하려는 오디오 클립 이름</param>
        /// <param name="audioSourceKey">오디오 클립이 재생될 오디오소스 레이어 이름</param>
        public virtual void PlayAudio(TAudioClip auidoClipName, TAudioSource audioSourceKey)
        {
            AudioSource source = _audioSources[audioSourceKey];
            source.clip = _audioClips[auidoClipName];
            source.Play();
        }

        /// <summary>
        /// 모든 오디오 일시정지
        /// </summary>
        public virtual void PauseAll()
        {
            foreach(KeyValuePair<TAudioSource, AudioSource> item in _audioSources)
            {
                item.Value.Pause();
            }
        }

        /// <summary>
        /// 특정 오디오소스 레이어 일시정지
        /// </summary>
        /// <param name="audioSourceKey"></param>
        public virtual void Pause(TAudioSource audioSourceKey)
        {
            _audioSources[audioSourceKey].Pause();
        }

        /// <summary>
        /// 모든 오디오 재생
        /// </summary>
        public virtual void ResumeAll()
        {
            foreach(KeyValuePair<TAudioSource, AudioSource> item in _audioSources)
            {
                item.Value.UnPause();
            }
        }

        /// <summary>
        /// 모든 오디오소스 레이어 재생
        /// </summary>
        /// <param name="audioSourceKey"></param>
        public virtual void Resume(TAudioSource audioSourceKey)
        {
            _audioSources[audioSourceKey].UnPause();
        }

        protected virtual void CheckAudioSourcesAssinged()
        {
            if(_audioSources.Count == 0)
            {
                Debug.LogError("Assign AudioSource.");
            }

            foreach(KeyValuePair<TAudioSource, AudioSource> item in _audioSources)
            {
                if(item.Value == null)
                {
                    Debug.LogError($"[AudioManager Error] Add AudioSource corresponding to {item.Key}.");
                }
            }
        }

        protected virtual void LoadAudioClips()
        {
            AudioClip[] clips = Resources.LoadAll<AudioClip>("");
            foreach(AudioClip clip in clips)
            {
                _audioClips.Add(EnumConverter.ToEnum<TAudioClip>(clip.name), clip);
            }
        }
    }
}
