namespace Shin_Megami_Tensei.Domain.ValueObjects;

public class TurnEffect
{
    public int FullTurnsConsumed { get; init; }
    public int BlinkingTurnsConsumed { get; init; }
    public int BlinkingTurnsGained { get; init; }
    public bool ConsumeAllTurns { get; init; }

    public TurnEffect()
    {
        FullTurnsConsumed = 0;
        BlinkingTurnsConsumed = 0;
        BlinkingTurnsGained = 0;
        ConsumeAllTurns = false;
    }

    public TurnEffect(int fullTurnsConsumed, int blinkingTurnsConsumed, int blinkingTurnsGained, bool consumeAllTurns = false)
    {
        FullTurnsConsumed = fullTurnsConsumed;
        BlinkingTurnsConsumed = blinkingTurnsConsumed;
        BlinkingTurnsGained = blinkingTurnsGained;
        ConsumeAllTurns = consumeAllTurns;
    }
}

