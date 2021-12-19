using System.Xml.Linq;
using BoincRpc;

class BoincClient
{
    string host, password;
    int port;
    public BoincClient(string host, string password, int port) {
        this.host = host;
        this.password = password;
        this.port = port;
    }

    async Task<RpcClient> PrepareClient(bool log = false) {
        var client = new RpcClient();

        if (log) Console.WriteLine($"Connecting to {host}:{port}...");
        await client.ConnectAsync(host, port);

        if (log) Console.WriteLine("Authorizing...");
        if (!await client.AuthorizeAsync(password)) {
            if (log) Console.WriteLine("Error: Failed to authorize");
            return null;
        }

        if (log) Console.WriteLine("Success!");

        return client;
    }

    public async Task<bool> Validate() {
        var client = await PrepareClient(log: true);
        if (client != null) client.Dispose();

        return client != null;
    }

    public async Task SetCpuPercent(String pct)
    {
        float fpct;
        if (!float.TryParse(pct, out fpct))
            throw new Exception("Invalid number: " + pct);

        using (var client = await PrepareClient()) {
            var newPrefs = await client.GetGlobalPreferencesOverrideAsync();
            newPrefs.Element("max_ncpus_pct").SetValue((int)fpct);
            await client.SetGlobalPreferencesOverrideAsync(newPrefs);
            await client.ReadGlobalPreferencesOverrideAsync();
        }
    }

    public async Task<string> GetCpuPercent()
    {
        using (var client = await PrepareClient()) {
            return (await client.GetGlobalPreferencesOverrideAsync()).Element("max_ncpus_pct").Value.ToString();
        }
    }

    public async Task SetRunMode(Mode mode) {
        using (var client = await PrepareClient()) {
            await client.SetRunModeAsync(mode);
        }
    }
}