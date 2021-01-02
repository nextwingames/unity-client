using MessagePack;
using Nextwin.Util;

namespace Nextwin.Client.Protocol
{
    /// <summary>
    /// 데이터 전송을 위한 최상위 클래스로 SerializableData를 상속받는 모든 클래스의 필드변수의 Key는 1부터 시작
    /// </summary>
    [MessagePackObject]
    public class SerializableData
    {
        [Key(0)]
        public int MsgType { get; set; }

        public SerializableData(int msgType)
        {
            if(!IsValidMsgType(msgType))
            {
                return;
            }

            MsgType = msgType;
        }

        public static int ReadMsgTypeFromBytes(byte[] bytes)
        {
            return bytes[1];
        }

        private static bool IsValidMsgType(int msgType)
        {
            if(msgType < 0 || msgType > 255)
            {
                Print.LogError($"Invalid msgType. It should be 0 or more and 255 or less but it is {msgType}");
                return false;
            }
            return true;
        }
    }
}
