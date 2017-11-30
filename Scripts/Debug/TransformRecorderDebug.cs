using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransformRecorderDebug : MonoBehaviour
{
    [Header("Options")]
    [SerializeField]
    private float timeScale = 1;

    [Header("Game")]
    [SerializeField]
    private TransformRecorder[] targets;

    [Header("UI")]
    [SerializeField]
    private Button playForward;

    [SerializeField]
    private Button playBackwards;

    [SerializeField]
    private Button pause;

    [SerializeField]
    private Button rec;

    [SerializeField]
    private Text currentTimeText;

    [SerializeField]
    private Text maxTimeText;

    [SerializeField]
    private Image progressBar;


    // Variables
    private float currentTime = 0;
    private float timeIncreaseMultiplier = 0;
    private bool isPlaying = false;
    private bool isRecording = false;

    // colors
    private Color recButtonColor;

    protected void Awake()
    {
        playForward.onClick.AddListener(()=> { OnPlayForward(); });
        playBackwards.onClick.AddListener(() => { OnPlayBackward(); });
        pause.onClick.AddListener(() => { Pause(); });
        rec.onClick.AddListener(() => { OnRec(); });

        recButtonColor = rec.image.color;
    }

    private void OnPlayForward()
    {
        isPlaying = true;
        timeIncreaseMultiplier = timeScale;
    }

    private void OnPlayBackward()
    {
        isPlaying = true;
        timeIncreaseMultiplier = -timeScale;

    }

    private void Pause()
    {
        isPlaying = !isPlaying;
    }

    private void OnRec()
    {
        if (!targets[0].IsRecording)
        {
            rec.image.color = recButtonColor * 0.5f;
            for(int i = 0; i < targets.Length; i++)
            {
                targets[i].StartRecording(true);
            }
        }
        else
        {
            StopRecording();
        }
    }

    private void StopRecording()
    {
        rec.image.color = recButtonColor;

        for (int i = 0; i < targets.Length; i++)
        {
            targets[i].StopRecording();
            targets[i].ApplyRecordedAction(targets[i].GetRecordedActionAt(0, TimeType.PercentageTime));
        }
    }

    protected void LateUpdate()
    {
        float timeRecorded = targets[0].TimeRecorded;
        currentTimeText.text = NormalFloat(currentTime).ToString();
        maxTimeText.text = NormalFloat(timeRecorded).ToString();
        Vector3 pbs = progressBar.transform.localScale;
        if(timeRecorded > 0)
            pbs.x = (currentTime / timeRecorded);

        progressBar.transform.localScale = pbs;

        if (isPlaying)
        {
            currentTime += timeIncreaseMultiplier * Time.deltaTime;
            if(currentTime > timeRecorded)
            {
                currentTime = timeRecorded;
                Pause();
            }
            else if (currentTime < 0)
            {
                currentTime = 0;
                Pause();
            }

            for (int i = 0; i < targets.Length; i++)
            {
                targets[i].ApplyRecordedAction(targets[i].GetRecordedActionAt(currentTime, TimeType.ExactTime));
            }
        }
    }

    private float NormalFloat(float f)
    {
        return (float)(Math.Round(f * 10f) / 10f);
    }
}
