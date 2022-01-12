﻿using GrillBot.Data.Models.API.Users;

namespace GrillBot.Data.Models.API.Channels
{
    public class ChannelDetail : GuildChannel
    {
        public User LastMessageFrom { get; set; }
        public User MostActiveUser { get; set; }
        public Channel ParentChannel { get; set; }

        public ChannelDetail() { }

        public ChannelDetail(Database.Entity.GuildChannel channel) : base(channel)
        {
            ParentChannel = channel.ParentChannel != null ? new Channel(channel.ParentChannel) : null;
        }
    }
}
