using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubspotFileUploading.ApiResponseModel
{
    internal class UploadFile
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public List<Object> objects { get; set; }
        public class Meta
        {
            public bool allows_anonymous_access { get; set; }
            public bool indexable { get; set; }
        }

        public class Object
        {
            public long id { get; set; }
            public int portal_id { get; set; }
            public string name { get; set; }
            public int size { get; set; }
            public object height { get; set; }
            public object width { get; set; }
            public object encoding { get; set; }
            public string type { get; set; }
            public string extension { get; set; }
            public string cloud_key { get; set; }
            public string s3_url { get; set; }
            public string friendly_url { get; set; }
            public Meta meta { get; set; }
            public long created { get; set; }
            public long updated { get; set; }
            public int deleted_at { get; set; }
            public long folder_id { get; set; }
            public bool hidden { get; set; }
            public string cloud_key_hash { get; set; }
            public bool archived { get; set; }
            public int created_by { get; set; }
            public object deleted_by { get; set; }
            public bool replaceable { get; set; }
            public string default_hosting_url { get; set; }
            public List<object> teams { get; set; }
            public string title { get; set; }
            public bool is_indexable { get; set; }
            public string url { get; set; }
            public object cdn_purge_embargo_time { get; set; }
            public string file_hash { get; set; }
        }

        public class Root
        {
            public List<Object> objects { get; set; }
        }


    }
}
