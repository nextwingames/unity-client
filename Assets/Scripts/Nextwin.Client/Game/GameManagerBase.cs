using Nextwin.Client.Protocol;
using Nextwin.Client.Util;
using Nextwin.Net;
using System.Threading;
using UnityEngine;

namespace Nextwin.Client.Game
{
    /// <summary>
    /// NetworkManager를 사용하여 서버와 통신하기 위한 GameManager의 상위 클래스
    /// </summary>
    /// <typeparam name="T">GameManagerBase를 상속받을 GameManager 클래스</typeparam>
    [RequireComponent(typeof(Serializer))]
    [RequireComponent(typeof(NetworkThreadManager))]
    public abstract class GameManagerBase<T> : Singleton<T> where T : GameManagerBase<T>
    {
        protected NetworkManager _networkManager;
        protected Thread _networkThread;


        [SerializeField]
        protected string _ip = "127.0.0.1";
        [SerializeField]
        protected int _port;

        protected virtual void Start()
        {
            _networkManager = CreateNetworkManager();
            _networkManager.Connect(_ip, _port);
        }

        /// <summary>
        /// Network Manager 생성
        /// </summary>
        /// <returns></returns>
        protected virtual NetworkManager CreateNetworkManager()
        {
            return new NetworkManager(Serializer.Instance);
        }

        protected virtual void Update()
        {
            if(!_networkManager.IsConnected)
            {
                return;
            }

            CreateNetworkThread();
            CheckServiceQueue();
        }

        protected virtual void CreateNetworkThread()
        {
            if(_networkThread != null)
            {
                return;
            }

            _networkThread = NetworkThreadManager.Instance.CreateNetworkThread(_networkManager);
            _networkThread.Start();
            _networkThread.IsBackground = true;
        }

        private void CheckServiceQueue()
        {
            if(NetworkThreadManager.Instance.ServiceQueue.Count == 0)
            {
                return;
            }

            if(!NetworkThreadManager.Instance.ServiceQueue.TryDequeue(out byte[] receivedData))
            {
                return;
            }
            
            OnReceivedData(SerializableData.ReadMsgTypeFromBytes(receivedData), receivedData);
        }

        /// <summary>
        /// 서버로부터 데이터를 수신하였을 때 호출됨
        /// </summary>
        /// <param name="msgType">받은 데이터의 메시지 타입</param>
        /// <param name="receivedData">직렬화된 수신 데이터</param>
        protected abstract void OnReceivedData(int msgType, byte[] receivedData);
    }
}
