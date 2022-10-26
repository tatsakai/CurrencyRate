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
using System.Linq;

namespace Contoso.CurrencyExchange
{

    public class CurrencyRale
    {
        public List<CurrencyRateDetail>? quotes { get; set; }
    }
    public class CurrencyRateDetail
    {
        public string high { get; set; }
        public string open { get; set; }
        public string bid { get; set; }
        public string currencyPairCode { get; set; }
        public string low { get; set; }
    }
    public class FunctionResult
    {
        public string  Rate { get; set; }
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
            string rtnval = "0.0";

            try
            {
                string requestBody = await apiclient.GetStringAsync("https://www.gaitameonline.com/rateaj/getrate");
                CurrencyRale data = JsonSerializer.Deserialize<CurrencyRale>(requestBody);
                
                if (data != null)
                {
                    string Rate;
                    if(data.quotes.Count > 0) 
                    { 
                        Rate = FromCurrency + ToCurrency;
                        var selrate = from q in data.quotes where q.currencyPairCode.Equals(Rate) select q;
                        if (selrate.Count<CurrencyRateDetail>() == 0)
                        {
                            Rate = ToCurrency + FromCurrency;
                            var selratervs = from q in data.quotes where q.currencyPairCode.Equals(Rate) select q;
                            if(selratervs.Count<CurrencyRateDetail>() > 0)
                            {
                                double calrate = 1 / Convert.ToDouble(selratervs.First().open);
                                rtnval = calrate.ToString("F5");
                            }
                        }
                        else
                        {
                            rtnval = selrate.First().open;
                        }


                    }
                }
            }
            catch(Exception e)
            {
                rtnval = "0.0";
            }

            return new OkObjectResult(new FunctionResult {Rate = rtnval});
        }
    }
}
