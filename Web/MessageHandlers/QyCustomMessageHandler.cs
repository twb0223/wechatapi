using System;
using System.IO;
using Senparc.Weixin.QY.Entities;
using Senparc.Weixin.QY.MessageHandlers;
using Senparc.Weixin.QY.CommonAPIs;
using Senparc.Weixin.QY.AdvancedAPIs.Media;
using System.Web.Configuration;
using System.Net;

namespace Web
{
    public class QyCustomMessageHandler : QyMessageHandler<QyCustomMessageContext>
    {
        private string appId = WebConfigurationManager.AppSettings["WeixinAppId"];
        private string appSecret = WebConfigurationManager.AppSettings["WeixinAppSecret"];
        private string imgPath = WebConfigurationManager.AppSettings["imgPath"];
        private string _appId = WebConfigurationManager.AppSettings["WeixinAppId"];

        private string wechaturl = WebConfigurationManager.AppSettings["WechatURL"];

        public QyCustomMessageHandler(Stream inputStream, PostModel postModel, int maxRecordCount = 0)
            : base(inputStream, postModel, maxRecordCount)
        {
        }

        public override IResponseMessageBase OnTextRequest(RequestMessageText requestMessage)
        {
            var responseMessage = this.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "您发送了消息：" + requestMessage.Content;
            return responseMessage;
        }

        public override IResponseMessageBase OnImageRequest(RequestMessageImage requestMessage)
        {
            var responseMessage = CreateResponseMessage<ResponseMessageImage>();
            responseMessage.Image.MediaId = requestMessage.MediaId;

            var accessToken = AccessTokenContainer.TryGetToken(appId, appSecret);       
            //下载图片
            string url = requestMessage.PicUrl;
            var name = string.Format(@"pic_{0}.jpg", DateTime.Now.Ticks);
            var filepath = imgPath + name;
            WebClient mywebclient = new WebClient();
            mywebclient.DownloadFile(url, filepath);
            //todo:写入dms

            return responseMessage;
        }

        public override IResponseMessageBase OnEvent_PicPhotoOrAlbumRequest(RequestMessageEvent_Pic_Photo_Or_Album requestMessage)
        {
            var responseMessage = this.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "您刚发送的图片如下：";
            return responseMessage;
        }

        public override IResponseMessageBase DefaultResponseMessage(Senparc.Weixin.QY.Entities.IRequestMessageBase requestMessage)
        {
            var responseMessage = this.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "这是一条没有找到合适回复信息的默认消息。";
            return responseMessage;
        }
    }
}
