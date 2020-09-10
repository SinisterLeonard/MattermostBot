using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Extensions;
using Nancy.Json;
using Nancy.Json.Simple;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MattermostBotBase
{
    public sealed class Mattermost : NancyModule
    {
        public Mattermost()
        {
            //Added to Team => Welcome XYZ
            //EMOJI ADDED => INFORM IN EMOJI CHANNEL
            //

            Get("", o => $"Usage: /mattermost/users /mattermost/backendposts /mattermosts/backendposts/format");
            Get("mattermost/users", o =>
            {
                Task<string> result = MakeApiCall("https://mattermost.it-emp.net/api/v4/users",
                    "{\"login_id\":\"" + Request.Query.email + "\",\"password\":\"" + Request.Query.password + "\"}", HttpMethod.Get, true);
                return result.Result;
            });
            Get("mattermost/backendposts", o =>
            {
                Task<string> result = GetChannels("{\"login_id\":\"" + Request.Query.email + "\",\"password\":\"" + Request.Query.password + "\"}");
                return result.Result;
            });
            Get("mattermost/backendposts/format", o =>
            {
                Task<string> result = GetChannels("{\"login_id\":\"" + Request.Query.email + "\",\"password\":\"" + Request.Query.password + "\"}");
                Task<string> res2 = FormatChannelPosts(result.Result);
                return res2.Result;
            });
        }

        private static readonly string MattermostBaseUrl = "https://mattermost.it-emp.net/api/v4";
        private static readonly string MattermostLogin = "/users/login";
        private static readonly string MattermostPosts = "/posts";
        private static readonly string MattermostUsersMe = "/users/me";
        private static readonly string MattermostBotChannelPosts = "/channels/e5d7bsf6qpnw3mntiztec63haw/posts";
        private static readonly string MattermostEmojis = "/emoji?per_page=200";

        private static readonly string InspiroBot = "http://inspirobot.me/api?generate=true";
        private static readonly string RandomFact = "https://uselessfacts.jsph.pl/random.json?language=de";
        private static readonly string RandomGeekJoke = "https://geek-jokes.sameerkumar.website/api";

        private static string Token { get; set; } = "";
        private static string UserId { get; set; } = "";

        private async Task<string> GetCurrentUser()
        {
            HttpClient client = new HttpClient();
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, MattermostBaseUrl + MattermostUsersMe);
            requestMessage.Headers.Authorization =
                AuthenticationHeaderValue.Parse("Bearer " + Token);

            var response2 = await client.SendAsync(requestMessage);

            return await response2.Content.ReadAsStringAsync();
        }
        private async Task<string> GetCall(string url, bool withAuth = false)
        {
            HttpClient client = new HttpClient();
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);


            if (withAuth)
                requestMessage.Headers.Authorization =
                    AuthenticationHeaderValue.Parse("Bearer " + Token);

            var response2 = await client.SendAsync(requestMessage);
            return await response2.Content.ReadAsStringAsync();
        }



        private async Task<string> GetChannels(string parameters)
        {
            Task<string> result = MakeApiCall(MattermostBaseUrl + MattermostBotChannelPosts, parameters, HttpMethod.Get, true);
            return result.Result;
        }

        private async Task<string> GetAuthToken(string parameters)
        {
            HttpClient client = new HttpClient();

            // Add a new Request Message
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, MattermostBaseUrl + MattermostLogin)
            {
                Content = new StringContent(parameters,
                    Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(requestMessage);


            if (response.IsSuccessStatusCode)
            {
                Token = response.Headers.GetValues("token").FirstOrDefault();
            }
            else
            {
                Token = ""; //null;//response.ReasonPhrase;
            }

            Task<string> UserTask = GetCurrentUser();
            var jsonObject = JObject.Parse(UserTask.Result);
            UserId = jsonObject.GetValue("id").ToString();

            return Token;
        }

        private async Task<string> FormatChannelPosts(string json)
        {

            var jsonObject = JObject.Parse(json);
            var posts = jsonObject.GetValue("posts");

            var postliste = new List<MattermostPost>();
            var checkChildren = new List<MattermostPost>();
            foreach (var child in posts.Children())
            {
                foreach (var child2 in child.Children())
                {
                    var test = new MattermostPost();
                    test.channel_id = child2["channel_id"].ToString();
                    test.create_at = child2["create_at"].ToString();
                    test.delete_at = child2["delete_at"].ToString();
                    test.edit_at = child2["edit_at"].ToString();
                    test.hashtags = child2["hashtags"].ToString();
                    test.id = child2["id"].ToString();
                    test.is_pinned = child2["is_pinned"].ToString();
                    test.message = child2["message"].ToString();
                    test.metadata = child2["metadata"].ToString();
                    test.original_id = child2["original_id"].ToString();
                    test.user_id = child2["user_id"].ToString();
                    test.update_at = child2["update_at"].ToString();
                    test.type = child2["type"].ToString();
                    test.root_id = child2["root_id"].ToString();
                    test.props = child2["props"].ToString();
                    test.pending_post_id = child2["pending_post_id"].ToString();
                    test.parent_id = child2["parent_id"].ToString();
                    postliste.Add(test);

                    if (test.message.Contains("!fact") || test.message.Contains("!inspirobot") || test.message.Contains("!geekjoke") || test.message.Contains("!emojis"))
                    {
                        checkChildren.Add(test);
                    }
                }
            }

            foreach (var check in checkChildren)
            {
                var noMatch = true;
                foreach (var post in postliste)
                {
                    if (check.id == post.parent_id && check.id != UserId)
                    {
                        noMatch = false;
                    }
                }

                if (noMatch)
                {
                    var result = "";

                    HttpClient client = new HttpClient();

                    if (Token != null)
                    {
                        var requestMessage = new HttpRequestMessage(HttpMethod.Post, MattermostBaseUrl + MattermostPosts);
                        requestMessage.Headers.Authorization =
                            AuthenticationHeaderValue.Parse("Bearer " + Token);


                        var message = "";


                        if (check.message.Contains("!inspirobot"))
                        {
                            Task<string> imageGetter = GetCall(InspiroBot);
                            var imageUrl = imageGetter.Result;
                            message += "InspiroBot image: " + imageUrl + "    ";
                        }
                        if (check.message.Contains("!fact"))
                        {
                            Task<string> randomFact = GetCall(RandomFact);
                            var jsonObjectRandomFact = JObject.Parse(randomFact.Result);
                            var randomFactString = jsonObjectRandomFact.GetValue("text").ToString();
                            message += "Random Fact: " + randomFactString + "    ";
                        }
                        if (check.message.Contains("!geekjoke"))
                        {
                            Task<string> randomFact = GetCall(RandomGeekJoke);
                            message += "Random Geek Fact: " + randomFact.Result.Replace("\"", "").Replace("\r", "").Replace("\n", "") + "    ";
                        }
                        if (check.message.Contains("!emojis"))
                        {
                            List<string> emojiNames = new List<string>();
                            var unfinished = true;
                            int counter = 0;
                            do
                            {
                                var page = "";

                                if (counter > 0)
                                    page = "&page=" + counter;

                                Task<string> emojis = GetCall(MattermostBaseUrl + MattermostEmojis + page, true);
                             
                                var jsonArray = JArray.Parse(emojis.Result);

                                foreach (var emoji in jsonArray)
                                {
                                    emojiNames.Add(emoji["name"].ToString());
                                }

                                if (jsonArray.Count < 200)
                                {
                                    unfinished = false;
                                }

                                counter++;
                            } while (unfinished);

                            foreach (var emojiName in emojiNames)
                            {
                                message += ":" + emojiName + ": ";
                            }
                        }

                        requestMessage.Content = new StringContent("{\"channel_id\":\"e5d7bsf6qpnw3mntiztec63haw\",\"message\":\"" + message + "\", \"root_id\":\"" + check.id + "\"}", Encoding.UTF8, "application/json");


                        var response2 = await client.SendAsync(requestMessage);
                        result = await response2.Content.ReadAsStringAsync();

                    }
                }

            }

            return json;
        }







        private async Task<string> MakeApiCall(string url, string parameters, HttpMethod httpMethod, bool withAuth = false)
        {
            var result = "";

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);

            if (Token == "")
                Token = await GetAuthToken(parameters);

            var requestMessage = new HttpRequestMessage(httpMethod, url);

            if (withAuth)
                requestMessage.Headers.Authorization =
                    AuthenticationHeaderValue.Parse("Bearer " + Token);

            var response2 = await client.SendAsync(requestMessage);

            result = await response2.Content.ReadAsStringAsync();

            client.Dispose();

            return result;
        }
    }
}