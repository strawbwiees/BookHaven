using BookHaven.Services;
using BookHaven.UI;

namespace BookHaven.Forms
{
    public class LoginForm : Form
    {
        private RoundedTextBox _txtUsername = null!;
        private RoundedTextBox _txtPassword = null!;
        private readonly LibraryDataService _service = LibraryDataService.Instance;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "BookHaven - Welcome Back";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = UITheme.AppBackground;
            ClientSize = new Size(UITheme.S(820), UITheme.S(600));

            // 1. Top banner (shared with the main window)
            var banner = new BufferedPanel
            {
                Dock = DockStyle.Top,
                Height = UITheme.S(88),
                BackColor = UITheme.BannerBackground
            };
            banner.Paint += (s, e) =>
                UITheme.DrawBrandBanner(e.Graphics, new Rectangle(0, 0, banner.Width, banner.Height), false);
            Controls.Add(banner);

            // 2. Login card - every row is placed from the previous one so nothing can overlap
            int cardW = UITheme.S(400);
            int padX = UITheme.S(36);
            int innerW = cardW - padX * 2;

            var card = new CardPanel
            {
                Width = cardW,
                BorderRadius = 24,
                BackgroundFill = Color.White
            };

            int y = UITheme.S(32);

            // Avatar
            int avatarSize = UITheme.S(72);
            var avatarBmp = new Bitmap(avatarSize, avatarSize);
            using (var g = Graphics.FromImage(avatarBmp))
            {
                UITheme.DrawLibrarianAvatar(g, new Rectangle(0, 0, avatarSize, avatarSize));
            }
            var picAvatar = new PictureBox
            {
                Size = new Size(avatarSize, avatarSize),
                Location = new Point((cardW - avatarSize) / 2, y),
                Image = avatarBmp,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White
            };
            card.Controls.Add(picAvatar);
            y = picAvatar.Bottom + UITheme.S(14);

            // Heading
            var headingFont = new Font("League Spartan", 16f, FontStyle.Bold);
            var lblWelcome = new Label
            {
                Text = "WELCOME BACK!",
                Font = headingFont,
                ForeColor = UITheme.NavyDark,
                BackColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Bounds = new Rectangle(0, y, cardW, headingFont.Height + UITheme.S(4))
            };
            card.Controls.Add(lblWelcome);
            y = lblWelcome.Bottom + UITheme.S(2);

            var subFont = new Font("League Spartan", 9.5f);
            var lblSub = new Label
            {
                Text = "Log in to manage the library",
                Font = subFont,
                ForeColor = UITheme.TextMuted,
                BackColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Bounds = new Rectangle(0, y, cardW, subFont.Height + UITheme.S(4))
            };
            card.Controls.Add(lblSub);
            y = lblSub.Bottom + UITheme.S(26);

            // Username
            var lblUser = MakeFieldLabel("Username", padX, y, innerW);
            card.Controls.Add(lblUser);
            y = lblUser.Bottom + UITheme.S(4);
            _txtUsername = new RoundedTextBox
            {
                Text = "admin",
                Location = new Point(padX, y),
                Size = new Size(innerW, UITheme.S(44)),
                BorderRadius = 12
            };
            card.Controls.Add(_txtUsername);
            y = _txtUsername.Bottom + UITheme.S(16);

            // Password
            var lblPass = MakeFieldLabel("Password", padX, y, innerW);
            card.Controls.Add(lblPass);
            y = lblPass.Bottom + UITheme.S(4);
            _txtPassword = new RoundedTextBox
            {
                Text = "admin",
                UseSystemPasswordChar = true,
                Location = new Point(padX, y),
                Size = new Size(innerW, UITheme.S(44)),
                BorderRadius = 12
            };
            card.Controls.Add(_txtPassword);
            y = _txtPassword.Bottom + UITheme.S(26);

            // Log in button
            var btnLogin = new PillButton
            {
                Text = "Log in",
                Font = new Font("League Spartan", 11f, FontStyle.Bold),
                CustomBackColor = UITheme.HotPink,
                Location = new Point(padX, y),
                Size = new Size(innerW, UITheme.S(48))
            };
            btnLogin.Click += HandleLogin;
            card.Controls.Add(btnLogin);
            AcceptButton = btnLogin;

            card.Height = btnLogin.Bottom + UITheme.S(34);
            Controls.Add(card);

            // 3. Demo hint under the card
            var hintFont = UITheme.SmallFont;
            var lblHint = new Label
            {
                Text = "Demo account: admin / admin",
                Font = hintFont,
                ForeColor = UITheme.TextMuted,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(ClientSize.Width, hintFont.Height + UITheme.S(6))
            };
            Controls.Add(lblHint);

            // Centre card + hint in the space below the banner
            int hintGap = UITheme.S(16);
            int blockH = card.Height + hintGap + lblHint.Height;
            int areaH = ClientSize.Height - banner.Height;
            int top = banner.Height + Math.Max(UITheme.S(16), (areaH - blockH) / 2);
            card.Location = new Point((ClientSize.Width - card.Width) / 2, top);
            lblHint.Location = new Point(0, card.Bottom + hintGap);
        }

        private static Label MakeFieldLabel(string text, int x, int y, int width)
        {
            return new Label
            {
                Text = text,
                Font = UITheme.SmallFont,
                ForeColor = UITheme.TextMuted,
                BackColor = Color.White,
                Bounds = new Rectangle(x, y, width, UITheme.SmallFont.Height + UITheme.S(4))
            };
        }

        private void HandleLogin(object? sender, EventArgs e)
        {
            string user = _txtUsername.Text?.Trim() ?? string.Empty;
            string pass = _txtPassword.Text ?? string.Empty;

            if (user == _service.CurrentAdmin.Username && pass == _service.CurrentAdmin.Password)
            {
                Hide();
                var mainForm = new MainForm();
                mainForm.FormClosed += (s, args) => Close();
                mainForm.Show();
            }
            else
            {
                MessageBox.Show("Invalid username or password. Default is: admin / admin", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
