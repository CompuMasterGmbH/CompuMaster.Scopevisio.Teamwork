# Native asynchronous Teamwork authorization

This draft adds cancellable `TeamworkRestClient.CreateAsync`, `TeamworkIOClient.CreateAsync`, and `TeamworkIOClient.AuthorizeAsync` factories. Existing constructors and synchronous directory operations remain available. Factories avoid synchronous account calls during construction.

`TeamworkOAuthInfoProvider` implements the optional asynchronous SDK contract. Account lookup and token refresh share a gate and account cache per OpenScope client. Cancellation reaches an active account or token request and can remove a queued waiter without canceling the owner. A failed or canceled account lookup is not cached. Login email, token user ID, and tenant ID retain their distinct meanings.

The asynchronous error handler actually refreshes the installed OpenScope token. When several operations reject the same token, later refresh callbacks reuse the replacement token rather than sending another refresh. Existing synchronous callback behavior is retained. Do not mutate credentials, replace tokens, or run synchronous authorization concurrently with these operations; create a new OpenScope client for another authentication context.

## Shared request policy

Create a caller-owned HTTP client with `CenterDeviceHttpTransport.CreateHttpClient(configuredHandler)` and install it in a fresh OpenScope `Configuration.HttpClient` before the first request. Pass that configuration to `TeamworkIOClient.AuthorizeAsync`. The caller owns the configured handler/client and must keep it alive for the returned Teamwork client's lifetime.

This shares the SDK's per-origin admission and cooldown policy between OpenScope token/account requests and Teamwork requests to the same origin. The defaults are one active request and thirty admissions per rolling minute in this process. These are conservative local defaults, not verified vendor quotas. No shared limiter across processes or repositories is claimed. Existing clients created with unrelated HTTP transports do not acquire this policy retrospectively. The factory requires an injected HTTP client but cannot verify how a custom client was constructed.

## Draft dependency consumption and verification

The native code depends on unpublished draft changes in [CenterDevice.IO PR #10](https://github.com/CompuMasterGmbH/CompuMaster.CenterDevice.IO/pull/10) and [OpenScope PR #13](https://github.com/CompuMasterGmbH/CompuMaster.Scopevisio.OpenApi/pull/13). Default builds keep the existing package references and exclude the new `*.Async.cs` files; they retain the existing synchronous API. Native API availability requires the explicit source-validation opt-in below. No new NuGet version is published or assumed.

```powershell
dotnet test Scopevisio.Teamwork.AsyncTests/Scopevisio.Teamwork.AsyncTests.csproj -c CI_CD -p:EnableNativeAsync=true -p:GeneratePackageOnBuild=false -p:CenterDeviceRestProject=<absolute-path-to-CenterDevice.Rest.csproj> -p:OpenScopeApiProject=<absolute-path-to-CompuMaster.Scopevisio.OpenApi.csproj>
```

All seven deterministic tests pass on both net8.0 and net48 using fake HTTP handlers. They cover shared account lookup, distinct identity fields, active cancellation, queued cancellation, failed initialization recovery, concurrent refresh deduplication, canceled-refresh recovery, and the configured authorization factory. The native library builds for netstandard2.0, net6.0, and net48. The SDK's existing log4net audit warning remains.

Remaining work includes high-level asynchronous directory/file composition, DMS consumption, combined sharing verification, official backend limit research, and serialized remote integration verification. Default builds must switch to the released compatible dependency versions and enable the additive APIs before production package release. None of these drafts authorizes publishing, merging, or closing the associated issues. No remote integration tests were run locally.
