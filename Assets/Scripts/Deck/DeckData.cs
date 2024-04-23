[System.Serializable]
public class DeckData
{
    public string DeckName;
    public ID_Grade[] cardID_Grades;

    public DeckData()
    {
        DeckName = "ºó µ¦";
        cardID_Grades = new ID_Grade[50];
        for (int i = 0; i < 50; i++)
        {
            cardID_Grades[i] = new ID_Grade();
        }
    }
}
