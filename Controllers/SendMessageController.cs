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
    public class SendMessageController : ControllerBase
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
            string result = _webClient.DownloadString(ListFollowerUrl);
            JObject res = new JObject();
            res.Add("ok", true);
            res.Add("data", result.ToString());
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
            res.Add("data", result.ToString());
            return res;
        }

        [HttpPost("test")]
        public JObject test(JObject model)
        {
            JObject res = new JObject();
            res = RequestUserInfo(model["UserId"].ToString());
            return res;
        }

        [HttpPost("SendMessageUserId")]
        public JObject SendMessageUserId(JObject model)
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
            //-----
            JObject res = new JObject();
            res.Add("ok", true);
            res.Add("data", result.ToString());
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
            //------ Gửi tin nhắn
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
            }
            //-----
            JObject res = new JObject();
            res.Add("ok", true);
            res.Add("data", result.ToString());
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

        private string Reply(string msg, string msgId){
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
            return msg + ReplyMessage;
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
            string key = msg.Split(' ')[0];
            string content = msg.Substring(key.Length);
            switch (key)
            {
                case "DK":
                    if (status != "0")
                        {
                            res.Add("error", "Bạn hãy follow chúng tôi để sử dụng chức năng này");
                            return res;
                        }
                    res = Register(model);
                    return res;
                case "TC":
                    res.Add("reply",Reply("Tiền mặt trong quỹ của bạn còn: 1.000.000",model["message"]["msg_id"].ToString()));
                    return res;
                case "Reply":
                    res.Add("reply",Reply("Bạn cần hỗ trợ gì ?",model["message"]["msg_id"].ToString()));
                    return res;
                case "BSTT":
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
            if (check!=true)
            {
                connection.Open();
                // Tạo đối tượng SqlCommand
                using var command = new SqlCommand();
                command.Connection = connection;
                // Câu truy vấn gồm: chèn dữ liệu vào và lấy định danh(Primary key) mới chèn vào
                string queryString = @"INSERT INTO Registers (id, UserId, Link, DATE_NEW, Status) VALUES (@id, @UserId, @Link, @DATE_NEW, @Status)";
                command.CommandText = queryString;
                command.Parameters.AddWithValue("@id", Guid.NewGuid());
                command.Parameters.AddWithValue("@UserId", UserId);
                command.Parameters.AddWithValue("@Link", msg.Split(' ')[1]);
                command.Parameters.AddWithValue("@DATE_NEW", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
                command.Parameters.AddWithValue("@Status", 1);
                var rows_affected = command.ExecuteNonQuery();
                connection.Close();
                res.Add("ok", true);
                res.Add("Status","Đăng ký thành công");
                Reply("Đăng ký thành công, bạn hãy bổ sung thông tin để được chăm sóc tốt hơn",model["message"]["msg_id"].ToString());
                RequestUserInfo(UserId);
                return res;
            } 
            else {
                res.Add("error", "Bạn đã đăng ký rồi");
                Reply("Bạn đã đăng ký rồi",model["message"]["msg_id"].ToString());
                return res;
            }
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

        private JObject Follow(JObject model){
            var connection = new SqlConnection(Configuration.GetSection("ConnectionStrings:MainConnection").Value);
            string UserId = model["follower"]["id"].ToString();
            JObject res = new JObject();
            connection.Open();
            // Tạo đối tượng SqlCommand
            using var command = new SqlCommand();
            command.Connection = connection;
            // Câu truy vấn gồm: chèn dữ liệu vào và lấy định danh(Primary key) mới chèn vào
            string queryString = @"INSERT INTO Followers (id, UserId, DATE_NEW) VALUES (@id, @UserId, @DATE_NEW)";
            command.CommandText = queryString;
            command.Parameters.AddWithValue("@id", Guid.NewGuid());
            command.Parameters.AddWithValue("@UserId", UserId);
            command.Parameters.AddWithValue("@DATE_NEW", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
            var rows_affected = command.ExecuteNonQuery();
            connection.Close();
            res.Add("ok", true);
            res.Add("Status","Follow thành công");
            return res;
        }

        private JObject UnFollow(JObject model){
            var connection = new SqlConnection(Configuration.GetSection("ConnectionStrings:MainConnection").Value);
            string UserId = model["follower"]["id"].ToString();
            JObject res = new JObject();
            connection.Open();
            // Tạo đối tượng SqlCommand
            using var command = new SqlCommand();
            command.Connection = connection;
            // Câu truy vấn gồm: chèn dữ liệu vào và lấy định danh(Primary key) mới chèn vào
            string queryString = @"DELETE FROM Followers WHERE UserId = @UserId";
            command.CommandText = queryString;
            command.Parameters.AddWithValue("@UserId", UserId);
            var rows_affected = command.ExecuteNonQuery();
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
            connection.Open();
            // Tạo đối tượng SqlCommand
            using var command = new SqlCommand();
            command.Connection = connection;
            // Câu truy vấn gồm: chèn dữ liệu vào và lấy định danh(Primary key) mới chèn vào
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
            JObject modelSenMessage = new JObject();
            var rows_affected = command.ExecuteNonQuery(); 
            if (rows_affected>=1) {
                modelSenMessage.Add("msg","Bạn đã bổ sung thông tin thành công");
            } else {
                modelSenMessage.Add("msg","Có lỗi xảy ra, có thể bạn chưa đăng ký");
            }
            connection.Close();
            JObject res = new JObject();
            JArray arr = new JArray();
            arr.Add(model["sender"]["id"].ToString());
            modelSenMessage.Add("listUserId",arr);
            // res.Add("ok", true);
            // res.Add("Status","Đăng ký thành công");
            res = SendMessageUserId(modelSenMessage);
            return res;
        }
    } 
}
