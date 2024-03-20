using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Sketch_Bot.Custom_Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RequireDevelopersSilentAttribute : PreconditionAttribute
    {
        private IApplication application;
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (application == null)
                application = await context.Client.GetApplicationInfoAsync();
            if (context.User.Id == 135446225565515776 || context.User.Id == 208624502878371840) return PreconditionResult.FromSuccess();
            return PreconditionResult.FromError("");
        }
    }
}