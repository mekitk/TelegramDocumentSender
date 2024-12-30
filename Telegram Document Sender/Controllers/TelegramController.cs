using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Telegram_Document_Sender.Controllers
{
    [Route("api/telegram")]
    [ApiController]
    public class TelegramController : Controller
    {
        [HttpPost("send")]
        public async Task<IActionResult> SendToTelegram([FromForm] TelegramRequest request)
        {
            if (string.IsNullOrEmpty(request.BotToken) || string.IsNullOrEmpty(request.ChatId))
            {
                return BadRequest(new { Message = "Bot Token and Chat ID are required." });
            }

            bool messageSent = false;
            bool documentSent = false;

            // Mesaj Gönderme
            if (!string.IsNullOrEmpty(request.Message))
            {
                var messageResponse = await SendMessage(request);
                if (messageResponse is OkObjectResult)
                {
                    messageSent = true;
                }
            }

            // Dosya Gönderme
            if (request.Document != null && request.Document.Length > 0)
            {
                var documentResponse = await SendDocument(request);
                if (documentResponse is OkObjectResult)
                {
                    documentSent = true;
                }
            }

            if (messageSent && documentSent)
            {
                return Ok(new { Message = "Message and Document sent successfully!" });
            }
            else if (messageSent)
            {
                return Ok(new { Message = "Message sent successfully, but no Document provided." });
            }
            else if (documentSent)
            {
                return Ok(new { Message = "Document sent successfully, but no Message provided." });
            }

            return BadRequest(new { Message = "Please provide either a message or a valid document." });
        }
        [HttpPost("sendMessage")]
        private async Task<IActionResult> SendMessage(TelegramRequest request)
        {
            try
            {
                var telegramApiUrl = $"https://api.telegram.org/bot{request.BotToken}/sendMessage";

                using (var httpClient = new HttpClient())
                {
                    var parameters = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("chat_id", request.ChatId),
                        new KeyValuePair<string, string>("text", request.Message)
                    });

                    var response = await httpClient.PostAsync(telegramApiUrl, parameters);

                    if (response.IsSuccessStatusCode)
                    {

                        return Ok(new { Message = "Message sent successfully!" });

                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        return BadRequest(new { Message = "Failed to send message", Details = error });
                    }
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred", Details = ex.Message });
            }
        }

        private async Task<IActionResult> SendDocument(TelegramRequest request)
        {
            try
            {
                var telegramApiUrl = $"https://api.telegram.org/bot{request.BotToken}/sendDocument";

                using (var httpClient = new HttpClient())
                using (var content = new MultipartFormDataContent())
                {
                    content.Add(new StringContent(request.ChatId), "chat_id");

                    var fileContent = new StreamContent(request.Document.OpenReadStream());
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(request.Document.ContentType);
                    content.Add(fileContent, "document", request.Document.FileName);

                    var response = await httpClient.PostAsync(telegramApiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        return Ok(new { Message = "Document sent successfully!" });
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        return BadRequest(new { Message = "Failed to send document", Details = error });
                    }
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred", Details = ex.Message });
            }
        }
    }

    public class TelegramRequest
    {
        public string BotToken { get; set; }
        public string ChatId { get; set; }
        public string Message { get; set; }
        public IFormFile Document { get; set; }
    }
}