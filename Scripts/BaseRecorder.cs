using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseRecorder<RA> : MonoBehaviour, IRecordable<RA> where RA : struct, IRecordableAction
{
    public delegate void RecordedDataDelegate(BaseRecordedData<RA> recordedData);
    public event RecordedDataDelegate RecorderCreatedEvent;
    public event RecordedDataDelegate RecorderDestroyedEvent;

    public BaseRecordedData<RA> RecordedData { get; private set; }

    public bool IsRecording
    {
        get { return RecordedData.IsRecording; }
    }

    public float TimeRecorded
    {
        get { return RecordedData.TimeRecorded; }
    }

    public void SetRecordedData(BaseRecordedData<RA> recordedData)
    {
        RecordedData = recordedData;
    }

    public void ApplyRecordedAction(RA action)
    {
        RecordedData.ApplyRecordedAction(action);
    }

    public RA GetRecordedActionAt(float time, TimeType timeType)
    {
        return RecordedData.GetRecordedActionAt(time, timeType);
    }

    public void StartRecording(bool overridePreviousRecording = true)
    {
        RecordedData.StartRecording(overridePreviousRecording, this.gameObject);
    }

    public void StopRecording()
    {
        RecordedData.StopRecording();
    }

    protected void Awake()
    {
        SetRecordedData(DefineRecorderData());

        if (RecorderCreatedEvent != null)
            RecorderCreatedEvent(RecordedData);
    }

    protected void LateUpdate()
    {
        RecordedData.LateUpdate();
    }

    protected void OnDestroy()
    {
        if (RecorderDestroyedEvent != null)
            RecorderDestroyedEvent(RecordedData);
    }

    protected abstract BaseRecordedData<RA> DefineRecorderData();
}

public abstract class BaseRecordedData<RA> : IRecordable<RA> where RA : struct, IRecordableAction
{
    public float TimeRecorded
    {
        get; private set;
    }

    public bool IsRecording
    {
        get; private set;
    }

    protected GameObject affected { get; private set; }

    protected List<RA> recordedData { get { return _recordedData; } }
    private List<RA> _recordedData = new List<RA>();

    public RA GetRecordedActionAt(float time, TimeType timeType)
    {
        RA action = new RA();

        float percentage = time;

        if (timeType == TimeType.ExactTime)
        {
            if (TimeRecorded != 0)
            {
                percentage = time / TimeRecorded; // Time To Percentage
            }
        }
        else
        {
            time = TimeRecorded * time; // Percentage To Time
        }

        percentage = Mathf.Clamp01(percentage);

        RA[] acs = GetActionsForTimePercentage(time);

        if (acs.Length == 1)
        {
            action = acs[0];
        }
        else if (acs.Length > 1)
        {
            RA acStart = acs[0];
            RA acLate = acs[1];
            float percentageBetween2 = Mathf.Abs((acStart.TimeStamp - time) / (acLate.TimeStamp - time));
            action = SetActionBetween2ActionValues(acStart, acLate, percentageBetween2, time, percentage);
        }

        return action;
    }

    public void StartRecording(bool overridePreviousRecording = true)
    {
        if (IsRecording) return;
        if (overridePreviousRecording)
        {
            recordedData.Clear();
        }
        StartedRecording(overridePreviousRecording);
    }

    public void StartRecording(bool overridePreviousRecording, GameObject affected)
    {
        if (IsRecording) return;
        this.affected = affected;
        if (overridePreviousRecording)
        {
            recordedData.Clear();
            TimeRecorded = 0;
        }
        IsRecording = true;
        StartedRecording(overridePreviousRecording);
    }

    public void StopRecording()
    {
        if (!IsRecording) return;
        IsRecording = false;
        StoppedRecording();
    }

    public void LateUpdate()
    {
        if (!IsRecording) return;
        TimeRecorded += Time.deltaTime;
        RecordUpdate();
    }

    private RA[] GetActionsForTimePercentage(float time)
    {
        if (recordedData.Count != 0)
        {
            if (recordedData[0].TimeStamp >= time) { return new RA[] { recordedData[0] }; }

            for (int i = recordedData.Count - 1; i >= 0; i--)
            {
                if (recordedData[i].TimeStamp <= time)
                {
                    if (i == recordedData.Count - 1 || recordedData.Count == 1 || recordedData[i].TimeStamp == time) { return new RA[] { recordedData[i] }; }

                    return new RA[] { recordedData[i], recordedData[i + 1] };
                }
            }
        }

        return new RA[] { };
    }

    private float TimeToPercentage(float time)
    {
        if (TimeRecorded == 0) return 0;
        return Mathf.Clamp01(time / TimeRecorded);
    }

    public abstract void ApplyRecordedAction(RA action);
    protected abstract RA SetActionBetween2ActionValues(RA earliestValue, RA latestValue, float percentageBetweenTwoValues, float time, float percentageTimeTotalTime);
    protected abstract void StartedRecording(bool deletedPreviousData);
    protected abstract void RecordUpdate();
    protected abstract void StoppedRecording();
}