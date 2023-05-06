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

                //Reading all files from given directory in customConfig.json file.
                //string[] fileArray = Directory.GetFiles(configData.uploadingFolder);
                
                //storing Files of directories, path of file, name and extensions too.
                //var allFiles = new List<Files>();
                //foreach (var item in Filedata)
                //{
                //    allFiles.Add( new Files() { data = File.ReadAllBytes(item.Location), filePath = item});
                //}

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
                    using (StreamWriter writer = new StreamWriter("Error Uploading Files -" + DateTime.Now.ToString())) 
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
            }
            return result;

        }  
        public async Task<HelperClass.Result> UploadFileMultiBatchable()
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

                //Reading all files from given directory in customConfig.json file.
                //string[] fileArray = Directory.GetFiles(configData.uploadingFolder);
                
                //storing Files of directories, path of file, name and extensions too.
                //var allFiles = new List<Files>();
                //foreach (var item in Filedata)
                //{
                //    allFiles.Add( new Files() { data = File.ReadAllBytes(item.Location), filePath = item});
                //}

                int counter = 0;
                List<string> errors = new List<string>();
                //Loop through all files

                int batchCounter = 200;
                bool batchComplete = false;
                bool firstTime = true;
                while (!batchComplete)
                {
                    List<ExcelUpload.FileData> Filedata2;
                    if (firstTime)
                    {
                        Filedata2 = Filedata.Take(batchCounter).ToList();
                        firstTime = false;
                        //batchCounter = 200;
                    }
                    else
                    {
                        Filedata2 = Filedata.Skip(batchCounter).Take(200).ToList();
                        batchCounter += batchCounter;
                    }
                    if (Filedata2.Count <= batchCounter)
                    {
                        batchComplete = true;
                    }
                }
                foreach (var item in Filedata)
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
                    if(response.IsSuccessStatusCode)
                    {
                        //Maping response into object
                        var responseModel = sd.StringToSingleCls<ApiResponseModel.UploadFile>();

                        association.fileId = responseModel.HaveData() && responseModel.objects.ListHaveData() ?  responseModel.objects.FirstOrDefault().id : 0;

                        //Deal Id is static for now : 13146268956
                        long idResult;
                        long.TryParse(item.RecordID,out idResult);
                        if(item.ObjectId == "Deal")
                        {
                            association.associationDetails.Add(new AssociationDetail() { dealIds = new List<long> { idResult } });
                        }
                        else if(item.ObjectId == "Contact")
                        {
                            association.associationDetails.Add(new AssociationDetail() { contactIds = new List<long> { idResult } });
                        }
                        else if(item.ObjectId == "Company")
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
                Console.WriteLine("-------------------------------------------------------------------");
                Console.WriteLine($"Uploading Process Completed. Uploaded Files {Filedata.Count() - errors.Count()}/{Filedata.Count()} ");
                if(errors.Count > 0)
                {
                    Console.WriteLine("");
                    Console.WriteLine("Uploading Errors");
                    foreach(var error in errors)
                    {
                        Console.WriteLine(error);
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
                    
                    using (StreamWriter writer = new StreamWriter("Error Assosiation - " + DateTime.Now.ToString()))
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
