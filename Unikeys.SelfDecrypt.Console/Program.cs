using System.Reflection;
using Spectre.Console;
using Unikeys.SelfDecrypt.Core;

Console.Title = "Unikeys.SelfDecrypt";
Console.BackgroundColor = ConsoleColor.Black;

var outputFile = Self.FileName.Replace(".unikeys.exe", "");

var header = string.Join(Environment.NewLine,
    @" __  __     __   __     __     __  __     ______     __  __     ______    ",
    @"/\ \/\ \   /\ ""-.\ \   /\ \   /\ \/ /    /\  ___\   /\ \_\ \   /\  ___\   ",
    @"\ \ \_\ \  \ \ \-.  \  \ \ \  \ \  _""-.  \ \  __\   \ \____ \  \ \___  \  ",
    @" \ \_____\  \ \_\\""\_\  \ \_\  \ \_\ \_\  \ \_____\  \/\_____\  \/\_____\ ",
    @"  \/_____/   \/_/ \/_/   \/_/   \/_/\/_/   \/_____/   \/_____/   \/_____/ ");


var table = new Table
{
    Border = TableBorder.HeavyEdge,
    Expand = true
};
table.AddColumn($"[turquoise2]{header}[/]").Centered();
table.AddRow(new Markup("Unikeys.SelfDecrypt.Console - " +
                        $"v{Assembly.GetExecutingAssembly().GetName().Version} - " +
                        "[gold1]Aeris[/] (c) 2023"));
AnsiConsole.Write(table);

AnsiConsole.Write(Environment.NewLine);

AnsiConsole.Write(new Panel(new TextPath(outputFile)
        .LeafColor(new Color(255, 255, 255)))
{
    Header = new PanelHeader("[bold] File to be decrypted [/]"),
});

AnsiConsole.Write(Environment.NewLine);

if (File.Exists(outputFile))
{
    var overwrite = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title($"Replace the [steelblue1_1 bold]existing file[/]?{Environment.NewLine}" +
                   "Use the [steelblue1_1]arrow keys[/] to move and [steelblue1_1]enter[/] to select.")
            .AddChoices("No", "Yes"));

    if (overwrite == "No")
    {
        AnsiConsole.Write(new Markup("Thank you for using [steelblue1_1]Unikeys.SelfDecrypt[/]!"));
        AnsiConsole.Write(Environment.NewLine);
        await AnsiConsole.Live(new Markup("Exiting in [steelblue1_1 bold]5[/] seconds..."))
            .StartAsync(async ctx =>
            {
                for (var i = 5; i > -2; i--)
                {
                    await Task.Delay(1000);
                    ctx.UpdateTarget(new Markup($"Exiting in [steelblue1_1 bold]{i}[/] seconds...{Environment.NewLine}"));
                }
            });
        return;
    }
}

var password = AnsiConsole.Prompt(
    new TextPrompt<string>("Enter [steelblue1_1]password[/] >")
        .PromptStyle("blue")
        .Secret());

var success = false;
Exception? ex = null;
AnsiConsole.Status().Spinner(Spinner.Known.Default)
    .Start("Decrypting file...", _ =>
    {
        try
        {
            success = Self.TryDecrypt(password);
        }
        catch (Exception e)
        {
            ex = e;
        }
    });

AnsiConsole.Write(Environment.NewLine);

// Output

if (success)
{
    AnsiConsole.MarkupLine("[green]File decrypted [rapidblink]successfully[/]![/]");
    AnsiConsole.MarkupLine("Output file: ");
    AnsiConsole.Write(new TextPath(outputFile));
}
else
{
    AnsiConsole.MarkupLine("[red][rapidblink]Error[/], file could not be decrypted! Is this a self-executable file?[/]");
    if (ex != null)
        AnsiConsole.MarkupLine($"[red]Details: {ex?.Message}[/]");
}

AnsiConsole.Write(Environment.NewLine);

// Exit sequence

AnsiConsole.Write(new Markup("Thank you for using [steelblue1_1]Unikeys.SelfDecrypt[/]!"));

AnsiConsole.Write(Environment.NewLine);

await AnsiConsole.Live(new Markup("Exiting in [steelblue1_1 bold]5[/] seconds..."))
    .StartAsync(async ctx =>
    {
        for (var i = 5; i > -1; i--)
        {
            await Task.Delay(1000);
            ctx.UpdateTarget(new Markup($"Exiting in [steelblue1_1 bold]{i}[/] seconds...{Environment.NewLine}"));
        }
    });
