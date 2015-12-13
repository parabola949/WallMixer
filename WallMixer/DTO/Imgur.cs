using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallMixer.DTO
{
    public class Imgur
    {
        public ImgurImage[] data { get; set; }
        public bool success { get; set; }
        public int status { get; set; }
    }

    public class ImgurImage
    {
        public string id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public int datetime { get; set; }
        public string type { get; set; }
        public bool animated { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int size { get; set; }
        public int views { get; set; }
        public long bandwidth { get; set; }
        public object vote { get; set; }
        public bool? favorite { get; set; }
        public bool nsfw { get; set; }
        public string section { get; set; }
        public object account_url { get; set; }
        public object account_id { get; set; }
        public object comment_preview { get; set; }
        public string link { get; set; }
        public object comment_count { get; set; }
        public object ups { get; set; }
        public object downs { get; set; }
        public object points { get; set; }
        public int score { get; set; }
        public bool is_album { get; set; }

    }
}
