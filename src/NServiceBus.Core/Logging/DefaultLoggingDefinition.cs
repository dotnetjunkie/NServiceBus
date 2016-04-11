namespace NServiceBus.Logging
{
    using System;
    using System.IO;
    using System.Web;
    using IODirectory=System.IO.Directory;

    /// <summary>
    /// The default <see cref="LoggingFactoryDefinition"/>.
    /// </summary>
    public class DefaultFactory : LoggingFactoryDefinition
    {

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultFactory"/>.
        /// </summary>
        public DefaultFactory()
        {
            directory = FindDefaultLoggingDirectory();
            level = LogLevelReader.GetDefaultLogLevel();
        }

        /// <summary>
        /// <see cref="LoggingFactoryDefinition.GetLoggingFactory"/>.
        /// </summary>
        protected internal override ILoggerFactory GetLoggingFactory()
        {
            var loggerFactory = new DefaultLoggerFactory(level, directory);
            var message = string.Format("Logging to '{0}' with level {1}", directory, level);
            loggerFactory.Write(GetType().Name,LogLevel.Info,message);
            return loggerFactory;
        }

        LogLevel level;

        /// <summary>
        /// Controls the <see cref="LogLevel"/>.
        /// </summary>
        public void Level(LogLevel level)
        {
            this.level = level;
        }

        string directory;

        /// <summary>
        /// The directory to log files to.
        /// </summary>
        public void Directory(string directory)
        {
            if (!IODirectory.Exists(directory))
            {
                var message = string.Format("Could not find logging directory: '{0}'", directory);
                throw new DirectoryNotFoundException(message);
            }
            this.directory = directory;
        }

        static string FindDefaultLoggingDirectory()
        {
            //use appdata if it exists
            if (HttpContext.Current != null)
            {
                var appDataPath = HttpContext.Current.Server.MapPath("~/App_Data/");
                if (IODirectory.Exists(appDataPath))
                {
                    return appDataPath;
                }
            }
            return AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}