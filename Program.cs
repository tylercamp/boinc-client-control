static class Program {
    static async Task Main(string[] args) {
        string clientIp = null, accessKey = null;
        int port = 31416;
        for (int i = 0; i < args.Length; i++) {
            switch (args[i])
            {
                case "--client-ip":
                    clientIp = args.Skip(i+1).FirstOrDefault();
                    i++;
                    break;

                case "--key":
                    accessKey = args.Skip(i+1).FirstOrDefault();
                    i++;
                    break;
                
                case "--port":
                    string maybePort = args.Skip(i+1).FirstOrDefault();
                    if (maybePort == null || !int.TryParse(maybePort, out port)) {
                        Console.WriteLine("Invalid port: " + maybePort);
                        return;
                    }
                    break;
            }
        }

        if (clientIp == null || accessKey == null) {
            Console.WriteLine("Required args: --client-ip <ip> --key <key>");
            return;
        }

        using (var rpcClient = new BoincRpc.RpcClient())
        {
            Console.WriteLine($"Connecting to {clientIp}:{port}...");
            await rpcClient.ConnectAsync(clientIp, port);

            Console.WriteLine("Authorizing...");
            if (!await rpcClient.AuthorizeAsync(accessKey)) {
                Console.WriteLine("Error: Failed to authorize");
                return;
            }
            Console.WriteLine("Success!");

            var boincClient = new BoincClient(rpcClient);

            Console.WriteLine("Starting HTTP server...");
            var app = WebApplication.CreateBuilder().Build();

            app.MapGet("cpu", async ctx => ctx.Response.WriteAsync(await boincClient.GetCpuPercent()));
            app.MapPost("cpu", async (c) => {
                var pct = await new StreamReader(c.Request.Body).ReadToEndAsync();
                try
                {
                    await boincClient.SetCpuPercent(pct);
                } catch (Exception e)
                {
                    c.Response.StatusCode = 400;
                    await c.Response.WriteAsync(e.Message);
                }
            });

            app.MapPost("start", async (c) => await boincClient.SetRunMode(BoincRpc.Mode.Always));
            app.MapPost("stop", async (c) => await boincClient.SetRunMode(BoincRpc.Mode.Never));
            app.MapPost("resume", async (c) => await boincClient.SetRunMode(BoincRpc.Mode.Auto));

            app.Run();
        }
    }
}

