using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

// Mostly based on:
// https://alastaircrabtree.com/implementing-the-retry-pattern-for-async-tasks-in-c/
// https://docs.microsoft.com/en-us/azure/architecture/patterns/retry

public class RetryHelper
{
    public static async Task RetryOnExceptionAsync<TException>(int aMaxAttempts, Func<Task> aOperation) where TException : Exception
    {
        if (aMaxAttempts <= 0)
            throw new ArgumentOutOfRangeException(nameof(aMaxAttempts));

        var attempts = 0;
        TException exception;
        do
        {
            try
            {
                Debug.Log("Attempt #" + attempts);
                await aOperation();
                break;
            }
            catch (TException ex)
            {
                Debug.Log("RetryHelper Exception encountered: " + ex.Message);

                if (attempts == aMaxAttempts)
                    throw;

                exception = ex;
            }

            attempts++;
            await CreateDelayForException(aMaxAttempts, attempts, exception);

        } while (true);
    }

    private static Task CreateDelayForException(int aMaxAttempts, int aAttempts, Exception aException)
    {
        var nextDelay = 3;
        Debug.Log($"Exception on attempt {aAttempts} of {aMaxAttempts}. Will retry after sleeping for {nextDelay}. " + aException.Message);
        return Task.Delay(nextDelay);
    }
}
