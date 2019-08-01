﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Xml;
using Formatting = Newtonsoft.Json.Formatting;
using System.Text;
using System.Web;
using JlueTaxSystemBeiJing.Models;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.SessionState;
using System.Web.Hosting;

namespace JlueTaxSystemBeiJing.Code
{
    public class YsbqcSetting : Controller
    {
        public static HttpRequest req { get; set; }

        HttpRequest request { get { return System.Web.HttpContext.Current == null ? req : System.Web.HttpContext.Current.Request; } }

        static HttpSessionState session { get { return System.Web.HttpContext.Current.Session; } }

        string fileName { get; set; }

        string reqPath { get { return AppDomain.CurrentDomain.BaseDirectory + (request ?? req).Path; } }

        DirectoryInfo Dir { get; set; }

        string JsonStr { get; set; }

        JToken retJtok { get; set; }

        JObject retJobj { get; set; }

        JArray retJarr { get; set; }

        JValue retJval { get; set; }

        string retStr { get; set; }

        ContentResult cr { get; set; }

        XmlDocument xd { get; set; }

        public const string functionNotOpen = "FunctionNotOpen";

        public JObject GetJsonObject(List<string> param)
        {
            lock (this)
            {
                fileName = "";
                foreach (string p in param)
                {
                    fileName += p + ".";
                }
                fileName += "json";
                Dir = Directory.GetParent(reqPath);
                JsonStr = System.IO.File.ReadAllText(Dir.GetFiles(fileName)[0].FullName);
                retJobj = JsonConvert.DeserializeObject<JObject>(JsonStr);
                return retJobj;
            }
        }

        public JArray GetJsonArray(List<string> param)
        {
            lock (this)
            {
                fileName = "";
                foreach (string p in param)
                {
                    fileName += p + ".";
                }
                fileName += "json";
                Dir = Directory.GetParent(reqPath);
                JsonStr = System.IO.File.ReadAllText(Dir.GetFiles(fileName)[0].FullName);
                retJarr = JsonConvert.DeserializeObject<JArray>(JsonStr);
                return retJarr;
            }
        }

        public JToken GetJsonValue(List<string> param)
        {
            lock (this)
            {
                fileName = "";
                foreach (string p in param)
                {
                    fileName += p + ".";
                }
                fileName += "json";
                Dir = Directory.GetParent(reqPath);
                JsonStr = System.IO.File.ReadAllText(Dir.GetFiles(fileName)[0].FullName);
                string val = JsonConvert.DeserializeObject<JValue>(JsonStr).Value<string>();
                bool bl = Regex.IsMatch(val, @"\A[\[\{]");
                if (!bl)
                {
                    retJtok = new JValue(val);
                }
                else
                {
                    retJtok = JsonConvert.DeserializeObject<JToken>(val);
                    //retJval = new JValue(JsonConvert.SerializeObject(retJtok));
                }
                return retJtok;
            }
        }

        public string GetJsonString(List<string> param)
        {
            lock (this)
            {
                fileName = "";
                foreach (string p in param)
                {
                    if (!string.IsNullOrEmpty(p))
                    {
                        fileName += p + ".";
                    }
                }
                fileName += "json";
                Dir = Directory.GetParent(reqPath);
                JsonStr = System.IO.File.ReadAllText(Dir.GetFiles(fileName)[0].FullName);
                JsonTextReader reader = new JsonTextReader(new StringReader(JsonStr));
                if (reader.TokenType == JsonToken.None)
                {
                    retStr = JsonStr;
                }
                else
                {
                    retJtok = JsonConvert.DeserializeObject<JToken>(JsonStr);
                    retStr = JsonConvert.SerializeObject(retJtok, Formatting.None);
                }
                return retStr;
            }
        }

        public ContentResult GetHtml(List<string> param, string fileExtension = "html")
        {
            lock (this)
            {
                fileName = "";
                foreach (string p in param)
                {
                    fileName += p + ".";
                }
                fileName += fileExtension;
                Dir = Directory.GetParent(reqPath);
                JsonStr = System.IO.File.ReadAllText(Dir.GetFiles(fileName)[0].FullName);
                cr = Content(JsonStr, "text/html", Encoding.UTF8);
                return cr;
            }
        }

        public JValue GetXmlValue(List<string> param)
        {
            lock (this)
            {
                fileName = "";
                foreach (string p in param)
                {
                    fileName += p + ".";
                }
                fileName += "xml";
                Dir = Directory.GetParent(reqPath);
                JsonStr = System.IO.File.ReadAllText(Dir.GetFiles(fileName)[0].FullName);
                xd = new XmlDocument();
                xd.LoadXml(JsonConvert.DeserializeObject<JValue>(JsonStr).Value.ToString());
                retJval = new JValue(xd.InnerXml);
                return retJval;
            }
        }

        public static SessionModel getSession()
        {
            SessionModel sm = new SessionModel();
            foreach (PropertyInfo pi in sm.GetType().GetProperties())
            {
                pi.SetValue(sm, session[pi.Name]);
            }
            return sm;
        }

        public static SessionModel getCache()
        {
            SessionModel sm = new SessionModel();
            foreach (PropertyInfo pi in sm.GetType().GetProperties())
            {
                pi.SetValue(sm, HttpRuntime.Cache[pi.Name]);
            }
            return sm;
        }

        public static void insertCache(JObject jo)
        {
            HttpRuntime.Cache.Insert("questionId", jo["questionId"].ToString());
            HttpRuntime.Cache.Insert("userquestionId", jo["userquestionId"].ToString());
            HttpRuntime.Cache.Insert("companyId", jo["companyId"].ToString());
            HttpRuntime.Cache.Insert("classId", jo["classId"].ToString());
            HttpRuntime.Cache.Insert("courseId", jo["courseId"].ToString());
            HttpRuntime.Cache.Insert("userId", jo["userId"].ToString());
            HttpRuntime.Cache.Insert("Name", jo["Name"].ToString());
        }

        public static string MapPath(string fileName)
        {
            string reqPath = HostingEnvironment.MapPath(fileName);
            DirectoryInfo Dir = Directory.GetParent(reqPath);
            string FullName = Dir.GetFiles(fileName)[0].FullName;
            return FullName;
        }

        public JValue JTokenToJValue(JToken jt)
        {
            Type type = jt.GetType();
            if (type == typeof(JObject) || type == typeof(JArray))
            {
                return new JValue(JsonConvert.SerializeObject(jt));
            }
            else
            {
                return new JValue(jt.Value<string>());
            }
        }

        public string JsonToString(JToken jt)
        {
            return JsonConvert.SerializeObject(jt, Formatting.None);
        }

        public ContentResult JsonResult(object obj)
        {
            string str = JsonConvert.SerializeObject(obj);
            return Content(str, "application/json", Encoding.UTF8);
        }

        public ContentResult PlainResult(object obj)
        {
            string str = JsonConvert.SerializeObject(obj);
            return Content(str, "text/plain", Encoding.UTF8);
        }

        public JsonResult ValueResult(object obj)
        {
            string str = JsonConvert.SerializeObject(obj);
            return Json(str, JsonRequestBehavior.AllowGet);
        }

    }
}