public class MonsterPartyException : System.Exception {
    public MonsterPartyException ()
    {}

    public MonsterPartyException (string message) 
        : base(message)
    {}

    public MonsterPartyException (string message, System.Exception innerException)
        : base (message, innerException)
    {}    
}