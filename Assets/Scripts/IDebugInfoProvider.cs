using System.Collections.Generic;

#nullable enable

namespace MonsterParty
{
    public interface IDebugInfoProvider
    {
        string DebugHeader { get; }
        void FillInDebugInfo(Dictionary<string, string> infoTarget);
    }
}