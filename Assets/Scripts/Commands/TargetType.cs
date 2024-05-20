
using System;

[Flags]
public enum TargetType
{
    MyMinion = 1,
    OpponentMinion = 2,
    TauntMinion = 4,
    MyHero = 8,
    OpponentHero = 16,
    AllMinion = MyMinion | OpponentMinion | TauntMinion,
    Hero = MyHero | OpponentHero,
    Mine = MyMinion | MyHero,
    Opponent = OpponentMinion | OpponentHero,
    All = int.MaxValue
}
