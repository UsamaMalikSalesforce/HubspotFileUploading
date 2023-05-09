using HubspotFileUploading.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static HubspotFileUploading.HubspotService;

namespace HubspotFileUploading
{
    internal class HubspotService : IHubsotService
    {
        string fileDataOptions = "";
        public HubspotService()
        {
            //Create static json options.
            var dataOptions = new HubData() { access = "PUBLIC_INDEXABLE", duplicateValidationScope = "EXACT_FOLDER", duplicateValidationStrategy = "NONE", overwrite = false };
            
            //Converting object to create JSON String Format.
            fileDataOptions = dataOptions.ObjectToString();
        }
        //Endpoint to upload API Hubspot
        static string endPoint = "https://api.hubapi.com/filemanager/api/v3/files/upload";
        public async Task<HelperClass.Result> UploadFileMulti()
        {
            var result = new HelperClass.Result();
            try
            {

                var associationList = new List<Association>();
                //Read CustomConfig Json file and Mapped into object.
                var configData = ((string)HelperClass.GetCustomConfig().Data).StringToSingleCls<HubspotModel>();

                // Getting data from the CSV
                IExcelUpload excelUpload = new ExcelUpload();
                var Filedata = excelUpload.ProcessExcel(configData.csvfile);

                

                int counter = 0;
                List<string> errors = new List<string>();
                //Loop through all files

                
                foreach (var item in Filedata)
                {
                    try
                    {
                        var association = new Association(item.FileName);
                        HttpClient httpClient = new HttpClient();
                        httpClient.DefaultRequestHeaders.Add("Authorization", configData.accessToken);
                        MultipartFormDataContent form = new MultipartFormDataContent();

                        //folderPath parameter.
                        form.Add(new StringContent(configData.folderPath), "folderPath");

                        //options json string parameter
                        form.Add(new StringContent(fileDataOptions), "options");
                        var file = item.data; //File.ReadAllBytes(item.filePath);

                        //Attaching file to send in form-body
                        form.Add(new ByteArrayContent(file, 0, file.Length), "file", item.FileName);

                        HttpResponseMessage response = await httpClient.PostAsync(endPoint, form);
                        response.EnsureSuccessStatusCode();
                        httpClient.Dispose();

                        //response of API
                        string sd = response.Content.ReadAsStringAsync().Result;

                        //Associate linking
                        if (response.IsSuccessStatusCode)
                        {
                            //Maping response into object
                            var responseModel = sd.StringToSingleCls<ApiResponseModel.UploadFile>();

                            association.fileId = responseModel.HaveData() && responseModel.objects.ListHaveData() ? responseModel.objects.FirstOrDefault().id : 0;

                            //Deal Id is static for now : 13146268956
                            long idResult;
                            long.TryParse(item.RecordID, out idResult);
                            if (item.ObjectId == "Deal")
                            {
                                association.associationDetails.Add(new AssociationDetail() { dealIds = new List<long> { idResult } });
                            }
                            else if (item.ObjectId == "Contact")
                            {
                                association.associationDetails.Add(new AssociationDetail() { contactIds = new List<long> { idResult } });
                            }
                            else if (item.ObjectId == "Company")
                            {
                                association.associationDetails.Add(new AssociationDetail() { companyIds = new List<long> { idResult } });
                            }

                            associationList.Add(association);
                            //                        Console.WriteLine(responseModel.objects[0].id);
                        }
                        else
                        {
                            errors.Add($"File Name: {item.FileName}, Record Id: {item.RecordID}, Object Id: {item.ObjectId}, Salesforce Attachment Id: {item.AttachmentID}");
                        }
                        result.Status = response.IsSuccessStatusCode;
                        var message = $"{++counter}) File Name: {item.FileName}, Hubspot FileId: {association.fileId} uploading ";
                        message += result.Status ? "Successful" : "Failed";
                        Console.WriteLine(message);

                    }
                    catch (Exception ex)
                    {
                        errors.Add($"File Name: {item.FileName}, Record Id: {item.RecordID}, Object Id: {item.ObjectId}, Salesforce Attachment Id: {item.AttachmentID}, ERROR: {ex.Message}");
                        Console.WriteLine(ex.Message);
                    }
                }
                Console.WriteLine("-------------------------------------------------------------------");
                Console.WriteLine($"Uploading Process Completed. Uploaded Files {Filedata.Count() - errors.Count()}/{Filedata.Count()} ");
                if(errors.Count > 0)
                {
                    Console.WriteLine("");
                    Console.WriteLine("Uploading Errors");
                    using (StreamWriter writer = new StreamWriter("D:\\Hubspot Integration\\Error Uploading Files")) 
                    {
                        foreach (string str in errors)
                        {
                            Console.WriteLine(str);
                            writer.WriteLine(str);
                        }
                    }
                    Console.WriteLine("");
                }
                Console.WriteLine("-------------------------------------------------------------------");

                Console.WriteLine("Association Process is in Progress");
                //Association Process
                await AssociationAPI(associationList,configData);
                result.Data = associationList;
                result.Status = true;
                result.Message = "Uploading Compelete";

            }
            catch (Exception e)
            {
                result = e.ExceptionResult();
                Console.WriteLine("Catch:" + e.Message);
            }
            return result;

        }  
       
