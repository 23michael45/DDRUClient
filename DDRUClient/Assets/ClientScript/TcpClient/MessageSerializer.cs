using DDRCommLib;
using DDRCommProto;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;



public class QueueBuffer<T> where T : struct
{
    //                                                    capacity  
    //    |      |d      |d      |d      |d      |      |      |                  |d used position           |empty position
    //    0      1       2       3       4       MessageSerializer.PROTOBUF_ENCRYPT_LEN      6      7  
    //          head                    tail                  
    //    |d      |d      |d      |      |      |      |d      |d                  |d used position           |empty position
    //                  tail                           head
    public int m_Capacity;
    public int m_Head = 0;
    public int m_Tail = 0;
    bool m_IsEmpty = true;

    public Mutex m_Mutex = new Mutex();

    T[] m_Buffer;

    public QueueBuffer(int capacity = 1024)
    {
        m_Capacity = capacity;
        m_Buffer = new T[m_Capacity];
        SetEmpty();
    }


    public int GetLength()
    {
        if (m_IsEmpty)
        {
            return 0;
        }


        if (m_Head <= m_Tail)
        {
            return m_Tail - m_Head + 1;
        }
        else
        {
            return (m_Capacity - m_Head) + (m_Tail + 1);
        }

    }
    public int GetAvaliable()
    {
        return m_Capacity - GetLength();
    }

    void SetEmpty()
    {
        m_IsEmpty = true;
        m_Tail = -1;
        m_Head = -1;
    }
    void PrintState()
    {
        //Debug.Log(string.Format("Dequeue Capacity: {0}  Size: {1} Head: {2} Tail: {3}", m_Capacity, GetLength(), m_Head, m_Tail));

    }
    void PrintStatePeek()
    {
        //Debug.Log(string.Format("Peek   Capacity: {0}  Size: {1} Head: {2} Tail: {3}", m_Capacity, GetLength(), m_Head, m_Tail));

    }

