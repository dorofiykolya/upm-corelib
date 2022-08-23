using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Framework.Runtime.TaskTransactions
{
    public static class TaskUnity
    {
        public static async Task Wait(float seconds)
        {
            var startTime = Time.time;
            while (startTime + seconds > Time.time)
            {
                await Task.Yield();
            }
        }
    }
}