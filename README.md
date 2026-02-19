# 🌟 OpenSteam

> **OpenSteam** is an open-source and secure alternative inspired by tools like SteamTools.  
> It focuses on safety, transparency, and performance, offering a fast and efficient user experience.

OpenSteam includes built-in Millennium installation and an integrated **Lua store**, allowing you to download and manage Lua scripts **without relying on external websites**.  
It also features a **manual section**, so you can easily install scripts that are not available in the store.

![Status](https://img.shields.io/badge/status-%20release-green)
![License](https://img.shields.io/badge/license-MIT-green)
![Version](https://img.shields.io/badge/version-1.0.0-blue)

---

## 📸 Preview

![Project Preview](https://i.postimg.cc/y8YJJsBM/v0-2-3-beta.png)

> Reference version: **0.2.3 beta**  
> The visual design is subject to change in future releases.

---

## 🚀 Features

- 🧩 **Millennium integration** with automatic installation
- 🛒 **Built-in Lua Store** (no external websites required)
- 📂 **Manual Lua installation** for exclusive games or games without support in the store
- ⚡ **Fast and lightweight interface**
- 🔒 **Open-source and security-focused**
- 🛠️ **Easy to use and understand**

---

## 🧰 Technologies Used

- **C#**
- **Native libraries and modified DLLs** (responsible for injection)
- **API:** Kernelos.org

---

## 📦 Installation

Download and install the **latest release** from GitHub:

👉 https://github.com/Abrahamqb/OpenSteam/releases

## ⚡OpenSteamCLI (Windows) 🆕

OpenSteamCLI allows you to patch and "download" 😉 Steam games from the **terminal** without needing to install the desktop version of OpenSteam.

> iwr -useb 'https://raw.githubusercontent.com/Abrahamqb/OpenSteam/refs/heads/master/CLI/OpenSteam.ps1' | iex

## 🔥 How to use

First, you need to patch Steam and restart it. 
Now you have three methods to add games.
- Manual Lua (LuaLoader): You need to download the Lua file (not the game) from an external website, for example: Openlua.cloud or fares.top
- OpenSteam official (LuaStore is free): Enter the ID (use steamdb to find it) of the game you want to add and click the button; it will be added to your account instantly.
- Millennium + KernelLua (Not official but recommended): Here you need to install Millennium (OpenSteam automates this) and activate the plugin to enable the option to add games natively from Steam itself.
For Millennim and KernelLua, here is the tutorial by the creator of this plugin: https://www.youtube.com/watch?v=zE3iYCI5QNk

⚠️**Important: After patching, adding, or removing a game, you must restart Steam for the changes to take effect and the game to be added. You can restart manually or using the designated button.**

### Compatibility

- 🪟 Windows: ✅ Supported  
- 🐧 Linux: ❌ Not supported  
- 🍎 macOS: ❌ Not supported

---

## ⚠️ Disclaimer

This project is provided **for educational and development purposes**.  
Use it responsibly and at your own risk.

---

## 📄 License

This project is licensed under the **MIT License**.  
See the [LICENSE](LICENSE) file for more details. (There is none for now)

---

## 👤 Author

**Abrahamqb**  
- GitHub: [@Abrahamqb](https://github.com/Abrahamqb)

---

⭐ If you find this project useful, consider giving it a star!
