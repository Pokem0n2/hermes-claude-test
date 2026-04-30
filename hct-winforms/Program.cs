using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace HCT;

public class ButtonConfig
{
    public string Name { get; set; } = "";
    public string Link { get; set; } = "";
}

public class AppConfig
{
    public List<ButtonConfig?> Buttons { get; set; } = Enumerable.Repeat<ButtonConfig?>(null, 25).ToList();
}

public class MainForm : Form
{
    private readonly Button[] _buttons = new Button[25];
    private readonly string _configPath;
    private AppConfig _config;

    public MainForm()
    {
        string exePath = AppContext.BaseDirectory;
        string exeName = Path.GetFileName(Process.GetCurrentProcess().MainModule?.FileName ?? "hct.exe");
        string exeDir = Path.GetDirectoryName(Path.Combine(exePath, exeName)) ?? exePath;
        _configPath = Path.Combine(exeDir, "hct-config.json");

        Text = "HCT - Hotkey Configuration Tool";
        Width = 460;
        Height = 520;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        BackColor = Color.FromArgb(30, 30, 30);

        _config = LoadConfig();
        CreateButtonGrid();
    }

    private AppConfig LoadConfig()
    {
        if (File.Exists(_configPath))
        {
            try
            {
                string json = File.ReadAllText(_configPath);
                return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
            catch { }
        }
        return new AppConfig();
    }

    private void SaveConfig()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(_config, options);
            File.WriteAllText(_configPath, json);
        }
        catch { }
    }

    private void CreateButtonGrid()
    {
        int gridSize = 5;
        int btnSize = 70;
        int padding = 8;
        int startX = (Width - (gridSize * btnSize + (gridSize - 1) * padding)) / 2;
        int startY = 30;

        for (int i = 0; i < 25; i++)
        {
            int row = i / gridSize;
            int col = i % gridSize;
            int x = startX + col * (btnSize + padding);
            int y = startY + row * (btnSize + padding);

            _buttons[i] = new Button
            {
                Size = new Size(btnSize, btnSize),
                Location = new Point(x, y),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Text = "+",
                Tag = i,
                Cursor = Cursors.Hand
            };
            _buttons[i].FlatAppearance.BorderSize = 0;
            _buttons[i].Click += Button_Click;
            Controls.Add(_buttons[i]);
        }
        UpdateAllButtons();
    }

    private void UpdateAllButtons()
    {
        for (int i = 0; i < 25; i++)
            UpdateButton(i);
    }

    private void UpdateButton(int index)
    {
        var cfg = _config.Buttons[index];
        if (cfg != null && !string.IsNullOrEmpty(cfg.Name))
        {
            _buttons[index].Text = cfg.Name.Length > 8 ? cfg.Name[..8] + "…" : cfg.Name;
            _buttons[index].BackColor = Color.FromArgb(0, 120, 215);
        }
        else
        {
            _buttons[index].Text = "+";
            _buttons[index].BackColor = Color.FromArgb(60, 60, 60);
        }
    }

    private void Button_Click(object? sender, EventArgs e)
    {
        if (sender is not Button btn) return;
        int index = (int)btn.Tag!;
        var cfg = _config.Buttons[index];

        if (cfg != null && !string.IsNullOrEmpty(cfg.Name) && !string.IsNullOrEmpty(cfg.Link))
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = cfg.Link,
                    UseShellExecute = true
                });
            }
            catch { }
        }
        else
        {
            ShowEditDialog(index);
        }
    }

    private void ShowEditDialog(int index)
    {
        using var form = new Form
        {
            Text = $"设置按钮 {index + 1}",
            Width = 350,
            Height = 220,
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.FromArgb(40, 40, 40)
        };

        var nameLabel = new Label { Text = "名称:", Location = new Point(20, 20), Size = new Size(60, 20), ForeColor = Color.White };
        var nameBox = new TextBox { Location = new Point(20, 45), Width = 300, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

        var linkLabel = new Label { Text = "链接:", Location = new Point(20, 80), Size = new Size(60, 20), ForeColor = Color.White };
        var linkBox = new TextBox { Location = new Point(20, 105), Width = 300, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

        var saveBtn = new Button { Text = "保存", Location = new Point(130, 145), Width = 90, Height = 30, DialogResult = DialogResult.OK, BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        var cancelBtn = new Button { Text = "取消", Location = new Point(230, 145), Width = 90, Height = 30, DialogResult = DialogResult.Cancel, BackColor = Color.FromArgb(80, 80, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

        var current = _config.Buttons[index];
        if (current != null)
        {
            nameBox.Text = current.Name;
            linkBox.Text = current.Link;
        }

        form.Controls.AddRange(new Control[] { nameLabel, nameBox, linkLabel, linkBox, saveBtn, cancelBtn });
        form.AcceptButton = saveBtn;
        form.CancelButton = cancelBtn;

        if (form.ShowDialog(this) == DialogResult.OK)
        {
            _config.Buttons[index] = new ButtonConfig { Name = nameBox.Text.Trim(), Link = linkBox.Text.Trim() };
            UpdateButton(index);
            SaveConfig();
        }
    }
}

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
