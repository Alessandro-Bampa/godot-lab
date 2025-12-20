using Godot;
using System;

public partial class GameSession : Node
{
    public static GameSession Instance { get; private set; }

    // Real time memory so read from this
    public GameSaveData ActiveData { get; private set; }

    private const string SavePath = "user://savegame.tres";

    public override void _Ready()
    {
        Instance = this;
        LoadGame();
    }

    public void LoadGame()
    {
        if (FileAccess.FileExists(SavePath))
        {
            // ResourceLoader ricostruisce l'oggetto completo
            ActiveData = ResourceLoader.Load<GameSaveData>(SavePath, null, ResourceLoader.CacheMode.Ignore); // In futuro probabilmente usare LoadThreadGet() per caricare in background il salvataggio durante schermata di caricamento
            GD.Print("Salvataggio caricato.");
        }
        else
        {
            // Nessun salvataggio, creiamo una nuova partita
            CreateNewGame();
        }
    }

    private void CreateNewGame()
    {
        GD.Print("Creazione nuova partita.");
        ActiveData = new GameSaveData();

        // ESEMPIO: Equipaggiamo uno zaino di base all'inizio
        // Carichiamo il template base
        var baseBackpack = GD.Load<ItemData>("res://resources/Items/Backpacks/BasicBackpack.tres");
        var baseBackpack2 = GD.Load<ItemData>("res://resources/Items/Backpacks/BasicBackpack.tres");
        var baseBackpack3 = GD.Load<ItemData>("res://resources/Items/Backpacks/BasicBackpack.tres");

        if (baseBackpack != null)
        {
            // 2. IMPORTANTE: Duplica con 'true' per copiare anche l'inventario interno
            var myBackpack = (ItemData)baseBackpack.Duplicate(true);
            var myBackpack2 = (ItemData)baseBackpack2.Duplicate(true);
            var myBackpack3 = (ItemData)baseBackpack3.Duplicate(true);

            // 3. Inseriscilo nello slot "Backpack" (deve coincidere con SlotType nell'editor)
            ActiveData.Equipment["Backpack"] = myBackpack;
            ActiveData.Equipment["Head"] = myBackpack2;
            ActiveData.Equipment["Body"] = myBackpack3;

            GD.Print("Zaino equipaggiato di default!");
        }
        else
        {
            GD.PushError("Non trovo BasicBackpack.tres! Controlla il percorso.");
        }
    }

    public void SaveGame()
    {
        // ResourceSaver salva tutta la struttura di ActiveData, 
        // inclusi i riferimenti agli ItemData, in un file binario o testo.
        Error err = ResourceSaver.Save(ActiveData, SavePath);
        if (err == Error.Ok)
        {
            GD.Print("Gioco salvato correttamente in Resource nativa.");
        }
        else
        {
            GD.PushError($"Errore nel salvataggio: {err}");
        }
    }

    public void DeleteSave()
    {
        if (FileAccess.FileExists(SavePath))
            DirAccess.RemoveAbsolute(SavePath);

        ActiveData = new GameSaveData();
    }

    // Funzione helper per ottenere l'inventario di uno slot specifico
    public InventoryData GetInventoryFromSlot(string slotName)
    {
        if (ActiveData.Equipment.ContainsKey(slotName))
        {
            var item = ActiveData.Equipment[slotName];
            if (item != null && item.InternalInventory != null)
            {
                return item.InternalInventory;
            }
        }
        return null; // Slot vuoto o oggetto senza inventario
    }
}
