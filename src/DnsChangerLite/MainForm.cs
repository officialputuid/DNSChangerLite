using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace DnsChangerLite
{
    internal sealed class MainForm : Form
    {
        // ── Colors (Windows 11 light theme) ──────────────────────────────────
        private static readonly Color BgBase       = ColorTranslator.FromHtml("#F3F3F3");
        private static readonly Color BgCard       = Color.White;
        private static readonly Color TextPrimary  = ColorTranslator.FromHtml("#1A1A1A");
        private static readonly Color TextSecondary= ColorTranslator.FromHtml("#616161");
        private static readonly Color Accent       = ColorTranslator.FromHtml("#0067C0");
        private static readonly Color SuccessColor = ColorTranslator.FromHtml("#0F7B0F");
        private static readonly Color ErrorColor   = ColorTranslator.FromHtml("#C42B1C");
        private static readonly Color BorderColor  = ColorTranslator.FromHtml("#E0E0E0");

        // ── Layout constants ─────────────────────────────────────────────────
        private const int PagePadH   = 24;   // page left/right
        private const int PagePadTop = 16;
        private const int CardPad    = 20;   // card inner padding all sides
        private const int Gap8       = 8;
        private const int Gap12      = 12;
        private const int Gap16      = 16;
        private const int InputH     = 32;
        private const int BtnH       = 36;
        private const int CardW      = 420;
        private const int InnerW     = CardW - CardPad * 2; // 380

        // ── Font ─────────────────────────────────────────────────────────────
        private static readonly Font FntTitle    = new Font("Segoe UI", 15F,  FontStyle.Bold);
        private static readonly Font FntSubtitle = new Font("Segoe UI", 9F);
        private static readonly Font FntSection  = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        private static readonly Font FntBody     = new Font("Segoe UI", 9F);
        private static readonly Font FntSmall    = new Font("Segoe UI", 8F);
        private static readonly Font FntBtn      = new Font("Segoe UI", 9F, FontStyle.Bold);

        // ── Controls ─────────────────────────────────────────────────────────
        // Adapter card
        private RoundedPanel cardAdapter;
        private ComboBox cboAdapters;
        private Label lblCurrentDns;

        // Preset card
        private RoundedPanel cardPreset;
        private ComboBox cboPreset;
        private Label lblPrimary, lblSecondary;
        private TextBox txtPrimary, txtSecondary;

        // Action card
        private RoundedPanel cardActions;
        private Button btnApply, btnReset, btnFlush;

        // Status bar
        private RoundedPanel pnlStatus;
        private Label lblStatusIcon, lblStatus;

        private DnsService _dns;

        // ── DNS Presets ──────────────────────────────────────────────────────
        private static readonly string[][] Presets =
        {
            new[] { "Cloudflare",       "1.1.1.1",         "1.0.0.1"         },
            new[] { "Google",           "8.8.8.8",         "8.8.4.4"         },
            new[] { "Quad9",            "9.9.9.9",         "149.112.112.112" },
            new[] { "AdGuard (No Ads)", "94.140.14.14",    "94.140.15.15"    },
            new[] { "OpenDNS",          "208.67.222.222",  "208.67.220.220"  },
        };

        public MainForm()
        {
            _dns = new DnsService();
            InitUI();
            LoadAdapters();
        }

        // ═════════════════════════════════════════════════════════════════════
        // UI CONSTRUCTION — Windows 11 Settings style, no scroll
        // ═════════════════════════════════════════════════════════════════════

        private void InitUI()
        {
            Text = "DNS Changer Lite";
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = BgBase;
            Font = FntBody;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            int cardX = PagePadH;
            int y = PagePadTop;

            // ── Title ────────────────────────────────────────────────────────
            var lblTitle = Lbl("DNS Changer", FntTitle, TextPrimary, cardX, y, CardW, 28);
            Controls.Add(lblTitle);
            y += 28 + Gap8;

            var lblSubtitle = Lbl(
                "Change your network DNS in one click. Requires administrator privileges.",
                FntSubtitle, TextSecondary, cardX, y, CardW, 16);
            Controls.Add(lblSubtitle);
            y += 16 + Gap16;

            // ── Card 1 — Network Adapter ─────────────────────────────────────
            cardAdapter = new RoundedPanel(CardW, 0, cardX, y);
            {
                int cy = CardPad;

                cardAdapter.Controls.Add(Lbl("Network Adapter", FntSection, TextPrimary, CardPad, cy, InnerW, 18));
                cy += 18 + Gap8;

                cardAdapter.Controls.Add(Lbl("Select which adapter to configure.", FntSmall, TextSecondary, CardPad, cy, InnerW, 14));
                cy += 14 + Gap12;

                cboAdapters = new ComboBox
                {
                    Location = new Point(CardPad, cy),
                    Size = new Size(InnerW, InputH),
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = FntBody,
                    FlatStyle = FlatStyle.Flat,
                };
                cboAdapters.SelectedIndexChanged += CboAdapters_SelectedIndexChanged;
                cardAdapter.Controls.Add(cboAdapters);
                cy += InputH + Gap8;

                lblCurrentDns = Lbl("Current DNS: —", FntSmall, TextSecondary, CardPad, cy, InnerW, 14);
                cardAdapter.Controls.Add(lblCurrentDns);
                cy += 14 + CardPad;

                cardAdapter.Height = cy;
            }
            Controls.Add(cardAdapter);
            y += cardAdapter.Height + Gap12;

            // ── Card 2 — DNS Preset ──────────────────────────────────────────
            cardPreset = new RoundedPanel(CardW, 0, cardX, y);
            {
                int cy = CardPad;

                cardPreset.Controls.Add(Lbl("DNS Preset", FntSection, TextPrimary, CardPad, cy, InnerW, 18));
                cy += 18 + Gap8;

                cardPreset.Controls.Add(Lbl("Choose a preset or enter custom addresses.", FntSmall, TextSecondary, CardPad, cy, InnerW, 14));
                cy += 14 + Gap12;

                cboPreset = new ComboBox
                {
                    Location = new Point(CardPad, cy),
                    Size = new Size(InnerW, InputH),
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = FntBody,
                    FlatStyle = FlatStyle.Flat,
                };
                foreach (var p in Presets) cboPreset.Items.Add(p[0]);
                cboPreset.Items.Add("Custom");
                cboPreset.SelectedIndex = 0;
                cboPreset.SelectedIndexChanged += CboPreset_SelectedIndexChanged;
                cardPreset.Controls.Add(cboPreset);
                cy += InputH + Gap12;

                // Row: Primary DNS
                int labelW = 84;
                int inputX = CardPad + labelW + Gap8;
                int inputW = InnerW - labelW - Gap8;

                lblPrimary = Lbl("Primary DNS", FntSmall, TextSecondary, CardPad, cy + 8, labelW, 14);
                txtPrimary = new TextBox
                {
                    Location = new Point(inputX, cy),
                    Size = new Size(inputW, InputH),
                    Font = FntBody,
                    BorderStyle = BorderStyle.FixedSingle,
                    Enabled = false,
                };
                cardPreset.Controls.Add(lblPrimary);
                cardPreset.Controls.Add(txtPrimary);
                cy += InputH + Gap8;

                // Row: Secondary DNS
                lblSecondary = Lbl("Secondary DNS", FntSmall, TextSecondary, CardPad, cy + 8, labelW, 14);
                txtSecondary = new TextBox
                {
                    Location = new Point(inputX, cy),
                    Size = new Size(inputW, InputH),
                    Font = FntBody,
                    BorderStyle = BorderStyle.FixedSingle,
                    Enabled = false,
                };
                cardPreset.Controls.Add(lblSecondary);
                cardPreset.Controls.Add(txtSecondary);
                cy += InputH + CardPad;

                cardPreset.Height = cy;
            }
            Controls.Add(cardPreset);
            y += cardPreset.Height + Gap12;

            // ── Card 3 — Actions ─────────────────────────────────────────────
            int btnGap = Gap8;
            int btnW = (InnerW - btnGap * 2) / 3;
            cardActions = new RoundedPanel(CardW, 0, cardX, y);
            {
                int cy = CardPad;

                btnApply = Btn("Apply DNS", Accent, Color.White, CardPad, cy, btnW, BtnH);
                btnApply.Click += BtnApply_Click;

                btnReset = Btn("Auto DHCP", BgCard, TextPrimary, CardPad + btnW + btnGap, cy, btnW, BtnH);
                btnReset.Click += BtnReset_Click;

                btnFlush = Btn("Flush DNS", BgCard, TextPrimary, CardPad + (btnW + btnGap) * 2, cy, btnW, BtnH);
                btnFlush.Click += BtnFlush_Click;

                cardActions.Controls.AddRange(new Control[] { btnApply, btnReset, btnFlush });
                cy += BtnH + CardPad;
                cardActions.Height = cy;
            }
            Controls.Add(cardActions);
            y += cardActions.Height + Gap12;

            // ── Status bar ───────────────────────────────────────────────────
            pnlStatus = new RoundedPanel(CardW, 0, cardX, y);
            {
                int cy = 12;
                lblStatusIcon = Lbl("●", FntBody, TextSecondary, CardPad, cy, 16, 16);
                lblStatus = Lbl("Ready.", FntBody, TextSecondary, CardPad + 16 + Gap8, cy, InnerW - 16 - Gap8, 16);
                pnlStatus.Controls.AddRange(new Control[] { lblStatusIcon, lblStatus });
                cy += 16 + 12;
                pnlStatus.Height = cy;
            }
            Controls.Add(pnlStatus);
            y += pnlStatus.Height;

            // ── Form size: fit exactly, no scroll ────────────────────────────
            int formClientH = y + 8; // small bottom breathing room
            int formW = CardW + PagePadH * 2 + 16; // card + padding + border allowance
            this.ClientSize = new Size(formW, formClientH);
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;
        }

        // ═════════════════════════════════════════════════════════════════════
        // HELPERS
        // ═════════════════════════════════════════════════════════════════════

        private static Label Lbl(string text, Font font, Color color, int x, int y, int w, int h)
        {
            return new Label
            {
                Text = text,
                Font = font,
                ForeColor = color,
                Location = new Point(x, y),
                Size = new Size(w, h),
                AutoSize = false,
                BackColor = Color.Transparent,
            };
        }

        private static Button Btn(string text, Color bg, Color fg, int x, int y, int w, int h)
        {
            var btn = new Button
            {
                Text = text,
                Font = FntBtn,
                FlatStyle = FlatStyle.Flat,
                BackColor = bg,
                ForeColor = fg,
                Location = new Point(x, y),
                Size = new Size(w, h),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0),
                Margin = new Padding(0),
            };
            btn.FlatAppearance.BorderSize = bg == BgBase || bg == BgCard ? 1 : 0;
            btn.FlatAppearance.BorderColor = BorderColor;
            return btn;
        }

        // ═════════════════════════════════════════════════════════════════════
        // EVENTS
        // ═════════════════════════════════════════════════════════════════════

        private void LoadAdapters()
        {
            cboAdapters.Items.Clear();
            var adapters = _dns.GetAdapters();
            foreach (var a in adapters) cboAdapters.Items.Add(a);
            if (cboAdapters.Items.Count > 0) cboAdapters.SelectedIndex = 0;
            else SetStatus("No connected adapters found.", false);
        }

        private void CboAdapters_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboAdapters.SelectedItem is NetworkAdapter adapter)
            {
                // Use Description (WMI) for DNS lookup, not NetConnectionID (netsh)
                var current = _dns.GetCurrentDns(adapter.Description);
                lblCurrentDns.Text = "Current DNS: " + current;
            }
        }

        private void CboPreset_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = cboPreset.SelectedIndex;
            bool isCustom = idx >= Presets.Length;

            txtPrimary.Enabled = isCustom;
            txtSecondary.Enabled = isCustom;

            if (!isCustom)
            {
                txtPrimary.Text = Presets[idx][1];
                txtSecondary.Text = Presets[idx][2];
            }
            else
            {
                txtPrimary.Text = "";
                txtSecondary.Text = "";
                txtPrimary.Focus();
            }
        }

        private async void BtnApply_Click(object sender, EventArgs e)
        {
            if (!(cboAdapters.SelectedItem is NetworkAdapter adapter))
            {
                SetStatus("Please select a network adapter.", false);
                return;
            }

            var primary = txtPrimary.Text.Trim();
            var secondary = txtSecondary.Text.Trim();

            if (string.IsNullOrEmpty(primary))
            {
                SetStatus("Primary DNS address is required.", false);
                return;
            }

            if (!System.Net.IPAddress.TryParse(primary, out _))
            {
                SetStatus("Invalid primary DNS address.", false);
                return;
            }

            if (!string.IsNullOrEmpty(secondary) && !System.Net.IPAddress.TryParse(secondary, out _))
            {
                SetStatus("Invalid secondary DNS address.", false);
                return;
            }

            SetBusy(true);
            var result = await Task.Run(() => _dns.SetDns(adapter.Name, primary, secondary));
            SetStatus(result.Message, result.Success);
            SetBusy(false);
            RefreshCurrentDns();
        }

        private async void BtnReset_Click(object sender, EventArgs e)
        {
            if (!(cboAdapters.SelectedItem is NetworkAdapter adapter))
            {
                SetStatus("Please select a network adapter.", false);
                return;
            }

            SetBusy(true);
            var result = await Task.Run(() => _dns.ResetDns(adapter.Name));
            SetStatus(result.Message, result.Success);
            SetBusy(false);
            RefreshCurrentDns();
        }

        private async void BtnFlush_Click(object sender, EventArgs e)
        {
            SetBusy(true);
            var result = await Task.Run(() => _dns.FlushDns());
            SetStatus(result.Message, result.Success);
            SetBusy(false);
        }

        // ═════════════════════════════════════════════════════════════════════
        // STATUS
        // ═════════════════════════════════════════════════════════════════════

        private void SetStatus(string message, bool success)
        {
            lblStatus.Text = message;
            lblStatusIcon.ForeColor = success ? SuccessColor : ErrorColor;
            lblStatus.ForeColor = success ? SuccessColor : ErrorColor;
            pnlStatus.BackColor = success
                ? ColorTranslator.FromHtml("#F6FFF6")
                : ColorTranslator.FromHtml("#FFF6F6");
        }

        private void SetBusy(bool busy)
        {
            btnApply.Enabled = !busy;
            btnReset.Enabled = !busy;
            btnFlush.Enabled = !busy;
            cboAdapters.Enabled = !busy;
            cboPreset.Enabled = !busy;
            txtPrimary.Enabled = !busy && cboPreset.SelectedIndex >= Presets.Length;
            txtSecondary.Enabled = !busy && cboPreset.SelectedIndex >= Presets.Length;

            if (busy)
            {
                lblStatus.Text = "Working...";
                lblStatusIcon.ForeColor = Accent;
                lblStatus.ForeColor = Accent;
                pnlStatus.BackColor = ColorTranslator.FromHtml("#F0F6FF");
            }
        }

        private void RefreshCurrentDns()
        {
            if (cboAdapters.SelectedItem is NetworkAdapter adapter)
            {
                var current = _dns.GetCurrentDns(adapter.Description);
                lblCurrentDns.Text = "Current DNS: " + current;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                FntTitle?.Dispose();
                FntSubtitle?.Dispose();
                FntSection?.Dispose();
                FntBody?.Dispose();
                FntSmall?.Dispose();
                FntBtn?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Rounded card panel — Region-based clipping for 8px corners
    // ═══════════════════════════════════════════════════════════════════════════

    internal class RoundedPanel : Panel
    {
        private const int Radius = 8;

        public RoundedPanel(int width, int height, int x, int y)
        {
            Size = new Size(width, height);
            Location = new Point(x, y);
            BackColor = Color.White;
            DoubleBuffered = true;
            ApplyRoundedRegion();
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            ApplyRoundedRegion();
        }

        private void ApplyRoundedRegion()
        {
            using (var path = RoundedRect(new Rectangle(0, 0, Width, Height), Radius))
            {
                Region = new Region(path);
            }
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (var brush = new SolidBrush(BackColor))
            using (var path = RoundedRect(new Rectangle(0, 0, Width, Height), Radius))
            {
                e.Graphics.FillPath(brush, path);
            }

            using (var pen = new Pen(ColorTranslator.FromHtml("#E0E0E0"), 1f))
            using (var path = RoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), Radius))
            {
                e.Graphics.DrawPath(pen, path);
            }
        }
    }
}
