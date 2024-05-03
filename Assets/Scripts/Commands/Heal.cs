using Fusion;


public class Heal : ICommand
{
    int amount;

    public Heal(int _amount)
    {
        amount = _amount;
    }

    public int CountOfRandomTarget()
    {
        return 0;
    }

    //public NetworkData[] DoAndGetRandomTarget(CardMono mine, NetworkId _target)
    //{
    //}

    public void Execute(CardMono mine, NetworkId target)
    {

    }

    public bool IsNeedTarget()
    {
        return true;
    }
}
