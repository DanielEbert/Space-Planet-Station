using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Job {
    //Range 0 to 1
    public float process;

    public bool AddProcess(float value) {
        process = Mathf.Min(process + value, 1);
        if(IsComplete()) {
            OnComplete();
            return true;
        }
        return false;
    }

    public bool IsComplete()  {
        return process >= 1;
    }

    public abstract void OnComplete();
}
