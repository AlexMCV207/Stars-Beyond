namespace stars_beyond;

public partial class GameplayPageMainUI : ContentPage
{
    public GameplayPageMainUI()
    {
        InitializeComponent();
        UpdateItemMenu();
    }

    enum TurnState
    {
        PlayerTurn,
        AttackMinigame,
        EnemyTurn,
        ItemMenu
    }

    TurnState currentState = TurnState.PlayerTurn;

    int enemyMaxHp = 30;
    int enemyHp = 30;


    Dictionary<string, (int heal, int count)> items = new()
    {
        { "Jablko", (1, 2) },
        { "Gruszka", (2, 1) },
        { "Banan", (3, 1) }
    };

    void UpdateItemMenu()
    {
        string text = "* ITEMY:\n";

        foreach (var i in items)
        {
            if (i.Value.count > 0)
                text += $"{i.Key} (+{i.Value.heal}HP) x{i.Value.count}\n";
        }

        if (text == "* ITEMY:\n")
            text += "Brak itemów";

        text += "\n\nKliknij dialog aby użyć";

        DialogLabel.Text = text;
    }

    void ItemClicked(object sender, EventArgs e)
    {
        if (currentState != TurnState.PlayerTurn)
            return;

        currentState = TurnState.ItemMenu;
        UpdateItemMenu();
    }

    void OnDialogTapped(object sender, EventArgs e)
    {
        if (currentState != TurnState.ItemMenu)
            return;

        UseItem();
    }

    void UseItem()
    {
        if (playerHp >= playerMaxHp)
        {
            DialogLabel.Text = "* Masz pełne HP!";
            currentState = TurnState.PlayerTurn;
            return;
        }

        var item = items.FirstOrDefault(x => x.Value.count > 0);

        if (item.Key == null)
        {
            DialogLabel.Text = "* Brak itemów!";
            currentState = TurnState.PlayerTurn;
            return;
        }

        int heal = item.Value.heal;

        int newHp = Math.Min(playerHp + heal, playerMaxHp);
        int realHeal = newHp - playerHp;

        playerHp = newHp;

        items[item.Key] = (item.Value.heal, item.Value.count - 1);

        UpdatePlayerHp();

        DialogLabel.Text = $"* Użyto {item.Key}! +{realHeal} HP";

        currentState = TurnState.PlayerTurn;
    }

    
    void FightClicked(object sender, EventArgs e)
    {
        if (currentState != TurnState.PlayerTurn)
            return;

        StartAttackMinigame();
    }

    bool isSliderMoving;
    double sliderX;
    double sliderSpeed = 10;

    void StartAttackMinigame()
    {
        currentState = TurnState.AttackMinigame;

        DialogLabel.IsVisible = false;
        AttackMinigame.IsVisible = true;

        sliderX = 0;
        AttackSlider.TranslationX = 0;

        isSliderMoving = true;

        _ = AnimateSlider();
    }

    async Task AnimateSlider()
    {
        await Task.Delay(500);

        double maxX = AttackMinigame.Width - AttackSlider.Width;

        while (isSliderMoving)
        {
            sliderX += sliderSpeed;

            if (sliderX >= maxX)
            {
                isSliderMoving = false;
                DealDamage(0);
                EndAttackMinigame();
                return;
            }

            AttackSlider.TranslationX = sliderX;
            await Task.Delay(16);
        }
    }

    void OnAttackTap(object sender, EventArgs e)
    {
        if (currentState != TurnState.AttackMinigame)
            return;

        isSliderMoving = false;

        double center = AttackBar.Width / 2;
        double hit = AttackSlider.TranslationX + (AttackSlider.Width / 2);

        double distance = Math.Abs(hit - center);
        double accuracy = 1 - (distance / center);
        accuracy = Math.Clamp(accuracy, 0, 1);

        int damage = (int)(accuracy * 70);

        DealDamage(damage);
        EndAttackMinigame();
    }

    void EndAttackMinigame()
    {
        AttackMinigame.IsVisible = false;

        currentState = TurnState.EnemyTurn;

        StartEnemyTurn();
    }

    async void StartEnemyTurn()
    {
        currentState = TurnState.EnemyTurn;

        DialogLabel.IsVisible = true;
        DialogLabel.Text = "* Wróg atakuje...";

        await Task.Delay(800);

        DialogLabel.IsVisible = false;

        BattleFrame.IsVisible = true;
        MovementControls.IsVisible = true;
        ActionButtons.IsVisible = false;

        await RunEnemyAttack();

        BattleFrame.IsVisible = false;
        MovementControls.IsVisible = false;

        EndEnemyTurn();
    }

    Random random = new();

    async Task RunEnemyAttack()
    {
        StartBulletHell();
        await Task.Delay(6000);
        StopBulletHell();
    }

