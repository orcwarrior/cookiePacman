using System;
using System.Collections.Generic;

using System.Text;
using OpenTK;
using OpenTK.Graphics;

namespace CookieMonster
{
	static class BinkGL
    {
        static EngineApp.Game engine { get { return CookieMonster_Objects.engineReference.getEngine(); } }
		static Int32 Get_desktop_color_depth()
		{
			return GraphicsContext.CurrentContext.GraphicsMode.Depth;
		}

		static IGraphicsContext Create_gl_context()
		{
			return GraphicsContext.CurrentContext;
		}
		public class RAD3D
		{
			public IntPtr window;
			public IGraphicsContext context;
			public DisplayDevice rendering_dc;
		} 


		// IntPtr with RAD3D
		static public RAD3D Open_RAD_3D()
		{
			RAD3D rad_3d;

			//
			// try to create a RAD3D structure
			//
			rad_3d = new RAD3D();
			if ( rad_3d == null )
			{
				return null;
			}
			//
			// Get a DC to use.
			//
			rad_3d.rendering_dc = DisplayDevice.Default;
			rad_3d.context = Create_gl_context();
			rad_3d.window = engine.windowHandle;

			return( rad_3d );			
		}
		public static void Close_RAD_3D( RAD3D rad_3d )
		{
			// Dont close nothing, all is based on same context that OpenTK uses 
		}
		public static void Start_RAD_3D_frame( RAD3D rad_3d )
		{
			if ( rad_3d != null)
			{// clear background:
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			}
		}
		public static void End_RAD_3D_frame(RAD3D rad_3d)
		{
			if ( rad_3d != null)
			{ // Swap buffers:
				engine.SwapBuffers();
			}
		}
		public static void Resize_RAD_3D( RAD3D rad_3d,
		Int32 width,
		Int32 height)
		{
			GL.Viewport(0, 0, width, height);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadIdentity();
		}
		public static UInt32 Round_up_to_next_2_power(UInt32 value)
		{
			if (value > 16)
			if (value > 64)
			if (value > 128)
			if (value > 256)
			if (value > 512)
			return (1024);
			else
			return (512);
			else
			return (256);
			else
			return (128);
			else
			if (value > 32)
			return (64);
			else
			return (32);
			else
			if (value > 4)
			if (value > 8)
			return (16);
			else
			return (8);
			else
			if (value > 2)
			return (4);
			return (value);
		}

		//## Setup the specified GL texture.                                       
		//############################################################################
		static unsafe void Setup_gl_texture(UInt32 texture, //input texture?
		UInt32 pitch,
		UInt32 pixel_size,
		UInt32 texture_width,
		UInt32 texture_height,
		UInt32 gl_surface_type,
        Byte[] buffer)
		{
			// Make the texture current.
			GL.BindTexture(TextureTarget.Texture2D, texture);

			//
			// Set the texture wrap and filtering options.
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);            
			
			
			//
			// Set the pixel format options.
			//
			GL.PixelStore(PixelStoreParameter.PackRowLength, pitch / pixel_size);
			GL.PixelStore(PixelStoreParameter.UnpackAlignment, (pitch % pixel_size) + 1);
			//
			// Upload data into the texture.
			//
            IntPtr buf = new IntPtr((void*)buffer[0]);
			GL.TexImage2D(TextureTarget.Texture2D, 0, (PixelInternalFormat)gl_surface_type,
			(Int32)texture_width, (Int32)texture_height, 0, (PixelFormat)gl_surface_type, PixelType.UnsignedByte,ref buf);
			
		}

