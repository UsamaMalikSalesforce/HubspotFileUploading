using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubspotFileUploading
{
    internal static class HelperClass
    {
        public class Result
        {
            public bool Status { get; set; }
            public string Message { get; set; }
            public object Data { get; set; }
        }
        //public static string BatchFileName = "PTIBatchFile.bat";
        // public static string DataLoaderPath = @"C:\\Users\\Usama Malik\\dataloader\\v57.0.0\\bin\";
        // public static string cmdBase = $"process.bat F:\\Projects\\PTI\\{CurrentDateWithDash()} ";
        public static HelperClass.Result ExceptionResult(this Exception exception)
        {
            return (new Result() { Status = true, Message = exception.Message });
        }
        public static string ClsListToString<TSource>(this IEnumerable<TSource> source)
        {
            try
            {
                return source.ListHaveData() ? Newtonsoft.Json.JsonConvert.SerializeObject(source) : "";
            }
            catch (Exception e)
            {
            }
            return "";
        }
        public static string ObjectToString(this object obj)
        {
            try
            {
                return obj.HaveData() ? Newtonsoft.Json.JsonConvert.SerializeObject(obj) : "";
            }
            catch (Exception e)
            { }
            return "";
        }
        public static bool HaveData<TSource>(this TSource source)
        {
            return source != null;
        }
        public static bool ListHaveData<TSource>(this IEnumerable<TSource> source)
        {
            return source != null && source.Any() && source.FirstOrDefault().HaveData();
        }
        public static List<TSource> StringToCls<TSource>(this string source)
        {
            try
            {
                return source.ListHaveData() ? JsonConvert.DeserializeObject<List<TSource>>(source) : new List<TSource>();
            }
            catch (Exception ee)
            { }
            return new List<TSource>();
        }
        public static TSource StringToSingleCls<TSource>(this string source)
        {
            try
            {
                return source.HaveData() ? JsonConvert.DeserializeObject<TSource>(source) : new List<TSource>().FirstOrDefault();
            }
            catch (Exception ee)
            { }
            return new List<TSource>().FirstOrDefault();
        }
        public static string CurrentDateWithDash()
        {
            var date = DateTime.Now.Date.ToShortDateString();
            date = date.Replace('/', '-');
            return date;
        }
        public static Result GetCustomConfig()
        {
            var result = new Result();
            try
            {
                using (StreamReader r = new StreamReader(@"customConfig.json"))
                {
                    string json = r.ReadToEnd();
                    var data = HelperClass.StringToSingleCls<HubspotModel>(json);
                    if (data.HaveData())
                    {
                        result.Status = true;
                        result.Message = "Success";
                        result.Data = json;
                    }
                    else
                    {
                        result.Status = false;
                        result.Message = "No Data found";
                    }
                }
            }
            catch (Exception e)
            {
                result.Status = false;
                result.Message = e.Message;
            }
            return result;
        }
    }
}
