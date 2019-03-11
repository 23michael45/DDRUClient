/********************************************************************
	created:	2014/12/26
	created:	26:12:2014   13:02
	filename: 	C:\WGame\XGame\Assets\XGameScript\Engine\ISM\ISMEvent.cs
	file path:	C:\WGame\XGame\Assets\XGameScript\Engine\ISM
	file base:	ISMEvent
	file ext:	cs
	author:		michael
	
	purpose:	
*********************************************************************/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;




public class ISMStateParam : ScriptableObject
{
    #region Member
    Dictionary<int, object> ParamDic = new Dictionary<int, object>();
    #endregion
    public void SetParam(int key, object value)
    {
        if (ParamDic.ContainsKey(key))
        {
            ParamDic[key] = value;
        }
        else
        {
            ParamDic.Add(key, value);
        }
    }
    public object GetParam(int key)
    {
        if (ParamDic.ContainsKey(key))
        {
            return ParamDic[key];
        }
        return null;
    }
}