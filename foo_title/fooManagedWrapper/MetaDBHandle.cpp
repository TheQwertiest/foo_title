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

#include "stdafx.h"
#include "MetaDBHandle.h"
#include "ManagedWrapper.h"


namespace fooManagedWrapper
{

CMetaDBHandle::CMetaDBHandle( const metadb_handle_ptr &src )
{
     this->handle = new metadb_handle_ptr( src );
}

CMetaDBHandle::!CMetaDBHandle()
{
     // The GC may postpone freeing objects after foobar's services are not
     // available, so trying to free the handle gives an assert violation.
     // As this happens only when exiting the process, we needn't care.
     if ( !core_api::are_services_available() ) return;

     if ( this->handle != NULL )
     {
          delete this->handle;
          this->handle = NULL;
     }
}

CMetaDBHandle::~CMetaDBHandle()
{
     this->!CMetaDBHandle();
}

metadb_handle_ptr CMetaDBHandle::GetHandle()
{
     return *handle;
}

String ^CMetaDBHandle::GetPath()
{
     if ( handle == NULL )
          throw gcnew System::NullReferenceException( "GetPath() called on a MetaDBHandle containing NULL handle" );

     const char *c_path = ( *handle )->get_path();
     return gcnew String( c_path, 0, strlen( c_path ), gcnew System::Text::UTF8Encoding( true, true ) );
}

double CMetaDBHandle::GetLength()
{
     return ( *handle )->get_length();
}

Bitmap ^CMetaDBHandle::GetArtworkBitmap( Boolean get_stub )
{
     static_api_ptr_t<album_art_manager_v2> p_album_art_manager_v2;
     abort_callback_dummy cb_abort;

     pfc::list_t<GUID> guids;
     guids.add_item( album_art_ids::cover_front );

     album_art_extractor_instance_v2::ptr artwork_api_v2;
     if ( get_stub )
     {
          artwork_api_v2 = p_album_art_manager_v2->open_stub( cb_abort );
     }
     else
     {
          artwork_api_v2 = p_album_art_manager_v2->open( pfc::list_single_ref_t<metadb_handle_ptr>( *handle ), guids, cb_abort );
     }

     boolean b_found = false;
     album_art_data_ptr data;
     try
     {
          data = artwork_api_v2->query( album_art_ids::cover_front, cb_abort );
          b_found = true;
     }
     catch ( const exception_aborted & )
     {
     }
     catch ( exception_io_not_found const & )
     {
     }
     catch ( exception_io const &e )
     {
          console::formatter formatter;
          formatter << "Requested Album Art entry could not be retrieved: " << e.what() << "; get_stub: " << get_stub;
     }

     if ( b_found && data.is_valid() )
     {
          System::IO::UnmanagedMemoryStream ^stream = gcnew System::IO::UnmanagedMemoryStream( (unsigned char*)data->get_ptr(), data->get_size() );
          Bitmap ^bmp = gcnew Bitmap( stream );
          delete stream;
          data.release();
          return bmp;
     }

     return nullptr;
}

} // namespace fooManagedWrapper