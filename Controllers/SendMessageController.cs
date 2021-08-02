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

        [HttpPost("SendMessageZalo")]
        public JObject SendMessage(JObject model)
        {
            _webClient = new WebClient();
            _webClient.Encoding = System.Text.Encoding.UTF8;
            _webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            _webClient.Headers.Add(HttpRequestHeader.Accept, "application/json");

            string msg = model["msg"].ToString();
            JArray listUserId = (JArray)model["listUserId"];
            //https://oauth.zaloapp.com/v3/permission?app_id=3139874215511393396&redirect_uri=https://zxcv.com&state=zxcv 
            long appId = long.Parse(Configuration.GetSection("SendMessageZalo:appId").Value);
            string secretKey = Configuration.GetSection("SendMessageZalo:secretKey").Value;
            string callbackUrl = Configuration.GetSection("SendMessageZalo:callbackUrl").Value;
            string code = Configuration.GetSection("SendMessageZalo:code").Value;
            ZaloAppInfo appInfo = new ZaloAppInfo(appId, secretKey, callbackUrl);
            ZaloAppClient appClient = new ZaloAppClient(appInfo);
            String loginUrl = appClient.getLoginUrl();
            JObject token = appClient.getAccessToken(code);
            string access_token = token["access_token"].ToString();
            JObject profile = appClient.getProfile(access_token, "id, name, birthday");
            string friendsUrls = "https://graph.zalo.me/v2.0/me/friends?access_token="+access_token+"&offset=0&limit=5&fields=id,name,gender,picture";
            string Invitable_friendsUrls = "https://graph.zalo.me/v2.0/me/invitable_friends?access_token=" + access_token + "&offset=0&limit=100&fields=id,name,gender,picture";
            string ApprequestUrl = "https://graph.zalo.me/v2.0/apprequests?access_token=" + access_token + "&message="+msg+"&to="+listUserId;
            
            //Object Invitable_friends = _webClient.DownloadString(Invitable_friendsUrls);
            //Object friends = _webClient.DownloadString(friendsUrls);
            //Object Apprequest_Result = _webClient.UploadString(ApprequestUrl,"");
            JArray result = new JArray();
            //------ Gửi tin nhắn
            foreach (string UserId in listUserId){
                string SendMessageUrls = "https://graph.zalo.me/v2.0/me/message?access_token="+ access_token + "&message="+msg+"&to="+UserId;
                Object SendMessage = _webClient.UploadString(SendMessageUrls,"");
                result.Add(SendMessage);
            }
            //------
            JObject res = new JObject();
            res.Add("ok", true);
            res.Add("data", result.ToString());
            return res;
        }
    }
}
