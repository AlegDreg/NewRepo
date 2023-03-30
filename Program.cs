using System;
using System.IO;
using System.Linq;
using VkLikes.Likes;
using VkLikes.Posts;
using VkLikes.Url;

namespace VkLikes
{
    internal class Program
    {
        private static string token = "";

        static void Main(string[] args)
        {
            string wallUrl = "";
            string searchUserUrl = "";


            var a = new GetUserID().GetUrl(wallUrl, token);

            var b = new GetPosts(a.object_id, a.type == "user" ? true : false, token, 1500, 3).Parse().Result;

            var c = new GetLikes(900, 2).Get(b, token, false);

            GetLikes.Proccess(wallUrl, c);

            int userID = new GetUserID().GetUrl(searchUserUrl, token).object_id;
            var d = GetLikes.SearchUserLike(c, userID);

            Save(d.Liked.Select(x => x.url), @"C:\Users\oliso\Desktop\r.txt");

            Console.ReadKey();
        }

        static void Save<T>(T t, string path)
        {
            using (StreamWriter st = new StreamWriter(path))
            {
                st.Write(Newtonsoft.Json.JsonConvert.SerializeObject(t, Newtonsoft.Json.Formatting.Indented));
            }
        }
    }
}
