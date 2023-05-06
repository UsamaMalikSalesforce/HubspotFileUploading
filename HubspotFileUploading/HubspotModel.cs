using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubspotFileUploading
{
    internal class HubspotModel
    {
        public string accessToken { get; set; }
        public string folderPath { get; set; }
        public string uploadingFolder { get; set; }
        public string csvfile { get; set; }
        public string ownerId { get; set; }

       
    }
}
