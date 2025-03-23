#nullable enable

using System.Collections.Generic;

public interface IDebugInfoProvider
{
    string DebugHeader { get; }
    void FillInDebugInfo(Dictionary<string, string> infoTarget);
}