    void EndEnemyTurn()
    {
        currentState = TurnState.PlayerTurn;
        ActionButtons.IsVisible = true;
        DialogLabel.IsVisible = true;
        DialogLabel.Text = "* Co zrobisz?";
    }

    void DealDamage(int damage)
    {
        enemyHp -= damage;
        enemyHp = Math.Max(0, enemyHp);

        DialogLabel.Text = $"* Zadałeś {damage} obrażeń!";
        _ = ShowDamageText(damage);
    }

    const int PlayerSize = 16;
    const int BulletSize = 10;

    const double PlayerSpeed = 6.0;
    const double BulletSpeed = 4.0;

    double moveX = 0;
    double moveY = 0;

    int playerHp = 20;
    int playerMaxHp = 20;

    bool gameRunning = false;

    IDispatcherTimer gameLoop;
    IDispatcherTimer spawnTimer;

    void LeftPressed(object s, EventArgs e) { moveX = -PlayerSpeed; moveY = 0; }
    void RightPressed(object s, EventArgs e) { moveX = PlayerSpeed; moveY = 0; }
    void UpPressed(object s, EventArgs e) { moveY = -PlayerSpeed; moveX = 0; }
    void DownPressed(object s, EventArgs e) { moveY = PlayerSpeed; moveX = 0; }
    void StopMove(object s, EventArgs e) { moveX = moveY = 0; }

    void StartBulletHell()
    {
        gameRunning = true;

        AbsoluteLayout.SetLayoutBounds(Player,
            new Rect(0, 0, PlayerSize, PlayerSize));

        gameLoop = Dispatcher.CreateTimer();
        gameLoop.Interval = TimeSpan.FromMilliseconds(16);
        gameLoop.Tick += GameTick;
        gameLoop.Start();

        spawnTimer = Dispatcher.CreateTimer();
        spawnTimer.Interval = TimeSpan.FromMilliseconds(180);
        spawnTimer.Tick += SpawnBullet;
        spawnTimer.Start();
    }

    void StopBulletHell()
    {
        gameRunning = false;

        spawnTimer?.Stop();
        gameLoop?.Stop();

        BattleField.Children.Clear();
        BattleField.Children.Add(Player);

        moveX = moveY = 0;
    }

    void GameTick(object sender, EventArgs e)
    {
        if (!gameRunning) return;
        if (BattleField.Width <= 0 || BattleField.Height <= 0) return;

        MovePlayer();
        CheckCollisions();
    }

    void MovePlayer()
    {
        double newX = Player.TranslationX + moveX;
        double newY = Player.TranslationY + moveY;

        double maxX = BattleField.Width - PlayerSize;
        double maxY = BattleField.Height - PlayerSize;

        Player.TranslationX = Math.Clamp(newX, 0, maxX);
        Player.TranslationY = Math.Clamp(newY, 0, maxY);
    }

    void SpawnBullet(object sender, EventArgs e)
    {
        var bullet = new BoxView
        {
            WidthRequest = BulletSize,
            HeightRequest = BulletSize,
            Color = Colors.White
        };

        double x = random.NextDouble() * BattleField.Width;
        double y = -BulletSize;

        AbsoluteLayout.SetLayoutBounds(bullet, new Rect(x, y, BulletSize, BulletSize));
        BattleField.Children.Add(bullet);

        _ = MoveBullet(bullet);
    }

    async Task MoveBullet(BoxView bullet)
    {
        while (gameRunning && BattleField.Children.Contains(bullet))
        {
            bullet.TranslationY += BulletSpeed;

            if (bullet.TranslationY > BattleField.Height + 50)
            {
                BattleField.Children.Remove(bullet);
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

                playerHp--;
                UpdatePlayerHp();
            }
        }
    }

    void UpdatePlayerHp()
    {
        HpLabel.Text = $"{playerHp}/{playerMaxHp}";

        double percent = (double)playerHp / playerMaxHp;
        HpBar.WidthRequest = 45 * percent;

        if (playerHp <= 0)
        {
            StopBulletHell();
            DialogLabel.Text = "* Zginąłeś";
        }
    }

    bool IsColliding(VisualElement a, VisualElement b)
    {
        var r1 = new Rect(a.TranslationX, a.TranslationY, a.Width, a.Height);
        var r2 = new Rect(b.X + b.TranslationX, b.Y + b.TranslationY, b.Width, b.Height);

        return r1.IntersectsWith(r2);
    }

    async Task ShowDamageText(int damage)
    {
        DamageLabel.Text = damage.ToString();
        DamageLabel.Opacity = 0;
        DamageLabel.IsVisible = true;

        await DamageLabel.FadeTo(1, 150);
        await DamageLabel.FadeTo(0, 400);

        DamageLabel.IsVisible = false;
    }
}
