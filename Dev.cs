using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Dalamud.Game.Chat;
using Dalamud.Game.Chat.SeStringHandling;
using Dalamud.Plugin;
using Dalamud.Game.Command;
using ImGuiNET;
using System.IO;
using System.Runtime.CompilerServices;
using Dalamud.Configuration;
using Num = System.Numerics;
using Dalamud.Interface;
using System.Runtime.InteropServices;

namespace Yalms
{
    public class Dev : IDalamudPlugin
    {
        public string Name => "Dev";
        private DalamudPluginInterface pluginInterface;
        public Config Configuration;

        public bool enabled = true;
        public bool config = false;

        //**********************************
        //**         DEV FROM HERE        **
        //**********************************

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr GetBaseUIObjDelegate();
        private GetBaseUIObjDelegate getBaseUIObj;
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        private delegate IntPtr GetUI2ObjByNameDelegate(IntPtr getBaseUIObj, string UIName, int index);
        private GetUI2ObjByNameDelegate getUI2ObjByName;

        public IntPtr scan1;
        public IntPtr scan2;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
            Configuration = pluginInterface.GetPluginConfig() as Config ?? new Config();
            this.pluginInterface.UiBuilder.OnBuildUi += DrawWindow;

            this.pluginInterface.UiBuilder.OnOpenConfigUi += ConfigWindow;
            this.pluginInterface.CommandManager.AddHandler("/dev", new CommandInfo(Command)
            {
                HelpMessage = "'"
            });


            //**********************************
            //**         DEV FROM HERE        **
            //**********************************

            scan1 = pluginInterface.TargetModuleScanner.ScanText("E8 ?? ?? ?? ?? 41 b8 01 00 00 00 48 8d 15 ?? ?? ?? ?? 48 8b 48 20 e8 ?? ?? ?? ?? 48 8b cf");
            scan2 = pluginInterface.TargetModuleScanner.ScanText("e8 ?? ?? ?? ?? 48 8b cf 48 89 87 ?? ?? 00 00 e8 ?? ?? ?? ?? 41 b8 01 00 00 00");

            //getBaseUIObj = Marshal.GetDelegateForFunctionPointer<GetBaseUIObjDelegate>(scan1);

            PluginLog.Log(getUI2ObjByName(scan1, "", 1).ToString());




        }

        public void Dispose()
        {
            pluginInterface.CommandManager.RemoveHandler("/dev");
            this.pluginInterface.UiBuilder.OnBuildUi -= DrawWindow;
            this.pluginInterface.UiBuilder.OnOpenConfigUi -= ConfigWindow;
        }

        private void Command(string command, string arguments)
        {
            if (enabled)
            { enabled = false; }
            else
            { enabled = true; }
        }

        private void ConfigWindow(object Sender, EventArgs args)
        {
            config = true;
        }

        private void DrawWindow()
        {
            if (enabled)
            {
                ImGui.SetNextWindowSize(new Num.Vector2(300, 500), ImGuiCond.FirstUseEver);
                ImGui.Begin("Value?", ref enabled);
                ImGui.Text(getUI2ObjByName(scan1, "", 1).ToString());
                ImGui.End();
            }

            if (config)
            {
                ImGui.SetNextWindowSize(new Num.Vector2(300, 500), ImGuiCond.FirstUseEver);
                ImGui.Begin("Dev Config", ref config);
                ImGui.Checkbox("Enable", ref enabled);
                ImGui.End();
            }
        }

    }

    public class Config : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public bool Enabled { get; set; } = false;
    }
}
