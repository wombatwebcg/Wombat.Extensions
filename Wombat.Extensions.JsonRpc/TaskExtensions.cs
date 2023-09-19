﻿using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Wombat.Extensions.JsonRpc
{
    static public class TaskExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Forget(this Task task)
        {
            // Empty on purpose!
        }

        private const int DEFAULTTIMEOUT = 5000;

        public static Task OrTimeout(this Task task, int milliseconds = DEFAULTTIMEOUT) => OrTimeout(task, new TimeSpan(0, 0, 0, 0, milliseconds));

        public static async Task OrTimeout(this Task task, TimeSpan timeout)
        {
            using (var cancel = new CancellationTokenSource())
            {
                var delayTask = Task.Delay(timeout, cancel.Token);
                if (await Task.WhenAny(task, delayTask) != task)
                {
                    throw new TimeoutException();
                }
                else
                {
                    cancel.Cancel();
                }
            }
        }

        public static Task<T> OrTimeout<T>(this Task<T> task, int milliseconds = DEFAULTTIMEOUT) => OrTimeout(task, new TimeSpan(0, 0, 0, 0, milliseconds));

        public static async Task<T> OrTimeout<T>(this Task<T> task, TimeSpan timeout)
        {
            using (var cancel = new CancellationTokenSource())
            {
                var delayTask = Task.Delay(timeout, cancel.Token);
                if (await Task.WhenAny(task, delayTask) != task)
                {
                    throw new TimeoutException();
                }
                else
                {
                    cancel.Cancel();
                }

                return task.Result;
            }
        }
    }
}
