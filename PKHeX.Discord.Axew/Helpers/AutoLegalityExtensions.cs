using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using static PKHeX.Core.AutoMod.APILegality;

namespace PKHeX.Discord.Axew
{
    public static class AutoLegalityExtensions
    {
        static AutoLegalityExtensions()
        {
            EncounterEvent.RefreshMGDB();
            Legalizer.EnableEasterEggs = false;
            APILegality.SetAllLegalRibbons = false;
            APILegality.Timeout = 99999;
            APILegality.GameVersionPriority = GameVersionPriorityType.NewestFirst;
            ParseSettings.Settings.Handler.CheckActiveHandler = false;
            ParseSettings.Settings.HOMETransfer.HOMETransferTrackerNotPresent = Severity.Fishy;
            ParseSettings.Settings.Nickname.SetAllTo(new NicknameRestriction
            {
                NicknamedTrade = Severity.Fishy,
                NicknamedMysteryGift = Severity.Fishy,
            });

            foreach (var game in GameUtil.GameVersions)
            {
                const string OT = "PKHeX-D";
                var blankSAV = BlankSaveFile.Get(game, OT);
                TrainerSettings.Register(blankSAV);
            }

            var trainer = TrainerSettings.GetSavedTrainerData(GameVersion.SV);
            RecentTrainerCache.SetRecentTrainer(trainer);
        }

        public static async Task ReplyWithLegalizedSetAsync(this ISocketMessageChannel channel, ITrainerInfo sav, ShowdownSet set)
        {
            if (set.Species <= 0)
            {
                await channel.SendMessageAsync("Oops! I wasn't able to interpret your message! If you intended to convert something, please double check what you're pasting!").ConfigureAwait(false);
                return;
            }

            APILegality.AsyncLegalizationResult almres;
            try
            {
                almres = sav.GetLegalFromSet(set);
            }
            catch (MissingMethodException)
            {
                await channel.SendMessageAsync("Version mismatch between PKHeX.Core and Auto-Legality Mod. Please update both.").ConfigureAwait(false);
                return;
            }

            var pkm = almres.Created;
            var la = new LegalityAnalysis(pkm);
            var spec = GameInfo.Strings.Species[set.Species];

            string msg;
            if (almres.Status == LegalizationResult.VersionMismatch)
            {
                msg = "Version mismatch between PKHeX.Core and Auto-Legality Mod. Please update both.";
            }
            else if (la.Valid)
            {
                msg = $"Here's your ({almres.Status}) legalized PKM for {spec} ({la.EncounterOriginal.Name})!";
            }
            else
            {
                msg = $"Oops! I wasn't able to create something from that. Here's my best attempt for that {spec}!";
            }

            await channel.SendPKMAsync(pkm, msg + $"\n{ReusableActions.GetFormattedShowdownText(pkm)}").ConfigureAwait(false);
        }

        public static async Task ReplyWithLegalizedSetAsync(this ISocketMessageChannel channel, string content, int gen)
        {
            content = ReusableActions.StripCodeBlock(content);
            var set = new ShowdownSet(content);
            var game = RegenUtil.GetGameVersionFromGen((byte)gen);
            var sav = TrainerSettings.GetSavedTrainerData(game);
            await channel.ReplyWithLegalizedSetAsync(sav, set).ConfigureAwait(false);
        }

        public static async Task ReplyWithLegalizedSetAsync(this ISocketMessageChannel channel, string content)
        {
            content = ReusableActions.StripCodeBlock(content);
            var set = new ShowdownSet(content);
            var sav = TrainerSettings.GetSavedTrainerData(GameVersion.SV);
            await channel.ReplyWithLegalizedSetAsync(sav, set).ConfigureAwait(false);
        }

        public static async Task ReplyWithLegalizedSetAsync(this ISocketMessageChannel channel, IAttachment att)
        {
            var download = await NetUtil.DownloadPKMAsync(att).ConfigureAwait(false);
            if (!download.Success)
            {
                await channel.SendMessageAsync(download.ErrorMessage).ConfigureAwait(false);
                return;
            }

            var pkm = download.Data;
            if (new LegalityAnalysis(pkm).Valid)
            {
                await channel.SendMessageAsync($"{download.SanitizedFileName}: Already legal.").ConfigureAwait(false);
                return;
            }

            var legal = pkm.Legalize();
            if (!new LegalityAnalysis(legal).Valid)
            {
                await channel.SendMessageAsync($"{download.SanitizedFileName}: Unable to legalize.").ConfigureAwait(false);
                return;
            }

            legal.RefreshChecksum();

            var msg = $"Here's your legalized PKM for {download.SanitizedFileName}!\n{ReusableActions.GetFormattedShowdownText(legal)}";
            await channel.SendPKMAsync(legal, msg).ConfigureAwait(false);
        }
    }
}