    void AutoAllocBuff(int howManyToAdd)
    {
        int availableSpace = GetAvaliable();
        if (howManyToAdd > availableSpace)
        {

            int dstCapacity = m_Capacity;
            do
            {
                dstCapacity *= 2;
            }
            while (howManyToAdd > (dstCapacity - m_Capacity + availableSpace));

            int oldCapacity = m_Capacity;
            m_Capacity = dstCapacity;

            var oldBuffer = m_Buffer;
            m_Buffer = new T[dstCapacity];

            if (!m_IsEmpty)
            {
                if (m_Head > m_Tail)
                {
                    int rightStart = m_Head;
                    int rightLen = (oldCapacity - m_Head);
                    int leftLen = m_Tail + 1;

                    Array.Copy(oldBuffer, rightStart, m_Buffer, 0, rightLen);
                    Array.Copy(oldBuffer, 0, m_Buffer, rightLen, leftLen);

                    m_Tail = rightLen + leftLen - 1;
                    m_Head = 0;
                }
                else
                {
                    int start = m_Head;
                    int len = m_Tail - m_Head + 1;

                    Array.Copy(oldBuffer, start, m_Buffer, 0, m_Tail - m_Head + 1);

                    m_Tail = len - 1;
                    m_Head = 0;
                }

            }

            //Debug.Log(string.Format("Allocate Capacity: {0} Size: {1} Head: {2} Tail: {3}" ,m_Capacity, GetLength(), m_Head, m_Tail));
        }
    }
    public void Enqueue(T[] src)
    {
        m_Mutex.WaitOne();
        int howManyToAdd = src.Length;

        AutoAllocBuff(howManyToAdd);

        if (howManyToAdd <= 0)
        {
            m_Mutex.ReleaseMutex();
            return;

        }

        if (m_Head > m_Tail)
        {

            int avaliableLen = m_Head - m_Tail - 1;

            if (howManyToAdd <= avaliableLen)
            {
                Array.Copy(src, 0, m_Buffer, m_Tail + 1, howManyToAdd);
                m_Tail += howManyToAdd;

            }
            else
            {
                Debug.Log("Not Enough Space To Enqueue,A Bug Curr 1");
            }


        }
        else
        {

            int rightAvaliableLen = m_Capacity - m_Tail - 1;
            int leftAvaliableLen = 0;

            if (m_Head == -1)
            {
                m_Head = 0;
                leftAvaliableLen = 0;
                m_IsEmpty = false;

            }
            else
            {
                leftAvaliableLen = m_Head;

            }


            if (howManyToAdd <= rightAvaliableLen)
            {
                Array.Copy(src, 0, m_Buffer, m_Tail + 1, howManyToAdd);
                m_Tail += howManyToAdd;
            }
            else if (howManyToAdd - rightAvaliableLen <= leftAvaliableLen)
            {
                Array.Copy(src, 0, m_Buffer, m_Tail + 1, rightAvaliableLen);
                Array.Copy(src, rightAvaliableLen, m_Buffer, 0, howManyToAdd - rightAvaliableLen);
                m_Tail = howManyToAdd - rightAvaliableLen - 1;
            }
            else
            {

                Debug.Log("Not Enough Space To Enqueue,A Bug Curr 1");
            }



        }
        m_Mutex.ReleaseMutex();

    }
    public int Dequeue(T[] dst)
    {
        m_Mutex.WaitOne();
        if (m_IsEmpty)
        {
            m_Mutex.ReleaseMutex();

            return 0;
        }

        if (m_Head > m_Tail)
        {

            int rightLen = (m_Capacity - m_Head);
            int leftLen = m_Tail + 1;
            int rightStart = m_Head;


            if (dst.Length <= rightLen)
            {
                Array.Copy(m_Buffer, rightStart, dst, 0, dst.Length);
                m_Head += dst.Length;
                m_Head %= m_Capacity;

                m_Mutex.ReleaseMutex();
                PrintState();
                return dst.Length;
            }
            else if (dst.Length < rightLen + leftLen)
            {
                Array.Copy(m_Buffer, rightStart, dst, 0, rightLen);
                Array.Copy(m_Buffer, 0, dst, rightLen, dst.Length - rightLen);

                m_Head = dst.Length - rightLen;


                m_Mutex.ReleaseMutex();


                PrintState();
                return dst.Length;

            }
            else
            {
                Array.Copy(m_Buffer, rightStart, dst, 0, rightLen);
                Array.Copy(m_Buffer, 0, dst, rightLen, leftLen);


                m_Head = leftLen;

                SetEmpty();

                m_Mutex.ReleaseMutex();


                PrintState();
                return rightLen + leftLen;
            }


        }
        else
        {
            int len = m_Tail - m_Head + 1;
            if (dst.Length < len)
            {
                Array.Copy(m_Buffer, m_Head, dst, 0, dst.Length);
                m_Head += dst.Length;

                m_Mutex.ReleaseMutex();


                PrintState();
                return dst.Length;
            }
            else
            {
                Array.Copy(m_Buffer, m_Head, dst, 0, len);
                m_Head += len;
                SetEmpty();


                m_Mutex.ReleaseMutex();


                PrintState();
                return len;
            }

        }


    }

    public int Peek(T[] dst)
    {

        if (m_Head > m_Tail)
        {

            int rightLen = (m_Capacity - m_Head);
            int leftLen = m_Tail + 1;
            int rightStart = m_Head;


            if (dst.Length <= rightLen)
            {
                Array.Copy(m_Buffer, rightStart, dst, 0, dst.Length);

                PrintStatePeek();
                return dst.Length;

            }
            else if (dst.Length < rightLen + leftLen)
            {
                Array.Copy(m_Buffer, rightStart, dst, 0, rightLen);
                Array.Copy(m_Buffer, 0, dst, rightLen, dst.Length - rightLen);


                PrintStatePeek();
                return dst.Length;
            }
            else
            {
                Array.Copy(m_Buffer, rightStart, dst, 0, rightLen);
                Array.Copy(m_Buffer, 0, dst, rightLen, leftLen);


                PrintStatePeek();
                return rightLen + leftLen;
            }


        }
        else
        {
            int len = m_Tail - m_Head + 1;
            if (dst.Length < len)
            {
                Array.Copy(m_Buffer, m_Head, dst, 0, dst.Length);

                PrintStatePeek();
                return dst.Length;
            }
            else
            {
                Array.Copy(m_Buffer, m_Head, dst, 0, len);

                PrintStatePeek();
                return len;
            }

        }


    }


}

