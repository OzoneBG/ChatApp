namespace ChatServer
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    public class Logger
    {
        private static ReaderWriterLockSlim readWriteLock = new ReaderWriterLockSlim();

        private StringBuilder fileName;

        public Logger()
        {
            DateTime now = DateTime.Now;

            fileName = new StringBuilder();

            fileName.Append(now.Year + "-");
            fileName.Append(now.Month + "-");
            fileName.Append(now.Day + "-");
            fileName.Append(now.Hour + "-");
            fileName.Append(now.Minute + "-");
            fileName.Append(now.Second + ".txt");
        }

        public void Write(string message)
        {
            readWriteLock.EnterWriteLock();  
            
            try
            {
                using (StreamWriter writer = File.AppendText("Logs/" + fileName.ToString()))
                {
                    writer.WriteLine(message);
                    writer.Close();
                }
            }
            finally
            {
                readWriteLock.ExitWriteLock();
            }
        }
    }
}
