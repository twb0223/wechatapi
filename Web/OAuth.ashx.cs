using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Web
{
    /// <summary>
    /// OAuth 的摘要说明
    /// </summary>
    public class OAuth : IHttpHandler
    {
        private string appId = WebConfigurationManager.AppSettings["WeixinAppId"];//appid
        private string appSecret = WebConfigurationManager.AppSettings["WeixinAppSecret"];//密钥


        public async void ProcessRequest(HttpContext context)
        {
            //1.微信平台调用下列URL，到这个方法，这个方法得到code
            //https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx2e7e2055680c7a53&redirect_uri=http://182.254.159.149/OAuth.ashx&response_type=code&scope=snsapi_base&state=1#wechat_redirect
            context.Response.ContentType = "text/plain";
            var code = context.Request.QueryString["code"];

            Task<string> openid = GetOpenID(code);
            string id = await openid;
            var msg = JsonConvert.DeserializeObject<OAuthMsg>(id);
            context.Response.Redirect("Register.htm?openid=" + code);

        }
        async Task<string> GetOpenID(string code)
        {
            //2.然后发起通过code 获取usrinfo 的请求，调用下面URL，得到返回的JSON。
            //https://api.weixin.qq.com/sns/oauth2/access_token?appid=wx2e7e2055680c7a53&secret=aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa&code=code&grant_type=authorization_code
            string getiopenidurl = string.Format("https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code", appId, appSecret, code);
            WebRequest hr = HttpWebRequest.Create(getiopenidurl);
            WebResponse response = await hr.GetResponseAsync();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8"));
            string ReturnVal = reader.ReadToEnd();
            reader.Close();
            response.Close();
            return ReturnVal;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }



    public class OAuthMsg
    {
        public string access_token { get; set; }
        public string expires_in { get; set; }

        public string refresh_token { get; set; }
        public string openid { get; set; }
        public string scope { get; set; }


    }
}