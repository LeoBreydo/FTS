using System.Collections.Generic;
using ProductInterfaces;

namespace ProductClasses
{
    public class Product : IProduct
    {
        private readonly List<IService> Subservices;
        private readonly IProductActivator ProductActivator;
        private readonly IProductDeactivator ProductDeactivator;
        public Product(IEnumerable<IService> services,IProductActivator productActivator,IProductDeactivator productDeactivator)
        {
            Subservices = new List<IService>(services);
            ProductActivator = productActivator;
            ProductDeactivator = productDeactivator;
        }
        /// <summary>
        /// Start work
        /// </summary>
        public void Start()
        {
            if (IsStarted) return;
            IsStarted = true;

            if (ProductActivator != null)
                ProductActivator.Activate();

            foreach (IService service in Subservices)
                service.Start();
#warning Заглушка, вместо этого должно проверяться, что все сервисы стартовались (у сервисов ввести состояние Starting,Started,Stopping,Stopped)
            System.Threading.Thread.Sleep(2000);
        }

        private bool deactivationCalled;
        /// <summary>
        /// Stop work
        /// </summary>
        public void Stop()
        {
            if (deactivationCalled || !IsStarted) return;
            deactivationCalled = true;

            if (ProductDeactivator != null)
                ProductDeactivator.Deactivate();

            for (int i = Subservices.Count - 1; i >= 0; --i)
                Subservices[i].Stop();
            IsStarted = false;
            deactivationCalled = false;
        }
        /// <summary>
        /// Returns true if work is started
        /// </summary>
        public bool IsStarted { get; private set; }
    }
}
