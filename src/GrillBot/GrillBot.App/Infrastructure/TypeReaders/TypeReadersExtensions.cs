﻿using Discord;
using Discord.Commands;
using Discord.Interactions;
using GrillBot.App.Infrastructure.TypeReaders.Interactions;
using GrillBot.App.Infrastructure.TypeReaders.TextBased;
using System;

namespace GrillBot.App.Infrastructure.TypeReaders
{
    public static class TypeReadersExtensions
    {
        public static void RegisterTypeReaders(this CommandService commandService)
        {
            commandService.AddTypeReader<Guid>(new GuidTypeReader());
            commandService.AddTypeReader<IMessage>(new MessageTypeReader(), true);
            commandService.AddTypeReader<IEmote>(new EmotesTypeReader());
            commandService.AddTypeReader<IUser>(new UserTypeReader(), true);
            commandService.AddTypeReader<DateTime>(new DateTimeTypeReader(), true);
            commandService.AddTypeReader<bool>(new BooleanTypeReader(), true);
        }

        public static void RegisterTypeConverters(this InteractionService service)
        {
            service.AddTypeConverter<bool>(new BooleanTypeConverter());
            service.AddTypeConverter<DateTime>(new DateTimeTypeConverter());
            service.AddTypeConverter<IEmote>(new EmotesTypeConverter());
            service.AddTypeConverter<Guid>(new GuidTypeConverter());
            service.AddTypeConverter<IMessage>(new MessageTypeConverter());
        }
    }
}