public class MessageSerializer
{

    public MessageSerializer(BaseSocketConnection bc, BaseMessageDispatcher dispather)
    {
        m_MessageDispatcher = dispather;
        m_BaseSocketConnection = bc;



        m_DataStreamReceive = new QueueBuffer<byte>(16);

        m_StateMachine = new ISMStateMachine<MessageSerializer>(this);
        m_StateMachine.CreateAndAdd<ParsePBHState>("ParsePBH", this);
        m_StateMachine.CreateAndAdd<ParseLengthState>("ParseLength", this);
        m_StateMachine.CreateAndAdd<ParseHeadState>("ParseHead", this);
        m_StateMachine.CreateAndAdd<ParseBodyState>("ParseBody", this);
        m_StateMachine.CreateAndAdd<WaitNextBuffState>("WaitNextBuff", this);
        m_StateMachine.Push("ParsePBH");
    }


    private BaseSocketConnection m_BaseSocketConnection;
    private BaseMessageDispatcher m_MessageDispatcher = null;
    private ISMStateMachine<MessageSerializer> m_StateMachine;

    public QueueBuffer<byte> m_DataStreamReceive;
    public Mutex m_MutexRec = new Mutex();


    public static string HeadString = "pbh\0";
    public static int PROTOBUF_ENCRYPT_LEN = 5;
    public static bool NeedEncrypt = true;

    public static string SharpClass2ProtoTypeName(string className)
    {
        string stype = className.Replace("class BaseCmd.Cmd\\$", "BaseCmd.");
        return stype;
    }
    public static string ProtoTypeName2SharpClassName(string typeName)
    {
        string className = typeName.Replace("BaseCmd\\.", "class BaseCmd.Cmd\\$");
        return className;
    }


    int count = 0;
    public void Update()
    {

        if(m_StateMachine != null)
        {
            m_StateMachine.Update();
        }
    }

    public void Dispatch(IMessage msg)
    {
        if(msg != null)
        {

            m_MessageDispatcher.Dispatcher(this, msg.GetType().ToString(), (IMessage)msg);
        }
        else
        {
            Debug.Log("Dispatch Input Null");
        }
    }

    public void ProcessReceive(byte[] buf)
    {
        m_MutexRec.WaitOne();
        m_DataStreamReceive.Enqueue(buf);


        //Debug.Log(string.Format("Enqueue Capacity: {0}  Size: {1} Head: {2} Tail: {3}", m_DataStreamReceive.m_Capacity, m_DataStreamReceive.GetLength(), m_DataStreamReceive.m_Head, m_DataStreamReceive.m_Tail));
        m_MutexRec.ReleaseMutex();

    }
    public void ProcessSend(byte[] buf)
    {

    }

