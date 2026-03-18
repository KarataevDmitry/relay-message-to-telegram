using System.Collections.Frozen;
using System.Text.Json;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using TelegramRelay.Mcp;

var tools = ToolCatalog.Build();

var options = new McpServerOptions
{
    ServerInfo = new Implementation { Name = "TelegramRelay.Mcp", Version = "0.1.0" },
    ProtocolVersion = "2024-11-05",
    Capabilities = new ServerCapabilities
    {
        Tools = new ToolsCapability { ListChanged = false }
    },
    Handlers = new McpServerHandlers
    {
        ListToolsHandler = (_, _) => ValueTask.FromResult(new ListToolsResult { Tools = tools }),
        CallToolHandler = async (request, cancellationToken) =>
        {
            var name = request.Params?.Name ?? "";
            var args = request.Params?.Arguments is IReadOnlyDictionary<string, JsonElement> providedArgs
                ? providedArgs
                : FrozenDictionary<string, JsonElement>.Empty;

            try
            {
                var text = await Handlers.HandleAsync(name, args, cancellationToken).ConfigureAwait(false);
                return new CallToolResult
                {
                    Content = [new TextContentBlock { Text = text }],
                    IsError = false
                };
            }
            catch (ArgumentException ex)
            {
                return new CallToolResult
                {
                    Content = [new TextContentBlock { Text = "Error: " + ex.Message }],
                    IsError = true
                };
            }
            catch (Exception ex)
            {
                return new CallToolResult
                {
                    Content = [new TextContentBlock { Text = "Error: " + ex.Message }],
                    IsError = true
                };
            }
        }
    }
};

var transport = new StdioServerTransport("TelegramRelay.Mcp");
await using var server = McpServer.Create(transport, options);
await server.RunAsync(cancellationToken: default).ConfigureAwait(false);
return 0;
