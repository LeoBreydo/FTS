using System.Collections.Generic;
using System.Linq;
using ProductInterfaces;

namespace ProductClasses
{
    public class ProductActivator:IProductActivator
    {
        private readonly IActivator[] Members;
        public ProductActivator(IEnumerable<IActivator> members)
        {
            Members = members.ToArray();
        }
        public ProductActivator(params IActivator[] membersArray)
        {
            Members = membersArray.ToArray();
        }
        public ProductActivator(List<string> memberIds, IResolver resolver)
        {
            Members = memberIds.Select(id => resolver.ResolveById<IActivator>(id)).ToArray();
        }
        public void Activate()
        {
            foreach (IActivator t in Members)
                t.Activate();
        }
    }
    public class ProductDeactivator : IProductDeactivator
    {
        private readonly IActivator[] Members;
        public ProductDeactivator(IEnumerable<IActivator> members)
        {
            Members = members.ToArray();
        }
        public ProductDeactivator(params IActivator[] membersArray)
        {
            Members = membersArray.ToArray();
        }
        public ProductDeactivator(List<string> memberIds, IResolver resolver)
        {
            Members = memberIds.Select(id => resolver.ResolveById<IActivator>(id)).ToArray();
        }
        public void Deactivate()
        {
            foreach (IActivator t in Members)
                t.Deactivate();
        }
    }
}
