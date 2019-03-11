/********************************************************************
	created:	2014/12/26
	created:	26:12:2014   12:55
	filename: 	C:\WGame\XGame\Assets\XGameScript\Engine\ISM\ISMState.cs
	file path:	C:\WGame\XGame\Assets\XGameScript\Engine\ISM
	file base:	ISMState
	file ext:	cs
	author:		michael
	
	purpose:	Base ISM State
*********************************************************************/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum ISMState_PRIORITY
{
    Idle = 0,
    Lowest = 200,
    BelowNormal = 300,
    Normal = 500,
    AboveNormal = 700,
    Heightest = 800,
    TimeCritical = 1000,
}

public class ISMStateGroup
{

    public int Index;
    public string Name;
    public ISMStateGroup(int i, string n)
    {
        Index = i;
        Name = n;
    }
}

public class ISMStateGroupMgr : Singleton<ISMStateGroupMgr>
{
    #region Member
    Dictionary<string, ISMStateGroup> GroupDic;
    #endregion
    public ISMStateGroupMgr()
    {
        Init();
    }


    void AddGroup(int i, string n)
    {
        ISMStateGroup g = new ISMStateGroup(i, n);
        if (GroupDic != null)
        {
            GroupDic.Add(n, g);
        }
    }
    public void Init()
    {
        GroupDic = new Dictionary<string, ISMStateGroup>();

        //this name can be set in any table such as lua
        //is a state is in group 0 and another is in 1,these two can be exist at the same time
        AddGroup(0, "Exclusive");
        AddGroup(1, "Timer");
        AddGroup(2, "Parallel1");
        AddGroup(3, "Parallel2");
        AddGroup(4, "Parallel3");
        AddGroup(5, "Parallel4");
    }

    public int CalculateGroupMask(params string[] groups)
    {
        int rt = 0;
        foreach (string sg in groups)
        {
            if (GroupDic != null && GroupDic.ContainsKey(sg))
            {
                rt |= 0x01 << GroupDic[sg].Index;
            }
        }
        return rt;
    }

    public bool IsTwoMaskCompatible(int mask1, int mask2)
    {
        return (mask1 & mask2) == 0;
    }

    public bool IsGroupInMask(int mask, string group)
    {
        if (GroupDic != null && GroupDic.ContainsKey(group))
        {
            IsGroupInMask(mask, GroupDic[group]);
        }
        return false;
    }
    public bool IsGroupInMask(int mask, ISMStateGroup group)
    {
        int bhas = (0x01 << group.Index) & mask;
        if (bhas != 0)
        {
            return true;
        }
        return false;
    }
    public ISMStateGroup GetGroup(string name)
    {
       if (GroupDic.ContainsKey(name))
       {
           return GroupDic[name];
       }
       return GroupDic["StandAlone"];
    }
}


public class ISMStateBase
{
    public delegate void EnterCallback(ISMStateBase sender);
    public delegate void ExitCallback(ISMStateBase sender);

    public event EnterCallback mEnterCallback;
    public event ExitCallback mExitCallback;

    public int PRIORITY;
    public int GroupMask;
    public enum StateFlag
    {
        BeforeEnter,
        Executing,
        BeforeExit,
        AfterExit,
    };


    public StateFlag mStateFlag;

    public string mName;
    
    protected bool IsPaused = false;

    protected ISMStateParam mStateParam;



    public ISMStateBase(string name, int priority, params string[] groups)
    {
        //mStateParam = ScriptableObject.CreateInstance(typeof(ISMStateParam)) as ISMStateParam;
        mName = name;
        if (groups == null)
        {
            GroupMask = 1;
        }
        else
        {
            GroupMask = ISMStateGroupMgr.Instance.CalculateGroupMask(groups);

        }
        PRIORITY = priority;
        mStateFlag = StateFlag.BeforeEnter;
    }

    public object GetParam(int key)
    {
        return mStateParam.GetParam(key);
    }
    public void SetParam(int key, object v)
    {
        mStateParam.SetParam(key, v);
    }

    public bool IsPause()
    {
        return IsPaused;
    }
    public void Pause()
    {
        IsPaused = true;
    }
    public void Resume()
    {
        IsPaused = false;
    }

