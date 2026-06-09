# OpenSteam

**OpenSteam** is an open-source and secure alternative inspired by tools like SteamTools.
It focuses on safety, transparency, and performance, offering a fast and efficient user experience.

OpenSteam includes built-in Millennium installation and an integrated **Lua Store**, allowing you to download and manage Lua scripts **without relying on external websites**.
It also features a **manual installation section**, so you can install scripts that are not available in the store.

![Status](https://img.shields.io/badge/status-release-green)
![License](https://img.shields.io/badge/license-GPL-green)
![Version](https://img.shields.io/badge/version-3.0.0-blue)

---

## Preview

![Project Preview](https://private-user-images.githubusercontent.com/90818635/604705484-bf15c8c9-4221-4456-b5ad-09781fe9cab8.png?jwt=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJnaXRodWIuY29tIiwiYXVkIjoicmF3LmdpdGh1YnVzZXJjb250ZW50LmNvbSIsImtleSI6ImtleTUiLCJleHAiOjE3ODA5NjYzOTQsIm5iZiI6MTc4MDk2NjA5NCwicGF0aCI6Ii85MDgxODYzNS82MDQ3MDU0ODQtYmYxNWM4YzktNDIyMS00NDU2LWI1YWQtMDk3ODFmZTljYWI4LnBuZz9YLUFtei1BbGdvcml0aG09QVdTNC1ITUFDLVNIQTI1NiZYLUFtei1DcmVkZW50aWFsPUFLSUFWQ09EWUxTQTUzUFFLNFpBJTJGMjAyNjA2MDklMkZ1cy1lYXN0LTElMkZzMyUyRmF3czRfcmVxdWVzdCZYLUFtei1EYXRlPTIwMjYwNjA5VDAwNDgxNFomWC1BbXotRXhwaXJlcz0zMDAmWC1BbXotU2lnbmF0dXJlPTgxYTEzZjVjYjFhZjg3ZDliOWJlYjEzODVkMDg1ZDY0YjJmODk4YzUxNjBjZDUwMDc5ZWI4NWY5ZjM4MWIyN2EmWC1BbXotU2lnbmVkSGVhZGVycz1ob3N0JnJlc3BvbnNlLWNvbnRlbnQtdHlwZT1pbWFnZSUyRnBuZyJ9.0Q2nhQYwiNM84FlXdJRLwd2yVjuaPXB4tC0hOb7LaOs)

> Reference version: **v3.0.0**.

---

## Features

- **Millennium integration** with automatic installation
- **Built-in Lua Store** — no external websites required
- **Manual Lua installation** for games without store support
- **Fast and lightweight interface**
- **Open-source and security-focused**

---

## Technologies Used

- **C#**
- **Native libraries and modified DLLs**
-  **Alternative Dll by** https://github.com/OpenSteam001/OpenSteamTool 
- **SOLVED by** openlua.cloud **and** steamproof.net. Thank you for the solution to the Connection Error.

---

## Installation

Download and install the latest release from GitHub:

https://github.com/Abrahamqb/OpenSteam/releases

---
## OpenSteamCLI (Windows)

OpenSteamCLI allows you to patch and unlock Steam games directly from the terminal,
without installing the desktop version of OpenSteam.
```powershell
iwr -useb 'https://raw.githubusercontent.com/Abrahamqb/OpenSteam/refs/heads/master/CLI/OpenSteam.ps1' | iex
```

---

## How to Use

First, patch Steam and restart it. Then choose one of the following methods to add games:

**Manual Lua (LuaLoader):** Download the Lua file from an external website such as
Openlua.cloud or fares.top and load it manually.

**Lua Store (official, free):** Enter the Steam game ID (find it on SteamDB) and click
the button. The game will be added to your account instantly.

**Millennium + LuaManager (recommended):** Install Millennium — OpenSteam automates this —
then activate the plugin to add games natively from Steam.

> **Important:** After patching, adding, or removing a game, you must restart Steam for
> the changes to take effect. You can restart manually or use the designated button in the app.

---

## Compatibility

| Platform | Status        |
|----------|---------------|
| Windows  | Supported     |
| Linux    | Not supported |
| macOS    | Not supported |

---

## Disclaimer

This project is provided **for educational and development purposes only**.
Use it responsibly and at your own risk.

---

## License

This project is licensed under the **MIT License**.
See the [LICENSE](LICENSE) file for more details.

---

## Author

**Abrahamqb** — [github.com/Abrahamqb](https://github.com/Abrahamqb)

---

If you find this project useful, consider giving it a star on GitHub.
