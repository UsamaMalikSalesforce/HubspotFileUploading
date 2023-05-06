using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubspotFileUploading.Interfaces
{
    internal interface IExcelUpload
    {
        List<ExcelUpload.FileData> ProcessExcel(string FilePath);
    }
}
