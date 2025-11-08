namespace Shin_Megami_Tensei.Domain.ValueObjects;

public readonly struct TurnCost
{
    public int FullTurnsConsumed { get; }
    public int BlinkingTurnsConsumed { get; }
    public int BlinkingTurnsGained { get; }
    public bool ConsumeAllTurns { get; }

    public TurnCost(
        int fullTurnsConsumed = 0,
        int blinkingTurnsConsumed = 0,
        int blinkingTurnsGained = 0,
        bool consumeAllTurns = false)
    {
        FullTurnsConsumed = fullTurnsConsumed;
        BlinkingTurnsConsumed = blinkingTurnsConsumed;
        BlinkingTurnsGained = blinkingTurnsGained;
        ConsumeAllTurns = consumeAllTurns;
    }

    public static TurnCost ConsumeAll() => new(consumeAllTurns: true);
    
    public static TurnCost ConsumeFull(int count = 1) => new(fullTurnsConsumed: count);
    
    public static TurnCost ConsumeBlinking(int count) => new(blinkingTurnsConsumed: count);
    
    public static TurnCost ConsumeFullAndGainBlinking(int fullConsumed = 1, int blinkingGained = 1) 
        => new(fullTurnsConsumed: fullConsumed, blinkingTurnsGained: blinkingGained);
    
    public static TurnCost ConsumeOneOfEither() 
        => new(fullTurnsConsumed: 1, blinkingTurnsConsumed: 1);
    
    public static TurnCost ConsumeOneOfEitherAndMaybeGainBlinking() 
        => new(fullTurnsConsumed: 1, blinkingTurnsConsumed: 1, blinkingTurnsGained: 1);
}

