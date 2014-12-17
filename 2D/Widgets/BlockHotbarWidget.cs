﻿using System;
using System.Drawing;
using OpenTK.Input;

namespace ClassicalSharp {
	
	public sealed class BlockHotbarWidget : Widget {
		
		public BlockHotbarWidget( Game window ) : base( window ) {
			HorizontalDocking = Docking.Centre;
			VerticalDocking = Docking.BottomOrRight;
		}
		
		Texture[] barTextures = new Texture[9];
		Texture selectedBlock;
		const int blockSize = 32;
		
		public override bool HandlesKeyDown( Key key ) {
			if( key >= Key.Number1 && key <= Key.Number9 ) {
				try {
					Window.HeldBlockIndex = (int)key - (int)Key.Number1;
				} catch( InvalidOperationException ) {
					Window.AddChat( "&e/client: &cThe server has forbidden you from changing your held block." );
				}
				return true;
			}
			return false;
		}
		
		public override void Init() {
			int y = Window.Height - blockSize;
			
			using( Bitmap bmp = new Bitmap( blockSize, blockSize ) ) {
				using( Graphics g = Graphics.FromImage( bmp ) ) {
					using( Pen pen = new Pen( Color.White, blockSize / 8 ) ) {
						g.DrawRectangle( pen, 0, 0, blockSize, blockSize );
					}
				}
				selectedBlock = Utils2D.Make2DTexture( GraphicsApi, bmp, 0, y );
			}
			
			int x = Window.Width / 2 - ( blockSize * barTextures.Length ) / 2;
			X = x;
			Y = y;
			Width = blockSize * barTextures.Length;
			Height = blockSize;
			
			for( int i = 0; i < barTextures.Length; i++ ) {
				Block block = Window.BlocksHotbar[i];
				int texId = Window.BlockInfo.GetOptimTextureLoc( (byte)block, TileSide.Left );
				TextureRectangle rec = Window.TerrainAtlas.GetTexRec( texId );
				
				int verSize = blockSize;
				float height = Window.BlockInfo.BlockHeight( (byte)block );
				int blockY = y;
				if( height != 1 ) {
					rec.V1 = rec.V1 + Window.TerrainAtlas.invVerElementSize * height;
					verSize = (int)( blockSize * height );
					blockY = y + blockSize - verSize;
				}
				barTextures[i] = new Texture( -1, x, blockY, blockSize, verSize, rec );
				x += blockSize;
			}
		}
		
		public override void Render( double delta ) {
			GraphicsApi.Texturing = true;
			// TODO: Maybe redesign this so we don't have to bind the whole atlas. Not cheap.
			GraphicsApi.Bind2DTexture( Window.TerrainAtlasTexId );
			int selectedX = 0;
			for( int i = 0; i < barTextures.Length; i++ ) {
				barTextures[i].RenderNoBind( GraphicsApi );
				if( i == Window.HeldBlockIndex ) {
					selectedX = barTextures[i].X1;
				}
			}
			selectedBlock.X1 = selectedX;
			selectedBlock.Render( GraphicsApi );
			GraphicsApi.Texturing = false;
		}
		
		public override void Dispose() {
			GraphicsApi.DeleteTexture( ref selectedBlock );
		}
		
		public override void MoveTo( int newX, int newY ) {
			int deltaX = newX - X;
			int deltaY = newY - Y;
			X = newX;
			Y = newY;
			selectedBlock.X1 += deltaX;
			selectedBlock.Y1 += deltaY;
			
			for( int i = 0; i < barTextures.Length; i++ ) {
				Texture tex = barTextures[i];
				tex.X1 += deltaX;
				tex.Y1 += deltaY;				
				barTextures[i] = tex;
			}
		}
	}
}