		public class RAD3DIMAGE
		{
			public UInt32 total_textures;
			public UInt32 pitch;
			public UInt32 width;
			public UInt32 height;
			public bool alpha_pixels;
			public UInt32 pixel_size;
			public UInt32 textures_across;
			public UInt32 textures_down;
			public UInt32 remnant_width;
			public UInt32 remnant_height;
			public UInt32 remnant_input_width;
			public UInt32 remnant_input_height;
			public UInt32 maximum_texture_size;
			public UInt32 rad_surface_type;
			public UInt32 gl_surface_type;
			public UInt32[] gl_textures;
			public Byte[] pixels;
			public Int32 download_textures;
			public UInt32 row_length;
			public UInt32 texture_count;
		}
		public static  RAD3DIMAGE Open_RAD_3D_image( RAD3D rad_3d,
		UInt32 width,
		UInt32 height,
		bool alpha_pixels,
		UInt32 maximum_texture_size)
		{
			RAD3DIMAGE rad_image;
			UInt32 remnant_width, remnant_height;
			UInt32 buffer_pitch, buffer_height;
			UInt32 pitch, pixel_size;
			UInt32 total_textures;
			UInt32[] textures;
			UInt32 x, y;

			// Calculate the pixel size and the pitch
			pixel_size = (uint)(( alpha_pixels) ? 4 : 3);
			pitch = (uint)(((width * pixel_size ) + 15 ) & ~15);

			// Calculate the remnant size (for the width and the height)
			remnant_width = Round_up_to_next_2_power( width % maximum_texture_size );
			remnant_height = Round_up_to_next_2_power( height % maximum_texture_size );

			// The buffer_pitch is the greater of the remnant size and the input pitch.
			buffer_pitch = remnant_width * pixel_size;
			if ( buffer_pitch < pitch )
			buffer_pitch = pitch;

			// The buffer_height is the greater of the remnant size and the input height.
			buffer_height = ( height > remnant_height) ? height : remnant_height;

			// Calculate the total number of textures we'll need.
			total_textures = ( ( width + ( maximum_texture_size - 1 ) ) / maximum_texture_size ) *
			( ( height + ( maximum_texture_size - 1 ) ) / maximum_texture_size );

			// Allocate enough memory for a RAD image, a list of textures and a buffer.
			rad_image = new RAD3DIMAGE();
			/* malloc( System.Runtime.InteropServices.Marshal.SizeOf( RAD3DIMAGE ) +
			( total_textures * 4 ) +
			31 + ( buffer_pitch * buffer_height) );
			*/
			if ( rad_image == null )
			{
				return( null );
			}


			// The textures come after the structure.
			rad_image.gl_textures = new UInt32[total_textures];

			// And the buffer comes after the textures (aligned to a 32-byte address).
            rad_image.pixels = new Byte[buffer_pitch * buffer_height];

			// Set all the variables in our new structure.
			rad_image.total_textures = total_textures;
			rad_image.pitch = pitch;
			rad_image.width = width;
			rad_image.height = height;
			rad_image.alpha_pixels = alpha_pixels;
			rad_image.pixel_size = pixel_size;
			rad_image.textures_across = width / maximum_texture_size;
			rad_image.textures_down = height / maximum_texture_size;
			rad_image.remnant_width = remnant_width;
			rad_image.remnant_height = remnant_height;
			rad_image.remnant_input_width = width % maximum_texture_size;
			rad_image.remnant_input_height = height % maximum_texture_size;
			rad_image.maximum_texture_size = maximum_texture_size;
			rad_image.rad_surface_type = (uint)(( alpha_pixels) ? 7 : 9);//7 9 is some enum stuff
			rad_image.gl_surface_type = (alpha_pixels) ? (uint)PixelFormat.Rgba : (uint)PixelFormat.Rgb;
			rad_image.download_textures = 0;
			rad_image.row_length = pitch / pixel_size;
			rad_image.texture_count = 0;

			// Clear the buffer.
			//memset( rad_image->pixels,0,buffer_pitch * buffer_height );

			// Call GL to create a bunch of textures (clear the last one as a flag to
			//   see if all of the textures were created).
			rad_image.gl_textures[ rad_image.total_textures - 1 ] = 0;

			GL.GenTextures((int)rad_image.total_textures, rad_image.gl_textures);

			if ( rad_image.gl_textures[ rad_image.total_textures - 1 ] == 0 )
			{
				// GL didn't allocate enough textures for us, so just fail.
				rad_image = null;
				return(null);
			}


			//
			// Loop through and init each texture (setting each of their sizes).
			//

			textures = rad_image.gl_textures;
			int i = 0;
			for (y = 0; y < rad_image.textures_down ; y++ )
			{
				for ( x = 0 ; x < rad_image.textures_across ; x++ )
				{
					//
					// Setup the texture.
					// (every one)
					
					Setup_gl_texture( textures[i++],
					rad_image.pitch,
					rad_image.pixel_size,
					rad_image.maximum_texture_size,
					rad_image.maximum_texture_size,
					rad_image.gl_surface_type,
					rad_image.pixels );
				}

				//
				// Do the rememnant texture at the end of the scanline.
				//

				if ( rad_image.remnant_width > 0 )
				{
					//
					// Setup the texture.
					//

					Setup_gl_texture(textures[i++],
					rad_image.pitch,
					rad_image.pixel_size,
					rad_image.maximum_texture_size,
					rad_image.maximum_texture_size,
					rad_image.gl_surface_type,
					rad_image.pixels);
				}
			}

			//
			// Do the remnants along the bottom edge (if any).
			//

			if ( rad_image.remnant_height > 0 )
			{
				for ( x = 0 ; x < rad_image.textures_across ; x++ )
				{
					//
					// Setup the texture.
					//

					Setup_gl_texture(textures[i++],
					rad_image.pitch,
					rad_image.pixel_size,
					rad_image.maximum_texture_size,
					rad_image.maximum_texture_size,
					rad_image.gl_surface_type,
					rad_image.pixels);
				}
				if ( rad_image.remnant_width > 0 )
				{
					//
					// Setup the texture.
					//
					Setup_gl_texture(textures[i++],
					rad_image.pitch,
					rad_image.pixel_size,
					rad_image.maximum_texture_size,
					rad_image.maximum_texture_size,
					rad_image.gl_surface_type,
					rad_image.pixels);
				}
			}

			return( rad_image );
		}
		
