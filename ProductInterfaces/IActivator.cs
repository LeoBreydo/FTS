namespace ProductInterfaces
{
    /// <summary>
    /// Interface of the object responsible for the product entity activation/deactivation
    /// </summary>
    public interface IActivator
    {
        void Activate();
        void Deactivate();
    }
    /// <summary>
    /// A singleton product entity, specifies the activation order of the product activators (IActivator)
    /// </summary>
    public interface IProductActivator
    {
        void Activate();
    }
    /// <summary>
    /// A singleton product entity, specifies the deactivation order of the product activators (IActivator)
    /// </summary>
    public interface IProductDeactivator
    {
        void Deactivate();
    }
}
