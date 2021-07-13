using CommonStructures;

namespace BrokerInterfaces
{
    /// <summary>
    /// Определяет способность брокеров отправлять отчеты по ордерам
    /// </summary>
    /// <remarks>
    /// Должно учитываться наличие соединения с брокером по сети, а также то, что 
    /// после установки соединения некоторое время брокер может обмениваться с нами пропущенными сообщениями
    /// Использовать будем для учета таймаутов по ожиданию отчетов от брокера.
    /// (возможно также расширить интерфейс для проверки - допустима ли отправка нового ордера с учетом расписания брокера)
    /// </remarks>
    public interface ITradingApiReadiness
    {
        void ProcessBrokerConnectionStatus(IMsg brokerConnectionStatus);
        void SecondPulse();
        bool BrokerIsReadyToSendReports(long brokerID);
    }
}
