using HubspotFileUploading.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HubspotFileUploading
{
    internal class HubspotService : IHubsotService
    {
        string fileDataOptions = "";
        public HubspotService()
        {
            var dataOptions = new HubData() { access = "PUBLIC_INDEXABLE", duplicateValidationScope = "EXACT_FOLDER", duplicateValidationStrategy = "NONE", overwrite = false };
            fileDataOptions = dataOptions.ObjectToString();
        }
        static string endPoint = "https://api.hubapi.com/filemanager/api/v3/files/upload";
        public async Task<HelperClass.Result> UploadFileMulti()
        {
            var result = new HelperClass.Result();
            try
            {

                var associationList = new List<Association>();
                
                var configData = ((string)HelperClass.GetCustomConfig().Data).StringToSingleCls<HubspotModel>();
                string[] fileArray = Directory.GetFiles(configData.uploadingFolder);
                var allFiles = new List<Files>();
                foreach (var item in fileArray)
                {
                    allFiles.Add( new Files() { data = File.ReadAllBytes(item), filePath = item});
                }
                int counter = 0;
                foreach (var item in allFiles)
                {
                    var association = new Association();
                    HttpClient httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Add("Authorization", configData.accessToken);
                    MultipartFormDataContent form = new MultipartFormDataContent();

                    form.Add(new StringContent(configData.folderPath), "folderPath");
                    form.Add(new StringContent(fileDataOptions), "options");
                    var file = File.ReadAllBytes(item.filePath);
                    form.Add(new ByteArrayContent(file, 0, file.Length), "file", item.fileName);
                    HttpResponseMessage response = await httpClient.PostAsync(endPoint, form);
                    response.EnsureSuccessStatusCode();
                    httpClient.Dispose();
                    string sd = response.Content.ReadAsStringAsync().Result;
                    if(response.IsSuccessStatusCode)
                    {
                        var responseModel = sd.StringToSingleCls<ApiResponseModel.UploadFile>();
                        association.fileId = responseModel.objects.FirstOrDefault().id;
                        association.associationDetails.Add(new AssociationDetail() { dealIds = new List<long> { 13146268956 } });
                        associationList.Add(association);
                        Console.WriteLine(responseModel.objects[0].id);
                    }
                    result.Status = response.IsSuccessStatusCode;
                    var message = $"{++counter} {item.fileName} uploading ";
                    message += result.Status ? "Successful" : "Failed";
                    Console.WriteLine(message);
                }
                AssociationAPI(associationList,configData);
                result.Data = associationList;
                result.Status = true;
                result.Message = "Uploading Compelete";

            }
            catch (Exception e)
            {
                result = e.ExceptionResult();
            }
            return result;

        }
        public async void AssociationAPI(List<Association> associations,HubspotModel configData)
        {
            try
            {
                foreach (var item in associations)
                {
                    string json = "{ \"engagement\": {" +
                    "\"active\": true," +
                    "\"ownerId\": 378520899," +
                    "\"type\": \"NOTE\"," +
                    "\"timestamp\":" + DateTimeOffset.Now.ToUnixTimeMilliseconds() +
                    "}," +
                "\"associations\": {" +
                    "\"contactIds\":" + "[]," +
                    "\"companyIds\":" + "[]," +
                    "\"dealIds\":" +  Newtonsoft.Json.JsonConvert.SerializeObject(item.associationDetails.FirstOrDefault().dealIds) + "," +
                    "\"ownerIds\":[]," +
                    "\"ticketIds\":[]" + //<-- ID OF THE OBJECT IN HUBSPOT
                "}," +
                "\"attachments\": [" +
                    "{" +
                        "\"id\":" + item.fileId +//-- ID OF THE FILE IN HUBSPOT
                    "}" +
                "]," +
                "\"metadata\": {" +
                    "\"body\": \"\"" +
                "}" +
            "}";
                    HttpClient httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Add("Authorization", configData.accessToken);
                    var res =   await httpClient.PostAsync("https://api.hubapi.com/engagements/v1/engagements", new StringContent(json, Encoding.UTF8, "application/json"));
                }
            }
            catch (Exception e)
            {

            }

        }

       

        public class HubData
        {
            public string access { get; set; }
            public bool overwrite { get; set; }
            public string duplicateValidationStrategy { get; set; }
            public string duplicateValidationScope { get; set; }
        }
        public class Files
        {
            public byte[] data { get; set; }
            public string filePath{ get; set; }
            public string fileName 
            { get 
                {
                    return Path.GetFileName(this.filePath);
                }
            }
            public string fileExtension
            {
                get
                {
                    return Path.GetExtension(this.filePath);
                }
            }
        }
        public class Association
        {
            public Association()
            {
                this.associationDetails = new List<AssociationDetail>();
            }
            public long fileId { get; set; }
            public List<AssociationDetail> associationDetails { get; set; }
        }
        public class AssociationDetail
        {
            public AssociationDetail()
            {
                this.dealIds = new List<long>();
                this.contactIds = new List<long>();
            }
            public List<long> contactIds { get; set; }
            public List<long> dealIds { get; set; }
        }
    }
}
