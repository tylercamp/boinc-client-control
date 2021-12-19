using System.Xml.Linq;
using BoincRpc;

class BoincClient
{
    RpcClient client;
    public BoincClient(RpcClient authorizedClient) {
        this.client = authorizedClient;
    }

    public Task<XElement> GlobalPreferences => client.GetGlobalPreferencesOverrideAsync();

    public async Task SetCpuPercent(String pct)
    {
        float fpct;
        if (!float.TryParse(pct, out fpct))
            throw new Exception("Invalid number: " + pct);

        var newPrefs = await GlobalPreferences;
        newPrefs.Element("max_ncpus_pct").SetValue((int)fpct);
        await client.SetGlobalPreferencesOverrideAsync(newPrefs);
        await client.ReadGlobalPreferencesOverrideAsync();
    }

    public async Task<string> GetCpuPercent()
    {
        return (await GlobalPreferences).Element("max_ncpus_pct").Value.ToString();
    }

    public Task SetRunMode(Mode mode) => client.SetRunModeAsync(mode);
}