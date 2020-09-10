using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattermostBotBase
{
    class MattermostPost
    {
        public string id { get; set; }
        public string create_at { get; set; }
        public string update_at { get; set; }
        public string edit_at { get; set; }
        public string delete_at { get; set; }
        public string is_pinned { get; set; }
        public string user_id { get; set; }
        public string channel_id { get; set; }
        public string root_id { get; set; }
        public string parent_id { get; set; }
        public string original_id { get; set; }
        public string message { get; set; }
        public string type { get; set; }
        public string props { get; set; }
        public string hashtags { get; set; }
        public string pending_post_id { get; set; }
        public string metadata { get; set; }
 

    }

    class MattermostProps
    {
        public List<MattermostAttachments> attachments { get; set; }
        public string from_webhook { get; set; }
        public string webhook_display_name { get; set; }
    }

    class MattermostAttachments
    {
        public string id { get; set; }
        public string fallback { get; set; }
        public string color { get; set; }
        public string pretext { get; set; }
        public string author_name { get; set; }
        public string author_link { get; set; }
        public string author_icon { get; set; }
        public string title { get; set; }
        public string text { get; set; }
        public List<MattermostFields> fields { get; set; }
        public string image_url { get; set; }
        public string thumb_url { get; set; }
        public string footer { get; set; }
        public string footer_icon { get; set; }
        public object ts { get; set; } //dunno what dis it
    }

    class MattermostFields
    {
        public string title { get; set; }
        public string value { get; set; }
        public string sshort { get; set; } //short
    }

    class MattermostMetadata
    {
        List<MattermostType> embeds { get; set; }

    }
    
    class MattermostType
    {
        public string type { get; set; }
    }
}
