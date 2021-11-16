using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.AI.TextAnalytics;
using Azure;
using System.Linq;
using Twilio.TwiML;

namespace WeWantTheFunc
{
    public static class IncomingSms
    {
        private static TextAnalyticsClient Client = InitializeTextAnalyticsClient();

        [FunctionName("IncomingSms")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("IncomingSms function triggered");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();            

            var body = GetMessageBody(requestBody);                
            log.LogInformation($"Received: {body}");

            // Run the text analysis
            DocumentSentiment documentSentiment = Client.AnalyzeSentiment(body);
            Console.WriteLine($"Document sentiment: {documentSentiment.Sentiment}\n");

            var reply = string.Empty;
            foreach (var sentence in documentSentiment.Sentences)
            {
                log.LogInformation($"\tText: \"{sentence.Text}\"");
                log.LogInformation($"\tSentence sentiment: {sentence.Sentiment}");
                log.LogInformation($"\tPositive score: {sentence.ConfidenceScores.Positive:0.00}");
                log.LogInformation($"\tNegative score: {sentence.ConfidenceScores.Negative:0.00}");
                log.LogInformation($"\tNeutral score: {sentence.ConfidenceScores.Neutral:0.00}\n");

                reply = $"Sentiment: {sentence.Sentiment}, Score: {sentence.ConfidenceScores.Positive:0.00}";
                break;
            }

            if (string.IsNullOrEmpty(reply))
                reply = "Text analysis unavailable";

            return new OkObjectResult(reply);
        }

        private static string GetMessageBody(string data)
        {
            try
            {
                // Twilio messages
                var formValues = data.Split('&')
                    .Select(value => value.Split('='))
                    .ToDictionary(pair => Uri.UnescapeDataString(pair[0]).Replace("+", " "),
                        pair => Uri.UnescapeDataString(pair[1]).Replace("+", " "));

                return formValues["Body"];
            }
            catch (Exception)
            {
                // Testing
                return data;
            }
        }

        private static TextAnalyticsClient InitializeTextAnalyticsClient()
        {
            var credentials = new AzureKeyCredential(Environment.GetEnvironmentVariable("TextAnalyticsApiKey"));
            var endpoint = new Uri(Environment.GetEnvironmentVariable("TextAnalyticsEndpoint"));

            var client = new TextAnalyticsClient(
                endpoint,
                credentials);

            return client;
        }
    }
}
