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
            var result = obj.UploadFileMulti().Result;
           // obj.AssociationAPI(new List<HubspotService.Association>());
            //             obj.UploadFileMulti("https://webhook.site/cdd011f4-55ab-42fe-aae2-289588811034");
            //  obj.UploadFileMulti("https://hubspot.requestcatcher.com/");

            Console.WriteLine(result.Message);
            Console.ReadLine();
        }
    }
}
