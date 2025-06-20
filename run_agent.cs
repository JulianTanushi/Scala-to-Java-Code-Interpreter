using Azure;
using Azure.Identity;
using Azure.AI.Projects;
using Azure.AI.Agents.Persistent;
using System.Text;
using System;
using System.Threading.Tasks;

partial class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            DisplayUsageInstructions();
            await RunScalaToJavaConversion();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    static async Task RunScalaToJavaConversion()
    {
        // Configuration
        string scalaInputDirectory = "/Users/julian/Documents/CustomProjects/ScalatoJava-Csharp/scala-files"; // Update with your actual path
        string javaOutputDirectory = "/Users/julian/Documents/CustomProjects/ScalatoJava-Csharp/java-files";  // Update with your actual path
        if (string.IsNullOrWhiteSpace(scalaInputDirectory) || string.IsNullOrWhiteSpace(javaOutputDirectory))
        {
            Console.WriteLine("Please set the scalaInputDirectory and javaOutputDirectory variables.");
            return;
        }
        var endpoint = new Uri("https://jtanushi-9347-resource.services.ai.azure.com/api/projects/jtanushi-9347");
        AIProjectClient projectClient = new(endpoint, new DefaultAzureCredential());

        PersistentAgentsClient agentsClient = projectClient.GetPersistentAgentsClient();

        PersistentAgent agent = agentsClient.Administration.GetAgent("asst_YQZlrfnHivwVMQBBAdWKWuPL");
        
        // Create a new thread for this conversion session
        PersistentAgentThread thread = agentsClient.Threads.CreateThread();
        
        try
        {
            // Ensure output directory exists
            Directory.CreateDirectory(javaOutputDirectory);
            
            // Get all Scala files from input directory
            string[] scalaFiles = Directory.GetFiles(scalaInputDirectory, "*.scala", SearchOption.AllDirectories);
            
            if (scalaFiles.Length == 0)
            {
                Console.WriteLine($"No Scala files found in directory: {scalaInputDirectory}");
                return;
            }
            
            Console.WriteLine($"Found {scalaFiles.Length} Scala files to convert.");
            
            foreach (string scalaFilePath in scalaFiles)
            {
                try
                {
                    Console.WriteLine($"Processing: {Path.GetFileName(scalaFilePath)}");
                    
                    // Read Scala file content
                    string scalaContent = await File.ReadAllTextAsync(scalaFilePath);
                    
                    // Create conversion message
                    string conversionPrompt = $@"Please convert the following Scala code to Java. 
                    
Scala file: {Path.GetFileName(scalaFilePath)}

You are an expert software engineer specializing in converting Scala code to Java.
        
        Please convert the following Scala code to equivalent Java code:
        
        Requirements:
        1. Maintain the same functionality and logic
        2. Use appropriate Java design patterns and conventions
        3. Handle Scala-specific features appropriately:
           - Case classes -> Java records or POJOs with builder pattern
           - Pattern matching -> switch expressions or if-else chains
           - Option types -> Optional<T>
           - Collections -> Java Collections API
           - Higher-order functions -> Java 8+ functional interfaces
        4. Add appropriate imports
        5. Use modern Java features (Java 17+) when beneficial
        6. Maintain code structure and comments
        
        Scala Code:
        ```scala
        {scalaContent}
        ```
        
        Please provide only the converted Java code without explanations.";

                    // Send message to agent
                    PersistentThreadMessage messageResponse = agentsClient.Messages.CreateMessage(
                        thread.Id,
                        MessageRole.User,
                        conversionPrompt);

                    // Create and run the conversion
                    ThreadRun run = agentsClient.Runs.CreateRun(
                        thread.Id,
                        agent.Id);

                    // Poll until the run reaches a terminal status
                    do
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500));
                        run = agentsClient.Runs.GetRun(thread.Id, run.Id);
                    }
                    while (run.Status == RunStatus.Queued || run.Status == RunStatus.InProgress);
                    
                    if (run.Status != RunStatus.Completed)
                    {
                        Console.WriteLine($"❌ Conversion failed for {Path.GetFileName(scalaFilePath)}: {run.LastError?.Message}");
                        continue;
                    }

                    // Get the latest messages (agent's response)
                    Pageable<PersistentThreadMessage> messages = agentsClient.Messages.GetMessages(
                        thread.Id, order: ListSortOrder.Descending);

                    // Extract Java code from agent's response
                    string javaCode = ExtractJavaCodeFromResponse(messages);
                    
                    if (!string.IsNullOrWhiteSpace(javaCode))
                    {
                        // Generate output file path
                        string relativeScalaPath = Path.GetRelativePath(scalaInputDirectory, scalaFilePath);
                        string javaFileName = Path.ChangeExtension(Path.GetFileName(relativeScalaPath), ".java");
                        string javaFilePath = Path.Combine(javaOutputDirectory, javaFileName);
                        
                        // Ensure subdirectory exists if needed
                        Directory.CreateDirectory(Path.GetDirectoryName(javaFilePath)!);
                        
                        // Write Java code to file
                        await File.WriteAllTextAsync(javaFilePath, javaCode);
                        
                        Console.WriteLine($"✅ Converted: {Path.GetFileName(scalaFilePath)} → {javaFileName}");
                    }
                    else
                    {
                        Console.WriteLine($"❌ No Java code extracted from response for {Path.GetFileName(scalaFilePath)}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error processing {Path.GetFileName(scalaFilePath)}: {ex.Message}");
                }
            }
            
            Console.WriteLine($"\n🎉 Conversion completed! Check output directory: {javaOutputDirectory}");
        }
        finally
        {
            // Optional: Clean up the thread
            try
            {
                agentsClient.Threads.DeleteThread(thread.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not delete thread: {ex.Message}");
            }
        }
    }

    static string ExtractJavaCodeFromResponse(Pageable<PersistentThreadMessage> messages)
    {
        var javaCodeBuilder = new StringBuilder();
        
        // Get the most recent assistant message
        foreach (PersistentThreadMessage message in messages)
        {
            if (message.Role == MessageRole.Agent)
            {
                foreach (MessageContent contentItem in message.ContentItems)
                {
                    if (contentItem is MessageTextContent textItem)
                    {
                        string responseText = textItem.Text;
                        
                        // Try to extract Java code from markdown code blocks
                        string javaCode = ExtractCodeFromMarkdown(responseText, "java");
                        if (!string.IsNullOrWhiteSpace(javaCode))
                        {
                            return javaCode;
                        }
                        
                        // If no markdown code blocks, return the entire response
                        // (assuming the agent returns pure Java code)
                        return responseText.Trim();
                    }
                }
                break; // Only process the first assistant message
            }
        }
        
        return string.Empty;
    }

    static string ExtractCodeFromMarkdown(string text, string language)
    {
        // Look for code blocks with specified language
        string startPattern = $"```{language}";
        string endPattern = "```";
        
        int startIndex = text.IndexOf(startPattern, StringComparison.OrdinalIgnoreCase);
        if (startIndex == -1)
        {
            // Try without language specifier
            startPattern = "```";
            startIndex = text.IndexOf(startPattern);
        }
        
        if (startIndex != -1)
        {
            startIndex += startPattern.Length;
            int endIndex = text.IndexOf(endPattern, startIndex);
            if (endIndex != -1)
            {
                return text.Substring(startIndex, endIndex - startIndex).Trim();
            }
        }
        // If no code block found, return empty string
        return string.Empty;
    }
            
    static void DisplayUsageInstructions()
    {
        Console.WriteLine("=== Scala to Java Converter ===");
        Console.WriteLine("Before running this program:");
        Console.WriteLine("1. Update the scalaInputDirectory path to point to your Scala files");
        Console.WriteLine("2. Update the javaOutputDirectory path to where you want Java files saved");
        Console.WriteLine("3. Ensure your Azure credentials are properly configured");
        Console.WriteLine("4. Make sure your agent is trained for Scala to Java conversion");
        Console.WriteLine();
    }
}