/********************************************************************
	created:	2014/12/26
	created:	26:12:2014   13:02
	filename: 	C:\WGame\XGame\Assets\XGameScript\Engine\ISM\ISMStateMachine.cs
	file path:	C:\WGame\XGame\Assets\XGameScript\Engine\ISM
	file base:	ISMStateMachine
	file ext:	cs
	author:		michael
	
	purpose:	ISM State Machine
*********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class ISMStateContainer<T>
{
#region Members
    protected T mEntity;
    protected List<ISMState<T>> m_states = new List<ISMState<T>>();

    protected Dictionary<string, ISMState<T>> m_statesDic = new Dictionary<string,ISMState<T>>();
#endregion
    public ISMStateContainer(T entity)
    {
        mEntity = entity;
    }

    protected int SortStatePriorityCompare(ISMState<T> state1, ISMState<T> state2)
    {
        if (state1.PRIORITY > state2.PRIORITY)
        {
            return -1;
        }
        else if (state1.PRIORITY < state2.PRIORITY)
        {
            return 1;
        }
      
        return 0;
    }

    protected bool Add(ISMState<T> state)
    {
        if (state == null)
        {
            return false;
        }

        foreach (ISMState<T> ts in m_states)
        {
            if (ts.mName == state.mName)
            {
                Debug.Log("State Already Exist In this ISMStateMachine!");
                return false;
            }
        }
        m_states.Add(state);
        m_states.Sort(SortStatePriorityCompare);
        return true;
    }

    protected void Remove(ISMState<T> state)
    {
        m_states.Remove(state);
        state = null;
    }

    protected virtual ISMState<T> GetState(string name)
    {
        foreach (ISMState<T> state in m_states)
        {
            if (state.mName == name)
            {
                return state;
            }
        }

        return null;
    }

    public TS GetState<TS>(string name) where TS : ISMState<T>
    {
        ISMState<T> state = GetState(name);
        if (state != null)
        {
            return (TS)state;
        }
        return null;

    }

    public bool ContainsState(string name)
    {
        return ContainsState(GetState(name));
    }
    public bool ContainsState(ISMState<T> state)
    {
        return m_states.Contains(state);
    }

   

    public List<ISMState<T>> GetAllState()
    {
        return m_states;
    }
    public string GetCurrentFirstState()
    {
        if (GetAllState().Count >0 )
        {
            return GetAllState()[0].mName;
        }
        return "";
    }

    protected void Add2Dic(ISMState<T> state)
    {
        if (!m_statesDic.ContainsKey(state.mName))
        {
            m_statesDic.Add(state.mName, state);
        }
    }
    protected ISMState<T> GetFromDic(string name)
    {
        if (m_statesDic.ContainsKey(name))
        {
            return m_statesDic[name];
        }
        return null;
    }

    public TS GetFromDic<TS>(string name) where TS : ISMState<T>
    {
        ISMState<T> state = GetFromDic(name);
        if (state != null)
        {
            return (TS)state;
        }
        return null;
    }


    public TS CreateAndAdd<TS>(string name, T entity ) where TS : ISMState<T>
    {
        int priority = (int)ISMState_PRIORITY.Normal;
        try
        {

            TS state = (TS)Activator.CreateInstance(typeof(TS), name, entity, this, priority);
            Add2Dic(state);
            return state;
        }
        catch (System.Exception ex)
        {
        	
        }

        return null;
    }

    public T GetEntity()
    {
        return mEntity;
    }
}


public class ISMStateMachine<T> : ISMStateContainer<T> 
{
#region Members
    //List<ISMState<T>> needPush = new List<ISMState<T>>();
    //List<ISMState<T>> needPop = new List<ISMState<T>>();

    //List<ISMState<T>> needPushTemp = new List<ISMState<T>>();
    //List<ISMState<T>> needPopTemp = new List<ISMState<T>>();

#endregion

#region Unit Functions

    //not in use currently
    /*
    void CalcutatePushPopState()
    {
        needPushTemp.Clear();
        needPopTemp.Clear();


        needPush.Sort(SortStatePriorityCompare);

        for (int i = 0; i < m_states.Count; i++)
        {
            ISMState<T> state = m_states[i];
            for (int j = 0; j < needPush.Count; j++)
            {
                ISMState<T> statepush = needPush[j];

                if (statepush.PRIORITY >= state.PRIORITY)
                {
                }
            }
        }



        int finalmask = 0xfffff;

        //calculate push state and current state list relation
        for (int i = 0; i < needPush.Count; i++)
        {
            ISMState<T> state = needPush[i];
            
            if (state.childStateMode == ISMStateBase.ChildMode.ExclusiveStates)
            {
                if (needPushTemp.Count == 0)
                {
                    needPushTemp.Add(state);
                    finalmask &= state.GroupMask;
                    break;
                }
                else
                {
                    continue;
                }
            }
            else if (state.childStateMode == ISMStateBase.ChildMode.ParallelStates)
            {
                if (ISMStateGroupMgr.Instance.IsTwoMaskCompatible(finalmask, state.GroupMask))
                {
                    finalmask &= state.GroupMask;
                    needPushTemp.Add(state);
                }
                else
                {
                    needPop.Add(state);
                }
                
            }

        }


        //add ended state to pop list
        for (int i = 0; i < m_states.Count; i++)
        {
            ISMState<T> state = m_states[i];
            if (state.mStateFlag == ISMStateBase.StateFlag.BeforeExit)
            {
                needPop.Add(state);
            }
        }


        foreach (ISMState<T> s in needPop)
        {
            needPopTemp.Add(s);
        }


        needPop.Clear();
        needPush.Clear();

    }
    */
    //void CalcuteteListToTemp()
    //{
    //    needPushTemp.Clear();
    //    needPushTemp.AddRange(needPush);
    //    needPush.Clear();

    //    needPopTemp.Clear();
    //    needPopTemp.AddRange(needPop);
    //    needPop.Clear();

    //}