    public void OnEnter()
    {
        if (mEnterCallback != null)
        {

            mEnterCallback(this);
            mEnterCallback = null;
        }
    }
    public void OnExit()
    {
        if (mExitCallback != null)
        {

            mExitCallback(this);
            mExitCallback = null;
        }
    }

    public virtual string GetFullState()
    {
        return mName;
    }
}
public class ISMState<T> : ISMStateBase
{
    protected ISMStateMachine<T> mParentISM;
    protected T mEntity;

    public ISMState(string name, T entity, ISMStateMachine<T> parentISM, int priority, params string[] groups) : 
        base(name,priority,groups)
    {
        mEntity = entity;
        mParentISM = parentISM;
    }


    public virtual bool Enter()
    {
        if (mStateFlag == StateFlag.AfterExit)
        {

            ReInit();
        }

        OnEnter();
        mStateFlag = StateFlag.Executing;
        return true;
    }

    public virtual void Exit()
    {
        mStateFlag = StateFlag.AfterExit;

        OnExit();
    }

    public virtual void Execute()
    {
        if (IsOver())
        {
            EndSelf();
        }
    }
    public virtual void LateExecute()
    {
    }
    public virtual void FixedExecute()
    {
    }

    protected virtual bool IsOver()
    {
        return false;
    }

    public virtual void EndSelf()
    {
        mStateFlag = StateFlag.BeforeExit;
    }
    public virtual void ReInit()
    {
        mStateFlag = StateFlag.BeforeEnter;
    }

    public T GetEntity()
    {
        return mEntity;
    }

}


public class ISMSequenceState<T> : ISMState<T> where T : MonoBehaviour
{
#region Members
    protected Queue<ISMState<T>> mStateQueue = new Queue<ISMState<T>>();
    protected Queue<ISMState<T>> mStateQueueRest = new Queue<ISMState<T>>();
    protected ISMState<T> mCurrentState;

#endregion

    public ISMSequenceState(string name, T entity, ISMStateMachine<T> parentISM, int priority, params string[] groups) :
        base(name,entity,parentISM, priority, groups)
    {
       
    }
    public override void ReInit()
    {
        base.ReInit();
        mStateQueue.Clear();
        foreach (ISMState<T> s in mStateQueueRest)
        {
            mStateQueue.Enqueue(s);
        }
    }
    public void EnqueueState(ISMState<T> state)
    {
        foreach (ISMState<T> s in mStateQueue)
        {
            if (s.mName == state.mName)
            {
                Debug.LogError("ISMSequenceState :vState Already In Queue");
                return;
            }
        }
        mStateQueueRest.Enqueue(state);
    }
   

    public override bool Enter()
    {
        base.Enter();

        if (mStateQueue.Count > 0)
        {
            mCurrentState = mStateQueue.Dequeue();

        }
        return true;
    }

    public override void Exit()
    {
        base.Exit();
    }


    public override void Execute()
    {
        if (mCurrentState != null)
        {
            if (mCurrentState.mStateFlag == ISMStateBase.StateFlag.BeforeEnter)
            {
                mCurrentState.Enter();
            }
            else if (mCurrentState.mStateFlag == ISMStateBase.StateFlag.Executing)
            {
                mCurrentState.Execute();
            }
            else if (mCurrentState.mStateFlag == ISMStateBase.StateFlag.BeforeExit)
            {
                mCurrentState.Exit();
            }
            else if (mCurrentState.mStateFlag == ISMStateBase.StateFlag.AfterExit)
            {
                mCurrentState.ReInit();
                mCurrentState = null;
            }
        }
        else
        {
            if (mStateQueue.Count > 0)
            {
                mCurrentState = mStateQueue.Dequeue();
            }
            else
            {
                EndSelf();
            }
        }

    }


    public override string GetFullState()
    {
        string substatename = "";
        if (mCurrentState != null)
        {
            substatename = mCurrentState.GetFullState();
        }
        return mName + " : " + substatename;
    }
}


public class ISMSequenceLoopState<T> : ISMSequenceState<T> where T : MonoBehaviour
{

