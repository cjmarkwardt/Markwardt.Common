namespace Markwardt;

public static class TaskExtensions
{
    public static async void Fork(this Task task)
        => await task;

    public static async void Fork(this ValueTask task)
        => await task;
}