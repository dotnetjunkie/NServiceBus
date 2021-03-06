namespace NServiceBus.Settings
{
    using System;
    using System.Configuration;
    using System.Transactions;
    using System.Transactions.Configuration;
    using NServiceBus.Unicast.Transport;

    /// <summary>
    ///     Configuration class for Transaction settings.
    /// </summary>
    public class TransactionSettings
    {
        internal TransactionSettings(BusConfiguration config)
        {
            this.config = config;
            maxTimeout = GetMaxTimeout();
        }

        /// <summary>
        ///     Configures the <see cref="ITransport" /> not to not use any transactions.
        /// </summary>
        public TransactionSettings Disable()
        {
            config.Settings.Set("Transactions.Enabled", false);
            config.Settings.SetDefault("Transactions.DoNotWrapHandlersExecutionInATransactionScope", false);
            config.Settings.SetDefault("Transactions.SuppressDistributedTransactions", true);

            return this;
        }

        /// <summary>
        ///     Configures the <see cref="ITransport" /> to use transactions.
        /// </summary>
        public TransactionSettings Enable()
        {
            config.Settings.Set("Transactions.Enabled", true);
            config.Settings.SetDefault("Transactions.DoNotWrapHandlersExecutionInATransactionScope", true);
            config.Settings.SetDefault("Transactions.SuppressDistributedTransactions", false);

            return this;
        }
              
        /// <summary>
        ///     Sets the isolation level of the transaction.
        /// </summary>
        /// <param name="isolationLevel">
        ///     A <see cref="IsolationLevel" /> enumeration that specifies the isolation level of the
        ///     transaction.
        /// </param>
        public TransactionSettings IsolationLevel(IsolationLevel isolationLevel)
        {
            config.Settings.Set("Transactions.IsolationLevel", isolationLevel);

            return this;
        }

        /// <summary>
        ///     Configures the <see cref="ITransport" /> not to enlist in Distributed Transactions.
        /// </summary>
        public TransactionSettings DisableDistributedTransactions()
        {
            config.Settings.Set("Transactions.SuppressDistributedTransactions", true);
            config.Settings.SetDefault("Transactions.DoNotWrapHandlersExecutionInATransactionScope", false);
            return this;
        }

        /// <summary>
        ///     Configures the <see cref="ITransport" /> to enlist in Distributed Transactions.
        /// </summary>
        public TransactionSettings EnableDistributedTransactions()
        {
            config.Settings.Set("Transactions.SuppressDistributedTransactions", false);
            return this;
        }

        /// <summary>
        ///     Configures this endpoint so that <see cref="IHandleMessages{T}">handlers</see> are not wrapped in a
        ///     <see cref="TransactionScope" />.
        /// </summary>
        public TransactionSettings DoNotWrapHandlersExecutionInATransactionScope()
        {
            config.Settings.Set("Transactions.DoNotWrapHandlersExecutionInATransactionScope", true);
            return this;
        }

        /// <summary>
        ///     Configures this endpoint so that <see cref="IHandleMessages{T}">handlers</see> not wrapped in a
        ///     <see cref="TransactionScope" />.
        /// </summary>
        public TransactionSettings WrapHandlersExecutionInATransactionScope()
        {
            config.Settings.Set("Transactions.DoNotWrapHandlersExecutionInATransactionScope", false);
            return this;
        }

        /// <summary>
        ///     Sets the default timeout period for the transaction.
        /// </summary>
        /// <param name="defaultTimeout">
        ///     A <see cref="TimeSpan" /> value that specifies the default timeout period for the
        ///     transaction.
        /// </param>
        public TransactionSettings DefaultTimeout(TimeSpan defaultTimeout)
        {
            if (defaultTimeout > maxTimeout)
            {
                throw new ConfigurationErrorsException(
                    "Timeout requested is longer than the maximum value for this machine. Please override using the maxTimeout setting of the system.transactions section in machine.config");
            }

            config.Settings.Set("Transactions.DefaultTimeout", defaultTimeout);
            return this;
        }

        static TimeSpan GetMaxTimeout()
        {
            //default is 10 always 10 minutes
            var maxTimeout = TimeSpan.FromMinutes(10);

            var systemTransactionsGroup = ConfigurationManager.OpenMachineConfiguration()
                .GetSectionGroup("system.transactions");

            if (systemTransactionsGroup != null)
            {
                var machineSettings = systemTransactionsGroup.Sections.Get("machineSettings") as MachineSettingsSection;

                if (machineSettings != null)
                {
                    maxTimeout = machineSettings.MaxTimeout;
                }
            }

            return maxTimeout;
        }

        BusConfiguration config;
        TimeSpan maxTimeout;
    }
}