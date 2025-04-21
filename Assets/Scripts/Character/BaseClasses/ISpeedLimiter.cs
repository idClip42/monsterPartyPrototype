#nullable enable

namespace MonsterParty
{
    public interface ISpeedLimiter
    {
        bool IsLimitingMaxSpeed { get; }
        float MaxSpeedPercentageLimit { get; }
    }
}