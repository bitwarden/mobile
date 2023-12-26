namespace Core
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Fires a task and ignores any exception.
        /// See http://stackoverflow.com/a/22864616/344182
        /// </summary>
        /// <param name="task">The task to be forgotten.</param>
        /// <param name="onException">Action to be called on exception.</param>
        public static async void FireAndForget(this Task task, Action<Exception> onException = null)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                onException?.Invoke(ex);
            }
        }
    }
}