		public static void Close_RAD_3D_image( RAD3DIMAGE rad_image )
		{
			if ( rad_image != null)
			{
				if ( rad_image.gl_textures != null)
				{
					//
					// Ask GL to delete the textures.
					GL.DeleteTextures((int)rad_image.total_textures, rad_image.gl_textures);
					rad_image.gl_textures = null;

					// Free our memory.
					rad_image = null;
				}
			}
		}

		public static bool Lock_RAD_3D_image( RAD3DIMAGE rad_image,
                                                    ref Byte[] pixel_buffer,
		                                            ref UInt32 buffer_pitch,
		                                            ref UInt32 surface_type,
		                                            UInt32 src_x,
		                                            UInt32 src_y,
		                                            UInt32 src_w,
		                                            UInt32 src_h )
		{
			if ( rad_image == null ) return true;
			// Fill the variables that were requested.
			if ( rad_image.texture_count < 1 )
			{

				if ( pixel_buffer != null )
				{
					pixel_buffer = rad_image.pixels;
				}

				if ( buffer_pitch != null)
				{
					buffer_pitch = rad_image.pitch;
				}

                if (surface_type != null)
				{
					surface_type = rad_image.rad_surface_type;
				}

                if (src_x != null)
				{
					src_x = 0;
				}

				if ( src_y != null)
                {
					src_y = 0;
				}

				if ( src_w != null)
				{
				    src_w = rad_image.width;
				}

				if ( src_h != null)
				{
					src_h = rad_image.height;
				}

				rad_image.texture_count = 1;

				return( true );
			}
			else
			{
				rad_image.texture_count = 0;

				return( false );
			}
		}
        public static void Unlock_RAD_3D_image( RAD3DIMAGE rad_image )
		{
		  if ( rad_image == null)
		  {
			return;
		  }

		  //
		  // Set the flag to redownload the texture for the next frame.
		  //

		  rad_image.download_textures = 1;
		}
        public static void Submit_texture( Byte[] pixels,
                                    UInt32 row_length,
                                    Int32 width,
                                    Int32 height,
                                    UInt32 surface_type)
        {
            // Set the pixel format options.
            GL.PixelStore(PixelStoreParameter.UnpackRowLength, row_length);
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 8);

            
            // Use TexSubImage because it is faster on some hardware.

            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, (PixelFormat)surface_type, PixelType.UnsignedByte, pixels);
        }


        public static void Submit_vertices(float dest_x,
                                     float dest_y,
                                     float scale_x,
                                     float scale_y,
                                     UInt32 width,
                                     UInt32 height,
                                     float alpha_level)
        {
            float right, bottom;

            // Start a quad.
            GL.Begin(BeginMode.Quads);


            // Set the colors for these vertices.
            GL.Color4(1.0f, 1.0f, 1.0f, alpha_level);

            // Draw around a rectangle.
            right = dest_x + (scale_x * (float)width);
            bottom = dest_y + (scale_y * (float)height);

            GL.TexCoord2(0, 0);
            GL.Vertex3(dest_x, dest_y, 0f);

            GL.TexCoord2(1.0, 0.0);
            GL.Vertex3(right, dest_y, 0f);

            GL.TexCoord2(1.0, 1.0);
            GL.Vertex3(right, bottom, 0f);

            GL.TexCoord2(0.0, 1.0);
            GL.Vertex3(dest_x, bottom, 0f);
            // Done with the vertices.
            GL.End();
        }

        static void Submit_lines( float dest_x,
                                  float dest_y,
                                  float scale_x,
                                  float scale_y,
                                  UInt32 width,
                                  UInt32 height)
        {
            float right, bottom;

            //
            // Start a quad.
            //
            GL.Begin(BeginMode.Lines);

            GL.Color4(1.0f, 1.0f, 1.0f, 1.0f);

            //
            // Draw around a rectangle.
            //

            right = dest_x + (scale_x * (float)width);
            bottom = dest_y + (scale_y * (float)height);

            GL.Vertex3(dest_x, dest_y, 0.0F);
            GL.Vertex3(right, dest_y, 0.0F);

            GL.Vertex3(right, dest_y, 0.0F);
            GL.Vertex3(right, bottom, 0.0F);

            GL.Vertex3(right, bottom, 0.0F);
            GL.Vertex3(dest_x, bottom, 0.0F);

            GL.Vertex3(dest_x, bottom, 0.0F);
            GL.Vertex3(dest_x, dest_y, 0.0F);

            GL.Color4(1.0f, 0.0f, 0.0f, 1.0f);

            GL.Vertex3(dest_x, dest_y, 0.0F);
            GL.Vertex3(right, bottom, 0.0F);

            GL.Vertex3(right, dest_y, 0.0F);
            GL.Vertex3(dest_x, bottom, 0.0F);

            GL.End();
        }
        public static void Blit_RAD_3D_image( RAD3DIMAGE rad_image,
										 float x_offset,
										 float y_offset,
										 float x_scale,
										 float y_scale,
										 float alpha_level )
		{
		  UInt32[] textures;
		  Byte[] pixels;
		  UInt32 x, y;
		  float dest_x, dest_y;
          float adjust_x, adjust_y;
		  UInt32 x_skip, y_skip;

          if (rad_image == null)
          {
              return;
          }
		  // If alpha is disabled and there is no texture alpha, turn alpha off.
		  if ( ( alpha_level >= (1.0F-0.0001) ) && ( rad_image.alpha_pixels ) )
		  {
              GL.Disable(EnableCap.Blend);
		  }
		  else
		  {
              GL.Enable(EnableCap.Blend);
              GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
		  }

		  // Now loop through all of our textures, submitting them.

		  pixels = rad_image.pixels;
		  textures = rad_image.gl_textures;

		  // Calculate how many bytes to move to the next texture block in X.
          // NOTE(TODO): im not sure if this correction is allRite
		  x_skip = ( rad_image.maximum_texture_size * rad_image.pixel_size );

		  //
		  // Calculate how many bytes to move to the next texture block in Y.
		  //

		  y_skip = ( rad_image.pitch *
					 rad_image.maximum_texture_size ) -
				   ( rad_image.textures_across *
					 rad_image.maximum_texture_size *
					1 /*rad_image.pixel_size*/ );

		  adjust_x = ( x_scale * (float) rad_image.maximum_texture_size );
		  adjust_y = ( y_scale * (float) rad_image.maximum_texture_size );
		  
		  dest_y = y_offset;

          int i = 0; int intMaxTexSize = (int)rad_image.maximum_texture_size;
		  for ( y = 0 ; y < rad_image.textures_down ; y++ )
		  {
			dest_x = x_offset;

			for ( x = 0 ; x < rad_image.textures_across ; x++ )
			{
			  // Select the proper texture.
                GL.BindTexture(TextureTarget.Texture2D, textures[i++] );

			  // If we got new pixels, download them.
			  if ( rad_image.download_textures > 0 )
			  {
				Submit_texture( pixels,
								rad_image.row_length,
                                intMaxTexSize,
                                intMaxTexSize,
								rad_image.gl_surface_type );
			  }

			  //
			  // Submit the vertices.
			  Submit_vertices( dest_x,
							   dest_y,
							   x_scale,
							   y_scale,
							   rad_image.maximum_texture_size,
							   rad_image.maximum_texture_size,
							   alpha_level );

			  dest_x += adjust_x;

              Array.Copy(pixels,x_skip,pixels,0,pixels.Length-x_skip);
              
			  //pixels. += x_skip;//wtf i should do now? ;/
			}


			// Handle the right side remnant (if any).

			if ( rad_image.remnant_width > 0 )
			{
			  // Select the proper texture.
                GL.BindTexture(TextureTarget.Texture2D, textures[i++]);

			  // If we got new pixels, download them.
			  if ( rad_image.download_textures > 0)
			  {
				Submit_texture( pixels,
								rad_image.row_length,
                                intMaxTexSize,
                                intMaxTexSize,
								rad_image.gl_surface_type );
			  }

			  //
			  // Submit the vertices.
			  Submit_vertices( dest_x,
							   dest_y,
							   x_scale,
							   y_scale,
							   rad_image.remnant_width,
							   rad_image.maximum_texture_size,
							   alpha_level );
			}

			dest_y += adjust_y;

            Array.Copy(pixels, y_skip, pixels, 0, pixels.Length - y_skip);
		  }

		  // Handle the bottom row remnants (if any).
		  if ( rad_image.remnant_height > 0)
		  {
			dest_x = x_offset;

			for ( x = 0 ; x < rad_image.textures_across ; x++ )
			{
                // Select the proper texture.
                GL.BindTexture(TextureTarget.Texture2D, textures[i++]);

			  // If we got new pixels, download them.
			  if ( rad_image.download_textures > 0)
			  {
				Submit_texture( pixels,
								rad_image.row_length,
								intMaxTexSize,
								(int)rad_image.remnant_input_height,
								rad_image.gl_surface_type );
			  }

			  // Submit the vertices.
			  Submit_vertices( dest_x,
							   dest_y,
							   x_scale,
							   y_scale,
							   rad_image.maximum_texture_size,
							   rad_image.remnant_height,
							   alpha_level );

			  dest_x += adjust_x;

              Array.Copy(pixels, x_skip, pixels, 0, pixels.Length - x_skip);
			}

			if ( rad_image.remnant_width > 0)
			{
                // Select the proper texture.
                GL.BindTexture(TextureTarget.Texture2D, textures[i++]);

			  // If we got new pixels, download them.
			  if ( rad_image.download_textures > 0)
			  {
				Submit_texture( pixels,
								rad_image.row_length,
								(int)rad_image.remnant_input_width,
                                (int)rad_image.remnant_input_height,
								rad_image.gl_surface_type );
			  }

			  //
			  // Submit the vertices.
			  //

			  Submit_vertices( dest_x,
							   dest_y,
							   x_scale,
							   y_scale,
							   rad_image.remnant_width,
							   rad_image.remnant_height,
							   alpha_level );
			}
		  }

		  //
		  // Clear the download texture flag after a blit.
		  //

		  rad_image.download_textures = 0;
		}
