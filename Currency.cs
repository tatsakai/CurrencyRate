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
        public List<CurrencyRateDetail> quotes { get; set; }
    }
    public class CurrencyRateDetail
    {
        public string high { get; set; }
        public string open { get; set; }
        public string bid { get; set; }
        public string currencyPairCode { get; set; }
        public string ask { get; set; }
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
            string rtnval = "0.000";
            string Rate ="";
            string selrate ="";
            try
            {
                string requestBody = await apiclient.GetStringAsync("https://www.gaitameonline.com/rateaj/getrate");
                CurrencyRale data = JsonSerializer.Deserialize<CurrencyRale>(requestBody);
                //comment 
                if (data != null)
                {
                    if(data.quotes.Count > 0) 
                    { 
                        Rate = FromCurrency + ToCurrency;
                        selrate = data.quotes.Where(q => q.currencyPairCode.Equals(Rate)).FirstOrDefault().ask;
                        if (selrate == null)
                        {
                            Rate = ToCurrency + FromCurrency;
                            selrate = data.quotes.Where(q => q.currencyPairCode.Equals(Rate)).FirstOrDefault().ask;
                            if (selrate != null)
                            {
                                double calrate = 1 / Convert.ToDouble(selrate);
                                rtnval = calrate.ToString("F5");
                            }
                        }
                        else
                        {
                            rtnval = selrate;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                rtnval = "0.0";
                log.LogError(e.Message);
            }

            return new OkObjectResult(new FunctionResult {Rate = rtnval});
        }
    }
}
