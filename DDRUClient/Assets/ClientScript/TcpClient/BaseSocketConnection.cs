

using Google.Protobuf;

public class BaseSocketConnection {

    protected  MessageSerializer m_MessageSerializer;

    protected virtual void ReadData(object sender,byte[] buf)
    {

    }
    protected virtual void SendData(IMessage buf)
    {

    }
    public virtual void Close()
    {

    }


}
