using Microsoft.Maui.Layouts;

namespace stars_beyond;

public partial class GameplayPage : ContentPage
{
    const int PlayerSize = 16;
    const int BulletSize = 10;
    const int MaxHp = 20;
    const int TargetDodges = 20;

    const double PlayerSpeed = 6.0;
    const double BulletSpeed = 4.0;

    int hp = MaxHp;
    int dodged = 0;

    bool gameOver = false;

    double moveX = 0;
    double moveY = 0;

    Random random = new();

    IDispatcherTimer gameLoop;
    IDispatcherTimer spawnTimer;

    enum SpawnSide { Top, Bottom, Left, Right }

    public GameplayPage()
    {
        InitializeComponent();

        AbsoluteLayout.SetLayoutBounds(Player,
            new Rect(0, 0, PlayerSize, PlayerSize));
        AbsoluteLayout.SetLayoutFlags(Player,
            AbsoluteLayoutFlags.None);

        gameLoop = Dispatcher.CreateTimer();
        gameLoop.Interval = TimeSpan.FromMilliseconds(16);
        gameLoop.Tick += GameTick;
        gameLoop.Start();

        spawnTimer = Dispatcher.CreateTimer();
        spawnTimer.Interval = TimeSpan.FromMilliseconds(180);
        spawnTimer.Tick += SpawnBullet;
        spawnTimer.Start();
    }

    void LeftPressed(object s, EventArgs e) { if (!gameOver) { moveX = -PlayerSpeed; moveY = 0; } }
    void RightPressed(object s, EventArgs e) { if (!gameOver) { moveX = PlayerSpeed; moveY = 0; } }
    void UpPressed(object s, EventArgs e) { if (!gameOver) { moveY = -PlayerSpeed; moveX = 0; } }
    void DownPressed(object s, EventArgs e) { if (!gameOver) { moveY = PlayerSpeed; moveX = 0; } }
    void StopMove(object s, EventArgs e) { moveX = moveY = 0; }

    void GameTick(object sender, EventArgs e)
    {
        if (gameOver) return;
        if (BattleField.Width <= 0 || BattleField.Height <= 0) return;

        MovePlayer();
        CheckCollisions();
        CheckEndConditions();
    }

    void MovePlayer()
    {
        double newX = Player.TranslationX + moveX;
        double newY = Player.TranslationY + moveY;

        double maxX = BattleField.Width - PlayerSize;
        double maxY = BattleField.Height - PlayerSize;

        if (maxX < 0 || maxY < 0) return;

        Player.TranslationX = Math.Clamp(newX, 0, maxX);
        Player.TranslationY = Math.Clamp(newY, 0, maxY);
    }

    void CheckEndConditions()
    {
        if (dodged >= TargetDodges)
            EndGame(true);

        if (hp <= 0)
            EndGame(false);
    }

    void EndGame(bool win)
    {
        gameOver = true;

        spawnTimer.Stop();
        gameLoop.Stop();

        moveX = moveY = 0;

        StatusLabel.Text = win ? "Wygra³eœ" : "Przegra³eœ";
    }

    void SpawnBullet(object sender, EventArgs e)
    {
        if (gameOver) return;
        if (BattleField.Width <= 0 || BattleField.Height <= 0) return;

        var bullet = new BoxView
        {
            WidthRequest = BulletSize,
            HeightRequest = BulletSize,
            Color = Colors.White
        };

        SpawnSide side = (SpawnSide)random.Next(4);

        double x = 0, y = 0;

        switch (side)
        {
            case SpawnSide.Top:
                x = random.NextDouble() * BattleField.Width;
                y = -BulletSize;
                break;

            case SpawnSide.Bottom:
                x = random.NextDouble() * BattleField.Width;
                y = BattleField.Height + BulletSize;
                break;

            case SpawnSide.Left:
                x = -BulletSize;
                y = random.NextDouble() * BattleField.Height;
                break;

            case SpawnSide.Right:
                x = BattleField.Width + BulletSize;
                y = random.NextDouble() * BattleField.Height;
                break;
        }

        AbsoluteLayout.SetLayoutBounds(bullet, new Rect(x, y, BulletSize, BulletSize));
        BattleField.Children.Add(bullet);

        double targetX = Player.TranslationX + PlayerSize / 2;
        double targetY = Player.TranslationY + PlayerSize / 2;

        _ = MoveBullet(bullet, targetX, targetY);
    }

    async Task MoveBullet(BoxView bullet, double tx, double ty)
    {
        double bx = bullet.X + BulletSize / 2;
        double by = bullet.Y + BulletSize / 2;

        double dx = tx - bx;
        double dy = ty - by;

        double len = Math.Sqrt(dx * dx + dy * dy);
        dx /= len;
        dy /= len;

        while (!gameOver && BattleField.Children.Contains(bullet))
        {
            bullet.TranslationX += dx * BulletSpeed;
            bullet.TranslationY += dy * BulletSpeed;

            if (bullet.TranslationX < -60 ||
                bullet.TranslationX > BattleField.Width + 60 ||
                bullet.TranslationY < -60 ||
                bullet.TranslationY > BattleField.Height + 60)
            {
                BattleField.Children.Remove(bullet);
                dodged++;
                StatusLabel.Text = $"Unikniête pociski: {dodged}/{TargetDodges}";
                break;
            }

            await Task.Delay(16);
        }
    }

    void CheckCollisions()
    {
        foreach (var b in BattleField.Children.OfType<BoxView>()
                    .Where(x => x != Player).ToList())
        {
            if (IsColliding(Player, b))
            {
                BattleField.Children.Remove(b);
                hp--;
                HpLabel.Text = $"HP: {hp}/{MaxHp}";
            }
        }
    }

    bool IsColliding(VisualElement a, VisualElement b)
    {
        var r1 = new Rect(a.TranslationX, a.TranslationY, a.Width, a.Height);
        var r2 = new Rect(b.X + b.TranslationX, b.Y + b.TranslationY, b.Width, b.Height);
        return r1.IntersectsWith(r2);
    }
}
