using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Newtonsoft.Json;
using System.Configuration;
using System.Data;

namespace Web
{
    /// <summary>
    /// Summary description for ImgHandler
    /// </summary>
    public class ImgHandler : IHttpHandler
    {
        private string mideaconstr = ConfigurationManager.ConnectionStrings["MideaCS"].ConnectionString;
        public void ProcessRequest(HttpContext context)
        {
            string flag = context.Request.QueryString["flag"];
            // string strPath = System.Web.HttpContext.Current.Server.MapPath("~/UploadImg/");
            context.Response.ContentType = "application/json";
            //List<string> dic = new List<string>();
            //DirectoryInfo folder = new DirectoryInfo(strPath);
            switch (flag)
            {
                case "getlist":
                    {
                        string str = "select MaterialName,MaterialSize,CreateBy,CreateAt from [RMS_TBM_Material] where Description like '%Openid%' order by createAt desc";
                        SQLHelper sql = new SQLHelper(mideaconstr);
                        DataTable dt = sql.GetDataTable(str);
                        var json = JsonConvert.SerializeObject(dt);
                        context.Response.Write(json);
                        break;
                    }
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
    public class FileModel
    {
        public int Count { get; set; }
        public List<string> FileNames { get; set; }
    }
}