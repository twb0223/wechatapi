using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Web
{
    /// <summary>
    /// Summary description for Register
    /// </summary>
    public class Register : IHttpHandler
    {
        private string mideaconstr = ConfigurationManager.ConnectionStrings["MideaCS"].ConnectionString;
        private string platfromconstr = ConfigurationManager.ConnectionStrings["PlatformCS"].ConnectionString;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            var flag = context.Request.QueryString["flag"];
            var openid = context.Request.QueryString["openid"];
            var account = context.Request.QueryString["account"];
            var password = context.Request.QueryString["password"];

            SQLHelper sql = new SQLHelper(platfromconstr);
            switch (flag)
            {
                case "check":
                    {
                        string strsql = string.Format(@"select UserName from [dbo].[DMS_TBM_User] where [WeChatOpenID]='{0}'", openid);
                        var uname = sql.GetValue(strsql);
                        context.Response.Write(uname);
                    }
                    break;
                case "reg":
                    {
                        var despassword = MD5Encrypt(MD5Encrypt(password));//DMS 平台是两次MD5加密，这里必须也是两次
                        //todo:将用户绑定的信息保存。与OpeniD关系是 1：1
                        string strsql = string.Format(@"update [dbo].[DMS_TBM_User] set [WeChatOpenID]='{0}' where [UserName]='{1}' and [password]='{2}'", openid, account, despassword);
                        var num = sql.ExcSql(strsql);
                        if (num > 0)
                        {
                            context.Response.Write("绑定成功!");
                        }
                        else
                        {
                            context.Response.Write("绑定失败,请检查用户名！");
                        }
                    }

                    break;
                default:
                    {
                        string strsql = string.Format(@"select UserName from [dbo].[DMS_TBM_User] where [WeChatOpenID]='{0}'", openid);
                        var uname = sql.GetValue(strsql);
                        context.Response.Write(uname);
                    }
                    break;
            }
        }
        public static string MD5Encrypt(string paramstr)
        {
            string privateKey = "loveyajuan"; //这个 loveyajuan 是DMS平台密码加密使用的密钥,迫于无奈才使用的  -_-!
            string tempStr = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(privateKey, "MD5");
            return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(paramstr + tempStr, "MD5").ToLower();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}