    public static IMessage Parse(byte[] buf)
    {

        MemoryStream ms = new MemoryStream(buf);


        byte[] head = new byte[4];
        ms.Read(head, 0, 4);

        string shead = System.Text.Encoding.ASCII.GetString(head);

        if (shead == HeadString)
        {
            byte[] btotallen = new byte[4];
            ms.Read(btotallen, 0, 4);
            int totallen = bytesToIntLittle(btotallen, 0);

            byte[] bheadlen = new byte[4];
            ms.Read(bheadlen, 0, 4);
            int headlen = bytesToIntLittle(bheadlen, 0);


            byte[] bheaddata = new byte[headlen];
            byte[] bbodydata = new byte[totallen - headlen - 8];


            ms.Read(bheaddata, 0, bheaddata.Length);
            ms.Read(bbodydata, 0, bbodydata.Length);

            CommonHeader headdata = null;
            IMessage bodydatamsg = null;
            if (NeedEncrypt)
            {



                byte[] bheaddataDE = new byte[bheaddata.Length - PROTOBUF_ENCRYPT_LEN];
                if (Encrypt.Txt_Decrypt(bheaddata, bheaddata.Length, bheaddataDE, bheaddataDE.Length))
                {

                }
                else
                {
                    Debug.LogError("Txt_Decrypt Error");

                }

                headdata = CommonHeader.Parser.ParseFrom(bheaddataDE);

                if (bbodydata.Length > PROTOBUF_ENCRYPT_LEN)
                {
                    byte[] bbodydataDE = new byte[bbodydata.Length - PROTOBUF_ENCRYPT_LEN];
                    if (Encrypt.Txt_Decrypt(bbodydata, bbodydata.Length, bbodydataDE, bbodydataDE.Length))
                    {

                        bodydatamsg = parseDynamic(headdata.BodyType, bbodydataDE);
                    }
                    else
                    {
                        Debug.LogError("Txt_Decrypt Error");

                    }

                }
                else
                {

                    bodydatamsg = parseDynamic(headdata.BodyType, null);
                }
            }
            else
            {

                headdata = CommonHeader.Parser.ParseFrom(bheaddata);
                bodydatamsg = parseDynamic(headdata.BodyType, bbodydata);
            }


            return bodydatamsg;
        }
        else
        {

        }
        return null;
    }


    /**
     * 发送数据
     *
     * @param data 需要发送的内容
     */
    public static MemoryStream Serialize<T>(T msg) where T : IMessage
    {


        byte[] bbody = msg.ToByteArray();
        string stype = msg.GetType().ToString();
        stype = SharpClass2ProtoTypeName(stype);
        int bodylen = bbody.Length;



        CommonHeader headdata = new CommonHeader();
        headdata.BodyType = stype;

        byte[] bhead = headdata.ToByteArray();
        int headlen = bhead.Length;


        int totallen = 8 + headlen + bodylen;


        byte[] bshead = Encoding.ASCII.GetBytes(HeadString);

        MemoryStream ms = new MemoryStream();
        ms.Write(bshead, 0, 4);

        
        if (MessageSerializer.NeedEncrypt)
        {

            ms.Write(intToBytesLittle(totallen + MessageSerializer.PROTOBUF_ENCRYPT_LEN * 2), 0, 4);
            ms.Write(intToBytesLittle(headlen + MessageSerializer.PROTOBUF_ENCRYPT_LEN), 0, 4);

            byte[] bheadE = new byte[bhead.Length + MessageSerializer.PROTOBUF_ENCRYPT_LEN];
            if (Encrypt.Txt_Encrypt(bhead, bhead.Length, bheadE, bheadE.Length))
            {

                ms.Write(bheadE, 0, bheadE.Length);
            }
            else
            {
                Debug.LogError("Txt_Encrypt Error");

            }

            if (bbody.Length > 0)
            {
                byte[] bbodyE = new byte[bbody.Length + MessageSerializer.PROTOBUF_ENCRYPT_LEN];
                if (Encrypt.Txt_Encrypt(bbody, bbody.Length, bbodyE, bbodyE.Length))
                {

                    ms.Write(bbodyE, 0, bbodyE.Length);
                }
                else
                {
                    Debug.LogError("Txt_Encrypt Error");
                }

            }


        }
        else
        {
            ms.Write(intToBytesLittle(totallen), 0, 4);
            ms.Write(intToBytesLittle(headlen), 0, 4);
            ms.Write(bhead, 0, bhead.Length);
            ms.Write(bbody, 0, bbody.Length);
        }

        return ms;
    }




