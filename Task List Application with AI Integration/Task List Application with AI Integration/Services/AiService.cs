using Microsoft.Extensions.Configuration;
using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Types;
using System.Text.Json;

namespace TaskAi.Api.Services
{
    public class AiService
    {
        private readonly string _apiKey;
        private readonly GoogleAI _ai;

        //public AiService()
        //{
        //    // v1 is required for stable 2.5 models
        //    _ai = new GoogleAI(_apiKey, "v1");
        //}

        public AiService(IConfiguration configuration)
        {
            _apiKey = configuration["Gemini:ApiKey"] ?? configuration["GEMINI_API_KEY"] ?? string.Empty;

            // Initialize with the key we just read
            _ai = new GoogleAI(_apiKey, "v1");
        }

        public async Task<string> GetConversationalSummary(string taskData)
        {
            var model = _ai.GenerativeModel("gemini-2.5-flash");
            var prompt = $"Summarize these tasks conversationally in 3 sentences: {taskData}";

            var response = await model.GenerateContent(prompt);

            // Fix "possibly null" by using ?? "" 
            return response.Text ?? "No summary available.";
        }

        // Changed return type from Task<string> to Task<object>
        public async Task<object> ExtractTasksFromText(string documentText)
        {
            var model = _ai.GenerativeModel("gemini-2.5-flash");
            var prompt = $"Extract tasks from: {documentText}. Return ONLY JSON with a 'tasks' array containing 'taskName' and 'dueDate'.";

            var response = await model.GenerateContent(prompt);

            // Use ! to tell compiler we've checked for null, or provide a fallback
            string rawText = response.Text ?? "{\"tasks\": []}";

            string cleanJson = rawText
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            try
            {
                // Deserialize into an object so it's sent as JSON, not a string
                var result = JsonSerializer.Deserialize<object>(cleanJson);
                return result ?? new { tasks = new List<object>() };
            }
            catch
            {
                // This now matches the Task<object> return type
                return new { tasks = new List<object>() };
            }
        }
    }
}