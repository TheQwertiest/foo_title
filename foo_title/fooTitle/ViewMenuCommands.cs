using System;

namespace fooTitle {
    internal class ToggleEnabledCommand : fooManagedWrapper.CCommand {

        public override void Execute() {
            Main.GetInstance().Hotkey_PopupToggle();
        }

        public override bool GetDescription(ref string desc) {
           desc = "Enables or disables foo_title popup.";
           return true;
        }

        public override Guid GetGuid() {
            return new Guid(457, 784, 488, 36, 48, 79, 54, 12, 36, 56, 1);
        }

        public override string GetName() {
            return "Toggle foo_title";
        }
    }

    internal class PopupPeekCmd : fooManagedWrapper.CCommand
    {
        public override void Execute()
        {
            Main.GetInstance().Hotkey_PopupPeek();
        }

        public override bool GetDescription(ref string desc)
        {
            desc = "Shows foo_title popup briefly.";
            return true;
        }

        public override Guid GetGuid()
        {
            return new Guid(457, 784, 488, 36, 48, 79, 54, 12, 36, 56, 2);
        }

        public override string GetName()
        {
            return "Peek foo_title";
        }

        public override uint GetFlags()
        {
            return (uint)fooManagedWrapper.CMainMenuCommandsImpl.Flags.Hidden;
        }
    }

    public class ViewMenuCommands : fooManagedWrapper.CMainMenuCommandsImpl{
        public ViewMenuCommands() {
            this.cmds.Add(new ToggleEnabledCommand());
            this.cmds.Add(new PopupPeekCmd());
        }

        public override Guid Parent => (Guid)fooManagedWrapper.CMainMenuCommandsImpl.view;

        public override uint SortPriority => (uint)fooManagedWrapper.CMainMenuCommandsImpl.Flags.PriorityDontCare;
    }
}
