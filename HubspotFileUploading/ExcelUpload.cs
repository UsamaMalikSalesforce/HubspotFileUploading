using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using System.Globalization;

namespace HubspotFileUploading
{
    internal class ExcelUpload : Interfaces.IExcelUpload
    {
        public ExcelUpload() { }
        public class FileData
        {
            [Name("Attachment ID")]
            public string AttachmentID { get; set; }
            [Name("Content Type")]
            public string ContentType { get; set; }
            [Name("Description")]
            public string Description { get; set; }
            [Name("File Name")]
            public string FileName { get; set; }
         
            //[Name("Parent ID")]
           // public string ParentID { get; set; }
            
            [Name("Record ID")]
            public string RecordID { get; set; }
            
            [Name("Location")]
            public string Location { get; set; }
            [Name("Object Id")]
            public string ObjectId { get; set; }

            public byte[] data { get { return File.ReadAllBytes(this.Location); } }
        }

        public List<FileData> ProcessExcel(string FilePath)
        {

            var documents = new List<FileData>();
            var file = new FileInfo(FilePath);
            try
            {
                using (var StreamReader = new StreamReader(file.FullName))
                {
                    using (var CSVReader = new CsvReader(StreamReader, CultureInfo.InvariantCulture))
                    {
                        documents = CSVReader.GetRecords<FileData>().ToList();
                    }
                }

                if (!documents.ListHaveData())
                {
                    throw new Exception("No records found!");
                }

            }
            catch (Exception e)
            {
                throw e;
            }
            return documents;
        }
    }
}
