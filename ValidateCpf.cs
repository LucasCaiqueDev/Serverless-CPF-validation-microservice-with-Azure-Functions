using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CpfValidator
{
    public class ValidateCpf
    {
        private readonly ILogger _logger;

        public ValidateCpf(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ValidateCpf>();
        }

        [Function("ValidateCpf")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string? cpf = req.Query["cpf"];

            if (string.IsNullOrEmpty(cpf))
            {
                // Generate a valid CPF
                string generatedCpf = GenerateValidCpf();
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync($"Generated CPF: {generatedCpf}");
                return response;
            }
            else
            {
                bool isValid = IsValidCpf(cpf);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync(isValid ? "Valid" : "Invalid");
                return response;
            }
        }

        private static bool IsValidCpf(string cpf)
        {
            // Remove non-digits
            cpf = new string(cpf.Where(char.IsDigit).ToArray());

            if (cpf.Length != 11) return false;

            // Check if all digits are the same
            if (cpf.All(c => c == cpf[0])) return false;

            // Calculate first check digit
            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                sum += (cpf[i] - '0') * (10 - i);
            }
            int remainder = sum % 11;
            int firstCheck = remainder < 2 ? 0 : 11 - remainder;
            if (firstCheck != (cpf[9] - '0')) return false;

            // Calculate second check digit
            sum = 0;
            for (int i = 0; i < 10; i++)
            {
                sum += (cpf[i] - '0') * (11 - i);
            }
            remainder = sum % 11;
            int secondCheck = remainder < 2 ? 0 : 11 - remainder;
            return secondCheck == (cpf[10] - '0');
        }

        private static string GenerateValidCpf()
        {
            Random random = new Random();
            int[] cpf = new int[11];

            // Generate first 9 digits
            for (int i = 0; i < 9; i++)
            {
                cpf[i] = random.Next(0, 10);
            }

            // Calculate first check digit
            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                sum += cpf[i] * (10 - i);
            }
            int remainder = sum % 11;
            cpf[9] = remainder < 2 ? 0 : 11 - remainder;

            // Calculate second check digit
            sum = 0;
            for (int i = 0; i < 10; i++)
            {
                sum += cpf[i] * (11 - i);
            }
            remainder = sum % 11;
            cpf[10] = remainder < 2 ? 0 : 11 - remainder;

            return string.Join("", cpf);
        }
    }
}