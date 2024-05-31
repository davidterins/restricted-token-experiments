//// Uncomment to verify restricted sids
//TokenInfo.PrintTokenSidInfo();
//Console.ReadKey();

try
{
    var url = "https://localhost:7254/weatherforecast";
    //var url = "https://127.0.0.1:7000v1/applications";
    Console.WriteLine($"Attempting to do http request to endpoint: {url}");

    var handler = new HttpClientHandler();
    handler.PreAuthenticate = true;
    handler.UseDefaultCredentials = true;
    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
    handler.ServerCertificateCustomValidationCallback =
        (httpRequestMessage, cert, cetChain, policyErrors) =>
        {
            return true;
        };

    var httpClient = new HttpClient(handler);

    var result = await httpClient.GetAsync(url);

    var content = await result.Content.ReadAsStringAsync();

    Console.WriteLine(result.StatusCode);
    Console.WriteLine($"Content: {content}");
}
catch (Exception e)
{
    Console.WriteLine($"{e}");
}


Console.ReadLine();