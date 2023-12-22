﻿using Azure.AI.OpenAI;

using Generellem.Orchestrator;

using Microsoft.Extensions.Hosting;

namespace GenerellemConsole;

/// <summary>
/// Starts and runs the main application as a .NET Hosted Service.
/// </summary>
internal class GenerellemHostedService(
    GenerellemOrchestratorBase orchestrator, 
    IHostApplicationLifetime lifetime) 
    : IHostedService
{
    readonly GenerellemOrchestratorBase orchestrator = orchestrator;
    readonly IHostApplicationLifetime lifetime = lifetime;

    /// <summary>
    /// Kicks off file retrieval/upload and starts Console UI.
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    public async Task StartAsync(CancellationToken cancelToken)
    {
        try
        {
            // Normally, this would run in a separate service that
            // runs on a periodic timer to grab the latest files.
            await orchestrator.ProcessFilesAsync(cancelToken);

            PrintBanner();

            await RunMainLoopAsync(cancelToken);
        }
        catch (OperationCanceledException opcEx)
        {
            Console.WriteLine($"Operation Canceled - Details\n\n{opcEx}");
        }
        finally
        {
            lifetime.StopApplication();

            do
            {
                // If the user runs outside of visual studio
                // the application runs and stops before they
                // can see output. Although this causes a second
                // keypress of Visual studio, it still helps when
                // used on the Windows command line, another
                // platform like Linux or MacOS, or when using an
                // editor that doesn't stop the temp command window.
                Console.WriteLine("\nPress Enter to stop...\n");
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                if (keyInfo.Key == ConsoleKey.Enter)
                    break;

            } while (true);
        }
    }

    /// <summary>
    /// Keeps prompting the user for questions until they enter a stop word.
    /// </summary>
    /// <param name="cancelToken"><see cref="CancellationToken"/></param>
    /// <returns></returns>
    async Task RunMainLoopAsync(CancellationToken cancelToken)
    {
        List<string> stopWords = ["abort", "adios", "bye", "chao", "end", "quit", "stop"];

        string? userInput;

        Queue<ChatMessage> chatHistory = new();

        do
        {
            Console.Write("generellem>");
            userInput = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(userInput))
                continue;

            string response = await orchestrator.AskAsync(userInput, chatHistory, cancelToken);

            Console.WriteLine($"\n{response}\n");

            if (cancelToken.IsCancellationRequested)
                break;

        } while (!stopWords.Contains(userInput));
    }

    static void PrintBanner()
    {
        string bannerMessage =
"""
*-----------------------------------------*
* Welcome to the Generellem Console Demo! *
*-----------------------------------------*

This Console app lets you try out Generellem to see how it works. You can
also use this to test new services like RAG Search and new LLMs.

You can start out with a question like, "How do I contribute to Generellem?".

You can ask any question that can be answered because of content from this 
repository. The root folder has Markdown files, like CONTRIBUTING.md. Also, 
the Documents folder has a growing list of content, covering supported 
scenarios. For testing, you can browse those documents and form questions
based on their content.

We'll add more content as time goes by, which will help to more clearly 
demonstrate how Generellem works.

Remember to follow instructions on the Getting Started page to configure
services properly:

https://github.com/generellem/generellem/wiki/Getting-Started

Let's get started!

""";

        Console.WriteLine(bannerMessage);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
