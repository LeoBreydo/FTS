using System.IO;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using ProductClasses;
using ProductInterfaces;

namespace TradingServiceInstallers
{
    public static class ChannelsInstaller
    {
        /// <summary>
        /// register of the common channels
        /// </summary>
        public static void RegisterChannels(
            WindsorContainer container,
            IPublicChannelListener publicChannelListener,
            string logoutFolderName,
            string[] publicChannelNames,
            string[] quoteChannelNames
            )
        {
            string overflowLogFileName = Path.Combine(logoutFolderName, "overflowChannelLog.txt");
            MessagesQueueOverflowPolicyInstance.Instance = new DefaultMessagesQueueOverflowPolicy(overflowLogFileName);

            foreach (string publicChannelName in publicChannelNames)
            {
#if DEBUG
                container.Register(
                    Component.For<IMsgChannel>().Named(publicChannelName).Instance(new PublicChannel(publicChannelListener) { Title = publicChannelName }));
#else
                container.Register(
                    Component.For<IMsgChannel>().Named(publicChannelName).Instance(new PublicChannel(publicChannelListener)));

                //container.Register(Component.For<IMsgChannel>().Named(publicChannelName).ImplementedBy<PublicChannel>());
#endif
            }

            //container.Register(Component.For<IQtChannel>().ImplementedBy<QtChannel>());
            foreach (string channelName in quoteChannelNames)
            {
#if DEBUG
                container.Register(
                    Component.For<IQtChannel>().Named(channelName).Instance(new QtChannel() { Title = channelName }));
#else
                container.Register(
                    Component.For<IQtChannel>().Named(channelName).Instance(new QtChannel()));
#endif
            }

        }
    }
}