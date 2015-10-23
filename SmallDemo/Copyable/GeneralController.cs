using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Net.Http;
using SmallDemo.Models.Entities;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Text;


namespace SmallDemo.Copyable
{
    [RoutePrefix("api/General")]
    public class GeneralController : ApiController
    {
  

       
        //发送短信
        [HttpGet]
        [Route("SMSSend")]
        public IHttpActionResult SMSSend(string m)
        {
            if (UtilityHelper.ConstVar.testAccount.Any(u=>u == m))
            {
                return Json(new
                {
                    Code = 10000,
                    Detail = new { }
                }); 
            }
            if (!UtilityHelper.IsMobilePhone(m))
            {
                return Json(new
                {
                    Code = 1,
                    Message = "手机格式不正确"
                });
            }

            Random ran = new Random();
            int RandKey = ran.Next(1000, 9999);

            try
            {
            
                string mobile = m,
                message = "验证码：" + RandKey.ToString() + " ，两分钟内有效【活动邮】",
                username = ConfigurationManager.AppSettings["SMSUsername"],
                password = ConfigurationManager.AppSettings["SMSKey"],
                url = ConfigurationManager.AppSettings["SMSUrl"];        
                byte[] byteArray = Encoding.UTF8.GetBytes("mobile=" + mobile + "&message=" + message);
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
                string auth = "Basic " + Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(username + ":" + password));
                webRequest.Headers.Add("Authorization", auth);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.ContentLength = byteArray.Length;

                Stream newStream = webRequest.GetRequestStream();
                newStream.Write(byteArray, 0, byteArray.Length);
                newStream.Close();
                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                StreamReader php = new StreamReader(response.GetResponseStream(), Encoding.Default);
                string Message = php.ReadToEnd();              
            }catch
            {
                return Json(new
                {
                    Code = 1,
                    Message = "验证码服务器有误"
                });
            }

            DataBaseEntities db = new DataBaseEntities();
  
            var verify = new cm_SMS_Verify
            {
                Id = Guid.NewGuid().ToString(),
                Code = RandKey,
                CreateDate = DateTime.Now,
                Mobile = m
            };

            db.cm_SMS_Verify.Add(verify);
            db.SaveChanges();

            return Json(new
            {
                Code = 10000,
                Detail = new
                {                    
                }
            }); 
        }
        

        //短信验证
        [HttpGet]
        [Route("SMSVerify")]
        public IHttpActionResult SMSVerify(string m,string code)
        {
            int VerifyCode = int.Parse(code);
            if (UtilityHelper.VerifyMobileCode(m,VerifyCode,false))
            {
                return Json(new
                {
                    Code = 10000,
                    Detail = new
                    {
                    }
                }); 
            }else
            {
                return Json(new
                {
                    Code = 1,
                    Message = "验证码不正确"
                }); 
            }
        }

        

    }
}