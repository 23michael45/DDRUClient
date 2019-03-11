using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Google.Protobuf;
using System.Threading;
using System.Threading.Tasks;
using System;
using DDRCommProto;
using UnityEngine.UI;
using Microsoft.IO;

public class MainEntry : MonoBehaviour
{


    






    private void Start()
    {


        ISMStateParam mStateParam = ScriptableObject.CreateInstance(typeof(ISMStateParam)) as ISMStateParam;

        int delen = 0;
        long len = 0;
        byte[] buf6 = new byte[6];
        byte[] buf8 = new byte[8];
        byte[] buf10 = new byte[10] { 0,1, 2, 3, 4 ,5,6,7,8,9};
        QueueBuffer<byte> ms = new QueueBuffer<byte>(15);


        ms.Enqueue(buf10);
        len = ms.GetLength();

        delen = ms.Dequeue(buf6);
        len = ms.GetLength();


        ms.Enqueue(buf10);
        len = ms.GetLength();

        delen = ms.Dequeue(buf8);
        len = ms.GetLength();


        delen = ms.Peek(buf8);
        len = ms.GetLength();


        delen = ms.Dequeue(buf8);
        len = ms.GetLength();


        ms.Enqueue(buf10);
        ms.Enqueue(buf10);

        delen = ms.Dequeue(buf8);
        len = ms.GetLength();

        delen = ms.Peek(buf8);
        len = ms.GetLength();


        delen = ms.Dequeue(buf8);
        len = ms.GetLength();
        delen = ms.Dequeue(buf8);
        len = ms.GetLength();
        delen = ms.Dequeue(buf8);
        len = ms.GetLength();


        Debug.Log(len);
    }
}
