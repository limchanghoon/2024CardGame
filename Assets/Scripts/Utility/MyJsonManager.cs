using System.IO;
using UnityEngine;

public class MyJsonManager : MonoBehaviour
{
    public void SaveDeckData(int slotIdx, DeckData deckData)
    {
        string dirPath = Path.Combine(Application.persistentDataPath, "Deck");
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);
        // Deck
        string path = Path.Combine(dirPath, "Slot" + slotIdx.ToString() + ".json");
        string json = JsonUtility.ToJson(deckData, true);
        File.WriteAllText(path, json);
    }

    public DeckData LoadDeckData(int slotIdx)
    {
        string dirPath = Path.Combine(Application.persistentDataPath, "Deck");
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);

        string path = Path.Combine(dirPath, "Slot" + slotIdx.ToString() + ".json");
        if (!File.Exists(path))
        {
            DeckData deckData = new DeckData();
            SaveDeckData(slotIdx, deckData);
            return deckData;
        }

        string loadedJson = File.ReadAllText(path);
        var loadedData = JsonUtility.FromJson<DeckData>(loadedJson);
        return loadedData;
    }
}
