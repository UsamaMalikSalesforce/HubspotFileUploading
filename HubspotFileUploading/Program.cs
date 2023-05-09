using HubspotFileUploading.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubspotFileUploading
{
    //new
    internal class Program
    {
        static void Main(string[] args)
        {
            IHubsotService obj = new HubspotService();
            // var result = obj.UploadFileMulti().Result;

            DateTime datetime = new DateTime(2023,5, 6, 0, 0, 1); //YYYY,MM,DD,HH,MM,SS
            long unixTimeMilliseconds = new DateTimeOffset(datetime).ToUnixTimeMilliseconds();
            var result = obj.GetUploadedFiles(unixTimeMilliseconds).Result;
            var data = result.Data.HaveData() ? (GetFilesJsonModel.Root)result.Data : null;
//            result = obj.DeleteFiles(data.objects).Result;
            Console.WriteLine(result.Message);
            Console.ReadLine();
        }
    }
}