#region Bink/RadSurface consts
        public static UInt32 BINKSURFACE8P    =  0;
        public static UInt32 BINKSURFACE24    =  1;
        public static UInt32 BINKSURFACE24R   =  2;
        public static UInt32 BINKSURFACE32    =  3;
        public static UInt32 BINKSURFACE32R   =  4;
        public static UInt32 BINKSURFACE32A   =  5;
        public static UInt32 BINKSURFACE32RA  =  6;
        public static UInt32 BINKSURFACE4444  =  7;
        public static UInt32 BINKSURFACE5551  =  8;
        public static UInt32 BINKSURFACE555   =  9;
        public static UInt32 BINKSURFACE565   = 10;
        public static UInt32 BINKSURFACE655   = 11;
        public static UInt32 BINKSURFACE664   = 12;
        public static UInt32 BINKSURFACEYUY2  = 13;
        public static UInt32 BINKSURFACEUYVY  = 14;
        public static UInt32 BINKSURFACEYV12  = 15;
        public static UInt32 BINKSURFACEMASK  = 15;

        static UInt32 RAD3DSURFACE32    = 0;
        static UInt32 RAD3DSURFACE32A   = 1;
        static UInt32 RAD3DSURFACE555   = 2;
        static UInt32 RAD3DSURFACE565   = 3;
        static UInt32 RAD3DSURFACE5551  = 4;
        static UInt32 RAD3DSURFACE4444  = 5;
        static UInt32 RAD3DSURFACE32R   = 6;
        static UInt32 RAD3DSURFACE32RA  = 7;
        static UInt32 RAD3DSURFACE24    = 8;
        static UInt32 RAD3DSURFACE24R   = 9;
        static UInt32 RAD3DSURFACECOUNT = ( RAD3DSURFACE24R + 1 );
