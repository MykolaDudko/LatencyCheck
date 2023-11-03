
using Library.Consts;
using Library.Models;
using Library.Repositories;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

IpRepository _ipRepository = new IpRepository();
IPAddress[] _addresses = Dns.GetHostAddresses(Consts.HostName);


//foreach (var item in _addresses)
//{
ServicePointManager.DnsRefreshTimeout = 0;
using (var ws = new ClientWebSocket())
{
    try
    {
        var uri = new Uri($"wss://{_addresses[1]}:443/ws-api/v3");
        ws.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        await ws.ConnectAsync(uri, CancellationToken.None);
        stopwatch.Stop();
        long delayConnection = stopwatch.ElapsedMilliseconds;
        while (true)
        {
            stopwatch.Restart();
            Console.WriteLine("Connected to Binance WebSocket API.");

            string request = $"{{\"id\": \"{Guid.NewGuid()}\", \"method\": \"time\"}}";
            byte[] requestBytes = Encoding.UTF8.GetBytes(request);
            var buffer = new byte[1024];

            stopwatch.Start();
            await ws.SendAsync(new ArraySegment<byte>(requestBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            stopwatch.Stop();
            long delayResponse = stopwatch.ElapsedMilliseconds;

            //var query = _ipRepository.CreateQuery();
            //query = query.Where(i => i.Ip == _addresses[0].ToString());
            //var ip = _ipRepository.Get(query);

            //if (ip != null)
            //{
            //    ip.DelayConnection = delayConnection;
            //    ip.DelayReponse = delayResponse;
            //    _ipRepository.Update(ip);
            //}
            //else
            //{
            //    await _ipRepository.AddAsync(new IpModel { Ip = _addresses[0].ToString(), DelayConnection = delayConnection, DelayReponse = delayResponse });
            //}


            if (result.MessageType == WebSocketMessageType.Text)
            {
                string response = Encoding.UTF8.GetString(buffer, 0, result.Count);
                CheckServerTimeModel myDeserializedClass = JsonSerializer.Deserialize<CheckServerTimeModel>(response);
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(myDeserializedClass.result.serverTime);


                Console.WriteLine(response);

                Console.WriteLine($"Delay of connection {delayConnection} in miliseconds");
                Console.WriteLine($"Delay of response {delayResponse} in miliseconds");
            }
            Thread.Sleep(TimeSpan.FromSeconds(3));
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
    //finally
    //{
    //    if (ws.State == WebSocketState.Open)
    //    {
    //        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", CancellationToken.None);
    //        ws.Dispose();
    //    }

    //}
    //}
}

Console.ReadKey();
