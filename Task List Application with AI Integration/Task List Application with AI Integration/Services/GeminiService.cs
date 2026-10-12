using Mscc.GenerativeAI;
using TaskAi.Api.Models;

namespace TaskAi.Api.Services
{
    public class GeminiService
    {
        private readonly string _apiKey;
        private readonly GoogleAI _googleAi;
        private readonly GenerativeModel _model;

        public GeminiService(IConfiguration config)
        {
            _apiKey = config["Gemini:ApiKey"] ?? config["GEMINI_API_KEY"]
                ?? throw new InvalidOperationException("Configure Gemini:ApiKey or GEMINI_API_KEY to use AI features.");
            _googleAi = new GoogleAI(_apiKey);
            // Using gemini-2.5-flash
            _model = _googleAi.GenerativeModel("gemini-2.5-flash");
        }

        public async Task<string> GetTaskSummaryAsync(List<TaskItem> tasks, string username)
        {
            // Prepare the data for the AI
            var today = DateTime.UtcNow.Date;
            var next7Days = today.AddDays(7);

            var totalCount = tasks.Count;
            var dueToday = tasks.Where(t => t.DueDate?.Date == today).Select(t => t.Title).ToList();
            var dueSoon = tasks.Where(t => t.DueDate?.Date > today && t.DueDate?.Date <= next7Days).Select(t => t.Title).ToList();

            // Construct the Prompt
            string prompt = $@"
                Context: You are a helpful AI Task Assistant for a user named {username}.
                Data:
                - Total tasks: {totalCount}
                - Tasks due today: {(dueToday.Any() ? string.Join(", ", dueToday) : "None")}
                - Tasks due in the next 7 days: {(dueSoon.Any() ? string.Join(", ", dueSoon) : "None")}

                Requirement: Please provide a friendly, conversational summary of these tasks. 
                Encourage the user and highlight what they should focus on first. Keep it concise.";

            var response = await _model.GenerateContent(prompt);
            return response.Text ?? "I couldn't generate a summary at this time.";
        }

        public async Task<string> ExtractTasksFromTextAsync(string documentText)
        {
            string prompt = $@"
                Extract tasks from the following text. 
                For each task, identify:
                1. Task Title (Action item)
                2. Due Date (If mentioned, otherwise null)
                3. Priority (High, Medium, Low based on context)
                4. Category (Work, Personal, etc.)

                Return ONLY a clean JSON array of objects. 
                Example format: 
                [{{ ""title"": ""Fix bug"", ""dueDate"": ""2026-02-01"", ""priority"": ""High"", ""category"": ""Work"" }}]

                Text to analyze:
                {documentText}";

            var response = await _model.GenerateContent(prompt);

            // The ?? provides an empty string fallback if the entire chain results in null.
            string cleanJson = response.Text?.Replace("```json", "").Replace("```", "").Trim() ?? "";
            return cleanJson;
        }
    }
}