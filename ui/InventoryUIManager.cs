using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class InventoryUIManager : Control
{
    [Export] public VBoxContainer RightPanelContainer;
    [Export] public Control LeftPanelContainer;
    [Export] public InventoryGridUI VicinityGrid;
    [Export] public PackedScene GridPrefab;
    [Export] public PackedScene ContainerWindowPrefab;

    // --- MODIFICA 1: Struttura per tenere traccia di Wrapper e Griglia insieme ---
    private class ContainerView
    {
        public VBoxContainer Wrapper; // Il contenitore che ha Label + Griglia
        public InventoryGridUI Grid;  // La griglia vera e propria
    }

    // Usiamo questa nuova struttura nel dizionario
    private Dictionary<string, ContainerView> _activeViews = new();

    private Dictionary<string, EquipmentSlot> _equipmentSlots = new();

    private Dictionary<InventoryData, ContainerWindow> _openWindows = new();

    public override void _Ready()
    {
        // Collegamenti base Slot Equipaggiamento
        foreach (var child in LeftPanelContainer.GetChildren())
        {
            if (child is EquipmentSlot slot)
            {
                _equipmentSlots[slot.SlotType] = slot;
                slot.UIManager = this;
            }
        }

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.UIManager = this;
            InventoryManager.Instance.InventoryToggled += OnInventoryToggled;
            InventoryManager.Instance.InventoryUpdated += RefreshAll;
            InventoryManager.Instance.EquipmentUpdated += OnEquipmentChanged;
        }

        Visible = false;

        // Inizializzazione Vicinity (Questa è fissa, se vuoi un titolo aggiungilo nella scena)
        if (VicinityGrid != null)
        {
            var lootData = new InventoryData();
            lootData.GridWidth = 6;
            lootData.GridHeight = 4;
            VicinityGrid.SetInventoryData(lootData);
        }
    }

    private void OnInventoryToggled(bool isOpen)
    {
        Visible = isOpen;
        if (isOpen) RefreshAll();
    }

    public void RefreshAll()
    {
        if (GameSession.Instance == null || GameSession.Instance.ActiveData == null) return;
        var data = GameSession.Instance.ActiveData;

        // 1. Aggiorna Slot Equipaggiamento
        foreach (var slotName in _equipmentSlots.Keys)
        {
            ItemData item = null;
            if (data.Equipment.ContainsKey(slotName))
            {
                item = data.Equipment[slotName];
            }
            UpdateEquipmentSlot(slotName, item);
        }

        // 2. Aggiorna Tasche
        if (data.Pockets != null)
        {
            // Passiamo "Tasche" come titolo
            ForceCreateOrUpdateGrid("Pockets", data.Pockets, "Tasche");
        }
    }

    private void OnEquipmentChanged(string slotName, ItemData newItem)
    {
        UpdateEquipmentSlot(slotName, newItem);
    }

    public void UpdateEquipmentSlot(string slotName, ItemData item)
    {
        if (_equipmentSlots.ContainsKey(slotName))
        {
            _equipmentSlots[slotName].RefreshVisual(item);
        }

        UpdateContainerPanel(slotName, item);
    }

    private void UpdateContainerPanel(string slotName, ItemData item)
    {
        if (item == null || item.InternalInventory == null)
        {
            RemoveContainerGrid(slotName);
            return;
        }
        // Usiamo item.Name (es. "Zaino Militare") come titolo
        ForceCreateOrUpdateGrid(slotName, item.InternalInventory, item.Name);
    }

    public void UpdateContainerPanel(string containerId, InventoryData data)
    {
        ForceCreateOrUpdateGrid(containerId, data, containerId);
    }

    // --- MODIFICA 2: Creazione Dinamica con Titolo ---
    private void ForceCreateOrUpdateGrid(string containerId, InventoryData data, string title)
    {
        // A. Se esiste già, aggiorniamo solo i dati
        if (_activeViews.ContainsKey(containerId))
        {
            _activeViews[containerId].Grid.SetInventoryData(data);
            return;
        }

        // B. Creazione nuova vista

        // 1. Wrapper verticale
        var wrapper = new VBoxContainer();
        wrapper.AddThemeConstantOverride("separation", 4);

        // 2. Creiamo la Griglia (La istanziamo PRIMA per poterla usare nell'evento della label)
        var newGridInstance = GridPrefab.Instantiate<InventoryGridUI>();

        // 3. Creiamo la Label "Cliccabile"
        var label = new Label();
        label.Text = "▼ " + title; // Aggiungiamo una freccetta indicativa
        label.HorizontalAlignment = HorizontalAlignment.Center;

        // FONDAMENTALE: Le Label di solito ignorano il mouse. Dobbiamo attivarlo.
        label.MouseFilter = MouseFilterEnum.Stop;

        // Opzionale: Cambia il cursore quando passi sopra la label per far capire che è cliccabile
        label.MouseDefaultCursorShape = CursorShape.PointingHand;

        // 4. Logica del Click (Fold/Unfold) con funzione Lambda
        label.GuiInput += (@event) =>
        {
            if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.Pressed)
            {
                // Inverti la visibilità
                bool isVisible = !newGridInstance.Visible;
                newGridInstance.Visible = isVisible;

                // Aggiorna la freccetta (▼ aperto, ▶ chiuso)
                label.Text = (isVisible ? "▼ " : "▶ ") + title;
            }
        };

        wrapper.AddChild(label);
        wrapper.AddChild(newGridInstance);

        // 5. Aggiungiamo tutto al pannello di destra
        RightPanelContainer.AddChild(wrapper);

        // 6. Configuriamo i dati
        newGridInstance.SetInventoryData(data);

        // 7. Salviamo il riferimento
        var entry = new ContainerView
        {
            Wrapper = wrapper,
            Grid = newGridInstance
        };
        _activeViews.Add(containerId, entry);
    }

    private void RemoveContainerGrid(string containerId)
    {
        if (_activeViews.ContainsKey(containerId))
        {
            // Distruggiamo il wrapper, che si porterà via anche la Label e la Griglia
            _activeViews[containerId].Wrapper.QueueFree();
            _activeViews.Remove(containerId);
        }
    }

    public void SetVicinityData(InventoryData lootData)
    {
        if (VicinityGrid != null) VicinityGrid.SetInventoryData(lootData);
    }

    public void OpenExternalContainer(InventoryData data, string title)
    {
        if (ContainerWindowPrefab == null) return;

        // 1. CONTROLLO DUPLICATI
        if (_openWindows.ContainsKey(data))
        {
            // Se la finestra esiste già ma per qualche motivo è nulla (bug), pulisci
            if (!IsInstanceValid(_openWindows[data]))
            {
                _openWindows.Remove(data);
            }
            else
            {
                // La finestra esiste! Portiamola in primo piano e lampeggiamola/evidenziamola
                GD.Print($"Il contenitore '{title}' è già aperto!");
                _openWindows[data].MoveToFront();

                // Opzionale: Effetto visivo per dire "Ehi sono qui"
                var tween = CreateTween();
                tween.TweenProperty(_openWindows[data], "modulate", Colors.Red, 0.1f);
                tween.TweenProperty(_openWindows[data], "modulate", Colors.White, 0.1f);
                return; // ESCI, non crearne un'altra
            }
        }

        // 2. CREAZIONE FINESTRA
        var window = ContainerWindowPrefab.Instantiate<ContainerWindow>();
        AddChild(window);

        // Posizionamento intelligente (Centrato sul mouse ma dentro lo schermo gestito poi dal Clamp)
        window.GlobalPosition = GetGlobalMousePosition() + new Vector2(10, 10);

        // 3. REGISTRAZIONE
        _openWindows.Add(data, window);

        // Ascolta quando si chiude per rimuoverla dalla lista
        window.WindowClosed += OnExternalWindowClosed;

        window.Init(data, title);
    }

    // Callback quando l'utente preme X
    private void OnExternalWindowClosed(ContainerWindow window, InventoryData data)
    {
        if (_openWindows.ContainsKey(data))
        {
            _openWindows.Remove(data);
        }
    }

}