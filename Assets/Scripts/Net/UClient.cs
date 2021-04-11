using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.Net
{
    //网络客户端代理
    public class UClient
    {
        public IPEndPoint endPoint;
        /// <summary>
        ///内部封装了发送的接口
        /// </summary>
        USocket uSocket;
        /// <summary>
        ///会话ID
        /// </summary>
        public int sessionID;

        /// <summary>
        ///发送序号 
        /// </summary>
        public int sendSN = 0;

        /// <summary>
        ///处理的序号 为了保证报文的顺序性
        /// </summary>
        public int handleSN = 0;

        Action<BufferEntity> handleAction;//处理报文的函数 实际就是分发报文给各个游戏模块

        /// <summary>
        /// 构造UClient
        /// </summary>
        public UClient(USocket uSocket,IPEndPoint endPoint,int sendSN,int handleSN,int sessionID, Action<BufferEntity> dispatchNetEvent) {
            this.uSocket = uSocket;
            this.endPoint = endPoint;
            this.sendSN = sendSN;
            this.handleSN = handleSN;
            this.sessionID = sessionID;
            handleAction = dispatchNetEvent;

            CheckOutTime();//超时检测
        }
        

        /// <summary>
        ///处理消息 :按照报文的序号 进行顺序处理  如果是收到超过当前顺序+1的报文 先进行缓存 
        /// </summary>
        /// <param name="buffer"></param>
        public void Handle(BufferEntity buffer) {
            if (this.sessionID==0&&buffer.session!=0)
            {
                Debug.Log($"服务发送给我们的会话ID是:{buffer.session}");
                this.sessionID = buffer.session;
            }
            //else
            //{
            //    if (buffer.session==this.sessionID)
            //    {

            //    }
            //}

            switch (buffer.messageType)
            {
                case 0://ACK确认报文
                    BufferEntity bufferEntity;
                    if (sendPackage.TryRemove(buffer.sn,out bufferEntity))//收到确认后移除发送缓存中的一确认报文
                    {
                        Debug.Log($"收到ACK确认报文,序号是:{buffer.sn}");
                    }
                    else
                    {
                        Debug.Log($"要确认的报文不存在，序号：{buffer.sn}");
                    }
                    break;
                case 1://业务报文
                    BufferEntity ackPacka = new BufferEntity(buffer);
                    uSocket.SendACK(ackPacka);//先告诉服务器 我已经收到这个报文

                    //再来处理业务报文
                    HandleLogincPackage(buffer);
                    break;
                default:
                    break;
            }
        }


        
        /// <summary>
        ///缓存已经发送的报文
        /// </summary>
        ConcurrentDictionary<int, BufferEntity> waitHandle = new ConcurrentDictionary<int, BufferEntity>();

        

        /// <summary>
        ///处理业务报文的逻辑
        /// </summary>
        /// <param name="buffer"></param>
        void HandleLogincPackage(BufferEntity buffer) {

            if (buffer.sn<=handleSN)
            {
                return;
            }

            //已经收到的报文是错序的
            if (buffer.sn-handleSN>1)
            {
                if (waitHandle.TryAdd(buffer.sn,buffer))
                {
                    Debug.Log($"收到错序的报文:{buffer.sn}");
                }
                return;
            }

            //更新已处理的报文 
            handleSN = buffer.sn;
            //派发 分发给游戏模块去处理
            handleAction?.Invoke(buffer);

            //检测缓存的数据 有没有包含下一条可以处理的数据
            BufferEntity nextBuffer;
            if (waitHandle.TryRemove(handleSN+1,out nextBuffer))
            {
                //这里是判断缓冲区有没有存在下一条数据
                HandleLogincPackage(nextBuffer);
            }

        }


        //缓存已经发送的报文
        ConcurrentDictionary<int, BufferEntity> sendPackage = new ConcurrentDictionary<int, BufferEntity>();
        
        /// <summary>
        ///  发送的接口 
        /// </summary>
        /// <param name="package"></param>
        public  void Send(BufferEntity package)
        {
            package.time = TimeHelper.Now();//暂时先等于0
            sendSN += 1;//
            package.sn = sendSN;

            package.Encoder(false);
            if (sessionID!=0)
            {
                //缓存起来 因为可能需要重发
                sendPackage.TryAdd(sendSN,package);
            }
            else
            {
                //还没跟服务器建立连接的 所以不需要进行缓存 
            }
            uSocket.Send(package.buffer,endPoint);
        }

        /// <summary>
        /// 超时时间
        /// </summary>
        int overtime = 150;
        
        /// <summary>
        /// 超时重传检测的接口
        /// </summary>
        public async void CheckOutTime() {

            await Task.Delay(overtime);
            foreach (var package in sendPackage.Values)
            {
                //确定是不是超过最大发送次数  关闭socket
                if (package.recurCount >= 10)
                {
                    Debug.Log($"重发次数超过10次,关闭socket");
                    OnDisconnect();
                    return;
                }
               
                //如果发送报文时间超过了超时时间，则冲发报文
                if (TimeHelper.Now()-package.time>=(package.recurCount+1)*overtime)
                {
                    package.recurCount += 1;
                    Debug.Log($"超时重发,次数:{package.recurCount}");
                    uSocket.Send(package.buffer, endPoint);
                }
            }
            CheckOutTime();
        }

        public void OnDisconnect()
        {
            handleAction = null;
            uSocket.Close();

        }

    }

    

}
