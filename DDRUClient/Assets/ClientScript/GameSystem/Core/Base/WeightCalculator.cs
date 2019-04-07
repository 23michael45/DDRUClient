//---------------------------------------------------------------------------------
// <copyright file="WeightCalculator.cs" company="BHGame Inc.">
//     Copyright (c) 2011 CMGE Inc. All rights reserved.
// </copyright>
// <author></author>
// <description>
//             
//  </description>
// <history created="2014/06/09">
//    <modified date ="2014/06/09"> </modified>
// </history>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class WeightCalculator<T>
{
    #region Members
    List<T> mElements = new List<T>();
    List<int> mWeights = new List<int>();

    int mTotalW;
    #endregion
    public WeightCalculator()
    {
        UnityEngine.Random.state = new UnityEngine.Random.State();
    }

    void ResetTotalW()
    {
        mTotalW = 0;
        foreach (int w in mWeights)
        {
            mTotalW += w;
        }
    }

    //w为权重
    public void AddElement(T obj,int W = 1)
    {
        mElements.Add(obj);
        mWeights.Add(W);
        ResetTotalW();
        
    }

    public void RemoveElement(T obj)
    {
        if (mElements.Contains(obj))
        {
            int index = mElements.IndexOf(obj);
            mWeights.RemoveAt(index);
            mElements.Remove(obj);


            ResetTotalW();
        }
    }
    public int Count()
    {
        return mElements.Count;
    }


    public T RandomGetElement()
    {
        int w = UnityEngine.Random.Range(0, mTotalW);

        int wacc = 0;
        for (int i = 0; i< mElements.Count;i++)
        {

            T ele = mElements[i];
            int elew = mWeights[i];


            wacc += elew;

            if (w < wacc)
            {
                return ele;
            }

        }


        return default(T);
    }

    public void Clear()
    {
        mElements.Clear();
        mWeights.Clear();
        mTotalW = 0;
    }
}

