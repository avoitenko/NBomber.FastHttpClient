using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NBomber.Contracts.Stats;
using NBomber.CSharp;
using NBomber.Http.CSharp;


using Newtonsoft.Json.Linq;


internal class CommonExample
{
    public void Run()
    {
        HttpClient client = new HttpClient();
        Random rnd = new Random();

        //---
        var scenario = Scenario.Create("common scenario", async context =>
        {
            //---
            var request = Http.CreateRequest("POST", "http://127.0.0.1:9090");

            //---
            request.WithHeader("Authorization", "qwe123!@#");
            //--- add session headers
            request.WithHeader("Time", String.Format("{0:ddd,' 'dd' 'MMM' 'yyyy' 'HH':'mm':'ss' 'K}", DateTime.Now));


            //--- build json body
            JObject json = new JObject();
            json.Add("int64", rnd.NextInt64());
            json.Add("double", rnd.NextDouble());
            string strJson = json.ToString();

            //--- add body
            request.WithBody(new StringContent(strJson, Encoding.UTF8, "application/json"));

            //--- make the request
            var response = await Http.Send(client, request);
            
            //--- read body
            string responseMessage = await response.Payload.Value.Content.ReadAsStringAsync();

            //--- logging
            string message = string.Format("Result: {0}, thread: {1}, iteration: {2}, body: {3}",
                                        response.StatusCode.ToString(),
                                        context.ScenarioInfo.ThreadNumber,
                                        context.InvocationNumber,
                                        responseMessage);
            //Console.WriteLine(message);
            context.Logger.Information(message);

            //---
            if (response.IsError)
            {
                return Response.Fail();
            }

            return Response.Ok();
        })
            .WithoutWarmUp()
            .WithMaxFailCount(int.MaxValue)
            .WithLoadSimulations(
                Simulation.KeepConstant(1, TimeSpan.FromSeconds(10))
            );

        //---
        NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFileName("report")
            .WithReportFolder("reports")
            .WithReportFormats(ReportFormat.Txt, ReportFormat.Html, ReportFormat.Md)
            .WithReportingInterval(TimeSpan.FromSeconds(10))
            .Run();
    }
}