#endregion
        static UInt32[] Bink_surface_type = new UInt32[RAD3DSURFACECOUNT];

        public static void Setup_surface_array()
        {
          Bink_surface_type[ RAD3DSURFACE24 ] = BINKSURFACE24;
          Bink_surface_type[ RAD3DSURFACE24R ] = BINKSURFACE24R;
          Bink_surface_type[ RAD3DSURFACE32 ] = BINKSURFACE32;
          Bink_surface_type[ RAD3DSURFACE32R ] = BINKSURFACE32R;
          Bink_surface_type[ RAD3DSURFACE32A ] = BINKSURFACE32A;
          Bink_surface_type[ RAD3DSURFACE32RA ] = BINKSURFACE32RA;
          Bink_surface_type[ RAD3DSURFACE555 ] = BINKSURFACE555;
          Bink_surface_type[ RAD3DSURFACE565 ] = BINKSURFACE565;
          Bink_surface_type[ RAD3DSURFACE5551 ] = BINKSURFACE5551;
          Bink_surface_type[ RAD3DSURFACE4444 ] = BINKSURFACE4444;
        }
        public static UInt32 Maximum_texture_size = 512;
        public unsafe static bool Allocate_3D_images(CookieMonster_Objects.VideoPlayer src)
        {
            // Now, all is based on BinkBuffer API
            // Try of implement Bink over c# and openTK was above my skills :(
            return false;
            /*
            RAD3DIMAGE New_image;
            // Try to open a 3D image for the foreground Bink.
            New_image = Open_RAD_3D_image(src.rad3d,
                                                src.binkRef->Width,
                                                src.binkRef->Height,
                                                ((src.binkRef->OpenFlags & DLL.Bink.openFlags.BINKALPHA) == DLL.Bink.openFlags.BINKALPHA),
                                                Maximum_texture_size);
            if (New_image != null)
            {

                if (src.radImage != null)
                    Close_RAD_3D_image(src.radImage);
                src.setNewRADIMAGE3D(New_image);
                //
                // Advance the Bink to fill the textures.
                //

                Decompress_frame(src, true);
                DLL.Bink.BinkNextFrame(src.binkRef);

                return (true);
            }

            // Free the images if any were opened.
            if (src.radImage!=null)
                Close_RAD_3D_image(src.radImage);

            return (false);
             */
        }
        static uint BINKCOPYALL = 0x80000000;
        static public unsafe void ProcessFrame(CookieMonster_Objects.VideoPlayer src)
        {
            // Added (orc):
            // set dst posx/y to position that will render video in the center of screen
            try
            {
                int hlp = (int)(engine.Width / 2 - src.binkRef->Width / 2);
                //if (hlp < 0) hlp = 0;
                UInt32 dstx = (uint)Math.Max(hlp, 0);
                hlp = (int)(engine.Height / 2 - src.binkRef->Height / 2);
                if (hlp < 0) hlp = 0;
                UInt32 dsty = (uint)Math.Max(hlp, 0);
                // Decompress the Bink frame.
                // (by binkw3.dll)
                DLL.Bink.BinkDoFrame(src.binkRef);
                // Lock the 3D image so that we can copy the decompressed frame into it.
                while (DLL.Bink.BinkWait(src.binkRef) > 0)
                    System.Threading.Thread.Sleep(6);

                if (DLL.Bink.BinkBufferLock(src.binkBufRef) != 0)
                {
                    //
                    // Copy the decompressed frame into the 3D image.
                    DLL.Bink.BinkCopyToBuffer(src.binkRef,
                                          src.binkBufRef->Buffer,
                                          src.binkBufRef->BufferPitch,
                                          src.binkBufRef->Height,
                                          dstx,
                                          dsty,
                                          src.binkBufRef->SurfaceType | BINKCOPYALL | 0x00080000);

                    DLL.Bink.BinkBufferUnlock(src.binkBufRef);
                }
                DLL.Bink.BinkBufferBlit(src.binkBufRef, src.binkRef->FrameRects, DLL.Bink.BinkGetRects(src.binkRef, src.binkBufRef->SurfaceType));
            }
            catch (Exception e)
            {
                try // exception catched mostly cause of wrong binkBufferOpen flags
                {   // so re-create bink buffer using different flag (1)
                    src.recreateBinkBuffer();
                }
                catch { }
            }
                DLL.Bink.BinkNextFrame(src.binkRef);
        }


        internal unsafe static void Show_foreground_frame(CookieMonster_Objects.VideoPlayer src)
        {

            // Now, all is based on BinkBuffer API
            // Try of implement Bink over c# and openTK was above my skills :(
            /*
           // Start_timer();
            Decompress_frame(src, false);
           //End_and_start_next_timer(Bink_microseconds);

            // Begin a 3D frame.
            Start_RAD_3D_frame(src.rad3d);//clearing and stuff

            // Draw the image on the screen.
            Blit_RAD_3D_image(src.radImage, 0, 0, 1.0F, 1.0F, 1.0F);
            
            // End a 3D frame.
            End_RAD_3D_frame(src.rad3d);

            //End_timer(Render_microseconds);

            // Keep playing the movie.
            DLL.Bink.BinkNextFrame(src.binkRef);
             */
        }
    }//BinkGL class
}
