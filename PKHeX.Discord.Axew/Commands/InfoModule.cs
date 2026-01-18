using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace PKHeX.Discord.Axew.Commands
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        private const string a = "A Pokémon Discord bot developed and created by Nexus Risen, powered by PKHeX.Core, Auto-Legality Mod, and other open source software.";
        [Command("info")]
        [Alias("about", "whoami", "owner")]
        public async Task InfoAsync()
        {
            var b = await Context.Client.GetApplicationInfoAsync().ConfigureAwait(false);

            var c = new EmbedBuilder
            {
                Color = new Color(114, 137, 218),
                Description = a,
            };

            var d = $"https://discordapp.com/oauth2/authorize?client_id={b.Id}&permissions=0&scope=bot";
            c.AddField("Info",
                $"- {Format.Bold("Developed By")}: Nexus Risen\n" +
                $"- {Format.Bold("Author")}: {b.Owner} ({b.Owner.Id})\n" +
                $"- {Format.Bold("Library")}: Discord.Net ({DiscordConfig.Version})\n" +
                $"- {Format.Bold("Uptime")}: {B()}\n" +
                $"- {Format.Bold("Runtime")}: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.ProcessArchitecture} " +
                $"({RuntimeInformation.OSDescription} {RuntimeInformation.OSArchitecture})\n" +
                $"- {Format.Bold("Buildtime")}: {C()}\n" +
                $"- {Format.Bold("Core")}: {D()}\n" +
                $"- {Format.Bold("AutoLegality")}: {E()}\n"
                );

            c.AddField("Stats",
                $"- {Format.Bold("Heap Size")}: {F()}MiB\n" +
                $"- {Format.Bold("Guilds")}: {Context.Client.Guilds.Count}\n" +
                $"- {Format.Bold("Channels")}: {Context.Client.Guilds.Sum(g => g.Channels.Count)}\n" +
                $"- {Format.Bold("Users")}: {Context.Client.Guilds.Sum(g => g.Users.Count)}\n"
                );

            await ReplyAsync("Here's a bit about me!", embed: c.Build()).ConfigureAwait(false);
        }

        private static string B() => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
        private static string F() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();

        private static string C()
        {
            var g = Assembly.GetExecutingAssembly();
            return File.GetLastWriteTime(g.Location).ToString(@"yy-MM-dd\.hh\:mm");
        }

        public static string D() => G("PKHeX.Core.dll");
        public static string E() => G("PKHeX.Core.AutoMod.dll");

        private static string G(string h)
        {
            var i = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (i == null)
                return "Unknown";
            var j = Path.Combine(i, h);
            var k = File.GetLastWriteTime(j);
            return k.ToString(@"yy-MM-dd\.hh\:mm");
        }
    }
}
