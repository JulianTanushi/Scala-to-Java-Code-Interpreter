# Scala to Java Code Interpreter

An intelligent C# application that leverages Azure AI Agents to automatically convert Scala code files to Java. This tool processes entire directories of Scala files and generates equivalent Java code while maintaining functionality and following Java best practices.

## Features Overview

- **Batch Processing**: Convert entire directories of Scala files at once
- **AI-Powered Conversion**: Uses Azure AI Agents for intelligent code translation
- **Modern Java Output**: Generates Java 17+ compatible code with modern features
- **Smart Mapping**: Handles Scala-specific constructs appropriately:
  - Case classes → Java records or POJOs with builder pattern
  - Pattern matching → Switch expressions or if-else chains
  - Option types → Optional<T>
  - Collections → Java Collections API
  - Higher-order functions → Java 8+ functional interfaces
- **Progress Tracking**: Real-time conversion status with success/failure indicators
- **Error Handling**: Robust error handling with detailed logging

## Prerequisites

- **.NET 6.0 or later**
- **Azure AI Foundry Service** with AI Foundry Projects API access
- **Azure credentials** configured (Azure CLI, Managed Identity, or Service Principal)
- **Scala source files** to convert

## Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/JulianTanushi/Scala-to-Java-Code-Interpreter.git
   cd scala-to-java-converter
   ```

2. **Install dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure Azure credentials**
   - Install Azure CLI: `az login`
   - Or set up environment variables for Service Principal authentication

## Configuration

Before running the application, update the following configurations in `run_agent.cs`:

### Required Configuration

```csharp
// Update these paths to match your environment
string scalaInputDirectory = "/path/to/your/scala-files";
string javaOutputDirectory = "/path/to/output/java-files";

// Update with your Azure AI Project endpoint
var endpoint = new Uri("https://your-resource.services.ai.azure.com/api/projects/your-project");

// Update with your agent ID
PersistentAgent agent = agentsClient.Administration.GetAgent("your-agent-id");
```

### Azure AI Agent Setup

1. Create an Azure AI Foundry Project in the Azure portal
2. Deploy a Persistent Azure AI Agent trained for code conversion
3. Note your agent ID and project endpoint
4. Ensure the agent has appropriate instructions for Scala to Java conversion

## Usage

### Basic Usage

1. **Prepare your directories**
   ```bash
   mkdir scala-files java-files
   # Copy your .scala files to scala-files directory
   ```

2. **Run the converter**
   ```bash
   dotnet run
   ```

3. **Monitor progress**
   The application will display real-time progress:
   ```
   === Scala to Java Converter ===
   Found 5 Scala files to convert.
   Processing: MyScalaClass.scala
   Converted: MyScalaClass.scala → MyScalaClass.java
   Processing: DataProcessor.scala
   Converted: DataProcessor.scala → DataProcessor.java
   Conversion completed! Check output directory: /path/to/java-files
   ```

### Directory Structure

```
project-root/
├── scala-files/           # Input: Place your .scala files here
│   ├── samplecode.scala
├── java-files/            # Output: Converted .java files
│   ├── HelloWorld.java
└── run-agent.cs             # Main application
```


## Customization

### Modifying Conversion Prompts

Edit the `conversionPrompt` string in `RunScalaToJavaConversion()` to customize how the AI agent processes your code:

```csharp
string conversionPrompt = $@"Please convert the following Scala code to Java.
    
// Add your custom requirements here
// Example: Use specific Java frameworks, coding standards, etc.
                  
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
        
        Please provide only the converted Java code without explanations.;
```

### Adding Custom Processing

Extend the `ExtractJavaCodeFromResponse()` method to handle specific formatting requirements or post-processing logic.

## Error Handling

The application includes comprehensive error handling:

- **File I/O errors**: Gracefully handles missing files or permission issues
- **Azure API errors**: Displays detailed error messages for API failures
- **Conversion failures**: Continues processing other files if one fails
- **Network issues**: Implements retry logic with exponential backoff

## Performance Considerations

- **Rate Limiting**: The application includes 500ms delays between API calls
- **Memory Usage**: Processes files sequentially to manage memory consumption
- **Concurrent Processing**: Can be modified for parallel processing if needed

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Troubleshooting

### Common Issues

1. **"No Scala files found"**
   - Verify the `scalaInputDirectory` path is correct
   - Ensure `.scala` files exist in the directory

2. **Azure authentication errors**
   - Run `az login` to authenticate with Azure CLI
   - Verify your Azure credentials have access to the AI Project

3. **Agent not found**
   - Check your agent ID in the Azure portal
   - Ensure the agent is deployed and active

4. **Conversion quality issues**
   - Review and customize the conversion prompt
   - Consider fine-tuning your Azure AI Agent with better examples


## Related Links

- [Azure AI Foundry Documentation](https://docs.microsoft.com/en-us/azure/cognitive-services/)
- [.NET Azure SDK](https://github.com/Azure/azure-sdk-for-net)
- [Scala Language Specification](https://scala-lang.org/files/archive/spec/)
- [Java Language Specification](https://docs.oracle.com/javase/specs/)

---
