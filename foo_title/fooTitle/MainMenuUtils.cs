/*
    Copyright 2005 - 2006 Roman Plasil
	http://foo-title.sourceforge.net
    This file is part of foo_title.

    foo_title is free software; you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation; either version 2.1 of the License, or
    (at your option) any later version.

    foo_title is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with foo_title; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

using System;
using System.Collections.Generic;
using fooManagedWrapper;

namespace fooTitle {
    
    public static class MainMenuUtils {

        public static Dictionary<Guid, CMainMenuGroup> GroupsByGuid;

        static MainMenuUtils() {
            InitGroupsByGuid();
        }

        private static void InitGroupsByGuid() {
            GroupsByGuid = new Dictionary<Guid, CMainMenuGroup>();

            foreach (CMainMenuGroup group in new CMainMenuGroupEnumerator()) {
                GroupsByGuid[group.MyGuid] = group;
            }
        }

        /// <summary>
        /// Checks that <paramref name="cmds"/> has <paramref name="parts"/> as its
        /// parents.
        /// </summary>
        private static bool CheckCommandsParents(CMainMenuCommands cmds, string[] parts) {
            try {
                int currentPart = parts.Length - 2;
                CMainMenuGroup currentGroup = GroupsByGuid[cmds.Parent];

                while (currentPart >= 0) {
                    string part = parts[currentPart];

                    CMainMenuGroupPopup popup = currentGroup as CMainMenuGroupPopup;
                    if (popup != null) {
                        if (popup.Name == part)
                            currentPart--;
                        else
                            return false;
                    }

                    if (currentGroup.Parent == Guid.Empty)
                    {
                        return currentPart < 0;
                    }
                    currentGroup = GroupsByGuid[currentGroup.Parent];

                }
                return true;
            } catch (KeyNotFoundException) {
                return false;
            }

        }

        /// <summary>
        /// Tries to find a command in the main menu.
        /// </summary>
        /// <param name="path">
        /// The menu popup names from the top level down to the command name, 
        /// separated by slashes. For example 'Playback/Order/Random'.
        /// </param>
        /// <param name="commands">If found, returns the commands containing the command.</param>
        /// <param name="index">Index of found command in commands.</param>
        /// <returns>True if the command was found, false if it was not found.</returns>
        /// <exception>Thrown when the path has invalid format.
        ///     <cref>InvalidArgumentException</cref>
        /// </exception>
        public static bool FindCommandByPath(string path, out CMainMenuCommands commands, out uint index) {
            string[] parts = path.Split('/');
            string commandName = parts[parts.Length - 1];

            // find all occurrences of the command and then check it's parents to make sure it's the
            // one we need
            foreach (CMainMenuCommands cmds in new CMainMenuCommandsEnumerator()) {
                for (uint i = 0; i < cmds.CommandCount; i++) {
                    if (cmds.GetName(i) == commandName && CheckCommandsParents(cmds, parts)) {
                        commands = cmds;
                        index = i;
                        return true;
                    }
                }
            }

            commands = null;
            index = 0;
            return false;
        }
    }
}
