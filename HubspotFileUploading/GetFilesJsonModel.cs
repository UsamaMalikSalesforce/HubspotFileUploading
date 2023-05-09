using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubspotFileUploading
{
    internal class GetFilesJsonModel
    {
        public class HostingInfo
        {
            public string provider { get; set; }
            public object id { get; set; }
            public string status { get; set; }
        }

        public class Icon
        {
            public string cloud_key { get; set; }
            public string friendly_url { get; set; }
            public string image_name { get; set; }
            public string s3_url { get; set; }
        }

        public class Medium
        {
            public string cloud_key { get; set; }
            public string friendly_url { get; set; }
            public string image_name { get; set; }
            public string s3_url { get; set; }
        }

        public class Meta
        {
            public Thumbs thumbs { get; set; }
            public bool allows_anonymous_access { get; set; }
            public bool indexable { get; set; }
            public int? duration { get; set; }
            public VideoData video_data { get; set; }
            public string charset_guess { get; set; }
            public long? expires_at { get; set; }
            public int? line_count { get; set; }
        }

        public class Object
        {
            public object id { get; set; }
            public object portal_id { get; set; }
            public string name { get; set; }
            public object size { get; set; }
            public object height { get; set; }
            public object width { get; set; }
            public string encoding { get; set; }
            public string type { get; set; }
            public string extension { get; set; }
            public string cloud_key { get; set; }
            public string s3_url { get; set; }
            public string friendly_url { get; set; }
            public Meta meta { get; set; }
            public object created { get; set; }
            public object updated { get; set; }
            public object deleted_at { get; set; }
            public long? folder_id { get; set; }
            public bool hidden { get; set; }
            public string cloud_key_hash { get; set; }
            public bool archived { get; set; }
            public object created_by { get; set; }
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

        public class Preview
        {
            public string cloud_key { get; set; }
            public string friendly_url { get; set; }
            public string image_name { get; set; }
            public string s3_url { get; set; }
        }

        public class Root
        {
            public int limit { get; set; }
            public int offset { get; set; }
            public List<Object> objects { get; set; }
            public int total_count { get; set; }
        }

        public class Stream
        {
            public string codec_name { get; set; }
            public string codec_long_name { get; set; }
            public string codec_type { get; set; }
            public string profile { get; set; }
            public string codec_time_base { get; set; }
            public object width { get; set; }
            public object height { get; set; }
            public object duration { get; set; }
            public object bitrate { get; set; }
            public object start_time { get; set; }
            public string display_aspect_ratio { get; set; }
            public string sample_aspect_ratio { get; set; }
            public string time_base { get; set; }
            public string frame_rate { get; set; }
            public string avg_frame_rate { get; set; }
            public object sample_rate { get; set; }
            public object channels { get; set; }
            public Tags tags { get; set; }
        }

        public class Tags
        {
            public string language { get; set; }
            public string handler_name { get; set; }
            public string encoder { get; set; }
        }

        public class Thumb
        {
            public string cloud_key { get; set; }
            public string friendly_url { get; set; }
            public string image_name { get; set; }
            public string s3_url { get; set; }
        }

        public class Thumbs
        {
            public Preview preview { get; set; }
            public Thumb thumb { get; set; }
            public Medium medium { get; set; }
            public Icon icon { get; set; }
        }

        public class VideoData
        {
            public string format_name { get; set; }
            public string format_long_name { get; set; }
            public object duration { get; set; }
            public object bitrate { get; set; }
            public string source_cloud_key { get; set; }
            public string source_version { get; set; }
            public object source_bitrate { get; set; }
            public object source_size { get; set; }
            public List<HostingInfo> hosting_infos { get; set; }
            public List<Stream> streams { get; set; }
        }
    }
}
