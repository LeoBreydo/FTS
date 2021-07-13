namespace ProductInterfaces
{
    /// <summary>
    /// Resolves entity by id
    /// </summary>
    /// <remarks>
    /// Used when product is compiled from configuration file(s) in the case when 
    /// multiple entities of the specified interfaces are registered but the specified one should be used in the owning object.
    /// Use this interface in the owning object ctor arguments to pass IResolver item with specified entities ids (type string).
    /// </remarks>
    public interface IResolver
    {
        T Resolve<T>(bool returnNullWhenNotRegistered = false);
        T[] ResolveAll<T>();
        T ResolveById<T>(string id, bool returnNullWhenUnknownId=false);
    }
}
