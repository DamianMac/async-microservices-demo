using Autofac;
using Microsoft.AspNetCore.Identity;
using Profile.Auth;
using Profile.Domain;

namespace Profile.Infrastructure;

public class AuthModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(ctx => ctx.Resolve<IConfiguration>().GetSection("Settings:Jwt").Get<JwtSettings>()!).SingleInstance();
        builder.RegisterType<TokenIssuer>().SingleInstance();
        builder.RegisterType<PasswordHasher<Customer>>().As<IPasswordHasher<Customer>>().SingleInstance();
    }
}
