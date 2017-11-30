using UnityEngine;

public class TransformRecorder : BaseRecorder<TransformRecordedAction>
{
    [SerializeField]
    private float recordDelayInSeconds = 0.5f;

    [SerializeField]
    private float minDistanceForSnapping = 1;

    protected override BaseRecordedData<TransformRecordedAction> DefineRecorderData()
    {
        return new TransformRecordedData(minDistanceForSnapping, recordDelayInSeconds);
    }
}

public class TransformRecordedData : BaseRecordedData<TransformRecordedAction>
{
    private float minDistanceForSnapping;
    private float recordDelayInSeconds;
    private float timeSinceLastRecording = 0;

    public TransformRecordedData(float minDistanceForSnapping, float recordDelayInSeconds)
    {
        this.minDistanceForSnapping = minDistanceForSnapping;
        this.recordDelayInSeconds = recordDelayInSeconds;
    }

    public override void ApplyRecordedAction(TransformRecordedAction action)
    {
        affected.transform.position = action.Position;
        affected.transform.rotation = Quaternion.Euler(action.Rotation);
        affected.transform.localScale = action.Scale;
    }

    protected override void RecordUpdate()
    {
        if (TimeRecorded >= timeSinceLastRecording + recordDelayInSeconds || timeSinceLastRecording == 0)
        {
            Record(TimeRecorded, affected.transform.position, affected.transform.rotation.eulerAngles, affected.transform.localScale);

            timeSinceLastRecording = TimeRecorded;
        }
    }

    protected override TransformRecordedAction SetActionBetween2ActionValues(TransformRecordedAction earliestValue, TransformRecordedAction latestValue
        , float percentageBetweenTwoValues, float time, float percentageTimeTotalTime)
    {
        float percentageBetween2 = Mathf.Abs((earliestValue.TimeStamp - time) / (latestValue.TimeStamp - time));
        Vector3 lerpedPos = LerpOrSnap(earliestValue.Position, latestValue.Position, percentageBetween2, minDistanceForSnapping);
        Vector3 lerpedRot = Quaternion.Lerp(Quaternion.Euler(earliestValue.Rotation), Quaternion.Euler(earliestValue.Rotation), percentageBetween2).eulerAngles;
        Vector3 lerpedScale = Vector3.Lerp(earliestValue.Scale, latestValue.Scale, percentageBetween2);

        return new TransformRecordedAction(time, lerpedPos, lerpedRot, lerpedScale);
    }

    protected override void StartedRecording(bool deletedPreviousData)
    {
        if (deletedPreviousData)
        {
            timeSinceLastRecording = 0;
        }
    }

    protected override void StoppedRecording()
    {

    }

    private void Record(float timeRecorded, Vector3 position, Vector3 eulerAngles, Vector3 localScale)
    {
        bool performAdd = recordedData.Count == 0;
        TransformRecordedAction newAction = new TransformRecordedAction(timeRecorded, position, eulerAngles, localScale);

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

public struct TransformRecordedAction : IRecordableAction
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
