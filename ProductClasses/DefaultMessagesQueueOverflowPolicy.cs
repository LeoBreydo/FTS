using System;
using System.IO;
using CommonStructures;
using ProductInterfaces;

namespace ProductClasses
{
    /// <summary>
    /// Default policy to process MessageQueue events Overflow and MessageHandledBeforeStart
    /// (saves the missing events to the special log file)
    /// </summary>
    public class DefaultMessagesQueueOverflowPolicy : IMessagesQueueOverflowPolicy
    {
        /// <summary>
        /// The log file to save to
        /// </summary>
        public readonly string LogFileName;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logFileName">the log file to save to</param>
        public DefaultMessagesQueueOverflowPolicy(string logFileName)
        {
            LogFileName = Path.GetFullPath(logFileName);
        }

        public void OnOverflow<TMessage>(TMessage message)
        {
            string txt = string.Format("ERROR\t{0}\tOverFlow (message ignored):\t{1}\n",
                                       DateTime.UtcNow.ToString(TimeHelper.TimeFormat), message);

            Console.WriteLine(txt);
            try
            {
                File.AppendAllText(LogFileName, txt);

            }
            catch (Exception exception)
            {
                Console.WriteLine("Message logout failed:" + exception.Message);
            }
        }
    }
}
