using System.Collections.Generic;
using Google.Protobuf;

public class BaseMessageDispatcher {

    protected Dictionary<string, BaseProcessor> m_ProcessorMap = new Dictionary<string, BaseProcessor>();
    

    public void Dispatcher(MessageSerializer serializer, string  typeName, IMessage msg)
    {
        if(m_ProcessorMap.ContainsKey(typeName))
        {
            m_ProcessorMap[typeName].Process(serializer, msg);
        }
    }
}
