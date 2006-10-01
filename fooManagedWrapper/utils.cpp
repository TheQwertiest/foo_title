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
#include "stdafx.h"

using namespace fooManagedWrapper;
using namespace System;
using namespace pfc;

MetaDBHandle::MetaDBHandle(metadb_handle *src) {
	this->handle = src;
}

MetaDBHandle::!MetaDBHandle() {
	handle = 0;
}

MetaDBHandle::~MetaDBHandle() {
	handle = 0;
}

String ^MetaDBHandle::GetPath() {
	if (handle == NULL)
		throw gcnew System::AccessViolationException("GetPath() called on a MetaDBHandle containing NULL handle");
	const char *c_path = handle->get_path();
	return gcnew String(c_path, 0, strlen(c_path), gcnew System::Text::UTF8Encoding(true, true));
}

String ^CPlayControl::FormatTitle(MetaDBHandle ^handle, String ^spec) {
	metadb_handle * handle_c = handle->GetHandle();
	const char* spec_c = (const char*)(System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(spec)).ToPointer();
	string8 out;

	static_api_ptr_t<play_control> pc;

	static_api_ptr_t<titleformat_compiler> titlecompiler;
	service_ptr_t<titleformat_object> compiledScript;
	titlecompiler->compile(compiledScript, spec_c);

	pc->playback_format_title_ex(handle_c, NULL, out, compiledScript, NULL,  playback_control::display_level_all);
	
	String ^res = gcnew String(out.get_ptr(), 0, out.length(), gcnew System::Text::UTF8Encoding());
	System::Runtime::InteropServices::Marshal::FreeHGlobal(IntPtr((void*)spec_c));
	return res;
}

/*
String ^CPlayControl::FormatTitle(String ^spec) {
	static_api_ptr_t<play_control> pc;
	
	static_api_ptr_t<titleformat_compiler> titlecompiler;
	service_ptr_t<titleformat_object> compiledScript;
	titlecompiler->compile(compiledScript, spec_c);
	

	

}
*/


void fooManagedWrapper::Console::Error(String ^a) {
	const char *c_msg = CManagedWrapper::ToCString(a);
	console::error(c_msg);
	CManagedWrapper::FreeCString(c_msg);
}

void fooManagedWrapper::Console::Warning(String ^a) {
	const char *c_msg = CManagedWrapper::ToCString(a);
	console::warning(c_msg);
	CManagedWrapper::FreeCString(c_msg);
}

void fooManagedWrapper::Console::Write(String ^a) {
	const char *c_msg = CManagedWrapper::ToCString(a);
	console::print(c_msg);
	CManagedWrapper::FreeCString(c_msg);
}