#endregion


    public ISMStateMachine(T entity)
        : base(entity)
    {

    }

   
    //this push is a complex operation,coz it will judge whether this state can be enter or if this state  cause other state stop
    protected void PushOne(ISMState<T> state)
    {
        if (!ContainsState(state))
        {
            if (state.mStateFlag == ISMStateBase.StateFlag.BeforeEnter ||
                state.mStateFlag == ISMStateBase.StateFlag.AfterExit)
            {
                //first must be reinit once
                state.ReInit();

                state.Enter();

                if (state.mStateFlag == ISMStateBase.StateFlag.Executing)
                {
                    Add(state);
                }
            }
        }
    }

    public void Push(string name)
    {
        ISMState<T> state = GetFromDic(name);
        Push(state);
    }
    public void Push(ISMState<T> state)
    {
        if (state == null)
        {
            return;
        }
        List<ISMState<T>> CompatibleList = new List<ISMState<T>>();
        List<ISMState<T>> IncompatibleList = new List<ISMState<T>>();

        foreach (ISMState<T> s in m_states)
        {
            //can exist at same time
            if (ISMStateGroupMgr.Instance.IsTwoMaskCompatible(s.GroupMask,state.GroupMask))
            {
                CompatibleList.Add(s);
            }
            else
            {
                IncompatibleList.Add(s);
            }
        }
        //CompatibleList.Sort(SortStatePriorityCompare);
        IncompatibleList.Sort(SortStatePriorityCompare);
        
        //if there some state incompatible
        if (IncompatibleList.Count > 0)
        {
            int highestpriority = IncompatibleList[0].PRIORITY;
            if (state.PRIORITY >= highestpriority)
            {
                foreach (ISMState<T> s in IncompatibleList)
                {
                    Pop(s);
                }

                if (m_states.Count > 0)
                {
                    Push(state);
                }
                else
                {
                    PushOne(state);
                }
            }
        }
        else
        {
            PushOne(state);
        }

    }


    public void ClearAll()
    {
        ISMState<T>[] states = new ISMState<T>[m_states.Count];
        m_states.CopyTo(states);
        foreach (ISMState<T> s in states)
        {
            Pop(s);
        }
    }

    public void Pop(ISMState<T> state)
    {
        if (ContainsState(state))
        {
            Remove(state);
            state.Exit();
            
        }
    }
    public void Pop(string name)
    {
        for (int i = 0 ; i< m_states.Count;i++)
        {
            ISMState<T> state = m_states[i];
            if (state.mName == name)
            {
                Remove(state);
                state.Exit();
            }
        }
    }
    public void Pause(string name)
    {
        foreach (ISMState<T> s in m_states)
        {
            if (s.mName == name)
            {
                s.Pause();
            }
        }
    }
    public void Pause(ISMState<T> state)
    {
        if (ContainsState(state))
        {
            state.Pause();
        }
    }

    public void Resume(string name)
    {
        foreach (ISMState<T> s in m_states)
        {
            if (s.mName == name)
            {
                s.Resume();
            }
        }
    }
    public void Resume(ISMState<T> state)
    {
        if (ContainsState(state))
        {
            state.Resume();
        }
    }


    //states will all ended except the state param put in
    public void PopAll(ISMState<T> state = null)
    {
        foreach (ISMState<T> s in m_states)
        {
            if (state != null && s.mName == state.mName)
            {
            }
            else
            {
                s.EndSelf();
            }
        }
    }


    public virtual void Update()
    {
        for (int i = 0; i < m_states.Count; i++)
        {
            ISMState<T> state = m_states[i];
            if (state.mStateFlag == ISMStateBase.StateFlag.Executing)
            {
                if (state.IsPause() == false)
                {

                    state.Execute();
                }
            }

            if (state.mStateFlag == ISMStateBase.StateFlag.BeforeExit)
            {
                Remove(state);

                state.Exit();
            }
        }


    }

    public virtual void LateUpdate()
    {
        for (int i = 0; i < m_states.Count; i++)
        {
            ISMState<T> state = m_states[i];
            if (state.mStateFlag == ISMStateBase.StateFlag.Executing)
            {
                if (state.IsPause() == false)
                {
                    state.LateExecute();
                }
            }
        }

    }

    public virtual void FixedUpdate()
    {
        for (int i = 0; i < m_states.Count; i++)
        {
            ISMState<T> state = m_states[i];
            if (state.mStateFlag == ISMStateBase.StateFlag.Executing)
            {
                if (state.IsPause() == false)
                {
                    state.FixedExecute();
                }
            }
        }

    }



    public virtual void Exit()
    {
        PopAll();
        Update();
    }
}

