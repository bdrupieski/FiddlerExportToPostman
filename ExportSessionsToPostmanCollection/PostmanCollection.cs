using System.Collections.Generic;
using Newtonsoft.Json;

namespace ExportSessionsToPostmanCollection
{
    public class PostmanCollection
    {
        public PostmanInfo Info { get; set; }
        [JsonProperty("item")]
        public List<PostmanItem> Items { get; set; }
    }

    public class PostmanInfo
    {
        public string Name { get; set; }
        public string Schema => "https://schema.getpostman.com/json/collection/v2.1.0/collection.json";
    }

    public class PostmanItem
    {
        public string Name { get; set; }
        public PostmanRequest Request { get; set; }
        public PostmanResponse Response { get; set; }
    }

    public class PostmanRequest
    {
        public PostmanRequestAuth Auth { get; set; }
        public string Method { get; set; }
        public List<PostmanListItem> Header { get; set; }
        public PostmanBody Body { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
    }

    public class PostmanUrl
    {
        public string Raw { get; set; }
        public string Protocol { get; set; }
        public List<string> Host { get; set; }
        public List<string> Path { get; set; }
    }

    public class PostmanBody
    {
        public string Mode { get; set; }
        public string Raw { get; set; }
    }

    public class PostmanRequestAuth
    {
        public string Type { get; set; }
        public List<PostmanListItem> Basic { get; set; }
    }

    public class PostmanResponse
    {
        public string Name { get; set; }
        public PostmanRequest OriginalRequest { get; set; }
        public string Status { get; set; }
        public int Code { get; set; }
        public List<PostmanListItem> Header { get; set; }
        public List<PostmanListItem> Cookie { get; set; }
        public string Body { get; set; }
    }

    public class PostmanListItem
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
    }
}