        public async Task<HelperClass.Result> AssociationAPI(List<Association> associations,HubspotModel configData)
        {
            var result = new HelperClass.Result();
            try
            {
                List<string> errors = new List<string>();
                //Simple JSON String passed with Auth Bearer.
                foreach (var item in associations)
                {
                    try
                    {
                        string json = "{ \"engagement\": {" +
"\"active\": true," +
"\"ownerId\": " + configData.ownerId + "," +
"\"type\": \"NOTE\"," +
"\"timestamp\":" + DateTimeOffset.Now.ToUnixTimeMilliseconds() +
"}," +
"\"associations\": {" +
"\"contactIds\":" + (item.isContact ? item.associationDetails.FirstOrDefault().contactIds.ObjectToString() : "[]") + "," +
"\"companyIds\":" + (item.isCompany ? item.associationDetails.FirstOrDefault().companyIds.ObjectToString() : "[]") + "," +
"\"dealIds\":" + (item.isDeal ? item.associationDetails.FirstOrDefault().dealIds.ObjectToString() : "[]") + "," +
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
                        var res = await httpClient.PostAsync("https://api.hubapi.com/engagements/v1/engagements", new StringContent(json, Encoding.UTF8, "application/json"));

                        Console.WriteLine($"{item.fileName} Association " + (res.IsSuccessStatusCode ? " Success" : "Failed"));
                        if (!res.IsSuccessStatusCode)
                        {
                            errors.Add($"{item.fileName} Association Failed, {res.Content.ReadAsStringAsync().Result}");
                        }
                        httpClient.Dispose();
                    }
                    catch (Exception ee)
                    {
                        Console.WriteLine(ee.Message);
                        errors.Add(ee.Message);
                    }
                }
                Console.WriteLine("------------------------------------------");
                Console.WriteLine($"Association Completed. Result {associations.Count()-errors.Count()}/{associations.Count}");
                Console.WriteLine("------------------------------------------");
                if(errors.Count() > 0)
                {
                    Console.WriteLine("Association Errors");
                    
                    using (StreamWriter writer = new StreamWriter("D:\\Hubspot Integration\\Error Assosiation "))
                    {
                        foreach (string str in errors)
                        {
                            Console.WriteLine(str);
                            writer.WriteLine(str);
                        }
                    }
                }
                result.Status = true;
                result.Message = "Passed";
            }
            catch (Exception e)
            {
                result = e.ExceptionResult();
                Console.WriteLine("Catch:"+ e.Message);
            }
            return result;

        }

        public async Task<HelperClass.Result> GetUploadedFiles(long? fromDateTime)
        {
            var result = new HelperClass.Result();
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer pat-na1-73c7e0ac-d743-4b69-b934-72aff3867977");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                var response = await httpClient.GetAsync($"https://api.hubapi.com/filemanager/api/v2/files?created__gt={fromDateTime}&limit=1000000");
                string sd = response.Content.ReadAsStringAsync().Result;
              //  var data = sd.StringToSingleCls<GetFilesJsonModel.Root>();
                Console.WriteLine("Get Files Status Code: " +  response.StatusCode);
                if(response.IsSuccessStatusCode)
                {
                    result.Status = true;
                    result.Message = "Get Success";
                    result.Data = sd.StringToSingleCls<GetFilesJsonModel.Root>();
                }
                httpClient.Dispose();
            }
            catch (Exception e)
            {
                result = e.ExceptionResult();
            }
            return result;
        }

        public async Task<HelperClass.Result> DeleteFiles(List<GetFilesJsonModel.Object> objects)
        {
            var result = new HelperClass.Result();
            List<string> errors = new List<string>();
            try
            {
                if(objects.ListHaveData())
                {
                    foreach (var item in objects.Take(5).ToList())
                    {
                        HttpClient httpClient = new HttpClient();
                        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer pat-na1-73c7e0ac-d743-4b69-b934-72aff3867977");
                        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                        var response = await httpClient.DeleteAsync("https://api.hubapi.com/filemanager/api/v2/files?created__gt=1683270000&limit=1000000");
                        string sd = response.Content.ReadAsStringAsync().Result;
                        if(!response.IsSuccessStatusCode)
                        {
                            var er = sd.StringToSingleCls<DeleteFileModel>();
                            errors.Add($"{item.name} unable to delete. {response.ToString()}");
                        }
                        Console.WriteLine($"{item.name} is deleted");
                        httpClient.Dispose();
                    }
                    errors.WriteErrorFile("D:\\Hubspot Integration\\Error Delete Files");
                }
                else
                {
                    result.Status = false;
                    result.Message = "Data not found";
                }
            }
            catch (Exception e)
            {
                result = e.ExceptionResult();
            }
            return result;
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
            {
                //get 
                //{
                //    return Path.GetFileName(this.filePath);
                //}
                get;set;
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
            public Association(string FileName)
            {
                this.associationDetails = new List<AssociationDetail>();
                this.fileName = FileName;
            }
            public long fileId { get; set; }
            public string fileName { get; set; }
            public List<AssociationDetail> associationDetails { get; set; }
            public bool isCompany { get { return this.associationDetails.ListHaveData() && this.associationDetails.FirstOrDefault().companyIds.ListHaveData(); } }
            public bool isContact { get { return this.associationDetails.ListHaveData() && this.associationDetails.FirstOrDefault().contactIds.ListHaveData(); } }
            public bool isDeal { get { return this.associationDetails.ListHaveData() && this.associationDetails.FirstOrDefault().dealIds.ListHaveData(); } }
        }
        public class AssociationDetail
        {
            public AssociationDetail()
            {
                this.dealIds = new List<long>();
                this.contactIds = new List<long>();
                this.companyIds = new List<long>();
            }
            public List<long> contactIds { get; set; }
            public List<long> dealIds { get; set; }
            public List<long> companyIds { get; set; }


        }
    }
}
