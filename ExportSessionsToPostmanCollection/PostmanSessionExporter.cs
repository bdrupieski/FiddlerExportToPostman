using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Fiddler;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

[assembly: RequiredVersion("2.0.0.0")]

namespace ExportSessionsToPostmanCollection
{
    [ProfferFormat("Postman", "Postman Collection schema version 2.1")]
    public class PostmanSessionExporter : ISessionExporter
    {
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy(),
            },
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
        };

        public bool ExportSessions(string sExportFormat, Session[] oSessions, Dictionary<string, object> dictOptions, EventHandler<ProgressCallbackEventArgs> evtProgressNotifications)
        {
            bool ReportProgress(float percentage, string message)
            {
                if (evtProgressNotifications == null)
                    return true;

                var eventArgs = new ProgressCallbackEventArgs(percentage, message);
                evtProgressNotifications(null, eventArgs);

                if (eventArgs.Cancel)
                    return false;

                return true;
            }

            try
            {
                string filenameKey = "Filename";
                string filename;

                if (dictOptions?.ContainsKey(filenameKey) == true)
                    filename = (string)dictOptions[filenameKey];
                else
                    filename = Utilities.ObtainSaveFilename($"Export As {sExportFormat}", "JSON Files (*.postman_collection.json)|*.postman_collection.json");

                if (string.IsNullOrEmpty(filename))
                    return false;

                if (oSessions.Length > 100)
                {
                    var confirmResult = MessageBox.Show($"You're about to export {oSessions.Length} sessions. That's a lot. Are you sure you want to do that?",
                        "Just Checking", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (confirmResult != DialogResult.Yes)
                        return false;
                }

                var name = Path.GetFileNameWithoutExtension(filename);
                var postmanCollection = new PostmanCollection
                {
                    Info = new PostmanInfo
                    {
                        Name = name
                    },
                    Items = new List<PostmanItem>(),
                };

                int countProcessed = 0;
                foreach (var session in oSessions)
                {
                    var postmanItem = new PostmanItem
                    {
                        Name = session.PathAndQuery,
                        Request = new PostmanRequest
                        {
                            Method = session.RequestMethod,
                            Header = new List<PostmanListItem>(session.RequestHeaders.Count() + 1),
                            Url = session.fullUrl,
                            Body = new PostmanBody
                            {
                                Mode = "raw",
                                Raw = session.GetRequestBodyAsString(),
                                Options = new PostmanRequestOptions
                                {
                                    Row = new PostmanRequestOptionsRow
                                    {
                                        Language = "json",
                                    }
                                }
                            },
                        },
                        Response = new List<PostmanResponse>(){
                            new PostmanResponse
                            {
                                Code = session.responseCode,
                                Name = session.PathAndQuery,
                                Body = session.GetResponseBodyAsString(),
                                Status = session.ResponseHeaders.StatusDescription,
                                Header = new List<PostmanListItem>(session.ResponseHeaders.Count() + 1),
                                _postman_previewlanguage = "json",
                            }
                        },
                    };

                    postmanItem.Response[0].OriginalRequest = postmanItem.Request;

                    foreach (var requestHeader in session.RequestHeaders)
                    {
                        if (requestHeader.Name == "Host") continue;
                        postmanItem.Request.Header.Add(new PostmanListItem
                        {
                            Type = "text",
                            Key = requestHeader.Name,
                            Value = requestHeader.Value
                        });
                    }

                    foreach (var responseHeader in session.ResponseHeaders)
                    {
                        postmanItem.Response[0].Header.Add(new PostmanListItem
                        {
                            Key = responseHeader.Name,
                            Value = responseHeader.Value
                        });
                    }

                    postmanCollection.Items.Add(postmanItem);

                    countProcessed++;
                    if (!ReportProgress(countProcessed / (float)oSessions.Length * 0.70f, $"{countProcessed} of {oSessions.Length}"))
                        return false;
                }

                if (!ReportProgress(0.80f, "Serializing JSON..."))
                    return false;

                var json = JsonConvert.SerializeObject(postmanCollection, _jsonSerializerSettings);

                if (!ReportProgress(0.90f, "Writing JSON..."))
                    return false;

                File.WriteAllText(filename, json);

                ReportProgress(1.00f, $"Finished Writing JSON to {filename}");

                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Failed to export. Exception: {e.Message}");
                return false;
            }
        }

        public void Dispose()
        {
        }
    }
}
