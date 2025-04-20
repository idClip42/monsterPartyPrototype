#nullable enable

public interface ISpeedLimiter
{
    bool IsLimitingMaxSpeed { get; }
    float MaxSpeedPercentageLimit { get; }
}
