using System.Collections.Generic;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.Entities.GoogleMap;
using Senparc.Weixin.MP.Entities.BaiduMap;
using Senparc.Weixin.MP.Helpers;

namespace Web
{
    public class LocationService
    {
        public ResponseMessageNews GetResponseMessage(RequestMessageLocation requestMessage)
        {
            var responseMessage = ResponseMessageBase.CreateFromRequestMessage<ResponseMessageNews>(requestMessage);

            var markersList = new List<BaiduMarkers>();
            markersList.Add(new BaiduMarkers()
            {
                Longitude = requestMessage.Location_Y,
                Latitude = requestMessage.Location_X,
                Color = "red",
                Label = "S",
                Size = BaiduMarkerSize.Default
            });
            var mapUrl = BaiduMapHelper.GetBaiduStaticMap(requestMessage.Location_Y, requestMessage.Location_X, 2, 11, markersList,500,300);
            responseMessage.Articles.Add(new Article()
            {
                Description = string.Format("您刚才发送了地理位置信息。经度：{0}，纬度：{1}，标签：{2}",
                              requestMessage.Location_Y, requestMessage.Location_X, requestMessage.Label),
                PicUrl = mapUrl,
                Title = "定位地点周边地图",
                Url = mapUrl
            });
            return responseMessage;
        }
    }
}