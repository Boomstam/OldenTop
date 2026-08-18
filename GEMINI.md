# Project Notes & Rules For Antigravity

## Unity MCP Server

This project uses the **AI Game Developer MCP server** (registered as `AI-Game-Developer`).

- Prefer the Unity MCP tools and `.agents/skills` workflows for Unity scene, GameObject, asset, package, profiler, screenshot, and script tasks.
- The Unity project must be open in the Unity Editor for MCP-backed operations to work.
- The Unity-side config is `UserSettings/AI-Game-Developer-Config.json`, with `connectionMode` set to `Custom`.
- The local MCP server listens on `http://localhost:23252`.
- The package and its `PlayerPrefsEx` dependency are embedded under `Packages/`; do not replace them with registry references unless explicitly requested.
- The embedded AI Game Developer package is currently version `0.82.3` and targets Unity `2022.3` or newer.
- Tool registration and tool enablement are separate. Use `unity-tool-list` to discover registered tools; if a required registered tool is disabled, enable only that tool with `tool-set-enabled-state`. Changes persist in the Unity-side config.
- `unity-mcp-cli` may not be on `PATH`. Before downloading anything, look for a cached executable under `~/.npm/_npx/*/node_modules/.bin/unity-mcp-cli`, or use the connected MCP tools directly.
- Sandboxed localhost checks may fail even when the MCP server is healthy; use an unsandboxed local check when verification is needed.

## Working Style & Invariants

- **Explicit Implementation Invariants**: Translate hard design constraints into explicit implementation invariants, then verify those exact invariants before handoff. Do not substitute a merely topologically similar representation (for example, adjacency links are not equivalent to geometry placed on hex edges).
- **Targeted Testing**: Tests are not a default completion ritual for early prototype changes. Add or run them only when they protect a concrete non-obvious invariant, investigate a regression, cover risky algorithmic/state work, or are explicitly requested. Use the narrowest targeted test available; do not fall back to the full project or package suite merely because a filter is inconvenient.
- **Selective Screenshots**: Play Mode screenshots are not a routine verification step. Capture one only when exact visual appearance is part of the requested outcome, a visual defect is being diagnosed, or the image can resolve a specific uncertainty that code/scene inspection cannot. Do not take screenshots just to prove that work was done.
- **Proportional Verification**: Keep verification effort proportional to the change. Before a costly check, identify the specific failure it could catch and whether a cheaper inspection would answer the same question.
- **Clarify When Uncertain**: Clarification is vital: if any requirement, intended visual result, rule interpretation, or implementation consequence is uncertain, ask the user before making the change. Do not guess when that guess could materially affect the result.

## Game Design Reference

- Read `Docs/GameDesign/LIVING_GAME_DESIGN.md` when work depends on the game's intended rules, theme, systems, terminology, or prototype scope.
- That document records exploratory brainstorming, not a frozen specification. Its contents are explicitly subject to revision through design work and playtesting.
