using System;
using System.Linq;
using System.Threading;
using Autofac;
using DocSaw.Rules;
using Microsoft.Extensions.Configuration;

namespace DocSaw
{
    public class RulesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var ruleTypes = typeof(Program).Assembly.GetTypes()
                .Where(x => !x.IsAbstract && !x.IsInterface)
                .Where(x => x.IsAssignableTo<IRule>());

            foreach (var ruleType in ruleTypes)
            {
                builder.RegisterType(ruleType).As<IRule>();

                var configType = ruleType.GetNestedType("Config");
                if (configType != null)
                {
                    builder.Register(ctx =>
                    {
                        var cfg = ctx.Resolve<IConfigurationRoot>();
                        var r = cfg.GetSection("Rules").GetSection(ruleType.Name).Get(configType);
                        return r ?? Activator.CreateInstance(configType);
                    }).As(configType);
                }
            }
        }
    }
}