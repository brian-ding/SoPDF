﻿using SoPDF.Core.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoPDF.Core
{
    public class PdfDocument
    {
        private List<Byte[]> _buffers;
        private List<Int32> _offsets;
        private Int32 _offset;

        public PdfDocument(String content)
        {
            _offset = 0;
            _buffers = new List<byte[]>();
            _offsets = new List<int>();

            // header
            _buffers.Add(Encoding.UTF8.GetBytes("%PDF-1.4\n%âãÏÓ\n"));
            _offset += _buffers.Last().Length;
            _offsets.Add(_offset);

            // pages
            PdfPageTree pageTree = new PdfPageTree();

            // page
            PdfPage page = new PdfPage();
            PdfFont font = new PdfFont();
            page.SetFont(font);
            PdfStream stream = new PdfStream(content);
            page.AddContent(stream);
            pageTree.AddPage(page);

            _buffers.Add(pageTree.ToPDF());
            _offset += _buffers.Last().Length;
            _offsets.Add(_offset);

            _buffers.Add(page.ToPDF());
            _offset += _buffers.Last().Length;
            _offsets.Add(_offset);

            _buffers.Add(font.ToPDF());
            _offset += _buffers.Last().Length;
            _offsets.Add(_offset);

            _buffers.Add(stream.ToPDF());
            _offset += _buffers.Last().Length;
            _offsets.Add(_offset);

            PdfCatalog catalog = new PdfCatalog(page);
            _buffers.Add(catalog.ToPDF());
            _offset += _buffers.Last().Length;


            // x ref table
            String table = String.Empty;
            table += "xref" + "\n";
            table += "0 " + _buffers.Count.ToString() + "\n";
            table += "0000000000 65535 f\r\n";
            foreach (var offset in _offsets)
            {
                table += offset.ToString().PadLeft(10, '0') + " " + "00000" + " " + "n" + "\r\n";
            }
            _buffers.Add(Encoding.UTF8.GetBytes(table));


            // trailer
            String trailer = String.Empty;

            trailer += "<<" + "\n";
            trailer += "/Root " + catalog.GetRefStr() + "\n";
            trailer += "/Size " + (_buffers.Count - 1).ToString() + "\n";
            trailer += ">>" + "\n";
            trailer += "endobj" + "\n";
            _buffers.Add(Encoding.UTF8.GetBytes(trailer));

            _buffers.Add(Encoding.UTF8.GetBytes("startxref" + "\n" + _offset.ToString() + "\n" + "%%EOF"));
        }

        public void Save(String path)
        {
            Int32 length = 0;
            foreach (Byte[] buffer in _buffers)
            {
                length += buffer.Length;
            }



            Byte[] pdfBuffer = new Byte[length];

            length = 0;
            foreach (Byte[] buffer in _buffers)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    pdfBuffer[length + i] = buffer[i];
                }
                length += buffer.Length;
            }


            File.WriteAllBytes(path, pdfBuffer);
        }
    }
}