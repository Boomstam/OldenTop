# Project Notes For Codex

## Unity MCP

This project uses the AI Game Developer MCP server through the Codex MCP entry named `AI-Game-Developer`.

- Prefer the Unity MCP tools and `.agents/skills` workflows for Unity scene, GameObject, asset, package, profiler, screenshot, and script tasks.
- The Unity project must be open in the Unity Editor for MCP-backed operations to work.
- The Unity-side config is `UserSettings/AI-Game-Developer-Config.json`, with `connectionMode` set to `Custom`.
- The local MCP server listens on `http://localhost:23252`.
- The package and its `PlayerPrefsEx` dependency are embedded under `Packages/`; do not replace them with registry references unless explicitly requested.
- Sandboxed localhost checks may fail even when the MCP server is healthy; use an unsandboxed local check when verification is needed.

## Working Style

- Treat bandwidth as precious: prefer local files and caches, avoid unnecessary downloads, and keep checks lightweight.
