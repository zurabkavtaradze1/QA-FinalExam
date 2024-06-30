using Bogus;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Net.Http;
using System.Text;

namespace FinalExam
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            // 1.	Generate Faker or Random Data:
            var faker = new Faker<Model>()
            .RuleFor(m => m.name, f => f.Name.FullName())
            .RuleFor(m => m.email, f => f.Internet.Email())
            .RuleFor(m => m.phone, f => f.Phone.PhoneNumber())
            .RuleFor(m => m.subject, f => f.Random.Word())
            .RuleFor(m => m.description, f => f.Lorem.Sentence());

            Model model = faker.Generate();

            try
            {
                var email = await SendRequestAsync(model);
                SubmitForm(email);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Post request could not succeed");
                throw ex;
            }
        }

        // 5.	Automate Form Submission (Selenium):
        private static void SubmitForm(string email)
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("--disable-notifications");

            using (var driver = new ChromeDriver(chromeOptions))
            {
                driver.Manage().Window.Maximize();
                driver.Navigate().GoToUrl("https://getbootstrap.com/docs/4.0/components/forms/");
                IWebElement element = driver.FindElement(By.Id("exampleInputEmail1"));
                System.Threading.Thread.Sleep(2000);
                element.SendKeys(email);

                string enteredEmail = element.GetAttribute("value");

                // 6.	Verify Email Entry:
                if (email != enteredEmail)
                    throw new Exception("Invalid email entered.");

                element.Submit();

                System.Threading.Thread.Sleep(2000);

                // 7.	Close Browser:
                driver.Quit();
            }
        }

        // 2.	Send HTTP POST Request:
        private static async System.Threading.Tasks.Task<string> SendRequestAsync(Model payload)
        {
            string url = "https://automationintesting.online/message/";

            string jsonPayload = JsonConvert.SerializeObject(payload);

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using (var httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.PostAsync(url, content);

                // 3.	Handle Response:
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var responseModel = JsonConvert.DeserializeObject<ResponseModel>(responseBody);

                    // 4.	Extract Email:
                    return responseModel.email;
                }
                else
                {
                    throw new Exception($"HTTP Error: {response.StatusCode}");
                }
            }
        }
    }

    class Model
    {
        public string name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string subject { get; set; }
        public string description { get; set; }
    }

    class ResponseModel : Model
    {
        public string messageId { get; set; }
    }
}
