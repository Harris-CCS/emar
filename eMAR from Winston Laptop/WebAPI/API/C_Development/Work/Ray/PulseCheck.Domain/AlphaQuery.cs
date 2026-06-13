using System.Configuration;
using System.Net.Configuration;
using System.Net.Mail;

namespace PulseCheck.Domain
{
    /// <summary>
    /// Alpha pager query object
    /// </summary>
    public class AlphaQuery : Query
    {
        /// <summary>
        /// Default empty constructor
        /// </summary>
        public AlphaQuery(Query copy) : base(copy)
        {
            Type = Constants.TYPE_ALPHA;
        }

        public override void Action()
        {
            if (string.IsNullOrWhiteSpace(Value))
                return;

            SmtpSection section = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
            var client = new SmtpClient
            {
                Port = section.Network.Port,
                Host = section.Network.Host,
                Credentials = new System.Net.NetworkCredential(section.Network.UserName, section.Network.Password),
                EnableSsl = section.Network.EnableSsl,
            };

            var mail = new MailMessage(section.From, Value)
            {
                Subject = "Contact ED: " + Order.Name,
                Body = "Contact ED: " + Order.Name,
            };

            client.Send(mail);
            client.Dispose();
        }
    }
}