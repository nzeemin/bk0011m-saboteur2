﻿using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        PrepareNchrs();
        PrepareGchrs();
        PrepareBchrsOchrs();
        PreparePanelTiles();
        Console.WriteLine("DONE");
    }

    // Prepare Ninja tiles, 246 tiles 8x8, no mask
    // At 0 usual tiles: mask word, pixels word
    // At 246 * 2 * 8 = 3936: mirrored tiles
    static void PrepareNchrs()
    {
        Bitmap bmpTiles = new Bitmap(@"..\..\..\njtiles.png");

        byte[] bytes = new byte[16384];
        for (int tile = 0; tile < 246; tile++)
        {
            int basex = 8 + (tile % 16) * 19;
            int basey = 8 + (tile / 16) * 10;
            for (int h = 0; h < 8; h++)
            {
                int offset = tile * 2 * 8 + h * 2;
                int offset2 = 3936 + offset;
                int bb = 0, bbr = 0; //mm = 0, mmr = 0;
                for (int x = 0; x < 8; x++)
                {
                    Color color = bmpTiles.GetPixel(basex + (7 - x), basey + h);
                    int index = ((color.ToArgb() & 0xffffff) == 0x000000) ? 3 : 0;
                    bb = bb << 2;  bb |= index;
                    bbr = bbr >> 2;  bbr |= index << 14;
                    //Color color2 = bmpTiles.GetPixel(basex + 9 + (7 - x), basey + h);
                    //int index2 = ((color2.ToArgb() & 0xffffff) == 0xB22222) ? 3 : 0;
                    //mm = mm << 2;  mm |= index2;
                    //mmr = mmr >> 2;  mmr |= index2 << 14;
                }
                
                //bytes[offset + 0] = (byte)(mm & 0xff);
                //bytes[offset + 1] = (byte)(mm >> 8);
                bytes[offset + 0] = (byte)(bb & 0xff);
                bytes[offset + 1] = (byte)(bb >> 8);
                //bytes[offset2 + 0] = (byte)(mmr & 0xff);
                //bytes[offset2 + 1] = (byte)(mmr >> 8);
                bytes[offset2 + 0] = (byte)(bbr & 0xff);
                bytes[offset2 + 1] = (byte)(bbr >> 8);
            }
        }
        
        FileStream fs = new FileStream("S2NCHR.DAT", FileMode.Create);
        BinaryWriter writer = new BinaryWriter(fs);
        writer.Write(bytes);
        writer.Flush();
        Console.WriteLine("S2NCHR.DAT saved");
    }

    // Prepare Guards tiles, 196 tiles 8x8 with mask
    // At 0 usual tiles: mask word, pixels word
    // At 196 * 4 * 8 = 6272 mirrored tiles
    static void PrepareGchrs()
    {
        Bitmap bmpTiles = new Bitmap(@"..\..\..\gdtiles.png");

        byte[] bytes = new byte[16384];
        for (int tile = 0; tile < 196; tile++)
        {
            int basex = 8 + (tile % 16) * 19;
            int basey = 8 + (tile / 16) * 10;
            for (int h = 0; h < 8; h++)
            {
                int offset = tile * 4 * 8 + h * 4;
                int offset2 = 6272 + offset;
                int bb = 0, bbr =0, mm = 0, mmr = 0;
                for (int x = 0; x < 8; x++)
                {
                    Color color = bmpTiles.GetPixel(basex + (7 - x), basey + h);
                    int index = ((color.ToArgb() & 0xffffff) == 0x000000) ? 3 : 0;
                    bb = bb << 2;  bb |= index;
                    bbr = bbr >> 2;  bbr |= index << 14;
                    Color color2 = bmpTiles.GetPixel(basex + 9 + (7 - x), basey + h);
                    int index2 = ((color2.ToArgb() & 0xffffff) == 0xB22222) ? 3 : 0;
                    mm = mm << 2;  mm |= index2;
                    mmr = mmr >> 2;  mmr |= index2 << 14;
                }
                
                bytes[offset + 0] = (byte)(mm & 0xff);
                bytes[offset + 1] = (byte)(mm >> 8);
                bytes[offset + 2] = (byte)(bb & 0xff);
                bytes[offset + 3] = (byte)(bb >> 8);
                bytes[offset2 + 0] = (byte)(mmr & 0xff);
                bytes[offset2 + 1] = (byte)(mmr >> 8);
                bytes[offset2 + 2] = (byte)(bbr & 0xff);
                bytes[offset2 + 3] = (byte)(bbr >> 8);
            }
        }
        
        FileStream fs = new FileStream("S2GCHR.DAT", FileMode.Create);
        BinaryWriter writer = new BinaryWriter(fs);
        writer.Write(bytes);
        writer.Flush();
        Console.WriteLine("S2GCHR.DAT saved");
    }
    
    // Prepare background tiles, 255 tiles 8x10
    // Prepare foregraund tiles, 142 tiles 8x10
    static void PrepareBchrsOchrs()
    {
        
        byte[] bytes = new byte[16384];
        
        var bmpBTiles = new Bitmap(@"..\..\..\bktiles.png");
        for (int tile = 0; tile < 255; tile++)
        {
            int basex = 8 + (tile % 16) * 10;
            int basey = 8 + (tile / 16) * 12;
            var words = DecodeTileFromBitmap(bmpBTiles, basex, basey);
            var offset = tile * 20;
            for (int i = 0; i < 10; i++)
            {
                bytes[offset + i * 2 + 0] = (byte)(words[i] & 0xff);
                bytes[offset + i * 2 + 1] = (byte)(words[i] >> 8);
            }
        }
        
        var bmpOTiles = new Bitmap(@"..\..\..\ontiles.png");
        for (int tile = 0; tile < 146; tile++)
        {
            int basex = 8 + (tile % 16) * 19;
            int basey = 8 + (tile / 16) * 12;
            var words = DecodeTileFromBitmap(bmpOTiles, basex, basey);
            var offset = 5120 + tile * 40;
            for (int i = 0; i < 10; i++)
            {
                int mm = 0;
                for (int x = 0; x < 8; x++)
                {
                    Color color2 = bmpOTiles.GetPixel(basex + 9 + (7 - x), basey + i);
                    int index2 = ((color2.ToArgb() & 0xffffff) == 0xB22222) ? 3 : 0;
                    mm = mm << 2;  mm |= index2;
                }
                mm = mm ^ 0xffff;  // Invert the mask
                
                bytes[offset + i * 4 + 0] = (byte)(mm & 0xff);
                bytes[offset + i * 4 + 1] = (byte)(mm >> 8);
                bytes[offset + i * 4 + 2] = (byte)(words[i] & 0xff);
                bytes[offset + i * 4 + 3] = (byte)(words[i] >> 8);
            }
        }
        
        FileStream fs = new FileStream("S2TILE.DAT", FileMode.Create);
        BinaryWriter writer = new BinaryWriter(fs);
        writer.Write(bytes);
        writer.Flush();
        Console.WriteLine("S2TILE.DAT saved");
    }

    static void PreparePanelTiles()
    {
        Bitmap bmpTiles = new Bitmap(@"..\..\..\pantiles.png");

        FileStream fs = new FileStream("S2PANC.MAC", FileMode.Create);
        StreamWriter writer = new StreamWriter(fs);
        writer.WriteLine("; START OF S2PANC.MAC");
        writer.WriteLine();

        for (int tile = 0; tile < 20; tile++)
        {
            int basex = 8 + (tile % 16) * 10;
            int basey = 8 + (tile / 16) * 12;
            var words = DecodeTileFromBitmap(bmpTiles, basex, basey, 8);
            WriteWordArrayDump(writer, words);
        }

        writer.WriteLine();
        writer.WriteLine("; END OF S2PANC.MAC");
        writer.Flush();
        Console.WriteLine("S2PANC.MAC saved");
    }
    
    static UInt16[] DecodeTileFromBitmap(Bitmap bmp, int basex, int basey, int lines = 10)
    {
        var words = new UInt16[lines];
        for (int i = 0; i < lines; i++)
        {
            int bb = 0;
            for (int x = 0; x < 8; x++)
            {
                Color color = bmp.GetPixel(basex + (7 - x), basey + i);
                int index = ColorToIndex(color);
                bb = bb << 2;
                bb |= index;
            }

            words[i] = (UInt16)bb;
        }

        return words;
    }

    static int ColorToIndex(Color color)
    {
        if ((color.ToArgb() & 0xffffff) == 0x00ffff) return 2;  // Cyan
        if ((color.ToArgb() & 0xffffff) == 0xff00ff) return 3;  // Magenta
        if ((color.ToArgb() & 0xffffff) == 0x0000ff) return 1;  // Blue
        return 0;
    }
    
    static void WriteWordArrayDump(StreamWriter file, UInt16[] array)
    {
        for (int offset = 0; offset < array.Length; offset++)
        {
            if (offset % 8 == 0)
                file.Write("\t.WORD\t");
            var value = array[offset];
            file.Write($"{EncodeOctalString2(value)}");
            if (offset % 8 == 7 || offset + 1 >= array.Length)
                file.WriteLine();
            else
                file.Write(", ");
        }
    }
    
    static string EncodeOctalString2(UInt16 x)
    {
        return string.Format(
            @"{0}{1}{2}{3}{4}{5}",
            ((x >> 15) & 7),
            ((x >> 12) & 7),
            ((x >> 9) & 7),
            ((x >> 6) & 7),
            ((x >> 3) & 7),
            (x & 7)
        );
    }
}