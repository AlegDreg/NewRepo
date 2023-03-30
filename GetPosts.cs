using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VkLikes.Posts
{
    public class GetPosts
    {
        private string token;
        private int _delay;
        private int _parallelCount;
        public string name;
        private string url;

        public GetPosts(int id, bool isUser, string _token, int delay, int parallelCount)
        {
            _parallelCount = parallelCount;
            _delay = delay;
            token = _token;

            if (isUser)
            {
                if (id < 0)
                    id *= -1;
            }
            else
            {
                if (id > 0)
                    id *= -1;
            }

            name = id.ToString();

            url = $"https://api.vk.com/method/wall.get?access_token={token}&v=5.131&owner_id={id}";
        }

        public async Task<List<Items>> Parse()
        {
            List<Items> result;
            ResponseVk e;
            try
            {
                result = new List<Items>();

                string res = Download(url);

                e = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseVk>(res);

                if (e.response == null)
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            long c = (e.response.count - (e.response.count % 100)) / 100;

            result.AddRange(e.response.items.ToList());




            List<Task> tasks = new List<Task>();

            for (long i = 0; i < c + 1; i++)
            {
                long id = i;
                tasks.Add(new Task(() =>
                {
                    try
                    {
                        var rs = ParseV(url + $"&offset={id * 100 + e.response.items.Count()}&count=100");
                        result.AddRange(rs);

                        Console.WriteLine($"Посты {id + 1} из {c}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка постов {id + 1} из {c} - {ex.Message} {ex.InnerException}");
                    }
                }));
            }

            TaskExecution taskExecution = new TaskExecution();
            var bool_res = taskExecution.Start(tasks, _parallelCount, _delay).Result;

            Console.WriteLine("Постов - " + result.Count);

            return result;
        }

        public List<Items> ParseV(string url)
        {
            string res = Download(url);

            var e = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseVk>(res);

            return e.response.items.ToList();
        }

        private string Download(string url)
        {
            using (WebClient web = new WebClient())
            {
                web.Encoding = Encoding.UTF8;
                return web.DownloadString(url);
            }
        }
    }

    public class ResponseVk
    {
        public Response response { get; set; }
    }

    public class Response
    {
        public long count { get; set; }
        public Items[] items { get; set; }
    }

    public class Items
    {
        public int id { get; set; }
        public int from_id { get; set; }
        public int owner_id { get; set; }
        public int date { get; set; }
        public int marked_as_ads { get; set; }
        public bool is_favorite { get; set; }
        public string post_type { get; set; }
        public string text { get; set; }
        public Attachment[] attachments { get; set; }
        public post_source post_source { get; set; }
        public comments comments { get; set; }
        public likes likes { get; set; }
        public reposts reposts { get; set; }
        public views views { get; set; }
        public donut donut { get; set; }
        public double short_text_rate { get; set; }
        public string hash { get; set; }
    }

    public class Attachment
    {
        public string type { get; set; }
        public Photo photo { get; set; }
    }

    public class Photo
    {
        public int album_id { get; set; }
        public int date { get; set; }
        public int id { get; set; }
        public int owner_id { get; set; }
        public string access_key { get; set; }
        public int post_id { get; set; }
        public string text { get; set; }
        public int user_id { get; set; }
        public bool has_tags { get; set; }
        public Size[] sizes { get; set; }
    }

    public class Size
    {
        public int height { get; set; }
        public int width { get; set; }
        public string url { get; set; }
        public string type { get; set; }
    }

    public class post_source
    {
        public string type { get; set; }
    }

    public class comments
    {
        public int count { get; set; }
        public int can_post { get; set; }
        public bool groups_can_post { get; set; }
    }

    public class views
    {
        public int count { get; set; }
    }

    public class donut
    {
        public bool is_donut { get; set; }
    }

    public class reposts
    {
        public int count { get; set; }
        public int user_reposted { get; set; }
    }

    public class likes
    {
        public int can_like { get; set; }
        public int count { get; set; }
        public int user_likes { get; set; }
        public int can_publish { get; set; }
    }
}