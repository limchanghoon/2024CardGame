[System.Serializable]
public class DeckData
{
    public string DeckName;
    public int[] cardIDs;

    public DeckData()
    {
        DeckName = "�� ��";
        cardIDs = new int[30];
    }
}
