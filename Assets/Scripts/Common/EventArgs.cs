using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UpdateBtnRedPointEventArgs : CustomEventArgs
{
    public Custom.Activity.ActivityType activityType;
}

public class UpdateWeeklyTaskEventArgs : CustomEventArgs
{
    public int id;
    public int progress;
}