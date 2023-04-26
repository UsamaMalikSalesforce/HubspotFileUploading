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
            Console.WriteLine(result.Message);
            Console.ReadLine();
        }
    }
}