    public static IMessage parseDynamic(string stype, byte[] bytes)
    {
        try
        {

            //type = ProtoTypeName2JavaClassName(type);

            //type = type.replace("class ", "");

            Debug.Log("Tcp Connect　parseDynamic:");


            string assemblyName = typeof(CommonHeader).Assembly.ToString();
            string assemblyQualifiedName = Assembly.CreateQualifiedName(assemblyName, stype);
            Type type = Type.GetType(assemblyQualifiedName);


            //object obj = Activator.CreateInstance(type);



            MethodInfo[] methods = type.GetMethods();


            object[] arguments = new object[2];
            arguments[0] = bytes;
            arguments[1] = bytes.Length;

            var parseFromMethod = Array.Find(methods, m => m.Name == "get_Parser");

            MessageParser parser = parseFromMethod.Invoke(null, null) as MessageParser;
            System.Object obj = parser.ParseFrom(bytes, 0, bytes.Length);


            //Assembly assembly = Assembly.GetExecutingAssembly();
            //IMessage obj = assembly.CreateInstance(stype) as IMessage; 
            return (IMessage)obj;
        }
        catch (Exception e)
        {
        }
        return null;
    }


    /**
     * 以大端模式将int转成byte[]
     */
    public static byte[] intToBytesBig(int value)
    {
        byte[] src = new byte[4];
        src[0] = (byte)((value >> 24) & 0xFF);
        src[1] = (byte)((value >> 16) & 0xFF);
        src[2] = (byte)((value >> 8) & 0xFF);
        src[3] = (byte)(value & 0xFF);
        return src;
    }

    /**
     * 以小端模式将int转成byte[]
     *
     * @param value
     * @return
     */
    public static byte[] intToBytesLittle(int value)
    {
        byte[] src = new byte[4];
        src[3] = (byte)((value >> 24) & 0xFF);
        src[2] = (byte)((value >> 16) & 0xFF);
        src[1] = (byte)((value >> 8) & 0xFF);
        src[0] = (byte)(value & 0xFF);
        return src;
    }

    /**
     * 以大端模式将byte[]转成int
     */
    public static int bytesToIntBig(byte[] src, int offset)
    {
        int value;
        value = (int)(((src[offset] & 0xFF) << 24)
                | ((src[offset + 1] & 0xFF) << 16)
                | ((src[offset + 2] & 0xFF) << 8)
                | (src[offset + 3] & 0xFF));
        return value;
    }

    /**
     * 以小端模式将byte[]转成int
     */
    public static int bytesToIntLittle(byte[] src, int offset)
    {
        int value;
        value = (int)((src[offset] & 0xFF)
                | ((src[offset + 1] & 0xFF) << 8)
                | ((src[offset + 2] & 0xFF) << 16)
                | ((src[offset + 3] & 0xFF) << 24));
        return value;
    }

}


public class MessageSerializerState : ISMState<MessageSerializer>
{
    public MessageSerializerState(string name,MessageSerializer entity,ISMStateMachine<MessageSerializer> parentISM,int priority):base(name,entity,parentISM,priority,"Exclusive")
    {

    }

    public override bool Enter()
    {
        return base.Enter();
    }
    public override void Execute()
    {
        base.Execute();
    }
    public override void Exit()
    {
        base.Exit();
    }
}
public class  ParsePBHState : MessageSerializerState
{
    public ParsePBHState(string name, MessageSerializer entity, ISMStateMachine<MessageSerializer> parentISM,int priority) : base(name, entity, parentISM, priority)
    {
    }

    public override void Execute()
    {

        mEntity.m_MutexRec.WaitOne();

        
        if (mEntity.m_DataStreamReceive.GetLength() < sizeof(int))
        {

            var state = mParentISM.GetFromDic<WaitNextBuffState>("WaitNextBuff");
            state.SetPreStateAndNextLen("ParsePBH", sizeof(int));
            mParentISM.Push(state);


            mEntity.m_MutexRec.ReleaseMutex();
            return;
        }

        byte[] buf = new byte[sizeof(int)];
        int len = mEntity.m_DataStreamReceive.Peek(buf);

        string sbuf = Encoding.ASCII.GetString(buf);
        if (sbuf != MessageSerializer.HeadString)//not head
        {
            byte[] debuf = new byte[1];
            mEntity.m_DataStreamReceive.Dequeue(debuf);
            return;
        }

        mEntity.m_DataStreamReceive.Dequeue(buf);

        mParentISM.Push("ParseLength");
        mEntity.m_MutexRec.ReleaseMutex();
    }
}
public class ParseLengthState : MessageSerializerState
{
    public ParseLengthState(string name, MessageSerializer entity, ISMStateMachine<MessageSerializer> parentISM, int priority) : base(name, entity, parentISM, priority)
    {

    }

