using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ZaloDotNetSDK;
using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace SendMessageZalo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SendMessageController : ControllerBase
    {
        private WebClient _webClient = null;
        public IConfiguration Configuration { get; }
        public SendMessageController(IConfiguration configuration) {
            Configuration = configuration;
        }

        [HttpPost("SendMessage")]
        public JObject SendMessage(JObject model)
        {
            _webClient = new WebClient();
            _webClient.Encoding = System.Text.Encoding.UTF8;
            _webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            _webClient.Headers.Add(HttpRequestHeader.Accept, "application/json");
            string msg = model["msg"].ToString();
            JArray listUserId = (JArray)model["listUserId"];
            //https://oauth.zaloapp.com/v3/oa/permission?app_id=3139874215511393396&redirect_uri=https://vacom.com.vn
            _webClient.Headers.Add("access_token:" + Configuration.GetSection("SendMessageZalo:access_token").Value);
            JArray result = new JArray();
            //------ Gửi tin nhắn
            foreach (string UserId in listUserId){
                JObject data = new JObject();
                JObject user_id = new JObject();
                JObject message = new JObject();
                user_id.Add("user_id",UserId);
                message.Add("text",model["msg"].ToString());
                data.Add("recipient",user_id);
                data.Add("message",message);
                string SendMessageUrls = "https://openapi.zalo.me/v2.0/oa/message";
                Object SendMessage = _webClient.UploadString(SendMessageUrls,data.ToString());
                result.Add(SendMessage);
            }
            //------
            JObject res = new JObject();
            res.Add("ok", true);
            res.Add("data", result.ToString());
            return res;
        }

        [HttpPost("ZaloManager")]
        public JObject ZaloManager(JObject model){
            JObject res = new JObject();
            string event_name = model["event_name"].ToString();
            switch (event_name)
            {
                case "user_send_text":
                    res = ReceiveMessage(model);
                    break;
                default:
                    break;
            }
            return res;
        }

        private JObject ReceiveMessage(JObject model) {
            _webClient = new WebClient();
            _webClient.Encoding = System.Text.Encoding.UTF8;
            _webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            _webClient.Headers.Add(HttpRequestHeader.Accept, "application/json");
            _webClient.Headers.Add("access_token:" + Configuration.GetSection("SendMessageZalo:access_token").Value);
            string user_send_id = model["sender"]["id"].ToString();
            string getprofileURL = @"https://openapi.zalo.me/v2.0/oa/getprofile?data={'user_id':'"+user_send_id+@"'}";
            JObject profile = JObject.Parse(_webClient.DownloadString(getprofileURL));
            string status = profile["error"].ToString();
            JObject res = new JObject();
            string msg = model["message"]["text"].ToString();
            string dangky = msg.Split(' ')[0];
            int i = 0;
            switch (dangky)
            {
                case "dkhocphi":
                    i = 1;
                    break;
                case "dkketoan":
                    i = 2;
                    break;
                default:
                    i = 0;
                    break;
            }
            
            // if (status != "0" && i!=0)
            // {
            //     res.Add("error", "Bạn hãy follow chúng tôi để sử dụng chức năng này");
            //     return res;
            // }

            res.Add("ok", true);
            res.Add("data",msg);
            return res;
        }
    }
}
