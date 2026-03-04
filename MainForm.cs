using System.Drawing.Drawing2D;
using System.Reflection;

namespace RE5SteamIdSwapper;

public class MainForm : Form
{
    // ── Controles ────────────────────────────────────────────────────────────
    private readonly TextBox _steamIdBox;
    private readonly Panel   _dropPanel;
    private readonly Label   _dropIcon;
    private readonly Label   _dropLabel;
    private readonly Label   _dropSubLabel;
    private readonly TextBox _pathBox;
    private readonly Button  _browseBtn;
    private readonly Button  _applyBtn;
    private readonly Button  _ngBtn;
    private readonly Button  _ngPlusBtn;
    private readonly Panel   _statusPanel;
    private readonly Label   _statusLabel;

    // ── Estado ───────────────────────────────────────────────────────────────
    private string? _droppedFile;

    // ── Paleta ───────────────────────────────────────────────────────────────
    private static readonly Color BgColor       = Color.FromArgb(13, 13, 18);
    private static readonly Color HeaderBg      = Color.FromArgb(8,  8,  12);
    private static readonly Color CardBg        = Color.FromArgb(22, 22, 32);
    private static readonly Color CardHoverBg   = Color.FromArgb(26, 26, 38);
    private static readonly Color AccentRed     = Color.FromArgb(185, 28, 28);
    private static readonly Color AccentRedHov  = Color.FromArgb(220, 38, 38);
    private static readonly Color AccentRedDown = Color.FromArgb(153, 27, 27);
    private static readonly Color QuickBg       = Color.FromArgb(28, 28, 42);
    private static readonly Color QuickHov      = Color.FromArgb(36, 36, 54);
    private static readonly Color QuickDown     = Color.FromArgb(20, 20, 32);
    private static readonly Color TextPrimary   = Color.FromArgb(238, 238, 245);
    private static readonly Color TextMuted     = Color.FromArgb(120, 120, 140);
    private static readonly Color BorderColor   = Color.FromArgb(48,  48,  65);
    private static readonly Color SuccessGreen  = Color.FromArgb(74,  222, 128);
    private static readonly Color SuccessBg     = Color.FromArgb(10,  30,  18);
    private static readonly Color ErrorRed      = Color.FromArgb(248, 113, 113);
    private static readonly Color ErrorBg       = Color.FromArgb(30,  10,  10);

