using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TimeType
{
    ExactTime,
    PercentageTime
}

public interface IRecordable<RA> where RA : IRecordableAction
{
    float TimeRecorded { get; }
    bool IsRecording { get; }

    void StartRecording(bool overridePreviousRecording = true);
    void StopRecording();

    void ApplyRecordedAction(RA action);
    RA GetRecordedActionAt(float time, TimeType timeType);

}

public interface IRecordableAction
{
    float TimeStamp { get; }
}