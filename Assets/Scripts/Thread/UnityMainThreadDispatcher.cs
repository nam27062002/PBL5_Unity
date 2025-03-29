using System.Collections.Generic;
using System;

public class UnityMainThreadDispatcher : SingletonMonoBehavior<UnityMainThreadDispatcher>
{
    private static readonly Queue<Action> ExecutionQueue = new();
    private void Update()
    {
        lock (ExecutionQueue)
        {
            while (ExecutionQueue.Count > 0)
            {
                ExecutionQueue.Dequeue().Invoke();
            }
        }
    }

    public void Enqueue(Action action)
    {
        lock (ExecutionQueue)
        {
            ExecutionQueue.Enqueue(action);
        }
    }
}