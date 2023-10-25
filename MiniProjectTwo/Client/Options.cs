using CommandLine;

namespace MiniProjectTwo.Client;

public class Options
{
    [Option('i', "identity",
        HelpText = "The identity indicates whether we are Alice, Bob or Charlie. Can be either, 'a', 'b' or 'c'.")]
    public char Identity { get; set; } = 'a';
}
