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

#include "MetaDBHandle.h"

using namespace System;

namespace fooManagedWrapper
{

public interface class IPlayControl {
	enum class StopReason {
		stop_reason_user = 0,
		stop_reason_eof = 1,
		stop_reason_starting_another = 2,
		stop_reason_shutting_down = 3
	};
	String ^FormatTitle(CMetaDBHandle ^dbHandle, String ^spec);
	CMetaDBHandle ^GetNowPlaying();
	double PlaybackGetPosition();
	bool IsPlaying();
	bool IsPaused();
};

private ref class CPlayControl : public IPlayControl {
public:
	virtual CMetaDBHandle ^GetNowPlaying();
	virtual String ^FormatTitle(CMetaDBHandle ^dbHandle, String ^spec);
	virtual double PlaybackGetPosition();
	virtual bool IsPlaying();
	virtual bool IsPaused();
};

}
