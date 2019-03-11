//---------------------------------------------------------------------------------
// <copyright file="CrudeElapsedTimer.cs" company="BHGame Inc.">
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
using System.Collections;
using UnityEngine;

/// <summary>
/// The class of a crude time.
/// </summary>
public class CrudeElapsedTimer
{
    #region fields

    /// <summary>
    /// The time limit of the timer.
    /// </summary>
    private float limit;

    /// <summary>
    /// The elapsed time.
    /// </summary>
    private float elapsedTime = 0;

    /// <summary>
    /// The elapsed time wrapped by the time limit.
    /// </summary>
    private float wrappedElapsedTime = 0;

    /// <summary>
    /// Time out count.
    /// </summary>
    private int timeOutCount = 0;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the CrudeElapsedTimer class.
    /// </summary>
    /// <param name="limit">The time out limit of the timer, which is a length in second.</param>
    public CrudeElapsedTimer(float limit)
    {
        this.limit = limit;
    }

    #endregion

    #region properties

    /// <summary>
    /// Gets or sets the limit.
    /// </summary>
    public float Limit
    {
        get { return this.limit; }
        set { this.limit = value; }
    }

    /// <summary>
    /// Gets the elapsed time.
    /// </summary>
    public float ElapsedTime
    {
        get { return this.elapsedTime; }
    }

    /// <summary>
    /// Gets the wrapped elapsed time.
    /// </summary>
    public float WrappedElapsedTime
    {
        get { return this.wrappedElapsedTime; }
    }

    /// <summary>
    /// Gets the saturated elapsed time.
    /// </summary>
    public float SaturatedElapsedTime
    {
        get { return Mathf.Min(this.ElapsedTime, this.Limit); }
    }

    /// <summary>
    /// Gets the saturated elapsed rate.
    /// </summary>
    public float SaturatedElapsedRate
    {
        get { return this.SaturatedElapsedTime / this.Limit; }
    }

    /// <summary>
    /// Gets or sets the time out count.
    /// </summary>
    public int TimeOutCount
    {
        get { return this.timeOutCount; }
        set { this.timeOutCount = value; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Resets the timer.
    /// </summary>
    public void Reset()
    {
        this.elapsedTime = 0;
        this.wrappedElapsedTime = 0;
        this.ResetTimeOutCount();
    }

    /// <summary>
    /// Resets the timer with a limit.
    /// </summary>
    /// <param name="limit">The limit.</param>
    public void ResetWithLimit(float limit)
    {
        this.Limit = limit;
        this.Reset();
    }

    /// <summary>
    /// Reset the time out count.
    /// </summary>
    public void ResetTimeOutCount()
    {
        this.timeOutCount = 0;
    }

    /// <summary>
    /// Add the elapsed delta time to the timer.
    /// </summary>
    /// <param name="deltaTime">The delta elapsed time.</param>
    /// <returns>Time out count.</returns>
    public int Advance(float deltaTime)
    {
        // Deals with the special case.
        if (this.Limit == 0f)
        {
            return ++this.timeOutCount;
        }

        this.elapsedTime += deltaTime;
        float wrappedElapsedTime = this.wrappedElapsedTime + deltaTime;

        int timeOutCount = 0;
        while (wrappedElapsedTime >= this.limit)
        {
            wrappedElapsedTime -= this.limit;
            ++timeOutCount;
        }

        this.wrappedElapsedTime = wrappedElapsedTime;
        this.timeOutCount += timeOutCount;

        return timeOutCount;
    }



    #endregion
}