    public override void Execute()
    {

        mEntity.m_MutexRec.WaitOne();


        if (mEntity.m_DataStreamReceive.GetLength() < sizeof(int) * 2)
        {

            WaitNextBuffState waitstate = mParentISM.GetFromDic<WaitNextBuffState>("WaitNextBuff");
            waitstate.SetPreStateAndNextLen("ParseLength", sizeof(int) * 2);
            mParentISM.Push(waitstate);

            mEntity.m_MutexRec.ReleaseMutex();
            return;
        }


        int totalLen = 0;
        int headLen = 0;
        

        byte[] btotallen = new byte[4];
        mEntity.m_DataStreamReceive.Dequeue(btotallen);
        totalLen = MessageSerializer.bytesToIntLittle(btotallen, 0);

        byte[] bheadlen = new byte[4];
        mEntity.m_DataStreamReceive.Dequeue(bheadlen);
        headLen = MessageSerializer.bytesToIntLittle(bheadlen, 0);

        
        

        if (totalLen < 0 || headLen < 0 || totalLen < headLen)
        {
            ParsePBHState pbhstate = mParentISM.GetFromDic<ParsePBHState>("ParsePBH");
            mParentISM.Push(pbhstate);

            mEntity.m_MutexRec.ReleaseMutex();
            return;
        }


        var headstate = mParentISM.GetFromDic<ParseHeadState>("ParseHead");
        headstate.SetLen(totalLen, headLen);
        mParentISM.Push(headstate);

        mEntity.m_MutexRec.ReleaseMutex();
    }
}
public class ParseHeadState : MessageSerializerState
{
    public ParseHeadState(string name, MessageSerializer entity, ISMStateMachine<MessageSerializer> parentISM, int priority) : base(name, entity, parentISM, priority)
    {

    }

    public override void Execute()
    {
        mEntity.m_MutexRec.WaitOne();

        if (mEntity.m_DataStreamReceive.GetLength() < m_HeadLen)
        {

            WaitNextBuffState waitstate = mParentISM.GetFromDic<WaitNextBuffState>("WaitNextBuff");
            waitstate.SetPreStateAndNextLen("ParseHead", m_HeadLen);
            mParentISM.Push(waitstate);

            mEntity.m_MutexRec.ReleaseMutex();
            return;
        }


        byte[] bheaddata = new byte[m_HeadLen];
        mEntity.m_DataStreamReceive.Dequeue(bheaddata);

        CommonHeader headdata = null;
        if (MessageSerializer.NeedEncrypt)
        {
            byte[] bheaddataDE = new byte[bheaddata.Length - MessageSerializer.PROTOBUF_ENCRYPT_LEN];
            if (Encrypt.Txt_Decrypt(bheaddata, bheaddata.Length, bheaddataDE, bheaddataDE.Length))
            {

            }
            else
            {
                Debug.LogError("Txt_Decrypt Error");

            }

            headdata = CommonHeader.Parser.ParseFrom(bheaddataDE);


        }
        else
        {

            headdata = CommonHeader.Parser.ParseFrom(bheaddata);
            
        }


        int bodyLen = m_TotalLen - m_HeadLen - sizeof(int) * 2;
        ParseBodyState bodystate = mParentISM.GetFromDic<ParseBodyState>("ParseBody");
        bodystate.SetLen(headdata, bodyLen);
        mParentISM.Push(bodystate);

        mEntity.m_MutexRec.ReleaseMutex();

    }

