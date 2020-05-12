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
using System.Windows.Forms;
using Dalamud.Hooking;
using Dalamud.Game;


namespace Dev
{
    public class Dev : IDalamudPlugin
    {
        public string Name => "Dev";
        private DalamudPluginInterface pluginInterface;
        public Config Configuration;

        public bool enabled = true;
        public bool config = true;

        //**********************************
        //**         DEV FROM HERE        **
        //**********************************

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr GetBaseUIObjDelegate();
        private GetBaseUIObjDelegate getBaseUIObj;
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        private delegate IntPtr GetUI2ObjByNameDelegate(IntPtr getBaseUIObj, string UIName, int index);
        private GetUI2ObjByNameDelegate getUI2ObjByName;

        [UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        private delegate IntPtr UnknownFunc(ulong param_1, byte param_2, int param_3);
        private UnknownFunc ukFunc;

        private Hook<UnknownFunc> ukFuncHook;

        public IntPtr scan1;
        public IntPtr scan2;
        public IntPtr funcPtr;

        public IntPtr chatLog;
        public IntPtr chatLogStuff;
        public IntPtr chatLogPanel_0;

        public float[] chatLogPosition;
        public int Width = 0;
        public int Height = 0;
        public byte Alpha = 0;
        public byte BoxHide = 0;
        public byte BoxOn = 50;
        public byte BoxOff = 82;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
            Configuration = pluginInterface.GetPluginConfig() as Config ?? new Config();
            this.pluginInterface.UiBuilder.OnBuildUi += DrawWindow;

            this.pluginInterface.UiBuilder.OnOpenConfigUi += ConfigWindow;
            this.pluginInterface.CommandManager.AddHandler("/dev", new CommandInfo(Command)
            {
                HelpMessage = ""
            });
            this.pluginInterface.CommandManager.AddHandler("/devc", new CommandInfo(CommandC)
            {
                HelpMessage = ""
            });


            //**********************************
            //**         DEV FROM HERE        **
            //**********************************

            scan1 = pluginInterface.TargetModuleScanner.ScanText("E8 ?? ?? ?? ?? 41 b8 01 00 00 00 48 8d 15 ?? ?? ?? ?? 48 8b 48 20 e8 ?? ?? ?? ?? 48 8b cf");
            scan2 = pluginInterface.TargetModuleScanner.ScanText("e8 ?? ?? ?? ?? 48 8b cf 48 89 87 ?? ?? 00 00 e8 ?? ?? ?? ?? 41 b8 01 00 00 00");

            funcPtr = pluginInterface.TargetModuleScanner.Module.BaseAddress + 0x4cd440;

            ukFunc = new UnknownFunc(ukFuncFunc);
            ukFuncHook = new Hook<UnknownFunc>(funcPtr, ukFunc, this);

            getBaseUIObj = Marshal.GetDelegateForFunctionPointer<GetBaseUIObjDelegate>(scan1);
            getUI2ObjByName = Marshal.GetDelegateForFunctionPointer<GetUI2ObjByNameDelegate>(scan2);
            chatLog = getUI2ObjByName(Marshal.ReadIntPtr(getBaseUIObj(), 0x20), "ChatLog", 1);
            chatLogStuff = getUI2ObjByName(Marshal.ReadIntPtr(getBaseUIObj(), 0x20), "ChatLog", 1);
            chatLogPanel_0 = getUI2ObjByName(Marshal.ReadIntPtr(getBaseUIObj(), 0x20), "ChatLog", 1);

            chatLogPosition = new float[2];


            PluginLog.Log(chatLog.ToString());




        }


        public IntPtr ukFuncFunc(ulong param_1, byte param_2, int param_3)
        {
            PluginLog.Log("Maybe?");
            return ukFuncHook.Original(param_1, param_2, param_3);
        }

        public void Dispose()
        {
            pluginInterface.CommandManager.RemoveHandler("/dev");
            pluginInterface.CommandManager.RemoveHandler("/devc");
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

        private void CommandC(string command, string arguments)
        {
            if (config)
            { config = false; }
            else
            { config = true; }
        }

        private void ConfigWindow(object Sender, EventArgs args)
        {
            config = true;
        }

        private void DrawWindow()
        {
            if (chatLogStuff.ToString() != "0")
            {
                var chatLogProperties = Marshal.ReadIntPtr(chatLog, 0xC8);
                Marshal.Copy(chatLogProperties + 0x44, chatLogPosition, 0, 2);
                Width = Marshal.ReadInt16(chatLogProperties + 0x90);
                Height = Marshal.ReadInt16(chatLogProperties + 0x92);
                Alpha = Marshal.ReadByte(chatLogProperties + 0x73);
                BoxHide = Marshal.ReadByte(chatLogPanel_0 + 0x182);
            }

            if (enabled)
            {
                ImGui.SetNextWindowSize(new Num.Vector2(300, 500), ImGuiCond.FirstUseEver);
                ImGui.Begin("Dev Info", ref enabled);
                ImGui.Text("ChatLog: "+chatLog.ToString("X"));
                ImGui.Text("ChatLogStuff: " + chatLogStuff.ToString("X")); ImGui.SameLine();

                if (ImGui.Button("copy"))
                {Clipboard.SetText(chatLogStuff.ToString("X"));}
                if (chatLog.ToString() != "0")
                {
                    ImGui.Text("X: " + chatLogPosition[0].ToString());
                    ImGui.Text("Y: " + chatLogPosition[1].ToString());
                    ImGui.Text("W:" + Width.ToString());
                    ImGui.Text("H:" + Height.ToString());
                    ImGui.Text("A:" + Alpha.ToString());
                }
                ImGui.Text("chatLogPanel_0: " + chatLogPanel_0.ToString("X")); ImGui.SameLine();
                if (ImGui.Button("copy"))
                { Clipboard.SetText(chatLogPanel_0.ToString("X")); }
                ImGui.Text(BoxHide.ToString());
                if (ImGui.Button("Show")) { Marshal.WriteByte(chatLogPanel_0 + 0x182, BoxOn); }
                if (ImGui.Button("Hide")) { Marshal.WriteByte(chatLogPanel_0 + 0x182, BoxOff); }



                ImGui.End();
            }

            if (config)
            {
                ImGui.SetNextWindowSize(new Num.Vector2(300, 500), ImGuiCond.FirstUseEver);
                ImGui.Begin("Dev Config", ref config);
                ImGui.Checkbox("Enable", ref enabled);
                if (ImGui.Button("Get"))
                {
                    chatLog = getUI2ObjByName(Marshal.ReadIntPtr(getBaseUIObj(), 0x20), "ChatLog", 1);
                    chatLogPanel_0 = getUI2ObjByName(Marshal.ReadIntPtr(getBaseUIObj(), 0x20), "ChatLogPanel_0", 1);

                    if (chatLog.ToString() != "0")
                    {
                        chatLogStuff = Marshal.ReadIntPtr(chatLog, 0xc8);
                    }
                }



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
