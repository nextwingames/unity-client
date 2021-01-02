using Nextwin.Client.Protocol;
using Nextwin.Client.Util;
using Nextwin.Net;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

namespace Nextwin.Client.Game
{
    /// <summary>
    /// 네트워크 스레드 작업자
    /// </summary>
    public class NetworkThreadManager : Singleton<NetworkThreadManager>
    {
        public ConcurrentQueue<byte[]> ServiceQueue { get; private set; }
        private NetworkManager _networkManager;
        [SerializeField]
        private int _sleepTime = 0;

        /// <summary>
        /// 네트워크 스레드 생성
        /// </summary>
        /// <param name="networkManager">게임에서 사용하는 NetworkManager</param>
        /// <returns></returns>
        public Thread CreateNetworkThread(NetworkManager networkManager = null)
        {
            _networkManager = networkManager ?? new NetworkManager(Serializer.Instance);
            ServiceQueue = new ConcurrentQueue<byte[]>();
            return new Thread(new ThreadStart(CheckReceivingAndEnqueueServices));
        }

        private void CheckReceivingAndEnqueueServices()
        {
            Debug.Log("Network thread created.");

            while(_networkManager.IsConnected)
            {
                byte[] receivedData = _networkManager.Receive();
                ServiceQueue.Enqueue(receivedData);

                if(_sleepTime <= 0)
                {
                    continue;
                }
                Thread.Sleep(_sleepTime);
            }

            Debug.Log("Network thread terminated.");
        }
    }
}
