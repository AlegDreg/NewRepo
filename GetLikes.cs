using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using VkLikes.Posts;

namespace VkLikes.Likes
{
    public class GetLikes
    {
        private int _delay;
        private int _parallelCount;
        private List<Likes> res = new List<Likes>();

        public GetLikes(int delay, int parallelCount)
        {
            _parallelCount = parallelCount;
            _delay = delay;
        }

        public List<Likes> Get(List<Items> items, string token, bool friendsOnly)
        {
            string req = "https://api.vk.com/method/likes.getList?" +
            $"type=post" +
            $"&access_token={token}" +
            $"&friends_only=" + (friendsOnly ? 1 : 0) +
            $"&owner_id={items[0].owner_id}" +
            "&v=5.131";

            List<Task> tasks = new List<Task>();

            for (int i = 0; i < items.Count; i++)
            {
                int id = i;
                tasks.Add(new Task(() =>
                {
                    try
                    {
                        GetLike(req, items[id], id);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Ошибка поста лайков - " + ex.Message);
                    }
                }));
            }

            TaskExecution taskExecution = new TaskExecution();
            var r = taskExecution.Start(tasks, _parallelCount, _delay).Result;

            return res;
        }

        private void GetLike(string req, Items items, int id)
        {
            List<Task> tasks = new List<Task>();

            long c = (items.likes.count - (items.likes.count % 100)) / 100;
            Likes like = new Likes();
            like.owner_id = items.owner_id;
            like.postId = items.id;
            res.Add(like);

            for (int j = 0; j < c + 1; j++)
            {
                int idj = j;

                tasks.Add(new Task(() =>
                {
                    try
                    {
                        var likes = Download(req + $"&item_id={items.id}&offset={idj * 100}&count=100").Result;

                        if (likes != null)
                        {
                            like.items.AddRange(likes);

                            Console.WriteLine("Пост " + id + " пачка_лайков " + idj + " успешно");
                        }
                        else
                        {
                            throw new Exception("Нет данных");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Пост " + id + " пачка_лайков " + idj + " ошибка");
                    }
                }));
            }

            TaskExecution taskExecution = new TaskExecution();
            var r = taskExecution.Start(tasks, _parallelCount, _delay).Result;
        }

        private async Task<List<int>> Download(string req)
        {
            string r;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    r = wc.DownloadString(req);
                }

                var e = Newtonsoft.Json.JsonConvert.DeserializeObject<R>(r);

                if (e.response == null)
                {
                    await Task.Delay(1000);
                    Console.WriteLine("Ещё попытка..");
                    return Download(req).Result;
                }

                if (e != null)
                {
                    if (e.response == null || e.response.items == null)
                    {
                        await Task.Delay(1000);
                        Console.WriteLine("Ещё попытка.....");
                        return Download(req).Result;
                    }

                    if (e.response.items.Length > 0)
                        return e.response.items.ToList();
                    else
                        return null;
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

        public static void Proccess(string defUrl, List<Likes> likes)
        {
            for (int i = 0; i < likes.Count; i++)
            {
                if (likes[i] != null)
                    likes[i].url = $"{defUrl}?w=wall{likes[i].owner_id}_{likes[i].postId}";
                else
                {

                }
            }
        }

        public static SearchLiked SearchUserLike(List<Likes> c, int userID)
        {
            SearchLiked searchLiked = new SearchLiked();
            searchLiked.Liked = new List<Likes>();
            searchLiked.NoLiked = new List<Likes>();

            for (int i = 0; i < c.Count; i++)
            {
                if (c[i] != null && c[i].items != null)
                    if (c[i].items.Contains(userID))
                    {
                        searchLiked.Liked.Add(c[i]);
                    }
                    else
                    {
                        searchLiked.NoLiked.Add(c[i]);
                    }
            }

            return searchLiked;
        }
    }

    public class R
    {
        public R1 response { get; set; }
    }

    public class R1
    {
        public int count { get; set; }
        public int[] items { get; set; }
    }

    public class Likes
    {
        public List<int> items { get; set; } = new List<int>();
        public int postId { get; set; }
        public int owner_id { get; set; }
        public string url { get; set; }
    }

    public class SearchLiked
    {
        public List<Likes> Liked { get; set; }
        public List<Likes> NoLiked { get; set; }
    }
}