    // ── Construtor ───────────────────────────────────────────────────────────
    public MainForm()
    {
        // ── Form base ─────────────────────────────────────────────────────
        Text            = "RE5 Steam ID Swapper";
        ClientSize      = new Size(480, 652);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;
        BackColor       = BgColor;
        StartPosition   = FormStartPosition.CenterScreen;
        Font            = new Font("Segoe UI", 9f);

        // ── Header ────────────────────────────────────────────────────────
        var header = new Panel
        {
            Location  = new Point(0, 0),
            Size      = new Size(480, 82),
            BackColor = HeaderBg,
        };

        var redLine = new Panel
        {
            Location  = new Point(0, 79),
            Size      = new Size(480, 3),
            BackColor = AccentRed,
        };

        var titleLbl = new Label
        {
            Text      = "RE5 STEAM ID SWAPPER",
            Font      = new Font("Segoe UI", 15f, FontStyle.Bold),
            ForeColor = TextPrimary,
            Location  = new Point(22, 16),
            AutoSize  = true,
        };

        var subtitleLbl = new Label
        {
            Text      = "Speedrun Save Tool  ·  Resident Evil 5",
            Font      = new Font("Segoe UI", 8.5f),
            ForeColor = AccentRed,
            Location  = new Point(24, 50),
            AutoSize  = true,
        };

        header.Controls.AddRange([titleLbl, subtitleLbl, redLine]);

        // ── Steam ID ──────────────────────────────────────────────────────
        var idLbl = MakeCaption("STEAM ID  (64-bit)", new Point(30, 105));

        _steamIdBox = new TextBox
        {
            Location        = new Point(30, 128),
            Size            = new Size(420, 32),
            BackColor       = CardBg,
            ForeColor       = TextPrimary,
            BorderStyle     = BorderStyle.FixedSingle,
            Font            = new Font("Consolas", 11f),
            PlaceholderText = "Ex: 76561198000000000",
        };
        _steamIdBox.Enter += (_, _) => _steamIdBox.BackColor = CardHoverBg;
        _steamIdBox.Leave += (_, _) => { _steamIdBox.BackColor = CardBg; SaveSettings(); };

        // ── Drop zone ─────────────────────────────────────────────────────
        var dropLbl = MakeCaption("ARQUIVO DE SAVE", new Point(30, 183));

        _dropPanel = new Panel
        {
            Location  = new Point(30, 206),
            Size      = new Size(420, 118),
            BackColor = CardBg,
            AllowDrop = true,
            Cursor    = Cursors.Hand,
        };

        _dropIcon = new Label
        {
            Text      = "⬇",
            Font      = new Font("Segoe UI", 22f),
            ForeColor = TextMuted,
            Location  = new Point(0, 10),
            Size      = new Size(420, 44),
            TextAlign = ContentAlignment.MiddleCenter,
        };

        _dropLabel = new Label
        {
            Text      = "Arraste o arquivo de save aqui",
            Font      = new Font("Segoe UI", 10f),
            ForeColor = TextMuted,
            Location  = new Point(0, 56),
            Size      = new Size(420, 24),
            TextAlign = ContentAlignment.MiddleCenter,
        };

        _dropSubLabel = new Label
        {
            Text      = "",
            Font      = new Font("Segoe UI", 8f),
            ForeColor = Color.FromArgb(72, 72, 90),
            Location  = new Point(0, 86),
            Size      = new Size(420, 20),
            TextAlign = ContentAlignment.MiddleCenter,
            Visible   = false,
        };

        _dropPanel.Controls.AddRange([_dropIcon, _dropLabel, _dropSubLabel]);
        _dropPanel.Paint     += DropPanel_Paint;
        _dropPanel.DragEnter += DropPanel_DragEnter;
        _dropPanel.DragLeave += DropPanel_DragLeave;
        _dropPanel.DragDrop  += DropPanel_DragDrop;

        // ── Pasta de destino ──────────────────────────────────────────────
        var pathLbl = MakeCaption("PASTA REMOTE DO STEAM", new Point(30, 342));

        _pathBox = new TextBox
        {
            Location    = new Point(30, 364),
            Size        = new Size(373, 32),
            BackColor   = CardBg,
            ForeColor   = TextMuted,
            BorderStyle = BorderStyle.FixedSingle,
            Font        = new Font("Segoe UI", 8.5f),
            Text        = @"C:\Program Files (x86)\Steam\userdata\SEU_ID\21690\remote",
        };
        _pathBox.Enter += (_, _) =>
        {
            _pathBox.BackColor = CardHoverBg;
            if (_pathBox.ForeColor == TextMuted) _pathBox.ForeColor = TextPrimary;
        };
        _pathBox.Leave += (_, _) => _pathBox.BackColor = CardBg;

        _browseBtn = new Button
        {
            Text      = "📁",
            Location  = new Point(408, 364),
            Size      = new Size(42, 32),
            BackColor = CardBg,
            ForeColor = TextMuted,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 13f),
            Cursor    = Cursors.Hand,
            TabStop   = false,
        };
        _browseBtn.FlatAppearance.BorderColor        = BorderColor;
        _browseBtn.FlatAppearance.BorderSize         = 1;
        _browseBtn.FlatAppearance.MouseOverBackColor = CardHoverBg;
        _browseBtn.FlatAppearance.MouseDownBackColor = Color.FromArgb(30, 30, 45);
        _browseBtn.Click += BrowseBtn_Click;

        // ── Botão aplicar ─────────────────────────────────────────────────
        _applyBtn = new Button
        {
            Text      = "ALTERAR STEAM ID",
            Location  = new Point(30, 416),
            Size      = new Size(420, 52),
            BackColor = AccentRed,
            ForeColor = TextPrimary,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
            Cursor    = Cursors.Hand,
        };
        _applyBtn.FlatAppearance.BorderSize          = 0;
        _applyBtn.FlatAppearance.MouseOverBackColor  = AccentRedHov;
        _applyBtn.FlatAppearance.MouseDownBackColor  = AccentRedDown;
        _applyBtn.Click += ApplyBtn_Click;

        // ── Início rápido ─────────────────────────────────────────────────
        var quickCaption = MakeCaption("INÍCIO RÁPIDO", new Point(30, 484));

        _ngBtn = MakeQuickBtn("NEW GAME", new Point(30, 504));
        _ngBtn.Click += (_, _) => QuickStartBtn_Click("Ng.bin", "New Game");

        _ngPlusBtn = MakeQuickBtn("NEW GAME +", new Point(247, 504));
        _ngPlusBtn.Click += (_, _) => QuickStartBtn_Click("NgPlus.bin", "New Game +");

        // ── Status ────────────────────────────────────────────────────────
        _statusPanel = new Panel
        {
            Location    = new Point(30, 562),
            Size        = new Size(420, 64),
            BackColor   = Color.Transparent,
            Visible     = false,
            BorderStyle = BorderStyle.None,
        };

        _statusLabel = new Label
        {
            Text      = "",
            Font      = new Font("Segoe UI", 9.5f),
            ForeColor = SuccessGreen,
            Location  = new Point(12, 0),
            Size      = new Size(396, 64),
            TextAlign = ContentAlignment.MiddleCenter,
        };
        _statusPanel.Controls.Add(_statusLabel);

