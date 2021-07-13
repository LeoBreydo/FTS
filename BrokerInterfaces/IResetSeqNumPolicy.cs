using System;

namespace BrokerInterfaces
{
    /// <summary>
    /// Определяет, когда должен производиться сброс SequenceNumbers fix-сессии
    /// </summary>
    public interface IResetSeqNumPolicy
    {
        /// <summary>
        /// Нужно ли выполнить сброс SequenceNumbers 
        /// </summary>
        /// <param name="utcNow">текущее время</param>
        /// <param name="utcLastTimeWhenConnectionEstablished">время, когда в последний раз было установлено соединение (DateTime.MinValue если ни разу)</param>
        /// <returns>true=нужно выполнить сброс SequenceNumbers </returns>
        bool NeedResetSeqNum(DateTime utcNow, DateTime utcLastTimeWhenConnectionEstablished);
    }
}
