namespace MakeGrid3D
{
    using System;
    using System.Collections;
    using System.IO;

    /// <summary>
    /// Логгер. 
    /// </summary>
    public static class LogService
    {
        #region Private Fields

        /// <summary>
        /// Название файла с логом. 
        /// </summary>
        private static string _logFileName = "log.txt";

        #endregion Private Fields

        #region Public Fields

        /// <summary>
        /// Включен ли режим логирования только ошибок. 
        /// </summary>
        public static bool IsLogOnlyErrors = false;

        #endregion Public Fields

        #region Private Methods

        /// <summary>
        /// Пробует записать сообщение в файл. 
        /// </summary>
        /// <param name="message">Сообщение.</param>
        /// <param name="writeDate">Логировать ли с датой.</param>
        private static void TryWriteToFile(string message, bool writeDate=true) 
        {
            try
            {
                if (writeDate)
                {
                    File.AppendAllText(_logFileName, $"[{DateTime.Now.ToString("yyyy.dd.MM HH:mm:ss")}] {message}\n");
                }
                else 
                {
                    File.AppendAllText(_logFileName, $"{message}\n");
                }
            }
            catch (Exception ex) { }
        }

        #endregion Private Methods

        #region Public Methods

        /// <summary>
        /// Логирует сообщение. 
        /// </summary>
        public static void Log(string message) 
        {
            if (IsLogOnlyErrors)
            {
                return;
            }

            TryWriteToFile(message);
        }

        /// <summary>
        /// Логирует предупреждение. 
        /// </summary>
        public static void LogWarning(string message) 
        {
            if (IsLogOnlyErrors)
            {
                return;
            }
            TryWriteToFile("[WARNING] " +  message);
        }

        /// <summary>
        /// Логирует ошибку. 
        /// </summary>
        public static void LogError(Exception ex)
        {
            TryWriteToFile("[ERROR] " + ex.StackTrace + " " + ex.Message);
        }

        /// <summary>
        /// Выводит элементы вектор в столбец. 
        /// </summary>
        public static void LogVector(IEnumerable vector) 
        { 
            foreach (var v in vector) 
            {
                TryWriteToFile(v.ToString(), false);
            }
        }

        #endregion Public Methods

        #region Constructors

        /// <summary>
        /// Статический конструктор, логирует сообщение в каждую новую сессию. 
        /// </summary>
        static LogService()
        {
            TryWriteToFile("NEW SESSION");
        }

        #endregion Constructors
    }
}
