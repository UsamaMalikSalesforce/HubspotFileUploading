using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HubspotFileUploading.HubspotService;

namespace HubspotFileUploading.Interfaces
{
    internal interface IHubsotService
    {
        Task<HelperClass.Result> UploadFileMulti();
       // void AssociationAPI  (List<Association> associations, HubspotModel configData);
    }
}
