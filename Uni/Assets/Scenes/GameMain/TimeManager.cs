using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager
{
    public static TimeManager Instance { get; private set; }

    public float MusicTimeLine { get; private set; }    // 現在時間(1小節 = 1.0)
    public float Bpm { get; set; }                      // 現在のBPM


    
    static TimeManager()
    {
        Instance = new TimeManager();
    }
    private TimeManager()
    {
        this.MusicTimeLine = 0f;
        this.Bpm = 60;
    }


	// Update is called once per frame
	public void Update ()
    {
        // 曲の進行時間を更新
        this.MusicTimeLine += (this.Bpm / 60) / 4 * Time.deltaTime;
    }


    public float GetMS()
    {
        return this.MusicTimeLine * 4f * (60f / this.Bpm) * 1000f;
    }
}
