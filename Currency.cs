using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Collections.Generic;

namespace Contoso.CurrencyExchange
{

     public class CurrencyRale
    {
        public bool status { get; set; }
        public string datetime { get; set; }
        public Dictionary<string,double> rate { get; set; }

    }
        public class FunctionResult
    {
        public double  Rate { get; set; }
    }
    public static class Currency
    {
        [FunctionName("Currency")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string FromCurrency = req.Query["From"];
            string ToCurrency   = req.Query["TO"];
            
         
            HttpClient apiclient = new HttpClient();

            string requestBody = await apiclient.GetStringAsync("https://dotnsf-fx.herokuapp.com/");
            CurrencyRale data = JsonSerializer.Deserialize<CurrencyRale>(requestBody);
                double rtnval =0.0 ;
                if (data != null)
                {
                    string Rate;
                    bool stat;
                    Rate = FromCurrency + ToCurrency;
                    stat = data.rate.TryGetValue(Rate, out rtnval);
                    if (!stat)
                    {
                        Rate = ToCurrency + FromCurrency;
                        stat = data.rate.TryGetValue(Rate, out rtnval);
                        if (stat) rtnval = 1 / rtnval;
                        else rtnval = 0.0;

                    }
                }

            return new OkObjectResult(new FunctionResult {Rate = rtnval});
        }
    }
}
