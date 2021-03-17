using Google.Protobuf;
using UnityEngine;

namespace Game.Net
{
    public class BufferFactory
    {
        enum MessageType
        {
            ACK = 0, //确认报文
            Login = 1, //业务逻辑的报文
        }

        /// <summary>
        /// 创建并且发送报文
        /// </summary>
        /// <param name="uClient"></param>
        /// <param name="messageID"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static BufferEntity CreateAndSendPackage(int messageID, IMessage message)
        {
            JsonHelper.Log(messageID,message);

            BufferEntity bufferEntity = new BufferEntity(USocket.local.endPoint, USocket.local.sessionID, 0, 0,
                MessageType.Login.GetHashCode(),
                messageID, ProtobufHelper.ToBytes(message));

            USocket.local.Send(bufferEntity);

            return bufferEntity;
        }
    }
}