    public ISMSequenceLoopState(string name, T entity, ISMStateMachine<T> parentISM, int priority, params string[] groups) :
        base(name, entity, parentISM,priority, groups)
    {

    }
    public override bool Enter()
    {
        base.Enter();
        mStateQueue.Enqueue(mCurrentState);
        return true;
    }
    public override void Execute()
    {
        if (mCurrentState != null)
        {
            if (mCurrentState.mStateFlag == ISMStateBase.StateFlag.BeforeEnter)
            {
                mCurrentState.Enter();
            }
            else if (mCurrentState.mStateFlag == ISMStateBase.StateFlag.Executing)
            {
                mCurrentState.Execute();
            }
            else if (mCurrentState.mStateFlag == ISMStateBase.StateFlag.BeforeExit)
            {
                mCurrentState.Exit();
            }
            else if (mCurrentState.mStateFlag == ISMStateBase.StateFlag.AfterExit)
            {
                mCurrentState.ReInit();
                mCurrentState = null;
            }
        }
        else
        {
            if (mStateQueue.Count > 0)
            {
                mCurrentState = mStateQueue.Dequeue();
                mStateQueue.Enqueue(mCurrentState);
            }
        }

    }
}

public class ISMAnimatorState<T> : ISMState<T> where T : MonoBehaviour
{
    public enum EParamName
    {
        Animator,
        AnimationState,
    }

    int OrgHash;
    int CurrentHash;
    bool HasPlayed = false;


    Animator mAnimator;
    string mAnimatiionState;

    public ISMAnimatorState(string name, T entity, ISMStateMachine<T> parentISM, int priority, params string[] groups)
        : base(name, entity, parentISM,priority, groups)
    {
    }

    public override bool Enter()
    {
        mAnimator = (Animator)GetParam((int)EParamName.Animator);
        mAnimatiionState = GetParam((int)EParamName.AnimationState) as string;


        AnimatorStateInfo astate = mAnimator.GetCurrentAnimatorStateInfo(0);
        OrgHash = astate.fullPathHash;
        CurrentHash = astate.fullPathHash;



        mAnimator.Play(mAnimatiionState);


        return base.Enter();
    }

    public override void Execute()
    {
        //是否开始播放了,两种方式效果一样
        //AnimatorStateInfo statinfo = mAnimator.GetCurrentAnimatorStateInfo(0);
        //if (statinfo.IsName(mAnimatiionState))
        //{
        //    HasPlayed = true;
        //}

        AnimatorStateInfo astate = mAnimator.GetCurrentAnimatorStateInfo(0);
        if (astate.fullPathHash != OrgHash)
        {
            HasPlayed = true;
        }

        base.Execute();
    }

    public override void Exit()
    {
        base.Exit();
    }
    protected override bool IsOver()
    {
        //用当前 播放时间判断结束 可能不准  目前用第二种
        //AnimatorStateInfo statinfo = mAnimator.GetCurrentAnimatorStateInfo(0);
        //if (statinfo.normalizedTime >= 1 && HasPlayed == true)
        //{
        //    HasPlayed = false;
        //    return true;
        //}

        //用当前动画是否与播放时不一样判断结束
        if (HasPlayed == true)
        {
            AnimatorStateInfo astate = mAnimator.GetCurrentAnimatorStateInfo(0);
            if (astate.fullPathHash == OrgHash)
            {
                return true;
            }
        }

        return false;
    }
}


public class ISMISMState<T> : ISMState<T> where T : MonoBehaviour
{
    #region sub ISM
    protected ISMStateMachine<T> mISM;



    public ISMStateMachine<T> GetISM()
    {
        return mISM;
    }
    protected virtual void MakeISM()
    {

    }
    #endregion

    public ISMISMState(string name, T entity, ISMStateMachine<T> parentISM,int priority, params string[] groups) :
        base(name, entity, parentISM,priority, groups)
    {

    }
    public override bool Enter()
    {
        base.Enter();
        MakeISM();
        return true;
    }
    public override void Execute()
    {
        if (mISM != null)
        {
            mISM.Update();
        }

      
        base.Execute();
    }


    public override void Exit()
    {
        if (mISM != null)
        {
            mISM.Exit();
        }
        base.Exit();
    }
}