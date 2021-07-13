namespace CommonStructures
{

    /// <summary>
    /// The messages sending by channels
    /// </summary>
    /// <remarks>
    /// The using messages must be derived from class Messages.BaseMessage and to have the default ctor
    /// </remarks>
    public interface IMsg 
    {
        TimeStamp Time { get; set; }
        int MessageNumber { get; }
        string Serialize();
    }

}
