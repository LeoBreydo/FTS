using System.Collections.Generic;
using ProductInterfaces;

namespace ProductClasses
{
    /// <summary>
    /// The Channel extension to get list of channels to logout.
    /// </summary>
    public class PublicChannel : Channel
    {
        /// <summary>
        /// List of created channels for outside use
        /// </summary>
        public static readonly List<Channel> AllChannels = new List<Channel>();
        /// <summary>
        /// ctor, add the creating instance to the AllChannels list
        /// </summary>
        public PublicChannel(IPublicChannelListener logoutListener)
        {
            AllChannels.Add(this);
            if (logoutListener!=null)
                Register(logoutListener);
        }
    }
}