        // ── Rodapé ────────────────────────────────────────────────────────
        var footer = new Label
        {
            Text      = "Para uso em speedruns  ·  Resident Evil 5",
            Font      = new Font("Segoe UI", 7.5f),
            ForeColor = Color.FromArgb(48, 48, 64),
            Location  = new Point(0, 632),
            Size      = new Size(480, 20),
            TextAlign = ContentAlignment.MiddleCenter,
        };

        // ── Carregar configurações salvas ─────────────────────────────────
        LoadSettings();

        // ── Montar form ───────────────────────────────────────────────────
        Controls.AddRange(
        [
            header,
            idLbl,      _steamIdBox,
            dropLbl,    _dropPanel,
            pathLbl,    _pathBox,    _browseBtn,
            _applyBtn,
            quickCaption, _ngBtn, _ngPlusBtn,
            _statusPanel,
            footer,
        ]);
    }

    // ── Drop zone ─────────────────────────────────────────────────────────────
    private void DropPanel_Paint(object? sender, PaintEventArgs e)
    {
        Color borderColor = _droppedFile != null ? SuccessGreen : BorderColor;
        using var pen = new Pen(borderColor, 1.5f) { DashStyle = DashStyle.Dash };
        e.Graphics.DrawRectangle(pen, 1, 1, _dropPanel.Width - 3, _dropPanel.Height - 3);
    }

    private void DropPanel_DragEnter(object? sender, DragEventArgs e)
    {
        if (!e.Data!.GetDataPresent(DataFormats.FileDrop)) return;
        e.Effect = DragDropEffects.Copy;
        _dropPanel.BackColor = Color.FromArgb(26, 34, 26);
        _dropIcon.ForeColor  = SuccessGreen;
        _dropLabel.ForeColor = SuccessGreen;
        _dropLabel.Text      = "Solte o arquivo aqui!";
        _dropPanel.Invalidate();
    }

    private void DropPanel_DragLeave(object? sender, EventArgs e)
    {
        _dropPanel.BackColor = CardBg;
        if (_droppedFile == null)
        {
            _dropIcon.ForeColor  = TextMuted;
            _dropLabel.ForeColor = TextMuted;
            _dropLabel.Text      = "Arraste o arquivo de save aqui";
        }
        _dropPanel.Invalidate();
    }

    private void DropPanel_DragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data!.GetData(DataFormats.FileDrop) is not string[] files || files.Length == 0)
            return;

        _droppedFile = files[0];
        string name = Path.GetFileName(_droppedFile);

        _dropIcon.Text       = "✓";
        _dropIcon.ForeColor  = SuccessGreen;
        _dropLabel.Text      = name;
        _dropLabel.ForeColor = SuccessGreen;
        _dropPanel.BackColor = Color.FromArgb(14, 28, 18);

        _dropSubLabel.Text    = Path.GetDirectoryName(_droppedFile) ?? "";
        _dropSubLabel.Visible = true;

        _statusPanel.Visible = false;
        _dropPanel.Invalidate();
    }

    // ── Browse ────────────────────────────────────────────────────────────────
    private void BrowseBtn_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description            = "Selecione a pasta remote do Steam (21690\\remote)",
            UseDescriptionForTitle = true,
            ShowNewFolderButton    = false,
        };

        string current = _pathBox.Text.Trim();
        if (Directory.Exists(current))
            dialog.InitialDirectory = current;

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            _pathBox.Text      = dialog.SelectedPath;
            _pathBox.ForeColor = TextPrimary;
            SaveSettings();
        }
    }

    // ── Botão aplicar ─────────────────────────────────────────────────────────
    private void ApplyBtn_Click(object? sender, EventArgs e)
    {
        _statusPanel.Visible = false;

        if (!TryGetInputs(out ulong steamId, out string targetDir)) return;

        if (_droppedFile == null)
        {
            ShowStatus("Por favor arraste um arquivo de save (.bin) para a área acima.", success: false);
            return;
        }

        if (!File.Exists(_droppedFile))
        {
            ShowStatus("Arquivo não encontrado. Tente arrastar novamente.", success: false);
            _droppedFile = null;
            ResetDropZone();
            return;
        }

        try
        {
            byte[] raw       = File.ReadAllBytes(_droppedFile);
            byte[] processed = RE5SaveHelper.ProcessSave(raw, steamId);

            WriteToRemote(processed, targetDir, out string shortPath);
            ShowStatus(
                $"Steam ID alterado com sucesso!\n" +
                $"savedata.bin salvo em:\n" +
                $"{shortPath}",
                success: true);

            SaveSettings();
            ResetDropZone();
        }
        catch (UnauthorizedAccessException)
        {
            ShowStatus("Acesso negado.\nExecute o programa como Administrador.", success: false);
        }
        catch (Exception ex)
        {
            ShowStatus($"Erro: {ex.Message}", success: false);
        }
    }

    // ── Botões início rápido ──────────────────────────────────────────────────
    private void QuickStartBtn_Click(string resourceName, string label)
    {
        _statusPanel.Visible = false;

        if (!TryGetInputs(out ulong steamId, out string targetDir)) return;

        try
        {
            var    assembly = Assembly.GetExecutingAssembly();
            string resName  = $"RE5SteamIdSwapper.Saves.{resourceName}";
            using  var stream = assembly.GetManifestResourceStream(resName)
                                ?? throw new Exception($"Recurso '{resourceName}' não encontrado.");

            byte[] raw = new byte[stream.Length];
            _ = stream.Read(raw, 0, raw.Length);

            byte[] processed = RE5SaveHelper.ProcessSave(raw, steamId);

            WriteToRemote(processed, targetDir, out string shortPath);
            ShowStatus(
                $"{label} pronto!\n" +
                $"savedata.bin salvo em:\n" +
                $"{shortPath}",
                success: true);
        }
        catch (UnauthorizedAccessException)
        {
            ShowStatus("Acesso negado.\nExecute o programa como Administrador.", success: false);
        }
        catch (Exception ex)
        {
            ShowStatus($"Erro: {ex.Message}", success: false);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Valida Steam ID e path; exibe erro e retorna false se inválido.</summary>
    private bool TryGetInputs(out ulong steamId, out string targetDir)
    {
        steamId   = 0;
        targetDir = "";

        string rawId = _steamIdBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(rawId))
        {
            ShowStatus("Por favor insira o seu Steam ID.", success: false);
            return false;
        }

        if (!ulong.TryParse(rawId, out steamId))
        {
            ShowStatus("Steam ID inválido.\nUse apenas números (ex: 76561198000000000).", success: false);
            return false;
        }

        targetDir = _pathBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(targetDir))
        {
            ShowStatus("Por favor selecione a pasta remote do Steam.", success: false);
            return false;
        }

        return true;
    }

    private static void WriteToRemote(byte[] data, string targetDir, out string shortPath)
    {
        Directory.CreateDirectory(targetDir);
        string targetPath = Path.Combine(targetDir, "savedata.bin");
        File.WriteAllBytes(targetPath, data);
        shortPath = ShortenPath(targetPath);
    }

    private void ShowStatus(string message, bool success)
    {
        _statusLabel.Text      = message;
        _statusLabel.ForeColor = success ? SuccessGreen : ErrorRed;
        _statusPanel.BackColor = success ? SuccessBg    : ErrorBg;
        _statusPanel.Visible   = true;
        _statusPanel.Invalidate();
    }

    private void ResetDropZone()
    {
        _droppedFile          = null;
        _dropIcon.Text        = "⬇";
        _dropIcon.ForeColor   = TextMuted;
        _dropLabel.Text       = "Arraste o arquivo de save aqui";
        _dropLabel.ForeColor  = TextMuted;
        _dropPanel.BackColor  = CardBg;
        _dropSubLabel.Visible = false;
        _dropPanel.Invalidate();
    }

    private void SaveSettings() =>
        SettingsManager.Save(new AppSettings
        {
            SteamId    = _steamIdBox.Text.Trim(),
            RemotePath = _pathBox.Text.Trim(),
        });

    private void LoadSettings()
    {
        var s = SettingsManager.Load();

        if (!string.IsNullOrWhiteSpace(s.SteamId))
            _steamIdBox.Text = s.SteamId;

        if (!string.IsNullOrWhiteSpace(s.RemotePath))
        {
            _pathBox.Text      = s.RemotePath;
            _pathBox.ForeColor = TextPrimary;
        }
    }

    private static string ShortenPath(string path)
    {
        string[] parts = path.Replace('/', '\\').Split('\\');
        if (parts.Length <= 4) return path;
        return @"...\" + string.Join(@"\", parts[^3..]);
    }

    private Button MakeQuickBtn(string text, Point location)
    {
        var btn = new Button
        {
            Text      = text,
            Location  = location,
            Size      = new Size(203, 42),
            BackColor = QuickBg,
            ForeColor = TextPrimary,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
            Cursor    = Cursors.Hand,
        };
        btn.FlatAppearance.BorderColor        = BorderColor;
        btn.FlatAppearance.BorderSize         = 1;
        btn.FlatAppearance.MouseOverBackColor = QuickHov;
        btn.FlatAppearance.MouseDownBackColor = QuickDown;
        return btn;
    }

    private static Label MakeCaption(string text, Point location) => new()
    {
        Text      = text,
        Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
        ForeColor = Color.FromArgb(100, 100, 120),
        Location  = location,
        AutoSize  = true,
    };
}
