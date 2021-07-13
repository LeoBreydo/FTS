using ProductInterfaces;

namespace BrokerInterfaces
{
    /// <summary>
    /// Каналы, передаваемые при инициализации в конструкторы BrokerFacade и сопутствующих классов
    /// </summary>
    public interface IBrokerFacadeChannels
    {
        /// <summary>
        /// Отправка статусных сообщений (BrokerConnectionStatus,SessionReadiness) и отчетов по запросам на подписку(SubscriptionPosted,SubscriptionRejection)
        /// </summary>
        IMsgChannel MainReportsChannel { get; }
        /// <summary>
        /// Отправка отчетов по исполнению ордеров
        /// </summary>
        IMsgChannel OrderReportsChannel { get; }
        /// <summary>
        /// Поток котировок и макреров QuoteCancel
        /// </summary>
        IQtChannel QuotesChannel { get; }

        IQtChannel BadQuotesChannel { get; }

        /// <summary>
        /// Сообщения, не используемые другими участниками системы для лога (текстовые сообщения и Fix сообщения уровня сессии кроме HeardBit и TestRequest)
        /// </summary>
        IMsgChannel LogChannel { get; }

        /// <summary>
        /// Командный канал (прослушивание команд активировать/деактивировать соединение)
        /// </summary>
        IMsgChannel CommandsChannel { get; }
    }
}
