using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Spectre.Console;
using System.Text;

namespace Ai_Learning_ComputerVision
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Display ASCII art logo
            AnsiConsole.MarkupLine("");
            AnsiConsole.MarkupLine("[bold blue] WELCOME TO AI LEARNING COMPUTER VISION![/]");
            AnsiConsole.MarkupLine("");
            AnsiConsole.MarkupLine("[green]---------------------------------------------------------------[/]");
            AnsiConsole.MarkupLine("[green] This AI was created to analyze images [/]\n");

            while (true)
            {
                // Main menu options
                var decision = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold green] What do you want to do? Select an option:[/]")
                        .AddChoices(new[] {
                            "Start Image Analysis",
                            "Clear Console",
                            "Exit"
                        }));

                switch (decision)
                {
                    case "Start Image Analysis":
                        await StartImageAnalysis();
                        break;
                    case "Clear Console":
                        Console.Clear();
                        AnsiConsole.MarkupLine("[green] Console cleared! Ready for your next command.[/]");
                        break;
                    case "Exit":
                        var exitDecision = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("[red] Are you sure you want to exit?[/]")
                                .AddChoices(new[] { "Yes", "No" }));

                        if (exitDecision == "Yes")
                        {
                            AnsiConsole.MarkupLine("[green] Thank you for using the AI Learning Computer Vision! Goodbye![/]");
                            return; // Exits the application
                        }
                        break;
                }
            }
        }
        //Analyze Function
        private static async Task StartImageAnalysis()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("------------------------");
            string key = ReadMaskedInput(">> Please enter your (Key): "); // Use masked input
            Console.WriteLine("----------------------------");
            string endpoint = ReadMaskedInput(">> Please enter your (Endpoint): "); // Use masked input
            Console.ResetColor();

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(endpoint))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" Key or Endpoint cannot be empty. Please provide valid values.");
                Console.ResetColor();
                return;
            }

            // Authenticate Access
            ComputerVisionClient client = Authenticate(endpoint, key);

            // Ask the user whether they want to analyze a local file or a URL
            var inputType = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green] Do you want to analyze an image from a URL or a Local File?[/]")
                    .AddChoices(new[] { "URL", "Local File" }));

            string imagePath;
            if (inputType == "URL")
            {
                imagePath = AnsiConsole.Ask<string>("[green]>> Please enter the URL of the image:[/]");
            }
            else // Local File
            {
                imagePath = AnsiConsole.Ask<string>("[green]>> Please enter the full path to the image on your desktop:[/]");

                // Check if the file exists
                if (!File.Exists(imagePath))
                {
                    AnsiConsole.MarkupLine("[red]Error: The specified file does not exist. Please check the path and try again.[/]");
                    return;
                }
            }

            // Spinner
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync(" Analyzing image...", async ctx =>
                {
                    try
                    {
                        if (inputType == "URL")
                        {
                            // Analyze the image from URL
                            ImageAnalysis results = await client.AnalyzeImageAsync(imagePath, new List<VisualFeatureTypes?>
                            {
                                VisualFeatureTypes.Description,
                                VisualFeatureTypes.Tags,
                                VisualFeatureTypes.Objects
                            });

                            // Display analysis results
                            DisplayAnalysisResults(results);
                        }
                        else // Local File
                        {
                            using (var imageStream = File.OpenRead(imagePath))
                            {
                                // Analyze the image from stream
                                ImageAnalysis results = await client.AnalyzeImageInStreamAsync(imageStream, new List<VisualFeatureTypes?>
                                {
                                    VisualFeatureTypes.Description,
                                    VisualFeatureTypes.Tags,
                                    VisualFeatureTypes.Objects
                                });

                                // Display analysis results
                                DisplayAnalysisResults(results);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
                    }
                });
        }
        // Authentication Function
        public static ComputerVisionClient Authenticate(string endpoint, string key)
        {
            ComputerVisionClient client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };
            return client;
        }
        // Display Function
        private static void DisplayAnalysisResults(ImageAnalysis results)
        {
            // Create a table for the analysis results
            var table = new Table();
            table.AddColumn(new TableColumn("[bold green]Tag[/]").Centered());
            table.AddColumn(new TableColumn("[bold green]Confidence[/]").Centered());

            // Add results to the table
            foreach (var tag in results.Tags)
            {
                table.AddRow(tag.Name, $"{tag.Confidence:P}");
            }

            // Display the table in a bordered style
            AnsiConsole.Render(table);

            // Additional descriptive output
            AnsiConsole.MarkupLine($"[green]Total Objects Detected: {results.Objects.Count}[/]");
            AnsiConsole.MarkupLine($"[green]Description: {(results.Description.Captions.Count > 0 ? results.Description.Captions[0].Text : "No description available.")}[/]");
            AnsiConsole.MarkupLine("[green]----------------------------------------------------------[/]");
        }

        private static string ReadMaskedInput(string prompt)
        {
            Console.Write(prompt);
            var input = new StringBuilder();
            while (true)
            {
                var keyInfo = Console.ReadKey(intercept: true); // Read key without displaying
                if (keyInfo.Key == ConsoleKey.Enter) // If Enter key is pressed
                {
                    Console.WriteLine();
                    break;
                }
                else if (keyInfo.Key == ConsoleKey.Backspace) // Handle Backspace
                {
                    if (input.Length > 0)
                    {
                        input.Remove(input.Length - 1, 1);
                        Console.Write("\b \b"); // Move back, overwrite with space, and move back again
                    }
                }
                else
                {
                    input.Append(keyInfo.KeyChar); // Append the character
                    Console.Write("*"); // Display asterisk for each character
                }
            }
            return input.ToString();
        }
    }
}
