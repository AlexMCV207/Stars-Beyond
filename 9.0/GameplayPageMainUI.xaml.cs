namespace stars_beyond;

public partial class GameplayPageMainUI : ContentPage
{
	public GameplayPageMainUI()
	{
        InitializeComponent();
	}
    enum TurnState
    {
        PlayerTurn,
        AttackMinigame,
        EnemyTurn
    }

    TurnState currentState = TurnState.PlayerTurn;

    int enemyMaxHp = 30;
    int enemyHp = 30;

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
        await Task.Delay(500); // slider delay

        double maxX = AttackMinigame.Width - AttackSlider.Width;

        while (isSliderMoving)
        {
            sliderX += sliderSpeed;

            if (sliderX >= maxX)
            {
                isSliderMoving = false;

                Console.WriteLine("MISS - 0 damage");

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

        Console.WriteLine($"HIT accuracy: {accuracy:0.00} damage: {damage}");

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
        int attackId = random.Next(1);

        switch (attackId)
        {
            case 0:
                StartBulletHell();
                await Task.Delay(6000); // czas trwania ataku
                StopBulletHell();
                break;
        }
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
        DialogLabel.Text = $"* Zada³eœ {damage} obra¿eñ!";
        _ = ShowDamageText(damage);

        if (enemyHp <= 0)
        {
            
        }
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
            DialogLabel.Text = "* Zgin¹³eœ";
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
        DamageLabel.TranslationX = 0;
        DamageLabel.TranslationY = 0;
        DamageLabel.IsVisible = true;

        Random rand = new();

        double side = rand.Next(2) == 0 ? -1 : 1;
        double angle = rand.NextDouble() * Math.PI * 2;
        double distance = rand.Next(40, 80);

        double startX = side * rand.Next(80, 140);
        double startY = rand.Next(-100, -60);
        double moveX = startX + Math.Cos(angle) * distance;
        double moveY = startY + -Math.Abs(Math.Sin(angle) * distance);

        var fadeIn = DamageLabel.FadeTo(1, 150);
        var move = DamageLabel.TranslateTo(moveX, moveY, 700, Easing.SinOut);

        await Task.WhenAll(fadeIn, move);

        await DamageLabel.FadeTo(0, 400);


        DamageLabel.IsVisible = false;
}
}