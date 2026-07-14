using System.Text;

namespace BuddyScript.Application.Common.Pagination;

/// <summary>Opaque keyset cursor encoding the last seen (CreatedAt, Id) pair.</summary>
public record FeedCursor(DateTimeOffset CreatedAt, Guid Id)
{
    public string Encode()
    {
        var raw = $"{CreatedAt.UtcTicks}|{Id}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
    }

    public static FeedCursor? Decode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        try
        {
            var raw = Encoding.UTF8.GetString(Convert.FromBase64String(value));
            var parts = raw.Split('|');
            if (parts.Length != 2) return null;

            var ticks = long.Parse(parts[0]);
            var id = Guid.Parse(parts[1]);
            return new FeedCursor(new DateTimeOffset(ticks, TimeSpan.Zero), id);
        }
        catch
        {
            return null; // malformed cursor → treat as first page
        }
    }
}
