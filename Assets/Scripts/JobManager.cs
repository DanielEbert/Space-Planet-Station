using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobManager : MonoBehaviour {

    Queue<Job> jobs;

    public static JobManager instance;

    void Awake() {
        instance = this;
        jobs = new Queue<Job>();
    }

    public Job TryGetJob() {
        if (jobs.Count > 0)
            return jobs.Dequeue();
        return null;
    }

    public void QueueJob(Job job) {
        print("Q");
        jobs.Enqueue(job);
    }
}
