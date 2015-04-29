using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Configuration;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.MP.MessageHandlers;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.Media;
using System.Net;
using Senparc.Weixin.MP;
using System.Configuration;

namespace Web
{
    /// <summary>
    /// 自定义MessageHandler
    /// 把MessageHandler作为基类，重写对应请求的处理方法
    /// </summary>
    public partial class CustomMessageHandler : MessageHandler<CustomMessageContext>
    {
        private string appId = WebConfigurationManager.AppSettings["WeixinAppId"];//appid
        private string appSecret = WebConfigurationManager.AppSettings["WeixinAppSecret"];//密钥
        private string imgPath = WebConfigurationManager.AppSettings["ImgPath"];//图片文件地址
        private int expireMinutes = int.Parse(WebConfigurationManager.AppSettings["ExpireMinutes"] == "" ? "3" : WebConfigurationManager.AppSettings["ExpireMinutes"]);//超时时间

        private string wechaturl = WebConfigurationManager.AppSettings["WechatURL"];

        private string mideaconstr = ConfigurationManager.ConnectionStrings["MideaCS"].ConnectionString;
        private string platfromconstr = ConfigurationManager.ConnectionStrings["PlatformCS"].ConnectionString;

        public CustomMessageHandler(Stream inputStream, PostModel postModel, int maxRecordCount = 0)
            : base(inputStream, postModel, maxRecordCount)
        {
            //这里设置仅用于测试，实际开发可以在外部更全局的地方设置
            WeixinContext.ExpireMinutes = expireMinutes;
        }
        public override void OnExecuting()
        {
            //测试MessageContext.StorageData
            if (CurrentMessageContext.StorageData == null)
            {
                CurrentMessageContext.StorageData = 0;
            }
            base.OnExecuting();
        }
        public override void OnExecuted()
        {
            base.OnExecuted();
            CurrentMessageContext.StorageData = ((int)CurrentMessageContext.StorageData) + 1;
        }
        /// <summary>
        /// 处理文字请求
        /// </summary>
        /// <returns></returns>
        public override IResponseMessageBase OnTextRequest(RequestMessageText requestMessage)
        {
            //TODO:这里的逻辑可以交给Service处理具体信息，参考OnLocationRequest方法或/Service/LocationSercice.cs
            var responseMessage = base.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "您可以上传图片作为素材！";
            return responseMessage;
        }
        /// <summary>
        /// 处理位置请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnLocationRequest(RequestMessageLocation requestMessage)
        {
            var locationService = new LocationService();
            var responseMessage = locationService.GetResponseMessage(requestMessage as RequestMessageLocation);
            return responseMessage;
        }
        /// <summary>
        /// 处理视频请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnShortVideoRequest(RequestMessageShortVideo requestMessage)
        {
            //var accessToken = AccessTokenContainer.GetToken(appId);
            //var url = string.Format("http://api.weixin.qq.com/cgi-bin/media/get?access_token={0}&media_id={1}",accessToken, requestMessage.MediaId);

            //var name = string.Format(@"vi_{0}.mp4", DateTime.Now.Ticks);
            //var filepath = imgPath + name;
            //WebClient mywebclient = new WebClient();
            //mywebclient.DownloadFile(url, filepath);


            var responseMessage = this.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "您刚才发送的是小视频";
            return responseMessage;
        }
        /// <summary>
        /// 处理图片请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnImageRequest(RequestMessageImage requestMessage)
        {
            var responseMessage = CreateResponseMessage<ResponseMessageNews>();
            var username=CheckUser(requestMessage.FromUserName);
            if (username==null)//该openid没有绑定平台账号
            {
                responseMessage.Articles.Add(new Article()
                {
                    Title = "您的微信未绑定",
                    Description = "点击绑定平台账号",
                    PicUrl=wechaturl+"/images/error.png",
                    Url = wechaturl+"/Register.htm?openid=" + requestMessage.FromUserName

                });
                return responseMessage;
            }
            //保存到本地
            string url = requestMessage.PicUrl;
            var name = string.Format(@"pic_{0}.jpg", DateTime.Now.Ticks);
            var filepath = imgPath + name;
            WebClient mywebclient = new WebClient();
            mywebclient.DownloadFile(url, filepath);

            System.IO.FileInfo fileInfo = new System.IO.FileInfo(filepath);
            if (fileInfo != null)
            {
                System.Drawing.Image pic = System.Drawing.Image.FromFile(filepath);//filepath是该图片的绝对路径
                int intWidth = pic.Size.Width;
                int intHeight = pic.Size.Height;
                //将图片信息名称，写入对应的数据库
                string str = string.Format(@"insert into [RMS_TBM_Material]([MaterialName]
                       ,[MaterialFileName]
                       ,[MaterialSize]
                       ,[MaterialTypeID]
                       ,[MaterialCheck]
                       ,[ThumbnailFileName]
                       ,[ThumbnailWidth]
                       ,[ThumbnailHeight]
                       ,[Attribute]
                       ,[Viewed]
                       ,[Used]
                       ,[Description]
                       ,[CreateBy]
                       ,[CreateAt]
                       ,[UpdateBy]
                       ,[UpdateAt]
                       ,[MaterialTime])
            values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}')", name, name, fileInfo.Length, 3, 1, name, intWidth, intHeight, 0, 0, 0, "来自微信用户(OpenID):" + requestMessage.FromUserName, username.ToString(), DateTime.Now, null, null, 0);
                try
                {
                    SQLHelper sql = new SQLHelper(mideaconstr);
                    var rnum = sql.ExcSql(str);
                    responseMessage.Articles.Add(new Article()
                    {
                        Title = "上传图片成功",
                        Description = "分辨率：" + intWidth + "x" + intHeight+"   大小:"+Math.Round((decimal)fileInfo.Length / 1024, 2) + "KB",
                        PicUrl = requestMessage.PicUrl,
                        Url = requestMessage.PicUrl
                    });
                }
                catch (Exception)
                {
                    responseMessage.Articles.Add(new Article()
                    {
                        Title = "异常",
                        Description ="服务暂时无法响应，请稍后再试！",
                    });
                }
                finally
                {
                    pic.Dispose();//释放图片对象。
                }
            }
            return responseMessage;
        }
        //检查用户是否绑定
        object CheckUser(string openid)
        {
            SQLHelper sql = new SQLHelper(platfromconstr);
            var str=string.Format(@"select [UserName] from [dbo].[DMS_TBM_User] where [WeChatOpenID]='{0}'",openid);
            return sql.GetValue(str);
        }
        /// <summary>
        /// 处理视频请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnVideoRequest(RequestMessageVideo requestMessage)
        {
            var responseMessage = CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "您发送了一条视频信息，ID：" + requestMessage.MediaId;
            return responseMessage;
        }
        /// <summary>
        /// 处理链接消息请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnLinkRequest(RequestMessageLink requestMessage)
        {
            var responseMessage = ResponseMessageBase.CreateFromRequestMessage<ResponseMessageText>(requestMessage);
            responseMessage.Content = string.Format(@"您发送了一条连接信息：
Title：{0}
Description:{1}
Url:{2}", requestMessage.Title, requestMessage.Description, requestMessage.Url);
            return responseMessage;
        }
        /// <summary>
        /// 处理事件请求（这个方法一般不用重写，这里仅作为示例出现。除非需要在判断具体Event类型以外对Event信息进行统一操作
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEventRequest(IRequestMessageEventBase requestMessage)
        {
            var eventResponseMessage = base.OnEventRequest(requestMessage);//对于Event下属分类的重写方法，见：CustomerMessageHandler_Events.cs
            //TODO: 对Event信息进行统一操作
            return eventResponseMessage;
        }
        /// <summary>
        /// 默认请求返回
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase DefaultResponseMessage(IRequestMessageBase requestMessage)
        {
            /* 所有没有被处理的消息会默认返回这里的结果，
             * 因此，如果想把整个微信请求委托出去（例如需要使用分布式或从其他服务器获取请求），
             * 只需要在这里统一发出委托请求，如：
             * var responseMessage = MessageAgent.RequestResponseMessage(agentUrl, agentToken, RequestDocument.ToString());
             * return responseMessage;
             */
            var responseMessage = base.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "您可以上传图片作为素材！";
            return responseMessage;
        }
    }
}
