﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Administrator.Services;
using Administrator.Services.Database;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Administrator.Extensions.Attributes
{
    public sealed class RequirePermRole : PreconditionAttribute
    {
        private static readonly Config Config = BotConfig.New();

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context,
            CommandInfo command, IServiceProvider services)
        {
            var db = services.GetService<DbService>();

            var guildConfig = await db.GetOrCreateGuildConfigAsync(context.Guild).ConfigureAwait(false);
            var permRole = context.Guild.Roles.FirstOrDefault(x => x.Id == (ulong) guildConfig.PermRole);

            if (permRole is null)
            {
                var eb = new EmbedBuilder()
                    .WithErrorColor()
                    .WithDescription(
                        $"This guild has not set up their permrole yet!\nUse `{Config.BotPrefix}permrole Your PermRole Here` to set it.")
                    .WithFooter("Contact a user with Administrator permissions.");
                await context.Channel.EmbedAsync(eb.Build()).ConfigureAwait(false);
                return PreconditionResult.FromError("Guild has not set up permrole.");
            }

            if (context.User is SocketGuildUser user)
                return user.Roles.Any(x => x.Id == permRole.Id)
                    ? PreconditionResult.FromSuccess()
                    : PreconditionResult.FromError("User does not have permrole.");

            return PreconditionResult.FromError("Internal error. Please report this.");
        }
    }
}