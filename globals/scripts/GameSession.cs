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
            ActiveData = ResourceLoader.Load<GameSaveData>(SavePath); // In futuro probabilmente usare LoadThreadGet() per caricare in background il salvataggio durante schermata di caricamento
            GD.Print("Salvataggio caricato.");
        }
        else
        {
            // Nessun salvataggio, creiamo una nuova partita
            GD.Print("Nessun salvataggio trovato. Creazione nuovi dati.");
            ActiveData = new GameSaveData();
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
}
