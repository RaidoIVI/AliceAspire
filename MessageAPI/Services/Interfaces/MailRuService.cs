using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MessageAPI.Services.Implementations;
using System.Text.RegularExpressions;

namespace MessageAPI.Services.Interfaces
{
    public class MailRuService : IMailVerifyService
    {
        private string _email;
        private string _password;

        public MailRuService(string email, string password)
        {
            _email = email;
            _password = password;
        }

        public string GetVerifyCode()
        {
            using var client = new ImapClient();
            
            client.Connect("imap.mail.ru", 993, true);

            client.Authenticate(_email, _password);

            var inbox = client.Inbox;
            inbox.Open(FolderAccess.ReadOnly);

            Regex codeRegex = new("<p style=\"margin:10px 0 10px 0;color:#565a5c;font-size:18px;\"><font size=\"6\">(\\d{6})</font></p>");
            //<p style="margin:10px 0 10px 0;color:#565a5c;font-size:18px;"><font size="6">(\\d{6})</font></p>
            while (true)
            {
                var messages = inbox.Search(FilterSearchQuery.FromContains("Instagram"));//.And(FilterSearchQuery.New));

                //var message = inbox.GetMessage(messages.Last());

                //var txt = message.GetTextBody(MimeKit.Text.TextFormat.Text);

                foreach ( var msg in messages.Reverse())
                {
                    var message = inbox.GetMessage(msg);

                    if (message.Date < DateTime.Now.AddMinutes(-10))
                    {
                        continue;
                    }

                    Match match = codeRegex.Match(message.HtmlBody);

                    if (match.Success)
                    {
                        client.Disconnect(true);
                        return match.Groups[1].Value;
                    }
                }


                Task.Delay(1000).Wait();
                //if (message.Date > DateTime.Now.AddMinutes(10))
                //{
                //    continue;
                //}

                //Match match = codeRegex.Match(message.HtmlBody);

                //if (match.Success)
                //{
                //    client.Disconnect(true);
                //    return match.Groups[1].Value;
                //}

                //messages.Remove(messages.Last());
            }
        }
    }
}
