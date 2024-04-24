using System;
using System.Net;
using System.Net.Mail;
using System.Security.Authentication;
using System.Threading;
using Artech.Architecture.Common.Services;
using Artech.MsBuild.Common;
using LSI.Packages.Extensiones.Utilidades;
using Microsoft.Build.Framework;

namespace LSI.Packages.Extensiones.MsBuild
{
    // TODO: What about TLS 1.2? May be required?...

    /// <summary>
    /// Send mails with the msbuild log result
    /// </summary>
    public class LsiSendEmail : ArtechTask
    {

        /// <summary>
        /// From email address
        /// </summary>
        public string FromEmail { get; set; }

        /// <summary>
        /// Destination email addresses
        /// </summary>
        public ITaskItem[] ToEmails { get; set; }

        /// <summary>
        /// Email server
        /// </summary>
        public string EmailHost { get; set; }

        /// <summary>
        /// Email user name
        /// </summary>
        public string EmailUserName { get; set; }

        /// <summary>
        /// Email password
        /// </summary>
        public string EmailPassword { get; set; }

        /// <summary>
        /// Use SSL?
        /// </summary>
        public bool Ssl { get; set; } = false;

        /// <summary>
        /// SMTP Port
        /// </summary>
        public int Port { get; set; } = 25;

        /// <summary>
        /// Connection timeout, in miliseconds
        /// </summary>
        public int TimeoutMiliseconds { get; set; } = 100000;

        /// <summary>
        /// Create a mail with the compilation process log
        /// </summary>
        /// <returns>The mail to send</returns>
        private MailMessage CreateEmail(Utilidades.Logging.Log log)
        {
            MailMessage msg = new MailMessage();
            msg.From = new MailAddress(FromEmail, "LSI.Extensions");
            foreach (ITaskItem to in ToEmails)
                msg.To.Add(new MailAddress(to.ItemSpec));

            msg.Subject = "[GxBuild] ";
            if (!MsBuildLog.Instance.ProcessOk)
                msg.Subject += "*** WITH ERRORS *** ";
            msg.Subject += "- " + this.KB.Name + " / " + this.KB.DesignModel.Environment.TargetModel.Name;

            msg.IsBodyHtml = true;

            msg.Body = Resources.CompilationEmailTemplate
                .Replace("%KBASEPATH%", KB.Location)
                .Replace("%ENVIRONMENTPATH%", Entorno.GetTargetDirectory(KB.DesignModel.Environment.TargetModel))
                .Replace("%ENVIROMENTDESCRIPTION%", KB.DesignModel.Environment.TargetModel.Name);

            // Format the logs:
            string compilationLogs = string.Empty;
            if (!MsBuildLog.Instance.ProcessOk)
                compilationLogs += "<p><b>WITH ERRORS</b></p>";

            msg.Body = msg.Body.Replace("%COMPILATIONLOG%", MsBuildLog.Instance.Html);

            return msg;
        }

        /// <summary>
        /// Run the task
        /// </summary>
        /// <returns>False if the msbuild script should be stopped</returns>
        public override bool Execute()
        {
            try
            {
                OutputSubscribe();

                if (LsiLogSectionStart.CurrentSection != null)
                    LsiLogSectionStart.CurrentSection.CloseCurrentSection();

                using (Utilidades.Logging.Log log = new Utilidades.Logging.Log())
                {
                    log.Output.AddLine("Sending email with tasks log");

                    // Check parameters:
                    if (string.IsNullOrEmpty(FromEmail) || ToEmails == null || ToEmails.Length == 0 ||
                        string.IsNullOrEmpty(EmailHost))
                    {
                        log.Output.AddLine("Wrong configuration (no To addresses, or no email host)");

                        MsBuildLog.Instance.Clear();
                        return true;
                    }

#if GX_15_OR_GREATER
                    // Starting .NET 4, SmtpClient implements IDisposable and should do the damn logout from mail server
                    using (var smtp = new SmtpClient(EmailHost))
                    {
#else
                        // NET 3.5 stuff:
                        SmtpClient smtp = new SmtpClient(EmailHost);

                        // Use TLS 1.2
                        // https://stackoverflow.com/questions/43240611/net-framework-3-5-and-tls-1-2
                        const SslProtocols _Tls12 = (SslProtocols)0x00000C00;
                        const SecurityProtocolType Tls12 = (SecurityProtocolType)_Tls12;
                        ServicePointManager.SecurityProtocol = Tls12;
#endif


                        smtp.EnableSsl = Ssl;
                        smtp.Port = Port;
                        smtp.Timeout = TimeoutMiliseconds;

                        if (!string.IsNullOrEmpty(EmailUserName) || !string.IsNullOrEmpty(EmailPassword))
                        {
                            smtp.UseDefaultCredentials = false;
                            smtp.Credentials = new NetworkCredential(EmailUserName, EmailPassword);
                        }

                        smtp.Send(CreateEmail(log));

                        // NET 3.5 workarrounds:
                        // La implementacion que trae .NET del correo no tiene soporte para desconectar la sesion....
                        // Esto fuerza la desconexion en breve. 
                        // Un bug conocido es que no manda el comando QUIT. Esta solventado en la version 4 de .NET
                        // En esa version, el SmtpClient implementa la interface IDisposable, asi que habria que llamar
                        // a dispose()
                        // Ver http://stackoverflow.com/questions/968506/optimal-way-to-send-mail-with-smtpclient
                        smtp.ServicePoint.MaxIdleTime = 2;
                        smtp.ServicePoint.ConnectionLimit = 1;
                        Thread.Sleep(10);

                        log.Output.AddLine("Email sent");

#if GX_15_OR_GREATER
                    } 
#endif
                }

                MsBuildLog.Instance.Clear();

                return true;
            }
            catch (Exception ex)
            {
                CommonServices.Output.AddErrorLine("Error sending email:");
                CommonServices.Output.AddErrorLine(ex.ToString());
                return true;
            }
            finally
            {
                OutputUnsubscribe();
            }
        }

    }
}
