﻿using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Helpers;

public static class ServiceExtensions
{
    public static IServiceCollection AddHelpers(this IServiceCollection services)
    {
        services
            .AddScoped<PointsHelper>()
            .AddScoped<DownloadHelper>()
            .AddScoped<ChannelHelper>()
            .AddScoped<UnverifyHelper>()
            .AddScoped<EmoteSuggestionHelper>();

        return services;
    }
}
