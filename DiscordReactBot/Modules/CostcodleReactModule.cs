using Discord;
using System.Text;

namespace DiscordReactBot.Modules
{
    internal class CostcodleReactModule
    {
        [Discord.Interactions.RequireBotPermission(ChannelPermission.AddReactions)]
        internal async Task ReactToMessage(IMessage message)
        {
            if (message.Content.Contains("Costcodle", StringComparison.InvariantCulture))
            {
                var messageLines = message.Content.Split('\n').Skip(1).ToList();
                int charOffset = 0;
                int rowOffset = 0;
                int upArrowCount = 0;
                int downArrowCount = 0;
                int greenSquareIndex = -1;
                foreach (var line in messageLines)
                {
                    bool lineIsCountable = true;
                    foreach (var rune in line.EnumerateRunes())
                    {
                        if (IsAllowedEmoji(rune))
                        {
                            if (charOffset == 0)
                            {
                                if (!(rune.Value == AttemptEmojiCodes.UP_ARROW || rune.Value == AttemptEmojiCodes.DOWN_ARROW))
                                {
                                    lineIsCountable = false;
                                    break;
                                }
                            }

                            if (rune.Value == AttemptEmojiCodes.WHITE_CHECK_MARK)
                            {
                                greenSquareIndex = rowOffset;
                                break;
                            }
                            else if (rune.Value == AttemptEmojiCodes.UP_ARROW)
                            {
                                upArrowCount++;
                            }
                            else if (rune.Value == AttemptEmojiCodes.DOWN_ARROW)
                            {
                                downArrowCount++;
                            }
                        }
                        charOffset++;
                    }
                    if (lineIsCountable)
                    {
                        rowOffset++;
                    }
                    charOffset = 0;
                }

                if (rowOffset == 0)
                {
                    var doughnutEmoji = new Emoji(ReactEmojiCodesAsString.DOUGHNUT);
                    await message.AddReactionAsync(doughnutEmoji);
                }
                else if (rowOffset < 0)
                {
                    // should be invalid
                }
                else
                {
                    if (upArrowCount > downArrowCount)
                    {
                        await message.AddReactionAsync(new Emoji(ReactEmojiCodesAsString.MONEY_MOUTH_FACE));
                        await message.AddReactionAsync(new Emoji(ReactEmojiCodesAsString.MONEY_BAG));
                    }
                    else if (downArrowCount > upArrowCount)
                    {
                        await message.AddReactionAsync(new Emoji(ReactEmojiCodesAsString.CHART_WITH_DOWNWARDS_TREND));
                    }
                    else
                    {
                        await message.AddReactionAsync(new Emoji(ReactEmojiCodesAsString.CREDIT_CARD));
                    }

                    if (rowOffset > 2)
                    {
                        var loadingEmote = Emote.Parse(ReactEmojiCodesAsString.LOADING);
                        var fEmoji = new Emoji(ReactEmojiCodesAsString.REGIONAL_INDICATOR_F);
                        await message.AddReactionAsync(loadingEmote);
                        await message.AddReactionAsync(fEmoji);

                    }
                }
            }
        }

        private bool IsEmoji(Rune rune)
        {
            return rune.Value >= 0x1F300 && rune.Value <= 0x1F9FF || rune.Value >= 0x2600 && rune.Value <= 0x26FF;
        }

        private bool IsAllowedEmoji(Rune rune)
        {
            return rune.Value switch
            {
                AttemptEmojiCodes.UP_ARROW => true,
                AttemptEmojiCodes.DOWN_ARROW => true,
                AttemptEmojiCodes.WHITE_CHECK_MARK => true,
                AttemptEmojiCodes.ORANGE_SQUARE => true,
                AttemptEmojiCodes.YELLOW_SQUARE => true,
                AttemptEmojiCodes.PURPLE_SQUARE => true,
                AttemptEmojiCodes.RED_SQUARE => true,
                AttemptEmojiCodes.GREEN_SQUARE => true,
                _ => false
            };
        }
    }

    
}
