using System;
using UnityEngine;
using System.Collections;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Game.Net
{
    public class USocket
    {
        private UdpClient udpClient;
        private string ip="192.168.0.104";
        private int port = 9988;
        public static IPEndPoint server;
        public USocket(Action<BufferEntity> dispatchNetEvent)
        {
            UdpClient udpClient = new UdpClient(0);
            server =new IPEndPoint(IPAddress.Parse(ip),port);
        }

        //报文缓存 ConcurrentQueue 线程安全的先进先出 (FIFO)
        private ConcurrentQueue<UdpReceiveResult> awaitHandle = new ConcurrentQueue<UdpReceiveResult>();

        /// <summary>
        /// 接受报文
        /// </summary>
        public async void ReceiveTask()//async是在声明异步方法时使用的修饰符，await表达式则负责消费异步操作。
        {
            while (udpClient!=null)
            {
                try
                {
                    UdpReceiveResult result=await udpClient.ReceiveAsync();
                    awaitHandle.Enqueue(result);
                }
                catch (Exception e)
                {
                    Debug.LogError($"接收异常:{e.Message}\n{e.StackTrace}");
                }
            }
        }

        /// <summary>
        /// 发送报文
        /// </summary>
        /// <param name="data"></param>
        /// <param name="endPoint"></param>
        public async void Send(byte[] data,IPEndPoint endPoint)
        {
            try
            {
                //《深入理解C#》—（await）一个异步的操作。这个“等待”看上去非常像一个普通的阻塞调用，
                //剩余的代码在异步操作完成前不会继续执行，但实际上它并没有阻塞当前执行线程
                await udpClient.SendAsync(data, data.Length, ip, port);
            }
            catch (Exception e)
            {
                Debug.LogError($"发送异常:{e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// 发送ACK报文，解包后立刻调用
        /// </summary>
        /// <param name="bufferEntity"></param>
        public void SendACK(BufferEntity bufferEntity)
        {
            Send(bufferEntity.buffer,server);
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            udpClient.Close();
            udpClient = null;
        }
    }
}