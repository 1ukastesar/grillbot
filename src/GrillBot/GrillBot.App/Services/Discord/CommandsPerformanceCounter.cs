﻿namespace GrillBot.App.Services.Discord;

public static class CommandsPerformanceCounter
{
    private static Dictionary<string, DateTime> RunningTasks { get; } = new();
    private static readonly object RunningTasksLock = new();

    private static string CreateContextKey(IInteractionContext context)
        => $"{context.Interaction.GetType().Name}|{context.User.Id}|{context.Interaction.Id}";

    private static string CreateContextKey(global::Discord.Commands.ICommandContext context)
        => $"TextBasedCommand|{context.User.Id}|{context.Message.Id}";

    public static void StartTask(IInteractionContext context)
        => StartRunningTask(CreateContextKey(context));

    public static void StartTask(global::Discord.Commands.ICommandContext context)
        => StartRunningTask(CreateContextKey(context));

    private static void StartRunningTask(string contextKey)
    {
        lock (RunningTasksLock)
        {
            if (RunningTasks.ContainsKey(contextKey))
                return;

            RunningTasks.Add(contextKey, DateTime.Now);
        }
    }

    public static int TaskFinished(IInteractionContext context)
        => RunningTaskFinished($"{context.Interaction.GetType().Name}|{context.User.Id}|{context.Interaction.Id}");

    public static int TaskFinished(global::Discord.Commands.ICommandContext context)
        => RunningTaskFinished($"TextBasedCommand|{context.User.Id}|{context.Message.Id}");

    private static int RunningTaskFinished(string contextKey)
    {
        var startAt = RunningTaskCompleted(contextKey);

        return Convert.ToInt32(
            Math.Round((DateTime.Now - startAt).TotalMilliseconds)
        );
    }

    private static DateTime RunningTaskCompleted(string contextKey)
    {
        lock (RunningTasksLock)
        {
            RunningTasks.Remove(contextKey, out var startAt);

            return startAt;
        }
    }
}