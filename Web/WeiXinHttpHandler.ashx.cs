using System;
using System.Web;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.MP;
using System.IO;
using System.Web.Configuration;
using Senparc.Weixin.MP.CommonAPIs;
namespace Web
{
    /// <summary>
    /// Summary description for WeiXinHttpHandler
    /// </summary>
    public class WeiXinHttpHandler : IHttpHandler
    {
        private readonly string Token = WebConfigurationManager.AppSettings["WeixinToken"];
        private readonly string _EncodingAESKey = WebConfigurationManager.AppSettings["WeixinEncodingAESKey"];
        private readonly string _AppId = WebConfigurationManager.AppSettings["WeixinAppId"];
        private readonly string WeixinAppSecret = WebConfigurationManager.AppSettings["WeixinAppSecret"];
        /// <summary>
        /// 
        /// </summary>
        public bool IsReusable
        {
            get { return true; }
        }
        /// <summary>
        /// 处理请求
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            MiniProcess(context);
        }
        private void WriteContent(HttpContext context,string str)
        {
            context.Response.Output.Write(str);
        }
        private void MiniProcess(HttpContext context)
        {
            string signature = context.Request["signature"];
            string timestamp = context.Request["timestamp"];
            string nonce = context.Request["nonce"];
            string echostr = context.Request["echostr"];

            if (context.Request.HttpMethod == "GET")
            {
                //get method - 仅在微信后台填写URL验证时触发
                if (CheckSignature.Check(signature, timestamp, nonce, Token))
                {
                    WriteContent(context,echostr); //返回随机字符串则表示验证通过
                }
                else
                {
                    WriteContent(context, "failed:" + signature + "," + CheckSignature.GetSignature(timestamp, nonce, Token));
                }
            }
            else
            {
                //post method - 当有用户想公众账号发送消息时触发
                if (!CheckSignature.Check(signature, timestamp, nonce, Token))
                {
                    WriteContent(context, "参数错误！");
                }
                //自定义MessageHandler，对微信请求的详细判断操作都在这里面。
                var messageHandler = new CustomMessageHandler(context.Request.InputStream, null);

                AccessTokenContainer.Register(_AppId, WeixinAppSecret);

                //执行微信处理过程
                messageHandler.Execute();
                //输出结果
                WriteContent(context, messageHandler.ResponseDocument.ToString());
            }
            context.Response.End();
        }
    }
}