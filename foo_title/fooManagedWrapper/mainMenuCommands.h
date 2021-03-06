/*
*  This file is part of foo_title.
*  Copyright 2005 - 2006 Roman Plasil (http://foo-title.sourceforge.net)
*  Copyright 2016 Miha Lepej (https://github.com/LepkoQQ/foo_title)
*  Copyright 2017 TheQwertiest (https://github.com/TheQwertiest/foo_title)
*
*  This library is free software; you can redistribute it and/or
*  modify it under the terms of the GNU Lesser General Public
*  License as published by the Free Software Foundation; either
*  version 2.1 of the License, or (at your option) any later version.
*
*  This library is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
*
*  See the file COPYING included with this distribution for more
*  information.
*/

#pragma once

#include "fooServices.h"
#include "EnumeratorAdapter.h"
#include "Command.h"


using namespace System::Collections::Generic;

namespace fooManagedWrapper {

	class CMainMenuCommandsFactoryWrapper;

	/*
	<summary>
		Use this class to create your own menu commands in C#.
	</summary>
	*/
	public ref class CMainMenuCommandsImpl abstract : public IFoobarService {
	private:
		void commonInit();
	protected:
		CMainMenuCommandsFactoryWrapper *wrapper;
		List<CCommand^> ^cmds;
	public:
		CMainMenuCommandsImpl(List<CCommand^> ^_cmds);
		CMainMenuCommandsImpl();
		virtual ~CMainMenuCommandsImpl();
		!CMainMenuCommandsImpl();

		// now the prototypes for the interface methods
		virtual property unsigned int CommandsCount {
			unsigned int get();
		};
		virtual Guid GetGuid(unsigned int index);
		virtual String ^GetName(unsigned int index);
		virtual bool GetDescription(unsigned int index, String^ %desc);
		virtual property Guid Parent {
			Guid get() = 0;
		};
		virtual void Execute(unsigned int index);
		virtual property unsigned int SortPriority {
			unsigned int get() = 0;
		};
		virtual bool GetDisplay(unsigned int index, String^ %text, unsigned int %flags);

		static System::Guid ^file, ^view, ^edit, ^playback, ^library, ^help;
		
		enum class Flags : uint32_t {
               Disabled = (uint32_t)mainmenu_commands::flag_disabled,
               Checked = (uint32_t)mainmenu_commands::flag_checked,
               Hidden = (uint32_t)mainmenu_commands::flag_defaulthidden,
               PriorityBase = (uint32_t)mainmenu_commands::sort_priority_base,
               PriorityDontCare = (uint32_t)mainmenu_commands::sort_priority_dontcare,
               PriorityLast = (uint32_t)mainmenu_commands::sort_priority_last
		};
	};

	class CCustomMainMenuCommands : public mainmenu_commands {
	protected:
		gcroot<CMainMenuCommandsImpl^> managedMainMenuCommands;
	
	public:
		void SetImplementation(gcroot<CMainMenuCommandsImpl ^> impl);

		//! Retrieves number of implemented commands. Index parameter of other methods must be in 0....command_count-1 range.
		virtual t_uint32 get_command_count();
		//! Retrieves GUID of specified command.
		virtual GUID get_command(t_uint32 p_index);
		//! Retrieves name of item, for list of commands to assign keyboard shortcuts to etc.
		virtual void get_name(t_uint32 p_index,pfc::string_base & p_out);
		//! Retrieves item's description for statusbar etc.
		virtual bool get_description(t_uint32 p_index,pfc::string_base & p_out);
		//! Retrieves GUID of owning menu group.
		virtual GUID get_parent();
		
		//! Retrieves sorting priority of the command; the lower the number, the upper in the menu your commands will appear. Third party components should use sorting_priority_base and up (values below are reserved for internal use). In case of equal priority, order is undefined.
		virtual t_uint32 get_sort_priority();
		//! Retrieves display string and display flags to use when menu is about to be displayed. If returns false, menu item won't be displayed. You can create keyboard-shortcut-only commands by always returning false from get_display().
		virtual bool get_display(t_uint32 p_index,pfc::string_base & p_text,t_uint32 & p_flags);
		
		//! Executes the command. p_callback parameter is reserved for future use and should be ignored / set to null pointer.
		virtual void execute(t_uint32 p_index,service_ptr_t<service_base> p_callback);

	};

	class CMainMenuCommandsFactoryWrapper {
	public:
		mainmenu_commands_factory_t< CCustomMainMenuCommands > mainMenuCommands;
	};

	/*
	<summary>
		This is a wrapper for mainmenu_commands, provides access to existing commands
		from C#.
	</summary>
	*/
	public ref class CMainMenuCommands {
	private:
		service_ptr_t<mainmenu_commands> *ptr;
	public:
		CMainMenuCommands(const service_ptr_t<mainmenu_commands> &_ptr);
		virtual ~CMainMenuCommands();
		!CMainMenuCommands() {
			this->~CMainMenuCommands();
		}

		property Guid Parent {
			Guid get();
		}

		property unsigned int CommandCount {
			unsigned int get() {
				return (*ptr)->get_command_count();
			}
		}

		Guid ^GetCommand(unsigned int index);
		String ^GetName(unsigned int index);
		void Execute(unsigned int index);
	};

	public ref class CMainMenuCommandsEnumerator :
		public CEnumeratorAdapter<mainmenu_commands, CMainMenuCommands, CMainMenuCommands^> {
	};
};
