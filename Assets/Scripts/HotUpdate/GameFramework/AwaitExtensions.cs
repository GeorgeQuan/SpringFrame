using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

public static class AwaitExtensions
{
    public static TaskAwaiter<int> GetAwaiter(this Process process)
    {
        var tcs = new TaskCompletionSource<int>();
        process.EnableRaisingEvents = true;

        process.Exited += (s, e) => tcs.TrySetResult(process.ExitCode);

        if (process.HasExited)
        {
            tcs.TrySetResult(process.ExitCode);
        }

        return tcs.Task.GetAwaiter();
    }

    // Any time you call an async method from sync code, you can either use this wrapper
    // method or you can define your own `async void` method that performs the await
    // on the given Task 像是把Task 任务转换成协程使用,不用在使用task 的时候声明异步和await
    public static async void Coroutine(this Task task)
    {

        await task;
    }
}
