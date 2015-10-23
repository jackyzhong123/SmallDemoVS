using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using SmallDemo.Models.Entities;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity; // Maybe this one too
using SmallDemo.Models;
using System.Text;
using System.IO;
using System.Web.Script.Serialization;
using System.Threading.Tasks;

namespace SmallDemo.Copyable
{
    public enum CodeStatus
    {
        SuccessNew = 1,
        ErrorFormat = 100
    }


    [RoutePrefix("api/Login")]
    public class LoginAccountController : ApiController
    {
        #region ==============  程序申明
        public LoginAccountController() { }

        public LoginAccountController(ApplicationUserManager userManager,
          ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        private ApplicationUserManager _userManager;

        #endregion

        ///////////////////////程序开始

        #region 注册及登录

  
        [HttpPost]
        [Route("Register")]   
        public async Task<IHttpActionResult> Register([FromBody] DM_Register model)
        {
            if (!UtilityHelper.VerifyMobileCode(model.Mobile, model.Code, true))
            {
                return Json(new { Code = 1, Message = "验证码错误或已经失效" });
            }
            DataBaseEntities db = new DataBaseEntities();
            //判断该手机号是否可以注册

            //

            string Id = Guid.NewGuid().ToString();
            var user = new ApplicationUser()
            {
                Id = Id,
                UserName = Id,
                MyMobilePhone = model.Mobile,
                RegisterTime = UtilityHelper.getNow(),
                EmailConfirmed = false,
                PhoneNumberConfirmed = false,
                AccessFailedCount = 0,
                LockoutEnabled = true,
                Sex = 2,  //表示没有性别认定
                Portrait = UtilityHelper.ConstVar.DefaultPortrait
            };
            IdentityResult result = await Request.GetOwinContext().GetUserManager<ApplicationUserManager>().CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                //注册不成功
                return Json(new { Code = 1, Message = "注册不成功" });
            }
            else
            {
                //注册成功
                #region 完成一些初始化工作

                #endregion
            }

            string grant_type = "password";
            ASCIIEncoding encoding = new ASCIIEncoding();
            string postData = "grant_type=" + grant_type;
            postData += ("&password=" + model.Password);
            postData += "&username=" + model.Mobile;
            byte[] data = encoding.GetBytes(postData);
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(UtilityHelper.ConstVar.WebApiTokenAddress);
            myRequest.Method = "POST";
            myRequest.ContentType = "application/x-www-form-urlencoded";
            myRequest.ContentLength = data.Length;
            Stream newStream = myRequest.GetRequestStream();

            // Send the data.
            newStream.Write(data, 0, data.Length);
            newStream.Close();

            // Get response
            HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
            StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.Default);
            string content = reader.ReadToEnd();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> json = (Dictionary<string, object>)serializer.DeserializeObject(content);
            string username = json["userName"].ToString();
            //   var myuser = db.AspNetUsers_Org.Include("AspNetUsers").Single(u => u.AspNetUsers.UserName == username);

            return Json(new
            {
                Code = 10000,
                Detail = new
                {
                    token = json["access_token"].ToString(),
                    IsValid = true,
                    expired = json[".expires"].ToString(),
                    //NickName = myuser.AspNetUsers.NickName,
                    //UserName = myuser.AspNetUsers.UserName,
                    //Portrait = myuser.AspNetUsers.Portrait
                }
            });
        }


        [HttpPost]
        [Route("Login")]
        public async Task<IHttpActionResult> Login([FromBody] DM_Login model)
        {
            DataBaseEntities db = new DataBaseEntities();
            string grant_type = "password";
            ASCIIEncoding encoding = new ASCIIEncoding();
            string postData = "grant_type=" + grant_type;
            postData += ("&password=" +   model.Password);
            postData += ("&username=" + UtilityHelper.ConstTokenPrefix.MobilePassword + model.NickName);
            byte[] data = encoding.GetBytes(postData);
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(UtilityHelper.ConstVar.WebApiTokenAddress);

            myRequest.Method = "POST";
            myRequest.ContentType = "application/x-www-form-urlencoded";
            myRequest.ContentLength = data.Length;
            Stream newStream = myRequest.GetRequestStream();

            // Send the data.
            newStream.Write(data, 0, data.Length);
            newStream.Close();

            // Get response
            HttpWebResponse myResponse = null;
            
            try
            {
                myResponse = (HttpWebResponse) myRequest.GetResponse();
            }catch(WebException ex)
            {
                if (ex.Message.Contains("400"))
                {
                    return Json(new
                    {
                        Code = 1,
                        Message = "手机号码或密码错误"
                    });
                }else
                {
                    return Json(new  {
                        Code = 1,
                        Message = "网络访问错误"
                    });
                }
            }
             
   
            
            StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.Default);
            string content = reader.ReadToEnd();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> json = (Dictionary<string, object>)serializer.DeserializeObject(content);

            return Json(new
            {
                Code = 10000,
                Detail = new
                {
                    token = json["access_token"].ToString(),
                    IsValid = true,
                    expired = json[".expires"].ToString()
                }

            });


        }




        #endregion

    }
}
