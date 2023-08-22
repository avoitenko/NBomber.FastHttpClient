using NBomber.FastHttpClient;

class Program
{
    static void Main(string[] args)
    {
        new CommonExample().Run();
        
        Thread.Sleep(3000);
        
        new FastExample().Run();
    }
}
