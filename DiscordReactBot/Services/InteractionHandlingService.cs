using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordReactBot.Modules;
using DiscordReactBot.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;


namespace DiscordReactBot.Services
{
    internal class InteractionHandlingService : IHostedService
    {
        private readonly DiscordSocketClient _discord;
        private readonly InteractionService _interactions;
        private readonly IServiceProvider _services;
        private readonly ILogger<InteractionService> _logger;
        private readonly CantareReactModule _cantareReactModule;
        private readonly CostcodleReactModule _costcodleReactModule;

        public InteractionHandlingService(
            DiscordSocketClient discord,
            InteractionService interactions,
            IServiceProvider services,
            IConfiguration config,
            ILogger<InteractionService> logger)
        {
            _discord = discord;
            _interactions = interactions;
            _services = services;
            _logger = logger;
            _cantareReactModule = new CantareReactModule();
            _costcodleReactModule = new CostcodleReactModule();

            _interactions.Log += msg => LogUtility.Log(_logger, msg, discord, config.GetSection("adminUserIDs").Get<List<ulong>>());
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _discord.Ready += () => _interactions.RegisterCommandsGloballyAsync(true);
            _discord.InteractionCreated += OnInteractionAsync;
            _discord.MessageReceived += MessageReceivedAsync;

            await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _interactions.Dispose();
            return Task.CompletedTask;
        }

        private async Task OnInteractionAsync(SocketInteraction interaction)
        {
            try
            {
                var context = new SocketInteractionContext(_discord, interaction);
                var result = await _interactions.ExecuteCommandAsync(context, _services);

                if (!result.IsSuccess)
                    await context.Channel.SendMessageAsync(result.ToString());
            }
            catch
            {
                if (interaction.Type == InteractionType.ApplicationCommand)
                {
                    await interaction.GetOriginalResponseAsync()
                        .ContinueWith(msg => msg.Result.DeleteAsync());
                }
            }
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            if (!message.Author.IsBot)   // ignore other bot messages (including this bot) for now
            {
                await _cantareReactModule.ReactToMessage(message);
                await _costcodleReactModule.ReactToMessage(message);
            }
        }
    }
}