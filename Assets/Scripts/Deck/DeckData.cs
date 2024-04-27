[System.Serializable]
public class DeckData
{
    public string DeckName;
    public int[] cardIDs;

    public DeckData()
    {
        DeckName = "ºó µ¦";
        cardIDs = new int[30];
    }

    public void Shuffle()
    {
        int random1, random2;
        int temp;

        for (int i = 0; i < cardIDs.Length; ++i)
        {
            random1 = UnityEngine.Random.Range(0, cardIDs.Length);
            random2 = UnityEngine.Random.Range(0, cardIDs.Length);

            temp = cardIDs[random1];
            cardIDs[random1] = cardIDs[random2];
            cardIDs[random2] = temp;
        }
    }
}
