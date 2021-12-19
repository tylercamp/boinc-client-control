# boinc-client-control

This is a simple app using [BoincRpc](https://github.com/chausner/BoincRpc) for remotely managing a [BOINC client](https://boinc.berkeley.edu/) via REST API. Dotnet Core 6 SDK is used to build.

Run the app locally with:

```
dotnet boinc-client-control.dll --client-ip <ip> --key <password>
```

... or via Docker with:

```
docker run -p 80:80 tcamps/boinc-client-control:v1.0 --client-ip <ip> --key <password>
```

The optional `--port` param can be used to specify a non-standard port. (Default is `31416`.)

The REST API will be at port `80` for release builds, `5000` otherwise.

Available endpoints are:

- `/cpu (GET)` - Reads `max_ncpus_pct` from `global_prefs_override.xml` (Options -> Computing preferences -> "Use at most X% of the CPUs")
- `/cpu (POST)` - Sets a new percentage for max CPUs/cores (0-100)
- `/start (POST)` - Sets Computing Activity mode to "Run always"
- `/stop (POST)` - Sets mode to "Suspend"
- `/resume (POST)` - Sets mode to "Run based on preferences"

## Notes

- `--key` will be the value from `gui_rpc_auth.cfg` (found in `C:/ProgramData/BOINC` on Windows)
- You must [configure remote access](https://boinc.berkeley.edu/wiki/Controlling_BOINC_remotely) if the app is running on a different machine than the one running BOINC