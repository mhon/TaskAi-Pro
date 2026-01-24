using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskAi.Api.Data;
using TaskAi.Api.Services;

namespace TaskAi.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly GeminiService _geminiService;

        public AiController(AppDbContext context, GeminiService geminiService)
        {
            _context = context;
            _geminiService = geminiService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var username = User.FindFirstValue(ClaimTypes.Name) ?? "User";

            // Get the user's tasks from the DB
            var tasks = await _context.Tasks
                .Where(t => t.UserId == userId)
                .ToListAsync();

            // Use the Gemini Service to get the conversational summary
            var summary = await _geminiService.GetTaskSummaryAsync(tasks, username);

            return Ok(new { Summary = summary });
        }

        [HttpPost("extract")]
        public async Task<IActionResult> ExtractFromDocument(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

            string extractedText = "";
            var extension = Path.GetExtension(file.FileName).ToLower();

            using var stream = file.OpenReadStream();

            if (extension == ".pdf")
            {
                using var pdf = UglyToad.PdfPig.PdfDocument.Open(stream);
                foreach (var page in pdf.GetPages()) extractedText += page.Text;
            }
            else if (extension == ".msg")
            {
                using var msg = new MsgReader.Outlook.Storage.Message(stream);
                extractedText = msg.BodyText; // Extracts the email body
            }
            else if (extension == ".txt")
            {
                using var reader = new StreamReader(stream);
                extractedText = await reader.ReadToEndAsync();
            }
            else
            {
                return BadRequest("Unsupported file type. Please upload PDF, MSG, or TXT.");
            }

            // Send the text to Gemini
            var jsonTasks = await _geminiService.ExtractTasksFromTextAsync(extractedText);

            // Using the built-in .NET JSON library instead of Newtonsoft
            return Ok(new
            {
                Source = file.FileName,
                ExtractedTasks = System.Text.Json.JsonSerializer.Deserialize<object>(jsonTasks)
            });
        }

    }
}