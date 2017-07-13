using System;
using System.Collections.Generic;
using System.Text;

namespace fooTitle {

    class ToggleEnabledCommand : fooManagedWrapper.CCommand {

        public override void Execute() {
            Main.GetInstance().ToggleEnabled();
        }

        public override bool GetDescription(ref string desc) {
           desc = "Enables or disables foo_title display.";
           return true;
        }

        public override Guid GetGuid() {
            return new Guid(457, 784, 488, 36, 48, 79, 54, 12, 36, 56, 1);
        }

        public override string GetName() {
            return "Toggle foo_title";
        }
    }

    class TriggerDisplay : fooManagedWrapper.CCommand
    {
        public override void Execute()
        {
            Main.GetInstance().TriggerDisplay();
        }

        public override bool GetDescription(ref string desc)
        {
            desc = "Triggers foo_title display briefly.";
            return true;
        }

        public override Guid GetGuid()
        {
            return new Guid(457, 784, 488, 36, 48, 79, 54, 12, 36, 56, 2);
        }

        public override string GetName()
        {
            return "Trigger foo_title";
        }

        public override uint GetFlags()
        {
            return (uint)fooManagedWrapper.CMainMenuCommandsImpl.Flags.Hidden;
        }
    }

    public class ViewMenuCommands : fooManagedWrapper.CMainMenuCommandsImpl{
        public ViewMenuCommands() {
            this.cmds.Add(new ToggleEnabledCommand());
            this.cmds.Add(new TriggerDisplay());
        }

        public override Guid Parent {
            get {
                return (Guid)fooManagedWrapper.CMainMenuCommandsImpl.view;
            }
        }

        public override uint SortPriority {
            get {
                unchecked {
                    return (uint)(int)fooManagedWrapper.CMainMenuCommandsImpl.Flags.PriorityDontCare;
                }
            }
        }
    }
}
