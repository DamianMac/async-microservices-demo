using Autofac;
using Email.Sending;
using Email.Templates;

namespace Email.Infrastructure;

public class EmailModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(ctx => ctx.Resolve<IConfiguration>().GetSection("Settings:Smtp").Get<SmtpSettings>()!).SingleInstance();
        builder.RegisterType<SmtpSender>().SingleInstance();
        builder.RegisterType<EmailTemplates>().SingleInstance();
    }
}
