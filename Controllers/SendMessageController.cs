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
using System.Data.SqlClient;

namespace SendMessageZalo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SendMessageController : Controller
    {
        private WebClient _webClient = null;
        public IConfiguration Configuration { get; }
        public SendMessageController(IConfiguration configuration) {
            Configuration = configuration;
        }

        [HttpGet("GetListFollower")]
        public JObject GetListFollower()
        {
            _webClient = new WebClient();
            _webClient.Encoding = System.Text.Encoding.UTF8;
            _webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            _webClient.Headers.Add(HttpRequestHeader.Accept, "application/json");
            _webClient.Headers.Add("access_token:" + Configuration.GetSection("SendMessageZalo:access_token").Value);
            string ListFollowerUrl = @"https://openapi.zalo.me/v2.0/oa/getfollowers?data={'offset':0,'count':'50'}";
            JObject result = JObject.Parse(_webClient.DownloadString(ListFollowerUrl));
            JObject res = new JObject();
            res.Add("ok", true);
            res.Add("data", result);
            return res;
        }

        [HttpGet("GetListPost")]
        public JObject GetListPost()
        {
            _webClient = new WebClient();
            _webClient.Encoding = System.Text.Encoding.UTF8;
            _webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            _webClient.Headers.Add(HttpRequestHeader.Accept, "application/json");
            _webClient.Headers.Add("access_token:" + Configuration.GetSection("SendMessageZalo:access_token").Value);
            string GetListPostUrl = "https://openapi.zalo.me/v2.0/article/getslice?access_token=" + Configuration.GetSection("SendMessageZalo:access_token").Value + "&offset=0&limit=10&type=normal";
            JObject GetListPostResult = JObject.Parse(_webClient.DownloadString(GetListPostUrl));
            JObject res = new JObject();
            res.Add("ok", true);
            res.Add("data", GetListPostResult);
            return res;
        }

        [HttpPost("GetPostByToken")]
        public JObject GetPostByToken(JObject model)
        {
            _webClient = new WebClient();
            _webClient.Encoding = System.Text.Encoding.UTF8;
            _webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            _webClient.Headers.Add(HttpRequestHeader.Accept, "application/json");
            _webClient.Headers.Add("access_token:" + Configuration.GetSection("SendMessageZalo:access_token").Value);
            JObject data = new JObject();
            data.Add("token",model["token"].ToString());
            string GetListPostUrl = "https://openapi.zalo.me/v2.0/article/getslice?access_token=" + Configuration.GetSection("SendMessageZalo:access_token").Value + "&offset=0&limit=10&type=normal";
            JObject GetListPostResult = JObject.Parse(_webClient.DownloadString(GetListPostUrl));
            JObject res = new JObject();
            res.Add("ok", true);
            res.Add("data", GetListPostResult);
            return res;
        }

        [HttpPost("GetFollower")]
        public JObject GetFollower(JObject model)
        {
            _webClient = new WebClient();
            _webClient.Encoding = System.Text.Encoding.UTF8;
            _webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            _webClient.Headers.Add(HttpRequestHeader.Accept, "application/json");
            _webClient.Headers.Add("access_token:" + Configuration.GetSection("SendMessageZalo:access_token").Value);
            string ListFollowerUrl = @"https://openapi.zalo.me/v2.0/oa/getprofile?data={'user_id':"+model["UserId"].ToString()+"}";
            string result = _webClient.DownloadString(ListFollowerUrl);
            JObject res = new JObject();
            res.Add("ok", true);
            res.Add("data", JObject.Parse(result));
            return res;
        }

        [HttpPost("CreatePost")]
        public JObject CreatePost(JObject model)
        {
            _webClient = new WebClient();
            _webClient.Encoding = System.Text.Encoding.UTF8;
            _webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            _webClient.Headers.Add(HttpRequestHeader.Accept, "application/json");
            _webClient.Headers.Add("access_token:" + Configuration.GetSection("SendMessageZalo:access_token").Value);
            JObject res = new JObject();
            JObject cover = new JObject();
            JArray body = new JArray();
            JObject data = new JObject();
            cover.Add("cover_type","photo");
            cover.Add("status","show");
            cover.Add("photo_url",model["photo_url"]);
            data.Add("type","normal");
            data.Add("title",model["title"].ToString());
            data.Add("author",model["author"].ToString());
            data.Add("cover",cover);
            data.Add("description",model["description"].ToString());
            data.Add("body",model["body"]);
            data.Add("status","show");
            data.Add("comment","show");
            string CreatePostUrl = "https://openapi.zalo.me/v2.0/article/create?access_token="+Configuration.GetSection("SendMessageZalo:access_token").Value;
            string CreatePost = _webClient.UploadString(CreatePostUrl,data.ToString());
            JObject CreatePostResult = JObject.Parse(CreatePost);
            if (CreatePostResult["error"].ToString() != "0")
            {
                res.Add("ok",false);
            } else
            {
                res.Add("ok",true);
            }
            res.Add("data",CreatePostResult);
            return res;
        }

        [HttpPost("SendBoadcast")]
        public JObject SendBoadcast(JObject model)
        {
            _webClient = new WebClient();
            _webClient.Encoding = System.Text.Encoding.UTF8;
            _webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            _webClient.Headers.Add(HttpRequestHeader.Accept, "application/json");
            _webClient.Headers.Add("access_token:" + Configuration.GetSection("SendMessageZalo:access_token").Value);
            JObject res = new JObject();
            string stringdata = @"{'recipient': {
                                            'target': {
                                                    'gender': '0'
                                                }
                                            },
                                            'message': {
                                                'attachment': {
                                                    'type': 'template',
                                                    'payload': {
                                                        'template_type': 'media',
                                                        'elements': [
                                                                {
                                                                    'media_type': 'article',
                                                                    'attachment_id': '"+model["attachment_id"].ToString() +  
                                                                @"'}
                                                            ]
                                                        }
                                                    }
                                                }
                                            }";
            JObject data = JObject.Parse(stringdata);
            string SendBoadcasttUrl = "https://openapi.zalo.me/v2.0/oa/message";
            JObject result = JObject.Parse(_webClient.UploadString(SendBoadcasttUrl,data.ToString()));
            if (result["error"].ToString() != "0")
            {
                res.Add("ok",false);
            } else
            {
                res.Add("ok",true);
            }
            res.Add("data",result);
            return res;
        }

        [HttpPost("SendList")]
        public JObject SendList(JObject model)
        {
            _webClient = new WebClient();
            _webClient.Encoding = System.Text.Encoding.UTF8;
            _webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            _webClient.Headers.Add(HttpRequestHeader.Accept, "application/json");
            _webClient.Headers.Add("access_token:" + Configuration.GetSection("SendMessageZalo:access_token").Value);
            JObject res = new JObject();
            JArray result = new JArray();
            JArray listPhoneNumber = (JArray)model["listPhoneNumber"];
            foreach (string PhoneNumber in listPhoneNumber){
                string Phone;
                if (PhoneNumber.StartsWith("0") == true)
                {
                    Phone = "84" + PhoneNumber.Substring(1);
                } else
                {
                    Phone = PhoneNumber; 
                }
                var connection = new SqlConnection(Configuration.GetSection("ConnectionStrings:MainConnection").Value);
                connection.Open();
                using var command = new SqlCommand();
                command.Connection = connection;
                string queryString = @"SELECT UserId FROM Registers WHERE PhoneNumber=@PhoneNumber";
                command.CommandText = queryString;
                command.Parameters.AddWithValue("@PhoneNumber", Phone);
                using var reader = command.ExecuteReader();
                if (reader.HasRows) {
                    while (reader.Read())
                    {
                        JObject data = new JObject();
                        JObject user_id = new JObject();
                        JObject message = new JObject();
                        JObject attachment = new JObject();
                        JObject payload = new JObject();
                        user_id.Add("user_id",String.Format("{0}", reader[0]));
                        message.Add("attachment",attachment);
                        payload.Add("template_type","list");
                        payload.Add("elements",model["elements"]);
                        attachment.Add("type","template");
                        attachment.Add("payload",payload);
                        data.Add("recipient",user_id);
                        data.Add("message",message);
                        string SendListUrl = "https://openapi.zalo.me/v2.0/oa/message";
                        string SendList = _webClient.UploadString(SendListUrl,data.ToString());
                        JObject json = JObject.Parse(SendList);
                        if (json["error"].ToString() != "0")
                        {
                            JObject err = new JObject();
                            err.Add("ok", false);
                            err.Add("PhoneNumber",PhoneNumber);
                            err.Add("error",json);
                            result.Add(err);
                        } else
                        {
                            JObject success = new JObject();
                            success.Add("ok", true);
                            success.Add("PhoneNumber",PhoneNumber);
                            success.Add("status",json);
                            result.Add(success);
                        }
                    }
                }
                else {
                    JObject err = new JObject();
                    err.Add("ok", false);
                    err.Add("PhoneNumber",PhoneNumber);
                    err.Add("error","số điện thoại không tồn tại trên hệ thống");
                    result.Add(err);
                }
                connection.Close();
            }
            res.Add("data", result);
            return res;
        }

        [HttpPost("SendMessageUserId")]
        public JObject SendMessageUserId(JObject model)
        {
            _webClient = new WebClient();
            _webClient.Encoding = System.Text.Encoding.UTF8;
            _webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            _webClient.Headers.Add(HttpRequestHeader.Accept, "application/json");
            _webClient.Headers.Add("access_token:" + Configuration.GetSection("SendMessageZalo:access_token").Value);
            string msg = model["msg"].ToString();
            JArray listUserId = (JArray)model["listUserId"];
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
                JObject SendMessage = JObject.Parse(_webClient.UploadString(SendMessageUrls,data.ToString()));
                result.Add(SendMessage);
            }
            //------
            JObject res = new JObject();
            res.Add("ok", true);
            res.Add("data", result);
            return res;
        }

        [HttpPost("SendMessagePhoneNumber")]
        public JObject SendMessagePhoneNumber(JObject model)
        {
            _webClient = new WebClient();
            _webClient.Encoding = System.Text.Encoding.UTF8;
            _webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            _webClient.Headers.Add(HttpRequestHeader.Accept, "application/json");
            string msg = model["msg"].ToString();
            JArray listPhoneNumber = (JArray)model["listPhoneNumber"];
            _webClient.Headers.Add("access_token:" + Configuration.GetSection("SendMessageZalo:access_token").Value);
            JArray result = new JArray();
            //------ Gửi danh sách
            foreach (string PhoneNumber in listPhoneNumber){
                string Phone;
                if (PhoneNumber.StartsWith("0") == true)
                {
                    Phone = "84" + PhoneNumber.Substring(1);
                } else
                {
                    Phone = PhoneNumber;
                }
                var connection = new SqlConnection(Configuration.GetSection("ConnectionStrings:MainConnection").Value);
                connection.Open();
                using var command = new SqlCommand();
                command.Connection = connection;
                string queryString = @"SELECT UserId FROM Registers WHERE PhoneNumber=@PhoneNumber";
                command.CommandText = queryString;
                command.Parameters.AddWithValue("@PhoneNumber", Phone);
                using var reader = command.ExecuteReader();
                if (reader.HasRows) {
                    while (reader.Read())
                    {
                        JObject data = new JObject();
                        JObject user_id = new JObject();
                        JObject message = new JObject();
                        user_id.Add("user_id",String.Format("{0}", reader[0]));
                        message.Add("text",model["msg"].ToString());
                        data.Add("recipient",user_id);
                        data.Add("message",message);
                        string SendMessageUrls = "https://openapi.zalo.me/v2.0/oa/message";
                        string SendMessage = _webClient.UploadString(SendMessageUrls,data.ToString());
                        JObject json = JObject.Parse(SendMessage);
                        if (json["error"].ToString() != "0")
                        {
                            JObject err = new JObject();
                            err.Add("ok", false);
                            err.Add("PhoneNumber",PhoneNumber);
                            err.Add("error",json);
                            result.Add(err);
                        } else
                        {
                            JObject success = new JObject();
                            success.Add("ok", true);
                            success.Add("PhoneNumber",PhoneNumber);
                            success.Add("status",json);
                            result.Add(success);
                        }
                    }
                }
                else {
                    JObject err = new JObject();
                    err.Add("ok", false);
                    err.Add("PhoneNumber",PhoneNumber);
                    err.Add("error","số điện thoại không tồn tại trên hệ thống");
                    result.Add(err);
                }
                connection.Close();
            }
            //-----
            JObject res = new JObject();
            res.Add("data", result);
            return res;
        }

        [HttpPost("SendAllMessage")]
        public JObject SendAllMessage(JObject model)
        {
            _webClient = new WebClient();
            _webClient.Encoding = System.Text.Encoding.UTF8;
            _webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            _webClient.Headers.Add(HttpRequestHeader.Accept, "application/json");
            _webClient.Headers.Add("access_token:" + Configuration.GetSection("SendMessageZalo:access_token").Value);
            JArray result = new JArray();
            var connection = new SqlConnection(Configuration.GetSection("ConnectionStrings:MainConnection").Value);
            connection.Open();
            using var command = new SqlCommand();
            command.Connection = connection;
            string queryString = @"SELECT UserId FROM Registers";
            command.CommandText = queryString;
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                JObject data = new JObject();
                JObject user_id = new JObject();
                JObject message = new JObject();
                user_id.Add("user_id",String.Format("{0}", reader[0]));
                message.Add("text",model["msg"].ToString());
                data.Add("recipient",user_id);
                data.Add("message",message);
                string SendMessageUrls = "https://openapi.zalo.me/v2.0/oa/message";
                Object SendMessage = _webClient.UploadString(SendMessageUrls,data.ToString());
                result.Add(SendMessage);
            }
            connection.Close();
            JObject res = new JObject();
            res.Add("ok", true);
            res.Add("data", result.ToString());
            return res;
        }

        [HttpPost("ZaloManager")]
        public JObject ZaloManager(JObject model){ 
            JObject res = new JObject();
            if (model["event_name"] != null)
            {
                string event_name = model["event_name"].ToString();
                switch (event_name)
                {
                    case "user_send_text":
                        res = ReceiveMessage(model);
                        break;
                    case "follow":
                        res = Follow(model);
                        break;
                    case "unfollow":
                        res = UnFollow(model);
                        break;
                    case "user_submit_info":
                        res = SaveUserInfo(model);
                        break;
                    default:
                        res.Add("ok",true);
                        break;
                }
            } 
            else {
                res.Add("event_name",null);
            }
            return res;
        }

        private JObject Reply(string msg, string msgId){
            JObject data = new JObject();
            JObject message_id = new JObject();
            JObject text = new JObject();
            message_id.Add("message_id",msgId);
            text.Add("text",msg);
            data.Add("recipient",message_id);
            data.Add("message",text);
            string SendMessageUrls = "https://openapi.zalo.me/v2.0/oa/message";
            _webClient = new WebClient();
            _webClient.Encoding = System.Text.Encoding.UTF8;
            _webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            _webClient.Headers.Add(HttpRequestHeader.Accept, "application/json");
            _webClient.Headers.Add("access_token:" + Configuration.GetSection("SendMessageZalo:access_token").Value);
            string ReplyMessage = _webClient.UploadString(SendMessageUrls,data.ToString());
            JObject res = new JObject();
            res.Add("msg",msg);
            res.Add("status",JObject.Parse(ReplyMessage));
            return  res;
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
            string key = msg;
            if (msg.Split(' ')[0].ToLower() == "dk")
            {
                key = msg.Split(' ')[0];
            } else
            {
                key = msg;
            }
            //string content = msg.Substring(key.Length);
            switch (key.ToLower())
            {
                case "dk":
                    if (status != "0")
                        {
                            res.Add("error", "Bạn hãy follow chúng tôi để sử dụng chức năng này");
                            return res;
                        }
                    res = Register(model);
                    return res;
                case "tc":
                    res.Add("reply",Reply("Tiền mặt trong quỹ của bạn còn: 1.000.000",model["message"]["msg_id"].ToString()));
                    return res;
                case "hỗ trợ" or "trợ giúp" or "cần hỗ trợ" or "cần trợ giúp" or "yêu cầu hỗ trợ" or "tôi cần hỗ trợ" or "tôi cần trợ giúp" or "tôi muốn hỗ trợ" or "#hotro":
                    if (status != "0")
                    {
                        res.Add("ok",false);
                        res.Add("error", "Bạn hãy follow chúng tôi để sử dụng chức năng này");
                        return res;
                    }
                    if (profile["data"]["shared_info"] == null)
                    {
                        res.Add("reply",Reply("Bạn chưa bổ sung thông tin, vui lòng bổ sung thông tin rồi gửi lại hỗ trợ",model["message"]["msg_id"].ToString()));
                        RequestUserInfo(user_send_id);
                        return res;
                    }
                    res.Add("reply",Reply("Yêu cầu hỗ trợ của bạn đã được gửi tới bộ phận tiếp nhận, xin vui lòng chờ trong giây lát",model["message"]["msg_id"].ToString()));
                    JObject modelSendMessage = new JObject();
                    JArray listPhoneNumber = new JArray();
                    var connection = new SqlConnection(Configuration.GetSection("ConnectionStrings:MainConnection").Value);
                    connection.Open();
                    var command = new SqlCommand();
                    command.Connection = connection;
                    string queryString = @"Select PhoneNumber from SupportPhoneNumbers where status = 1";
                    command.CommandText = queryString;
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        listPhoneNumber.Add(String.Format("{0}", reader[0]));
                    }
                    connection.Close();
                    string phoneNumber = "";
                    //if (profile["data"]["shared_info"] != null)
                    phoneNumber ="có số điện thoại " + "0" +  profile["data"]["shared_info"]["phone"].ToString().Substring(2);
                    modelSendMessage.Add("msg","Khách hàng " + profile["data"]["display_name"].ToString() + " " + phoneNumber + " đang yêu cầu hỗ trợ");
                    modelSendMessage.Add("listPhoneNumber",listPhoneNumber);
                    res.Add("sendMessage",SendMessagePhoneNumber(modelSendMessage));
                    return res;
                case "bstt":
                    res = RequestUserInfo(user_send_id);
                    return res;
                default:
                    break;
            }
            return res;
        }

        private JObject Register(JObject model){
            var connection = new SqlConnection(Configuration.GetSection("ConnectionStrings:MainConnection").Value);
            string msg = model["message"]["text"].ToString();
            string UserId = model["sender"]["id"].ToString();
            bool check = CheckRegister(UserId);
            JObject res = new JObject();
            connection.Open();
            using var command = new SqlCommand();
            command.Connection = connection;
            string queryString = @"UPDATE Registers set Link = @Link where UserId = @UserId";
            command.CommandText = queryString;
            command.Parameters.AddWithValue("@Link", msg.Split(' ')[1]);
            command.Parameters.AddWithValue("@UserId", UserId);
            var rows_affected = command.ExecuteNonQuery();
            connection.Close();
            res.Add("ok", true);
            res.Add("Status","Đăng ký thành công");
            Reply("Đăng ký thành công",model["message"]["msg_id"].ToString());
            return res;
        }

        private bool CheckRegister(string UserId){
            bool check = false;
            var connection = new SqlConnection(Configuration.GetSection("ConnectionStrings:MainConnection").Value);
            connection.Open();
            using var command = new SqlCommand();
            command.Connection = connection;
            string queryString = @"SELECT TOP 1 UserId FROM Registers WHERE UserId=@UserId";
            command.CommandText = queryString;
            command.Parameters.AddWithValue("@UserId", UserId);
            using var reader = command.ExecuteReader();
            if (reader.HasRows) check = true;
            connection.Close();
            return check;
        }

        private bool CheckProfileUser(string UserId){
            bool check = false;
            var connection = new SqlConnection(Configuration.GetSection("ConnectionStrings:MainConnection").Value);
            connection.Open();
            using var command = new SqlCommand();
            command.Connection = connection;
            string queryString = @"SELECT TOP 1 UserId FROM Registers WHERE UserId=@UserId and PhoneNumber <> ''";
            command.CommandText = queryString;
            command.Parameters.AddWithValue("@UserId", UserId);
            using var reader = command.ExecuteReader();
            if (reader.HasRows) check = true;
            connection.Close();
            return check;
        }

        private JObject Follow(JObject model){
            var connection = new SqlConnection(Configuration.GetSection("ConnectionStrings:MainConnection").Value);
            string UserId = model["follower"]["id"].ToString();
            JObject res = new JObject();
            connection.Open();
            using var command = new SqlCommand();
            command.Connection = connection;
            string queryString = @"INSERT INTO Followers (id, UserId, DATE_NEW) VALUES (@id, @UserId, @DATE_NEW)";
            command.CommandText = queryString;
            command.Parameters.AddWithValue("@id", Guid.NewGuid());
            command.Parameters.AddWithValue("@UserId", UserId);
            command.Parameters.AddWithValue("@DATE_NEW", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
            var rows_affected = command.ExecuteNonQuery();
            connection.Close();
            bool check = CheckRegister(UserId);
            if (check!=true)
            {
                connection.Open();
                var commandRG = new SqlCommand();
                commandRG.Connection = connection;
                var queryRegister = @"INSERT INTO Registers (id, UserId, Link, DATE_NEW, Status) VALUES (@id, @UserId, @Link, @DATE_NEW, @Status)";
                commandRG.CommandText = queryRegister;
                commandRG.Parameters.AddWithValue("@id", Guid.NewGuid());
                commandRG.Parameters.AddWithValue("@UserId", UserId);
                commandRG.Parameters.AddWithValue("@Link", "");
                commandRG.Parameters.AddWithValue("@DATE_NEW", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
                commandRG.Parameters.AddWithValue("@Status", 1);
                var rows = commandRG.ExecuteNonQuery();
                connection.Close();
                res.Add("ok", true);
                JObject sendmessage = new JObject();
                sendmessage.Add("msg","Bạn hãy bổ sung thông tin để được chăm sóc tốt hơn nhé");
                JArray listUserId = new JArray();
                listUserId.Add(UserId);
                sendmessage.Add("listUserId",listUserId);
                res.Add("SendMessage",SendMessageUserId(sendmessage));
                RequestUserInfo(UserId);
                return res;
            } else
            {
                // Nếu đã đăng kí thì đổi trạng thái hoạt động
                connection.Open();
                var commandRG = new SqlCommand();
                commandRG.Connection = connection;
                var queryRegister = @"UPDATE Registers set Status = 1 where UserId = @UserId";
                commandRG.Parameters.AddWithValue("@UserId", UserId);
                commandRG.CommandText = queryRegister;
                var rows = commandRG.ExecuteNonQuery();
                connection.Close();
            }
            res.Add("ok", true);
            res.Add("Status","Follow thành công");
            return res;
        }

        private JObject UnFollow(JObject model){
            var connection = new SqlConnection(Configuration.GetSection("ConnectionStrings:MainConnection").Value);
            string UserId = model["follower"]["id"].ToString();
            JObject res = new JObject();
            connection.Open();
            // Xóa đối tượng Followers
            using var command = new SqlCommand();
            command.Connection = connection;
            string queryString = @"DELETE FROM Followers WHERE UserId = @UserId";
            command.CommandText = queryString;
            command.Parameters.AddWithValue("@UserId", UserId);
            var rows_affected = command.ExecuteNonQuery();

            // Cập nhật trạng thái tắt hoạt động của Registers
            var commandRG = new SqlCommand();
            commandRG.Connection = connection;
            var queryRegister = @"UPDATE Registers set Status = 0 where UserId = @UserId";
            commandRG.Parameters.AddWithValue("@UserId", UserId);
            commandRG.CommandText = queryRegister;
            var rowRG = commandRG.ExecuteNonQuery();
            connection.Close();
            res.Add("ok", true);
            res.Add("Status","Bỏ theo dõi thành công");
            return res;
        }

        private JObject RequestUserInfo(string userId){
            _webClient = new WebClient();
            _webClient.Encoding = System.Text.Encoding.UTF8;
            _webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            _webClient.Headers.Add(HttpRequestHeader.Accept, "application/json");
            _webClient.Headers.Add("access_token:" + Configuration.GetSection("SendMessageZalo:access_token").Value);
            JObject recipient = new JObject();
            JObject message = new JObject();
            JObject attachment = new JObject();
            JObject payload = new JObject();
            JObject template_type = new JObject();
            JObject elements = new JObject();
            JArray arrElements = new JArray();
            elements.Add("title","Công ty Cổ Phần Vacom");
            elements.Add("subtitle","Đang yêu cầu thông tin từ bạn");
            elements.Add("image_url","https://oa-wg-stc.zdn.vn/oa/customer/images/fillform.jpg");
            arrElements.Add(elements);
            payload.Add("template_type","request_user_info");
            payload.Add("elements",arrElements);
            attachment.Add("type","template");
            attachment.Add("payload",payload);
            recipient.Add("user_id",userId);
            message.Add("attachment",attachment);
            JObject data = new JObject();
            data.Add("recipient",recipient);
            data.Add("message",message);
            string Urls = "https://openapi.zalo.me/v2.0/oa/message";
            string RequestUserInfo = _webClient.UploadString(Urls,data.ToString()); 
            JObject res = new JObject();
            res.Add("status",RequestUserInfo);
            return res;
        }

        private JObject SaveUserInfo(JObject model){
            var connection = new SqlConnection(Configuration.GetSection("ConnectionStrings:MainConnection").Value);
            bool check = CheckRegister(model["sender"]["id"].ToString());
            JObject res = new JObject();
            JArray arr = new JArray();
            JObject modelSenMessage = new JObject();
            if (check)
            {
                connection.Open();
                // Tạo đối tượng SqlCommand
                using var command = new SqlCommand();
                command.Connection = connection;
                string queryString = @"UPDATE Registers
                                    SET  Address = @Address, PhoneNumber = @PhoneNumber, District = @District, Name = @Name, 
                                            Ward = @Ward, City = @City
                                    WHERE UserId=@UserId";
                command.CommandText = queryString;
                command.Parameters.AddWithValue("@Address", model["info"]["address"].ToString());
                command.Parameters.AddWithValue("@PhoneNumber", model["info"]["phone"].ToString());
                command.Parameters.AddWithValue("@District", model["info"]["district"].ToString());
                command.Parameters.AddWithValue("@Name", model["info"]["name"].ToString());
                command.Parameters.AddWithValue("@Ward", model["info"]["ward"].ToString());
                command.Parameters.AddWithValue("@City", model["info"]["city"].ToString());
                command.Parameters.AddWithValue("@UserId", model["sender"]["id"].ToString());
                var rows_affected = command.ExecuteNonQuery();      
                connection.Close();
            } else {
                connection.Open();
                // Tạo đối tượng SqlCommand
                using var command = new SqlCommand();
                command.Connection = connection;
                string queryString = @"INSERT INTO Registers (id, UserId, Link, DATE_NEW, Status, Address, PhoneNumber, District, Name, Ward, City) 
                                                      VALUES (@id, @UserId, '', @DATE_NEW, 1, @Address, @PhoneNumber, @District, @Name, @Ward, @City)";
                command.CommandText = queryString;
                command.Parameters.AddWithValue("@id", Guid.NewGuid());
                command.Parameters.AddWithValue("@UserId", model["sender"]["id"].ToString());
                command.Parameters.AddWithValue("@DATE_NEW", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
                command.Parameters.AddWithValue("@Address", model["info"]["address"].ToString());
                command.Parameters.AddWithValue("@PhoneNumber", model["info"]["phone"].ToString());
                command.Parameters.AddWithValue("@District", model["info"]["district"].ToString());
                command.Parameters.AddWithValue("@Name", model["info"]["name"].ToString());
                command.Parameters.AddWithValue("@Ward", model["info"]["ward"].ToString());
                command.Parameters.AddWithValue("@City", model["info"]["city"].ToString());
                var rows_affected = command.ExecuteNonQuery();   
            }
            modelSenMessage.Add("msg","Bạn đã bổ sung thông tin thành công");
            arr.Add(model["sender"]["id"].ToString());
            modelSenMessage.Add("listUserId",arr);
            res = SendMessageUserId(modelSenMessage);
            return res;
        }
    } 
}
