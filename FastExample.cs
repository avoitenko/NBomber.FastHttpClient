using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NBomber.Contracts.Stats;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using Newtonsoft.Json.Linq;
using Serilog;

namespace NBomber.FastHttpClient;

internal class FastExample
{
    public void Run()
    {
        FastHttpClient client = new FastHttpClient();
        Random rnd = new Random();

        //---
        var scenario = Scenario.Create("fast scenario", async context =>
        {
            //---
            var request = FastHttp.CreateRequest("POST", "http://127.0.0.1:9090");

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
            var response = await FastHttp.Send(client, request);


            //--- logging
            string message = string.Format("Result: {0}, thread: {1}, iteration: {2}, body: {3}",
                                        response.StatusCode.ToString(),
                                        context.ScenarioInfo.ThreadNumber,
                                        context.InvocationNumber,
                                        response.Message);
            //Console.WriteLine(message);
            context.Logger.Information(message);

            //---
            if (response.IsError)
            {
                return Response.Fail();
            }

            return Response.Ok();
        })
            .WithInit(async context =>
            {
                await Task.Delay(0);

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Authorization", "qwe123!@#");
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
