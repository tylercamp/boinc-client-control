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

        var client = new BoincClient(clientIp, accessKey, port);
        if (!(await client.Validate())) {
            return;
        }

        Console.WriteLine("Starting HTTP server...");
        var app = WebApplication.CreateBuilder().Build();

        app.MapGet("cpu", async ctx => await ctx.Response.WriteAsync(await client.GetCpuPercent()));
        app.MapPost("cpu", async (c) => {
            var pct = await new StreamReader(c.Request.Body).ReadToEndAsync();
            try
            {
                await client.SetCpuPercent(pct);
            } catch (Exception e)
            {
                c.Response.StatusCode = 400;
                await c.Response.WriteAsync(e.Message);
            }
        });

        app.MapPost("start", async (c) => await client.SetRunMode(BoincRpc.Mode.Always));
        app.MapPost("stop", async (c) => await client.SetRunMode(BoincRpc.Mode.Never));
        app.MapPost("resume", async (c) => await client.SetRunMode(BoincRpc.Mode.Auto));

        app.Run();
    }
}

