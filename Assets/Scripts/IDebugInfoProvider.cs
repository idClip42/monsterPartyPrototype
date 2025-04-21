#nullable enable

using System.Collections.Generic;

namespace MonsterParty
{
    public interface IDebugInfoProvider
    {
        string DebugHeader { get; }
        void FillInDebugInfo(Dictionary<string, string> infoTarget);
    }
}