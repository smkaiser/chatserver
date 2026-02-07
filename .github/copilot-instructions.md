# ChatServer Copilot Instructions

This repository contains an ASP.NET Core 10.0 SignalR chat application.

## Build and Run

- **Build**: `dotnet build`
- **Run**: `dotnet run`
- **Watch**: `dotnet watch run` (Recommended for development)
- **Restore Dependencies**: `dotnet restore`

## Testing

There are no unit tests. **Playwright** is recommended for end-to-end testing of the chat functionality.
- Use the Playwright MCP server to automate browser interactions.
- Key scenarios to test:
  - User login/naming.
  - Sending messages (text and code).
  - Receiving messages on a second client.
  - Reconnection logic.

## High-Level Architecture

### Backend (ASP.NET Core)
- **SignalR Hub**: `Hubs/ChatHub.cs` manages real-time communication.
  - Uses `ConcurrentDictionary<string, User>` for in-memory, transient user state tracking.
  - Broadcasts messages to all connected clients.
- **Utility**: `Util.cs` handles message processing.
  - **Sanitization**: Uses `HtmlSanitizer` to prevent XSS.
  - **Markdown**: Uses `Markdig` to render messages to HTML.

### Frontend (Razor Pages)
- **Entry Point**: `Pages/Index.cshtml` serves the chat interface.
- **Client Logic**: All chat logic is embedded directly in `Pages/Index.cshtml` (script tag), NOT in `wwwroot/js/site.js`.
  - Handles SignalR connection, reconnection, and UI updates.
  - Uses `navigator.locks` to prevent tab sleeping/disconnection.

## Key Conventions

- **Embedded JavaScript**: Do not look for chat logic in `wwwroot/js`. It is inline in `Index.cshtml` to keep the context tight with the Razor view.
- **Server-Side Rendering**: Messages are converted from Markdown to HTML on the server (`Util.MdToHtml`) before being broadcast.
- **Security**: All inputs must be sanitized via `Util.Sanitize` before storage or broadcast.
- **Transient State**: The application does not use a database. All user state is lost on server restart.
- **Raw Messages**: The "Send Code" feature wraps text in triple backticks (\`\`\`) to trigger code block formatting.
- **User Identity**: Users are identified by a generated session name or user prompt, stored only in memory. Colors are deterministically generated from the username on the client side.
- **File Sharing**: Files are sent as base64 data URLs through SignalR — no server-side storage. Images display inline; other files render as download links. Upload is gated on ≥2 connected users. Max file size is 5MB (client-enforced). The SignalR message limit is 10MB to accommodate base64 overhead.
