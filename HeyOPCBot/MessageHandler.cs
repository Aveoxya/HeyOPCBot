using System;
using System.Threading.Tasks;
using Discord;
using System.Timers;
using System.Diagnostics;

namespace HeyOPCBot
{
    public partial class MessageHandler
    {
        public static async Task<IUserMessage> SendChannel(IChannel channel, string message, Embed embed = null)
        {
            var TextChan = (ITextChannel)channel;
            var MesgChan = (IMessageChannel)channel;
            IUserMessage Message;
            await MesgChan.TriggerTypingAsync();
            if (embed == null)
                Message = await MesgChan.SendMessageAsync(message);
            else
            {
                var perm = (await TextChan.Guild.GetUserAsync(Program.Client.CurrentUser.Id)).GetPermissions(TextChan).EmbedLinks;
                if (!perm)
                {
                    if (message != null)
                        Message = await MesgChan.SendMessageAsync(message + "\n" + ConvertEmbedToText(embed));
                    else
                        Message = await MesgChan.SendMessageAsync(ConvertEmbedToText(embed));
                }
                else
                    Message = await MesgChan.SendMessageAsync(message, false, embed);
            }
            return Message;
        }
        private static IUserMessage dmsg = null;
        public static async Task SendChannel(IChannel channel, string message, double Timeout, Embed embed = null)
        {
            var TextChan = (ITextChannel)channel;
            var MesgChan = (IMessageChannel)channel;
            await MesgChan.TriggerTypingAsync();
            if (embed == null)
                dmsg = await MesgChan.SendMessageAsync(message);
            else
            {
                var perm = (await TextChan.Guild.GetUserAsync(Program.Client.CurrentUser.Id)).GetPermissions(TextChan).EmbedLinks;
                if (!perm)
                {
                    if (message != null)
                        dmsg = await MesgChan.SendMessageAsync(message + "\n" + ConvertEmbedToText(embed));
                    else
                        dmsg = await MesgChan.SendMessageAsync(ConvertEmbedToText(embed));
                }
                else if (perm)
                    dmsg = await MesgChan.SendMessageAsync(message, false, embed);
            }
            Stopwatch timer = new Stopwatch();
            timer.Start();
            while (timer.IsRunning)
            {
                if (timer.ElapsedMilliseconds == Timeout * 1000)
                {
                    await dmsg.DeleteAsync();
                    timer.Stop();
                }
            }
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (dmsg != null)
            {
                dmsg.DeleteAsync().Wait();
                dmsg = null;
            }
        }

        public static async Task<IUserMessage> SendDMs(IMessageChannel channel, IDMChannel user, string message, Embed embed = null)
        {
            try
            {
                IUserMessage dmsg = null;
                if (embed == null)
                    dmsg = await user.SendMessageAsync(message);
                else
                    dmsg = await user.SendMessageAsync(message, false, embed);
                await SendChannel(channel, $":ok_hand: Check your DMs fam.");
                return dmsg;
            }
            catch (Exception ex)
            {
                await SendChannel(channel, "I couldn't send it to your DMs, so I sent it here instead... " + message, embed);
                return null;
            }
        }
    }
}
