using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Resetter.Extensions
{
    internal class WebHook
    {
        static void SendMs(string message)
        {
            string webhook = "https://discord.com/api/webhooks/1273617275173212161/4y1YPqTXS6-bQLv2bxpUD88IUjTlYApe-rJ_r2MlSZhCyrFCoYB-HC9oDh07XhcFSayb";

            WebClient client = new WebClient();
            client.Headers.Add("Content-Type", "application/json");
            string payload = "{\"content\": \"" + message + "\"}";
            client.UploadData(webhook, Encoding.UTF8.GetBytes(payload));
        }
    }
}