    public void SetLen(int totalLen, int headLen)
    {
        m_TotalLen = totalLen;
        m_HeadLen = headLen;
    }
    int m_TotalLen;
    int m_HeadLen;
}
public class ParseBodyState : MessageSerializerState
{
    public ParseBodyState(string name, MessageSerializer entity, ISMStateMachine<MessageSerializer> parentISM, int priority) : base(name, entity, parentISM, priority)
    {

    }

    public override void Execute()
    {
        mEntity.m_MutexRec.WaitOne();


        IMessage bodydatamsg = null;

        if (MessageSerializer.NeedEncrypt)
        {

            if (m_BodyLen == MessageSerializer.PROTOBUF_ENCRYPT_LEN)
            {

            }
            else if (mEntity.m_DataStreamReceive.GetLength() < m_BodyLen)
            {

                WaitNextBuffState waitstate = mParentISM.GetFromDic<WaitNextBuffState>("WaitNextBuff");
                waitstate.SetPreStateAndNextLen("ParseBody", m_BodyLen);
                mParentISM.Push(waitstate);

                mEntity.m_MutexRec.ReleaseMutex();
                return;
            }
        }
        else
        {
            if (mEntity.m_DataStreamReceive.GetLength() < m_BodyLen)
            {

                WaitNextBuffState waitstate = mParentISM.GetFromDic<WaitNextBuffState>("WaitNextBuff");
                waitstate.SetPreStateAndNextLen("ParseBody", m_BodyLen);
                mParentISM.Push(waitstate);

                mEntity.m_MutexRec.ReleaseMutex();
                return;
            }

        }



        byte[] bbodydata = new byte[m_BodyLen];
        mEntity.m_DataStreamReceive.Dequeue(bbodydata);
        if (MessageSerializer.NeedEncrypt)
        {
            if (m_BodyLen == MessageSerializer.PROTOBUF_ENCRYPT_LEN)
            {

                bodydatamsg = MessageSerializer.parseDynamic(m_CommonHeader.BodyType, null);
            }
            else if (bbodydata.Length > MessageSerializer.PROTOBUF_ENCRYPT_LEN)
            {
                byte[] bbodydataDE = new byte[bbodydata.Length - MessageSerializer.PROTOBUF_ENCRYPT_LEN];
                if (Encrypt.Txt_Decrypt(bbodydata, bbodydata.Length, bbodydataDE, bbodydataDE.Length))
                {

                    bodydatamsg = MessageSerializer.parseDynamic(m_CommonHeader.BodyType, bbodydataDE);
                }
                else
                {
                    Debug.LogError("Txt_Decrypt Error");

                }

            }
        }
        else
        {
            bodydatamsg = MessageSerializer.parseDynamic(m_CommonHeader.BodyType, bbodydata);
        }

        
        mEntity.Dispatch(bodydatamsg);


        ParsePBHState pbhstate = mParentISM.GetFromDic<ParsePBHState>("ParsePBH");
        mParentISM.Push(pbhstate);


        mEntity.m_MutexRec.ReleaseMutex();
    }



    public void SetLen(CommonHeader header, int bodyLen)
    {
        m_CommonHeader = header;
        m_BodyLen = bodyLen;
    }
    int m_BodyLen;
    CommonHeader m_CommonHeader;
}
public class WaitNextBuffState : MessageSerializerState
{
    public WaitNextBuffState(string name, MessageSerializer entity, ISMStateMachine<MessageSerializer> parentISM, int priority) : base(name, entity, parentISM, priority)
    {

    }

    public override void Execute()
    {
        mEntity.m_MutexRec.WaitOne();

        if (mEntity.m_DataStreamReceive.GetLength() < m_NextLen)
        {
            mEntity.m_MutexRec.ReleaseMutex();
            return;
        }
        else
        {
            mParentISM.Push(m_PreStateName);
        }

        mEntity.m_MutexRec.ReleaseMutex();
    }

    public void SetPreStateAndNextLen(string prestate, int len)
    {
        m_NextLen = len;
        m_PreStateName = prestate;
    }
    
	int m_NextLen;
    string m_PreStateName;
}


