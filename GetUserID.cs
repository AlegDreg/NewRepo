using System;
using System.Net;

namespace VkLikes.Url
{
    public class GetUserID
    {
        public R1 GetUrl(string url, string token)
        {
            var v = url.Split('/');

            string req = "https://api.vk.com/method/utils.resolveScreenName?" +
            $"screen_name={v[v.Length - 1]}" +
                $"&access_token={token}" +
                "&v=5.131";

            string r;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    r = wc.DownloadString(req);
                }

                var e = Newtonsoft.Json.JsonConvert.DeserializeObject<R>(r);

                if (e != null)
                {
                    return e.response;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public class R
    {
        public R1 response { get; set; }
    }

    public class R1
    {
        public int object_id { get; set; }
        public string type { get; set; }
    }
}
