using System.Collections.Generic;
using UnityEngine;

public class TransformRecorder : MonoBehaviour, IRecordable<TransformRecordedAction>
{
    public bool IsRecording
    {
        get; private set;
    }

    public float TimeRecorded
    {
        get
        {
            return timeAmountRecorded;
        }
    }

    [SerializeField]
    private float recordDelayInSeconds = 0.5f;

    [SerializeField]
    private float minDistanceForSnapping = 1;

    private float timeSinceLastRecording = 0;
    private float timeAmountRecorded = 0;

    private List<TransformRecordedAction> recordedData = new List<TransformRecordedAction>();

    public void ApplyRecordedAction(TransformRecordedAction action)
    {
        transform.position = action.Position;
        transform.rotation = Quaternion.Euler(action.Rotation);
        transform.localScale = action.Scale;
    }

    public TransformRecordedAction GetRecordedActionAt(float time, TimeType timeType)
    {
        TransformRecordedAction action = new TransformRecordedAction();

        float percentage = time;

        if(timeType == TimeType.ExactTime)
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

        TransformRecordedAction[] acs = GetActionsForTimePercentage(time);

        if(acs.Length == 1)
        {
            action = acs[0];
        }
        else if(acs.Length > 1)
        {
            TransformRecordedAction acStart = acs[0];
            TransformRecordedAction acLate = acs[1];
            float percentageBetween2 = Mathf.Abs((acStart.TimeStamp - time)/(acLate.TimeStamp - time));
            Vector3 lerpedPos = LerpOrSnap(acStart.Position, acLate.Position, percentageBetween2, minDistanceForSnapping);
            Vector3 lerpedRot = Quaternion.Lerp(Quaternion.Euler(acStart.Rotation), Quaternion.Euler(acStart.Rotation), percentageBetween2).eulerAngles;
            Vector3 lerpedScale = Vector3.Lerp(acStart.Scale, acLate.Scale, percentageBetween2);

            action = new TransformRecordedAction(time, lerpedPos, lerpedRot, lerpedScale);
        }

        return action;
    }

    public void StartRecording(bool overridePreviousRecording = true)
    {
        if (IsRecording) return;

        if (overridePreviousRecording)
        {
            recordedData.Clear();
            timeSinceLastRecording = 0;
            timeAmountRecorded = 0;
        }

        IsRecording = true;
    }

    public void StopRecording()
    {
        if (!IsRecording) return;
        IsRecording = false;
    }

    protected void LateUpdate()
    {
        if (!IsRecording) return;

        timeAmountRecorded += Time.deltaTime;

        if(timeAmountRecorded >= timeSinceLastRecording + recordDelayInSeconds || timeSinceLastRecording == 0)
        {
            Record(timeAmountRecorded, transform.position, transform.rotation.eulerAngles, transform.localScale);

            timeSinceLastRecording = timeAmountRecorded;
        }
    }

    private void Record(float time, Vector3 pos, Vector3 rot, Vector3 scale)
    {
        bool performAdd = recordedData.Count == 0;
        TransformRecordedAction newAction = new TransformRecordedAction(time, pos, rot, scale);

        if (!performAdd)
        {
            TransformRecordedAction preAction = recordedData[recordedData.Count - 1];
            performAdd = !((newAction.Position == preAction.Position) && (newAction.Rotation == preAction.Rotation) && (newAction.Rotation == preAction.Rotation));
        }

        if (performAdd)
        {
            recordedData.Add(newAction);
        }
    }

    /// <summary>
    /// Returns 1 element if time == a recoded time, else it returns 2 surrounding elements
    /// </summary>
    /// <param name="time">time</param>
    /// <returns>Element(s) of interest, returns an empty array if nothing has been recorded</returns>
    private TransformRecordedAction[] GetActionsForTimePercentage(float time)
    {
        if (recordedData.Count != 0)
        {
            if (recordedData[0].TimeStamp >= time) { return new TransformRecordedAction[] { recordedData[0] }; }

            for (int i = recordedData.Count - 1; i >= 0; i--)
            {
                if (recordedData[i].TimeStamp <= time)
                {
                    if (i == recordedData.Count - 1 || recordedData.Count == 1 || recordedData[i].TimeStamp == time) { return new TransformRecordedAction[] { recordedData[i] }; }

                    return new TransformRecordedAction[] { recordedData[i], recordedData[i + 1] };
                }
            }
        }

        return new TransformRecordedAction[] { };
    }

    private float TimeToPercentage(float time)
    {
        if (TimeRecorded == 0) return 0;
        return Mathf.Clamp01(time / TimeRecorded);
    }

    private Vector3 LerpOrSnap(Vector3 start, Vector3 end, float percentage, float minDistanceToSnap)
    {
        float dist = Vector3.Distance(start, end);
        if (dist < minDistanceForSnapping)
        {
            return Vector3.Lerp(start, end, percentage);
        }
        else
        {
            return end;
        }
    }
}

public struct TransformRecordedAction : IRecordedAction
{
    public float TimeStamp
    {
        get; private set;
    }

    public Vector3 Position { get; private set; }
    public Vector3 Rotation { get; private set; }
    public Vector3 Scale { get; private set; }

    public TransformRecordedAction(float time, Vector3 position, Vector3 rotation, Vector3 scale)
    {
        TimeStamp = time;
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }
}
