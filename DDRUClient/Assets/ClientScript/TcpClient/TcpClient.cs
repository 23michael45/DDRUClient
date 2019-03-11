using Google.Protobuf;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class AsyncTcpClient : BaseSocketConnection
{
    
    public AsyncTcpClient()
    {
        m_MessageSerializer = new MessageSerializer(this, new ClientMessageDispatcher());

        OnDataReceived += ReadData;
    }


    public MessageSerializer GetMessageSerializer()
    {
        return m_MessageSerializer;
    }
    
    private TcpClient tcpClient;

    private int minBufferSize = 8192;
    private int maxBufferSize = 15 * 1024 * 1024;
    private int bufferSize = 8192;

    private int BufferSize
    {
        get
        {
            return this.bufferSize;
        }
        set
        {
            this.bufferSize = value;
            if (this.tcpClient != null)
                this.tcpClient.ReceiveBufferSize = value;
        }
    }

    public int MinBufferSize
    {
        get
        {
            return this.minBufferSize;
        }
        set
        {
            this.minBufferSize = value;
        }
    }

    public int MaxBufferSize
    {
        get
        {
            return this.maxBufferSize;
        }
        set
        {
            this.maxBufferSize = value;
        }
    }

    public int SendBufferSize
    {
        get
        {
            if (this.tcpClient != null)
                return this.tcpClient.SendBufferSize;
            else
                return 0;
        }
        set
        {
            if (this.tcpClient != null)
                this.tcpClient.SendBufferSize = value;
        }
    }

    public event EventHandler<byte[]> OnDataReceived;
    public event EventHandler OnDisconnected;

    public bool IsConnected
    {
        get
        {
            return this.tcpClient != null && this.tcpClient.Connected;
        }
    }

    public void Connect(string ip ,int port)
    {
        //IPHostEntry ipHostInfo = Dns.GetHostEntry(ip);
        //IPAddress ipAddress = ipHostInfo.AddressList[0];

        IPAddress ipAddress = IPAddress.Parse(ip);
        IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

        Task t = ConnectAsync(remoteEP);
    }
    

    public async Task SendAsync(MemoryStream ms)
    {
        try
        {
            this.tcpClient.GetStream().BeginWrite(ms.GetBuffer(), 0, (int)ms.Length, DataSendCallback, null);

            //await Task.Factory.FromAsync(this.stream.BeginWrite, this.stream.EndWrite, ms.GetBuffer(), 0, (int)ms.Length,null);
            //await this.stream.FlushAsync();
        }
        catch (IOException ex)
        {
            if (ex.InnerException != null && ex.InnerException is ObjectDisposedException) // for SSL streams
                ; // ignore
            else if (this.OnDisconnected != null)
                this.OnDisconnected(this, null);
        }
    }

    protected virtual void DataSendCallback(IAsyncResult asyncResult)
    {
        try
        {
            tcpClient.GetStream().EndWrite(asyncResult);

        }
        catch (ObjectDisposedException) // can occur when closing, because tcpclient and stream are disposed
        {
            // ignore
        }
        catch (IOException ex)
        {
            if (ex.InnerException != null && ex.InnerException is ObjectDisposedException) // for SSL streams
                ; // ignore
            else if (this.OnDisconnected != null)
                this.OnDisconnected(this, null);
        }
    }

    public async Task ConnectAsync(IPEndPoint ep, CancellationTokenSource cancellationTokenSource = null)
    {
        try
        {
            await Task.Run(() => this.tcpClient = new TcpClient());


            await Task.Factory.FromAsync(this.tcpClient.BeginConnect, this.tcpClient.EndConnect,ep.Address, ep.Port, null);

            await Task.Factory.StartNew(() => StartReceiving());
           

            if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
            {
                this.Dispose();
            }
        }
        catch (Exception)
        {
            // if task has been cancelled, then we don't care about the exception;
            // if it's still running, then the caller must receive the exception

            if (cancellationTokenSource == null || !cancellationTokenSource.IsCancellationRequested)
                throw;
        }
    }

    public void StartReceiving()
    {
        byte[] buffer = new byte[bufferSize];
        this.tcpClient.GetStream().BeginRead(buffer, 0, buffer.Length, DataReceivedCallback, buffer);
    }

    protected virtual void DataReceivedCallback(IAsyncResult asyncResult)
    {
        try
        {
            if(tcpClient == null)
            {
                return;
            }

            Debug.Log("DataReceivedCallback");
            byte[] buffer = asyncResult.AsyncState as byte[];
            int bytesRead = this.tcpClient.GetStream().EndRead(asyncResult);

            if (bytesRead > 0)
            {
                // adapt buffer if it's too small / too large

                if (bytesRead == buffer.Length)
                    this.BufferSize = Math.Min(this.BufferSize * 10, this.maxBufferSize);
                else
                {
                reduceBufferSize:
                    int reducedBufferSize = Math.Max(this.BufferSize / 10, this.minBufferSize);
                    if (bytesRead < reducedBufferSize)
                    {
                        this.BufferSize = reducedBufferSize;

                        if (bytesRead > this.minBufferSize)
                            goto reduceBufferSize;
                    }
                }

                // forward received data to subscriber

                if (this.OnDataReceived != null)
                {
                    byte[] data = new byte[bytesRead];
                    Array.Copy(buffer, data, bytesRead);
                    this.OnDataReceived(this, data);
                }

                // recurse

                byte[] newBuffer = new byte[bufferSize];
                this.tcpClient.GetStream().BeginRead(newBuffer, 0, newBuffer.Length, DataReceivedCallback, newBuffer);
            }
            else
            {
                this.OnDisconnected(this, null);

            }

        }
        catch (ObjectDisposedException) // can occur when closing, because tcpclient and stream are disposed
        {
            // ignore
        }
        catch (IOException ex)
        {
            if (ex.InnerException != null && ex.InnerException is ObjectDisposedException) // for SSL streams
                ; // ignore
            else if (this.OnDisconnected != null)
                this.OnDisconnected(this, null);
        }
    }
    public void Dispose()
    {
        if (this.tcpClient != null)
        {
            this.tcpClient.Close();
            this.tcpClient = null;
        }
        OnDataReceived -= ReadData;

        GC.SuppressFinalize(this);
    }

    public override void Close()
    {
        Dispose();
    }

    protected override void ReadData(object sender,byte[] buf)
    {
        m_MessageSerializer.ProcessReceive(buf);

    }
    protected override void SendData(IMessage msg)
    {
        Task t = SendAsync(MessageSerializer.Serialize(msg));
    }
    public void Send(IMessage msg)
    {
        SendData(msg);
    }
}
