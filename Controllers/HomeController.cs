//using Microsoft.AspNetCore.Mvc;
//using System.Diagnostics;
//using UseCase3._0.Models;

//namespace UseCase3._0.Controllers
//{
//    public class HomeController : Controller
//    {
//        private readonly ILogger<HomeController> _logger;

//        public HomeController(ILogger<HomeController> logger)
//        {
//            _logger = logger;
//        }

//        public IActionResult Index()
//        {
//            return View();
//        }

//        public IActionResult Privacy()
//        {
//            return View();
//        }

//        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
//        public IActionResult Error()
//        {
//            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;

namespace UseCase3._0.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
           return View();
       }
      

        // OpenAI API key
        private const string API_KEY = "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";
        // Email configuration
        private const string EMAIL_USERNAME = "sreepriyapasalvadhi@gmail.com";
        private const string EMAIL_PASSWORD = "Sree@4236";
        private const string EMAIL_HOST = "smtp.gmail.com";
        private const int EMAIL_PORT = 587;

        private static readonly SmtpClient smtpClient = new SmtpClient(EMAIL_HOST, EMAIL_PORT)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(EMAIL_USERNAME, EMAIL_PASSWORD)
        };

        private static readonly MailAddress fromAddress = new MailAddress(EMAIL_USERNAME);

        private static string GetChatbotResponse(string prompt)
        {
            string result = string.Empty;
            try
            {
                string apiUrl = "https://api.openai.com/v1/chat/completions";
                WebClient webClient = new WebClient();
                webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                webClient.Headers[HttpRequestHeader.Authorization] = "Bearer " + API_KEY;

                // Prepare the request body
                var requestBody = new
                {
                    messages = new dynamic[]
                    {
                        new { role = "system", content = "You are a helpful assistant." },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 3000,
                    temperature = 0.2,
                    model = "gpt-3.5-turbo"
                };

                string requestBodyJson = JsonConvert.SerializeObject(requestBody);
                string response = webClient.UploadString(apiUrl, "POST", requestBodyJson);

                // Parse the API response
                result = ParseChatbotResponse(response);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error making API request: " + e.Message);
            }

            return result;
        }

        private static string ParseChatbotResponse(string response)
        {
            string result = string.Empty;
            try
            {
                dynamic jsonObject = JsonConvert.DeserializeObject(response);
                if (jsonObject.choices.Count > 0)
                {
                    result = jsonObject.choices[0].message.content;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error parsing API response: " + e.Message);
            }
            return result;
        }

        private static void SendEmail(string result, string email)
        {
            try
            {
                MailMessage mailMessage = new MailMessage(fromAddress, new MailAddress(email))
                {
                    Subject = "Chatbot Result",
                    Body = result
                };
                smtpClient.Send(mailMessage);
                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error sending email: " + e.Message);
            }
        }

        public static string GetChatbotResponseForOption(string option, string prompt)
        {
            return GetChatbotResponse(option + ", " + prompt);
        }


       // Other using statements...



            [HttpPost]
            public ActionResult GetResponse(IFormCollection form)
            {
                // Retrieve user choice and prompt from the form data
                string choice = form["choice"];
                string prompt = form["prompt"];

                // Get the chatbot response
                string response = GetChatbotResponseForOption(choice, prompt);

                // Store the chatbot response in the ViewBag to display it in the view
                ViewBag.Result = response;

                // Return the Index view with the chatbot response
                return View("Index");
            }

            [HttpPost]
            public ActionResult SendEmail(IFormCollection form)
            {
                // Retrieve the email address and chatbot response from the form data
                string email = form["email"];
                string response = ViewBag.Result;

                // Send the chatbot response via email
                SendEmail(response, email);

                // Return the Index view with the chatbot response (the ViewBag.Result will be empty)
                return View("Index");
            }
        }
    }


