using UnityEngine;

#nullable enable

namespace MonsterParty
{
    public class MonsterPartyException : System.Exception
    {
        public MonsterPartyException()
        { }

        public MonsterPartyException(string message)
            : base(message)
        { }

        public MonsterPartyException(string message, System.Exception innerException)
            : base(message, innerException)
        { }
    }

    public class MonsterPartyNullReferenceException : MonsterPartyException
    {
        public MonsterPartyNullReferenceException(
            string fieldName,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = ""
        )
            : base($"'{fieldName}' is NULL in '{memberName}'.")
        { }

        public MonsterPartyNullReferenceException(
            MonoBehaviour context, 
            string fieldName,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = ""
        )
            : base($"'{fieldName}' is NULL on '{context.gameObject.name}' ({context.GetType().Name}) in '{memberName}'")
        { }
    }

    public class MonsterPartyFindFailException<T> : MonsterPartyException
    {
        public MonsterPartyFindFailException()
            : base($"'{typeof(T).Name}' could not be found in scene.")
        { }
    }

    public class MonsterPartyGetComponentException<T> : MonsterPartyException
    {
        public MonsterPartyGetComponentException(MonoBehaviour target)
            : base($"'{typeof(T).Name}' could not be found on GameObject '{target.gameObject}'.")
        { }
    }

    public class MonsterPartyUnhandledEnumException<T> : MonsterPartyException
        where T : System.Enum
    {
        public MonsterPartyUnhandledEnumException(
            T value,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = ""
        )
            : base($"Unhandled value '{value}' of enum '{typeof(T).Name}' in '{memberName}'.")
        { }
    }
}