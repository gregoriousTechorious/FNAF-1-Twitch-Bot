using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

// --- State ---
var monitorUp = false;
var votes = new Dictionary<string, int>();
var voteLock = new object();
var voteWindow = TimeSpan.FromSeconds(3);

// --- Command sets ---
var camCommands   = new HashSet<string> { "cam1a", "cam1b", "cam1c", "cam2a", "cam2b", "cam3", "cam4a", "cam4b", "cam5", "cam6", "cam7" };
var panelCommands = new HashSet<string> { "leftdoor", "rightdoor", "leftlight", "rightlight" };

// --- Coords ---
var coords = new Dictionary<string, (int x, int y)>
{
    { "monitor",    (580,  674) },
    { "cam1a",      (984,  357) },
    { "cam1b",      (950,  415) },
    { "cam1c",      (919,  484) },
    { "cam2a",      (982,  601) },
    { "cam2b",      (983,  637) },
    { "cam3",       (893,  580) },
    { "cam4a",      (1089, 607) },
    { "cam4b",      (1084, 652) },
    { "cam5",       (857,  442) },
    { "cam6",       (1180, 570) },
    { "cam7",       (1176, 440) },
    { "leftdoor",   (58,   325) },
    { "rightdoor",  (1203, 355) },
    { "leftlight",  (52,   453) },
    { "rightlight", (1219, 475) },
};

// --- Vote loop ---
_ = Task.Run(async () =>
{
    while (true)
    {
        await Task.Delay(voteWindow);

        string? winner = null;
        lock (voteLock)
        {
            if (votes.Count > 0)
            {
                winner = votes.MaxBy(v => v.Value).Key;
                votes.Clear();
            }
        }

        if (winner != null)
        {
            Console.WriteLine($"[Vote] Winner: !{winner} — executing");
            await ExecuteCommand(winner);
        }
    }
});

// --- Twitch bot ---
var credentials    = new ConnectionCredentials("galactic_amy", "fwo0m79e6bgfh6b0vfn1x6tnz6yn4g");
var clientOptions  = new ClientOptions();
var webSocketClient = new WebSocketClient(clientOptions);
var client         = new TwitchClient(webSocketClient);

client.Initialize(credentials, "galactic_amy");
client.OnConnected      += async (s, e) => { Console.WriteLine("[Twitch] Connected!"); await client.JoinChannelAsync("galactic_amy"); };
client.OnJoinedChannel  += async (s, e) => { Console.WriteLine($"[Twitch] Joined: {e.Channel}"); await Task.CompletedTask; };
client.OnMessageReceived += OnMessageReceived;

await client.ConnectAsync();
await Task.Delay(Timeout.Infinite);

// --- Message handler ---
async Task OnMessageReceived(object? sender, OnMessageReceivedArgs e)
{
    var message = e.ChatMessage.Message.ToLower().Trim();
    if (!message.StartsWith("!")) return;

    var command = message[1..];
    if (!coords.ContainsKey(command))
    {
        Console.WriteLine($"[Chat] Unknown command: {message}");
        return;
    }

    if (camCommands.Contains(command) && !monitorUp)
    {
        Console.WriteLine($"[Chat] Ignored !{command} — monitor is down");
        return;
    }

    if (panelCommands.Contains(command) && monitorUp)
    {
        Console.WriteLine($"[Chat] Ignored !{command} — monitor is up");
        return;
    }

    lock (voteLock)
    {
        if (!votes.ContainsKey(command)) votes[command] = 0;
        votes[command]++;
        Console.WriteLine($"[Vote] !{command} — {votes[command]} vote(s)");
    }

    await Task.CompletedTask;
}

// --- Command executor ---
async Task ExecuteCommand(string command)
{

    await client.SendMessageAsync("galactic_amy", $"Chat voted: !{command}");

    var (x, y) = coords[command];

    switch (command)
    {
        case "leftdoor":
        case "leftlight":
            WinInput.MoveTo(0, 400);
            await Task.Delay(600);
            WinInput.Click(x, y);
            break;

        case "rightdoor":
        case "rightlight":
            WinInput.MoveTo(1280, 400);
            await Task.Delay(600);
            WinInput.Click(x, y);
            break;

        case "monitor" when !monitorUp:
            WinInput.MoveTo(x, y);
            await Task.Delay(600);
            monitorUp = true;
            break;

        case "monitor" when monitorUp:
            WinInput.MoveTo(x, y);
            await Task.Delay(200);
            WinInput.MoveTo(x, y - 100);
            await Task.Delay(200);
            WinInput.MoveTo(640, 360);
            monitorUp = false;
            break;

        default:
            WinInput.Click(x, y);
            break;
    }
}