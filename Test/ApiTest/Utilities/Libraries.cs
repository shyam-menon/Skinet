using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiTest.Utilities
{
    public static class Libraries
    {

        public static Dictionary<string, string> DeserializeResponse(this IRestResponse restResponse)
        {
            var JSONObj = new JsonDeserializer().Deserialize<Dictionary<string, string>>(restResponse);

            return JSONObj;
        }

        public static string GetResponseObject(this IRestResponse response, string responseObject)
        {
            JObject obs = JObject.Parse(response.Content);
            return obs[responseObject].ToString();
        }

        public static string GetResponseObjectv2(this IRestResponse response, string responseObject)
        {
            JObject obs = JObject.Parse(response.Content.TrimStart(new char[] { '[' }).TrimEnd(new char[] { ']' }));
            return obs[responseObject].ToString();
        }



        public static string GetResponseObjectArray(this IRestResponse response, string responseObject)
        {


            JArray jArray = JArray.Parse(response.Content);
            foreach (var content in jArray.Children<JObject>())
            {
                foreach (JProperty property in content.Properties())
                {
                    if (property.Name == responseObject)
                        return property.Value.ToString();
                }
            }

            return string.Empty;
        }

        public static async Task<IRestResponse<T>> ExecuteAsyncRequest<T>(this RestClient client, IRestRequest request) where T : class, new()
        {
            var taskCompletionSource = new TaskCompletionSource<IRestResponse<T>>();

            client.ExecuteAsync<T>(request, restResponse =>
            {
                if (restResponse.ErrorException != null)
                {
                    const string message = "Error retrieving response.";
                    throw new ApplicationException(message, restResponse.ErrorException);
                }

                taskCompletionSource.SetResult(restResponse);
            });

            return await taskCompletionSource.Task;
        }


    }
}
