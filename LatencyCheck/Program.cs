using Library.Consts;
using Library.Models;
using Library.Repositories;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


class Program
{
    static Modes _mode1 = Modes.None;
    static Modes _mode2 = Modes.Find;
    static IPAddress _bestIp;
    static double _oldTime;
    static IPAddress[] _addresses;
    static double _averageDelay = 0;
    static double _sumDelay = 0;
    static double _iteration = 10;
    static BaseRepository<LogModel> _logsRepository = new BaseRepository<LogModel>();

    /// <summary>
    /// Operační režimy.
    /// </summary>
    private enum Modes
    {
        Read,
        Find,
        None
    };

    static async Task Main()
    {
        var task1 = Task.Run(() => Do());
        var task2 = Task.Run(() => Do2());
        await Task.WhenAny(task1, task2);
    }

    /// <summary>
    /// První úkol běžící paralelně s Do2(). Také provádí Hledání a Čtení v smyčce.
    /// </summary>
    private async static Task Do()
    {
        while (true)
        {
            // Inicializace nového klienta WebSocket.
            ClientWebSocket ws = new ClientWebSocket();
            // Pokus o nalezení nejlepší IP a poté čtení dat, pokud je v režimu Hledání.
            while (_mode1 == Modes.Find)
            {
                _addresses = Dns.GetHostAddresses(Consts.HostName);

                foreach (var ip in _addresses)
                {
                    ws = new ClientWebSocket();
                    var result = await Find(ws, ip);
                    if (result)
                    {
                        _mode1 = Modes.Read;
                        _mode2 = Modes.Find;
                        break;
                    }
                }
            }
            // Čtení dat z WebSocketu, pokud je v režimu Čtení.
            while (_mode1 == Modes.Read && _bestIp != null)
            {
                await Read(ws);
            }
            // Zavření WebSocketu, pokud je stále otevřen po dokončení.
            if (ws.State == WebSocketState.Open)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Zavírání spojení", CancellationToken.None);
                ws.Dispose();
            }
        }
    }

    /// <summary>
    /// Druhý úkol běžící paralelně s Do(). Také provádí Hledání a Čtení v smyčce.
    /// </summary>
    private async static Task Do2()
    {
        while (true)
        {
            // Inicializace nového klienta WebSocket.
            ClientWebSocket ws = new ClientWebSocket();
            // Pokus o nalezení nejlepší IP a poté čtení dat, pokud je v režimu Hledání.
            while (_mode2 == Modes.Find)
            {
                _addresses = Dns.GetHostAddresses(Consts.HostName);

                foreach (var ip in _addresses)
                {
                    ws = new ClientWebSocket();
                    var result = await Find(ws, ip);
                    if (result)
                    {
                        _mode2 = Modes.Read;
                        _mode1 = Modes.Find;
                        break;
                    }
                }
            }
            // Čtení dat z WebSocketu, pokud je v režimu Čtení.
            while (_mode2 == Modes.Read)
            {
                await Read(ws);
            }
            // Zavření WebSocketu, pokud je stále otevřen po dokončení.
            if (ws.State == WebSocketState.Open)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Zavírání spojení", CancellationToken.None);
                ws.Dispose();
            }
        }
    }

    /// <summary>
    /// Pokusí se připojit k WebSocketu a určit, zda má spojení nejlepší odezvu, kterou jsme dosud našli.
    /// </summary>
    private static async Task<bool> Find(ClientWebSocket ws, IPAddress ip)
    {
        try
        {
            var uri = new Uri($"wss://{ip}/ws-api/v3");
            if (ws.Options.RemoteCertificateValidationCallback == null)
            {
                ws.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            }
            await ws.ConnectAsync(uri, CancellationToken.None);
            Console.WriteLine("Připojeno k Binance WebSocket API.");
            string request = $"{{\"id\": \"{Guid.NewGuid()}\", \"method\": \"time\"}}";
            _sumDelay = 0;
            for (int i = 0; i < _iteration; i++)
            {
                Console.WriteLine("Mode: Hledání");
                _sumDelay += await SendRequest(ws, request);
            }

            _averageDelay = _sumDelay / _iteration;
            await _logsRepository.AddAsync(new LogModel { Ip = ip, AverageResponseTime = _averageDelay, Key = Guid.NewGuid() });
            if (_oldTime == 0 || _oldTime > _averageDelay)
            {
                _oldTime = _averageDelay;
                _bestIp = ip;
                Console.WriteLine($"Lepší spojení bylo nalezeno na IP {_bestIp} s průměrnou odezvou {_averageDelay} ms");
                Console.WriteLine($"************************************************************************************");
                Console.WriteLine($"************************************************************************************");
                return true;
            }
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Zavírání spojení", CancellationToken.None);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chyba: {ex.Message}");
            ws.Dispose();
            return false;
        }

        ws.Dispose();
        return false;
    }

    /// <summary>
    /// Čte data ze WebSocketu v smyčce.
    /// </summary>
    private static async Task Read(ClientWebSocket ws)
    {
        try
        {
            Console.WriteLine("Mode: Čtení");
            string request = $"{{\"id\": \"{Guid.NewGuid()}\", \"method\": \"time\"}}";
            await SendRequest(ws, request);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chyba: {ex.Message}");
        }
        await Task.Delay(TimeSpan.FromSeconds(2));
    }

    /// <summary>
    /// Odešle požadavek přes WebSocket a měří čas potřebný pro odpověď.
    /// </summary>
    private async static Task<double> SendRequest(ClientWebSocket ws, string request)
    {
        Stopwatch stopwatch = new Stopwatch();
        byte[] requestBytes = Encoding.UTF8.GetBytes(request);
        var buffer = new byte[1024];
        stopwatch.Start();
        await ws.SendAsync(new ArraySegment<byte>(requestBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        stopwatch.Stop();
        var delayResponse = stopwatch.Elapsed.TotalMilliseconds;
        Console.WriteLine($"Čas odezvy {delayResponse} ms");
        stopwatch.Reset();

        if (result.MessageType == WebSocketMessageType.Text)
        {
            string response = Encoding.UTF8.GetString(buffer, 0, result.Count);
            // Deserializace JSON odpovědi.
            //CheckServerTimeModel data = JsonSerializer.Deserialize<CheckServerTimeModel>(response);
            //Console.WriteLine($"Prodleva odpovědi {delayResponse} v milisekundách");
        }
        await Task.Delay(TimeSpan.FromSeconds(2));
        return delayResponse;
    }
}
