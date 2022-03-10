using System.Net;
using System.Text;
using Infra.Dto;
using Newtonsoft.Json;

namespace Hook_Api.Helpers
{
    public class AppToken
    {
        // The description of the authorization method is available here: https://developers.sumsub.com/api-reference/#app-tokens
        private static readonly string SUMSUB_SECRET_KEY = "TqsyALW3bKCONmvFeBzMo21FUFD5zoH3"; // Example: TqsyALW3bKCONmvFeBzMo21FUFD5zoH3 - Please don't forget to change when switching to production
        private static readonly string SUMSUB_APP_TOKEN = "sbx:0ge5ViqDHi5m7aNmc3lzse3U.cndEWhXf2VPeaEBjqrGlDuBBaKxjT6Kn";  // Example: sbx:uY0CgwELmgUAEyl4hNWxLngb.0WSeQeiYny4WEqmAALEAiK2qTC96fBad - Please don't forget to change when switching to production
        private static readonly string SUMSUB_TEST_BASE_URL = "https://api.sumsub.com";

        // https://developers.sumsub.com/api-reference/#getting-applicant-status-sdk
        public async Task<ApplicantDto> CreateApplicant(string externalUserId, string levelName)
        {
            Console.WriteLine("Creating an applicant...");

            var body = new
            {
                externalUserId = externalUserId,
                email =  "alican.bulunur1@gmail.com",
                phone = "+$905312439774",
            };

            // Create the request body
            var requestBody = new HttpRequestMessage(HttpMethod.Post, SUMSUB_TEST_BASE_URL)
            {
                Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
            };

            // Get the response
            var response = await SendPost($"/resources/applicants?levelName={levelName}", requestBody);
            var applicant = JsonConvert.DeserializeObject<ApplicantDto>(ContentToString(response.Content));

            Console.WriteLine(response.IsSuccessStatusCode
                ? $"The applicant was successfully created: {applicant?.Id}"
                : $"ERROR: {ContentToString(response.Content)}");

            return applicant?? new ApplicantDto();
        }

        // https://developers.sumsub.com/api-reference/#adding-an-id-document
        public async Task<HttpResponseMessage> AddDocument(string applicantId)
        {
            Console.WriteLine("Adding document to the applicant...");

            // metadata object
            var metaData = new
            {
                idDocType = "PASSPORT",
                country = "GBR"
            };

            using (var formContent = new MultipartFormDataContent())
            {
                // Add metadata json object
                formContent.Add(new StringContent(JsonConvert.SerializeObject(metaData)), "\"metadata\"");

                // Add binary content
                var binaryImage = File.ReadAllBytes("../../resources/sumsub-logo.png");
                formContent.Add(new StreamContent(new MemoryStream(binaryImage)), "content", "sumsub-logo.png");

                // Request body
                var requestBody = new HttpRequestMessage(HttpMethod.Post, SUMSUB_TEST_BASE_URL)
                {
                    Content = formContent
                };

                var response = await SendPost($"/resources/applicants/{applicantId}/info/idDoc", requestBody);

                Console.WriteLine(response.IsSuccessStatusCode
                    ? $"Document was successfully added"
                    : $"ERROR: {ContentToString(response.Content)}");

                return response;
            }
        }

        // https://developers.sumsub.com/api-reference/#getting-applicant-status-api
        public async Task<HttpResponseMessage> GetApplicantStatus(string applicantId)
        {
            Console.WriteLine("Getting the applicant status...");

            var response = await SendGet($"/resources/applicants/{applicantId}/requiredIdDocsStatus");
            return response;
        }

        public  async Task<HttpResponseMessage> GetAccessToken(string applicantId, string levelName)
        {
            var response = await SendPost($"/resources/accessTokens?userId={applicantId}&levelName={levelName}", new HttpRequestMessage());
            return response;
        }

        private  async Task<HttpResponseMessage> SendPost(string url, HttpRequestMessage requestBody)
        {

            var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var signature = CreateSignature(ts, HttpMethod.Post, url, RequestBodyToBytes(requestBody));

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var client = new HttpClient
            {
                BaseAddress = new Uri(SUMSUB_TEST_BASE_URL)
            };
            client.DefaultRequestHeaders.Add("X-App-Token", SUMSUB_APP_TOKEN);
            client.DefaultRequestHeaders.Add("X-App-Access-Sig", signature);
            client.DefaultRequestHeaders.Add("X-App-Access-Ts", ts.ToString());

            var response = await client.PostAsync(url, requestBody.Content);

            if (!response.IsSuccessStatusCode)
            {
                // https://developers.sumsub.com/api-reference/#errors
                // If an unsuccessful answer is received, please log the value of the "correlationId" parameter.
                // Then perhaps you should throw the exception. (depends on the logic of your code)
            }

            // debug
            //var debugInfo = response.Content.ReadAsStringAsync().Result;
            return response;
        }

        private  async Task<HttpResponseMessage> SendGet(string url)
        {
            long ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var client = new HttpClient
            {
                BaseAddress = new Uri(SUMSUB_TEST_BASE_URL)
            };
            client.DefaultRequestHeaders.Add("X-App-Token", SUMSUB_APP_TOKEN);
            client.DefaultRequestHeaders.Add("X-App-Access-Sig", CreateSignature(ts, HttpMethod.Get, url, null));
            client.DefaultRequestHeaders.Add("X-App-Access-Ts", ts.ToString());

            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                // https://developers.sumsub.com/api-reference/#errors
                // If an unsuccessful answer is received, please log the value of the "correlationId" parameter.
                // Then perhaps you should throw the exception. (depends on the logic of your code)
            }

            return response;
        }

        private  string CreateSignature(long ts, HttpMethod httpMethod, string path, byte[] body)
        {
            Console.WriteLine("Creating a signature for the request...");

            var hmac256 = new System.Security.Cryptography.HMACSHA256(Encoding.ASCII.GetBytes(SUMSUB_SECRET_KEY));

            byte[] byteArray = Encoding.ASCII.GetBytes(ts + httpMethod.Method + path);

            if (body != null)
            {
                // concat arrays: add body to byteArray
                var s = new MemoryStream();
                s.Write(byteArray, 0, byteArray.Length);
                s.Write(body, 0, body.Length);
                byteArray = s.ToArray();
            }

            var result = hmac256.ComputeHash(
                new MemoryStream(byteArray)).Aggregate("", (s, e) => s + String.Format("{0:x2}", e), s => s);

            return result;
        }

        private  string ContentToString(HttpContent httpContent)
        {
            return httpContent == null ? "" : httpContent.ReadAsStringAsync().Result;
        }

        private  byte[] RequestBodyToBytes(HttpRequestMessage requestBody)
        {
            return requestBody.Content == null ?
                new byte[] { } : requestBody.Content.ReadAsByteArrayAsync().Result;
        }
    }
}