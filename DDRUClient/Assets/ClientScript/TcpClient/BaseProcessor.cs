using Google.Protobuf;

public class BaseProcessor {
    
    public virtual void Process(MessageSerializer route, IMessage msg)
    {

    }
    public virtual void AsyncProcess(MessageSerializer route, IMessage msg)
    {

    }
}
