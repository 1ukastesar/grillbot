﻿using GrillBot.App.Infrastructure.Embeds;

namespace GrillBot.App.Modules.Implementations.Reminder;

public class RemindListMetadata : PaginatedMetadataBase
{
    public override string EmbedKind => "Remind";

    public ulong OfUser { get; set; }

    protected override void Save(IDictionary<string, string> destination)
    {
        destination[nameof(OfUser)] = OfUser.ToString();
    }

    protected override bool TryLoad(IReadOnlyDictionary<string, string> values)
    {
        ulong ofUser = 0;
        var success = values.TryGetValue(nameof(OfUser), out var _ofUser) && ulong.TryParse(_ofUser, out ofUser);

        if (success)
        {
            OfUser = ofUser;
            return true;
        }

        return false;
    }

    protected override void Reset()
    {
        OfUser = default;
    }
}
