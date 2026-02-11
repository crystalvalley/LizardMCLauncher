using CmlLib.Core.Auth;
using CmlLib.Core.Auth.Microsoft;

namespace Launcher.Services;

public interface IAuthService
{
    MSession? CurrentSession { get; }
    Task<MSession> LoginSilentlyAsync();
    Task<MSession> LoginAsync();
}


public class AuthService : IAuthService
{
    public MSession? CurrentSession { get; private set; }

    public async Task<MSession> LoginSilentlyAsync()
    {
        var handler = JELoginHandlerBuilder.BuildDefault();
        var session = await handler.AuthenticateSilently();
        CurrentSession = session;
        return session;
    }

    public async Task<MSession> LoginAsync()
    {
        var handler = JELoginHandlerBuilder.BuildDefault();
        var session = await handler.AuthenticateInteractively();
        CurrentSession = session;
        return session;
    }
}
