using SmallDemo.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace SmallDemo.Copyable
{
    

    public partial class UtilityHelper
    {
        #region 一些常用变量
        public static class ConstVar
        {
           
            //用来做测试手机号码用
            public static string[] testAccount = new string[] { 
                "12345678901",
                "12345678902",
                "12345678903",
                "12345678904",
                "12345678905",
                "12345678906", 
                "12345678907", 
                "12345678908", 
                "12345678909", 
                "12345678900", 
                "13123456789"
            };

            public const string DefaultPortrait = "http://hdy.awblob.com/portrait/orgdefault.jpg";
            public const string WebApiTokenAddress = "http://smalldemo.chinacloudsites.cn/token";
         
        }

        public static class ConstTokenPrefix
        {
            public const string UserName = "1";
            public const string MobilePassword = "2";
            public const string MobileCode = "3";
        }

        #endregion

        #region 一些正则表达式
        public static class PrePatterns
        {
            public const string MobilePhone = "^[1][0-9]{10}$";
        }

        /// <summary>
        /// 正则表达式判断，是否是手机号码
        /// </summary>
        /// <param name="InputString"></param>
        /// <returns></returns>
        public static bool IsMobilePhone(string InputString)
        {
            var RegeStr = PrePatterns.MobilePhone;
            return Regex.IsMatch(InputString, RegeStr);
        }
        #endregion

        /// <summary>
        /// 获取当前时间
        /// </summary>
        /// <returns></returns>
        public static DateTime getNow()
        {
            //var myTimeZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
            //var currentDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, myTimeZone);
            //return currentDateTime;            
            return DateTime.Now.ToUniversalTime().AddHours(8);
        }

        /// <summary>
        /// 判断该电话号码和验证码是否合法
        /// </summary>
        /// <param name="Mobile">手机号码</param>
        /// <param name="Code">验证码</param>
        /// <param name="NeedDelete">是否需要删除</param>
        /// <returns></returns>
        public static bool VerifyMobileCode(string Mobile, int Code, bool NeedDelete)
        {
            if (UtilityHelper.ConstVar.testAccount.Contains(Mobile))
            {
                return true;
            }
            DataBaseEntities db = new DataBaseEntities();
            DateTime dt = UtilityHelper.getNow().AddMinutes(-5);  //延迟5分钟
            if (db.cm_SMS_Verify.Any(u => (u.Mobile == Mobile && u.Code == Code && u.CreateDate > dt)))
            {
                if (NeedDelete)
                {
                    var obj = db.cm_SMS_Verify.Where(u => u.Mobile == Mobile).ToList();
                    db.cm_SMS_Verify.RemoveRange(obj);
                }
                return true;
            }
            else
            {
                return false;
            }
        } 

    }
}