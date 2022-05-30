﻿using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Counters;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Common;

public static class ManagersExtensions
{
    public static IServiceCollection AddCommonManagers(this IServiceCollection services)
    {
        return services
            .AddSingleton<InitManager>()
            .AddSingleton<CounterManager>();
    }
}
