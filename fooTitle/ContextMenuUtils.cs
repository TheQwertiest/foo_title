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
using System.Text;

using fooManagedWrapper;
using naid;

namespace fooTitle {
    public class ContextMenuUtils {
        
        /// <summary>
        /// Tries to find a contextmenu command given by its default path. Use
        /// forward slashes '/' to separate categories.
        /// </summary>
        public static bool FindContextCommandByDefaultPath(string path, out CContextMenuItem commands, out uint index) {
            foreach (CContextMenuItem cmds in new CContextMenuItemEnumerator()) {
                for (uint i = 0; i < cmds.Count; i++) {
                    
                    string currentPath = cmds.GetName(i);
                    if (!String.IsNullOrEmpty(cmds.GetDefaultPath(i))) {
                        currentPath = cmds.GetDefaultPath(i) + '/' + currentPath;
                    }

                    if (path.ToLowerInvariant() == currentPath.ToLowerInvariant()) {
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
