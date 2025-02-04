using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class CoroutineWrapper
{
    private readonly IEnumerator _coroutine;

    public CoroutineWrapper(IEnumerator coroutine)
    {
        _coroutine = coroutine;
    }

    public IEnumerator RunCoroutine()
    {
        yield return _coroutine;
    }
}

[DefaultExecutionOrder(-19550)]
public class CoroutineDispatcher : SingletonMonoBehavior<CoroutineDispatcher>
{
    public static readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();
    public static readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();

    private readonly Queue<CoroutineWrapper> _coroutines = new Queue<CoroutineWrapper>();

    private int _pendingCoroutineCount = 0;
    private const int ConcurrentOperations = 1;

    private void Update()
    {
        if (CanRunCoroutine())
        {
            RunNextCoroutine();
        }
    }

    private void RunNextCoroutine()
    {
        StartCoroutine(RunCoroutine(_coroutines.Dequeue()));
    }

    private IEnumerator RunCoroutine(CoroutineWrapper coroutine)
    {
        _pendingCoroutineCount++;
        yield return coroutine.RunCoroutine();
        _pendingCoroutineCount--;
    }

    private bool CanRunCoroutine()
    {
        return _pendingCoroutineCount < ConcurrentOperations && _coroutines.Count > 0;
    }

    private void AddCoroutineInternal(IEnumerator coroutine)
    {
        _coroutines.Enqueue(new CoroutineWrapper(coroutine));
    }

    public static void QueueCoroutine(IEnumerator coroutine)
    {
        if (!HasInstance)
            return;

        Instance.AddCoroutineInternal(coroutine);
    }

    public static Coroutine RunCoroutine(IEnumerator coroutine)
    {
        return !HasInstance ? null : Instance.StartCoroutine(coroutine);
    }

    public static void CancelCoroutine(Coroutine coroutine)
    {
        if (!HasInstance)
            return;

        Instance.StopCoroutine(coroutine);
    }
}